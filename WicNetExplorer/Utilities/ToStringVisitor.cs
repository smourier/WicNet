using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using WicNet.Utilities;

namespace WicNetExplorer.Utilities
{
    public partial class ToStringVisitor
    {
        private int? _indent = -1;

        public ToStringVisitor(TextWriter writer, string? tabString = null)
        {
            ArgumentNullException.ThrowIfNull(writer);
            tabString ??= IndentedTextWriter.DefaultTabString;
            Writer = new IndentedTextWriter(writer, tabString);
        }

        public IndentedTextWriter Writer { get; }

        public static string Visit(object? value, string? tabString = null)
        {
            using var sw = new StringWriter();
            var visitor = new ToStringVisitor(sw, tabString);
            visitor.Visit(new ToStringVisitorContext(), value);
            return sw.ToString();
        }

        public virtual void Visit(ToStringVisitorContext context, object? value, object? parent = null)
        {
            ArgumentNullException.ThrowIfNull(context);
            if (value == null)
                return;

            if (context.VisitedObjects.Contains(value))
                return;

            context.VisitedObjects.Add(value);

            if (IsValue(context, value))
            {
                WriteValueAsString(value);
                return;
            }

            var enumerable = GetEnumerable(context, value);
            if (enumerable != null)
            {
                var i = 0;
                Indent++;
                foreach (var item in enumerable)
                {
                    if (IsValue(context, item))
                    {
                        Writer.Write("[" + i + "]: ");
                        WriteValueAsString(item);
                    }
                    else
                    {
                        Writer.WriteLine("[" + i + "]:");
                        Visit(context, item, value);
                    }
                    i++;
                }
                Indent--;
                return;
            }

            Indent++;

            var anyMember = false;
            foreach (var member in GetMembers(context, value))
            {
                object? memberValue;
                try
                {
                    memberValue = member.GetValue(value);
                }
                catch
                {
                    if (context.ThrowOnGetValue)
                        throw;

                    continue;
                }

                if ((!member.WriteIfEmpty || !Settings.Current.CopyEmptyElementsToClipboard) && isValueEmpty())
                    continue;

                var name = member.DisplayName;
                if (IsValue(context, memberValue))
                {
                    Writer.Write(name + ": ");
                    WriteValueAsString(memberValue);
                }
                else
                {
                    Writer.WriteLine(name + ":");
                    Visit(context, memberValue, value);
                }
                anyMember = true;

                bool isValueEmpty()
                {
                    if (memberValue == null)
                        return true;

                    if (memberValue is IEnumerable enumerable && !enumerable.Cast<object>().Any())
                        return true;

                    if (memberValue is ICustomTypeDescriptor desc && desc.GetProperties().Count == 0)
                        return true;

                    return false;
                }
            }

            if (!anyMember)
            {
                WriteValueAsString(value);
            }
            Indent--;
        }

        // we need this because
        // 1) IndentedTextWriter doesn't handle negative (start) indent values
        // 2) IndentedTextWriter has a bug where it doesn't output the tab of the first line (as no writeline was ever called before)
        private int Indent
        {
            get
            {
                if (_indent.HasValue)
                    return _indent.Value;

                return Writer.Indent;
            }
            set
            {
                if (value < 0)
                {
                    _indent = value;
                    return;
                }
                _indent = null;
                Writer.Indent = value;
            }
        }

        protected virtual void WriteValueAsString(object? value)
        {
            if (value is IValueProvider valueProvider)
            {
                value = valueProvider.Value;
            }

            if (value == null)
            {
                Writer.WriteLine();
                return;
            }

            if (value is byte[] bytes)
            {
                var max = Math.Max(0, Settings.Current.MaxArrayElementToCopyToClipboard);
                Writer.WriteLine(string.Format(Resources.ArrayOfCount, bytes.Length, typeof(byte).Name + "(s)"));
                Writer.Indent++;
                bytes.WriteHexaDump(Writer, max);

                if (max < bytes.Length)
                {
                    Writer.Write("...");
                }

                Writer.Indent--;
                Writer.WriteLine();
                return;
            }

            var enumerable = getEnumerable();
            if (enumerable != null)
            {
                var max = Math.Max(0, Settings.Current.MaxArrayElementToCopyToClipboard);
                var i = 0;
                int? count = null;
                if (enumerable is Array array && array.Rank == 1)
                {
                    count = array.Length;
                }

                var written = 0;
                const int chunkSize = 16;
                foreach (var chunk in enumerable.OfType<object>().Chunk(chunkSize))
                {
                    if (chunk.Length == 0)
                        break;

                    if (i == 0)
                    {
                        if (count.HasValue)
                        {
                            Writer.WriteLine(string.Format(Resources.ArrayOfCount, count.Value, chunk.First().GetType().Name + "(s)"));
                        }
                        else
                        {
                            Writer.WriteLine(string.Format(Resources.ArrayOf, chunk.First().GetType().Name));
                        }
                        Writer.Indent++;
                        if (max == 0)
                            break;
                    }

                    var take = chunkSize;
                    if (max > 0)
                    {
                        var left = max - written;
                        if (left < take)
                        {
                            take = left;
                        }
                    }

                    var s = string.Join(", ", chunk.Take(take));
                    Writer.WriteLine(s);
                    written += chunk.Length;
                    i++;
                }

                if (i == 0)
                {
                    Writer.WriteLine();
                }
                Writer.Indent--;
                return;
            }

            var svalue = GetValueDisplayString(value);
            Writer.WriteLine(svalue);

            IEnumerable? getEnumerable()
            {
                if (value is string || value is char[])
                    return null;

                return value as IEnumerable;
            }
        }

        protected virtual string GetValueDisplayString(object? value)
        {
            if (value is IValueProvider valueProvider)
            {
                value = valueProvider.Value;
            }
            if (value == null)
                return string.Empty;

            return value.ToString()!;
        }

        protected virtual bool IsValue(ToStringVisitorContext context, object? value)
        {
            ArgumentNullException.ThrowIfNull(context);
            if (value == null)
                return true;

            var tc = Type.GetTypeCode(value.GetType());
            if (tc != TypeCode.Object)
                return true;

            if (value is byte[])
                return true;

            if (value is Guid)
                return true;

            if (value is TimeSpan)
                return true;

            if (value is DateTimeOffset)
                return true;

            var enumerable = value as IEnumerable;
            if (enumerable == null && !HasAnyMember(context, value))
                return true;

            var sva = value.GetType().GetCustomAttribute<ToStringVisitorAttribute>();
            if (sva != null)
            {
                if (sva.ForceIsValue)
                    return true;
            }

            return false;
        }

        private bool HasAnyMember(ToStringVisitorContext context, object value) => GetMembers(context, value).Any();
        protected virtual IEnumerable<Member> GetMembers(ToStringVisitorContext context, object value)
        {
            if (value == null)
                yield break;

            if (value is ICustomTypeDescriptor)
            {
                foreach (var desc in TypeDescriptor.GetProperties(value).OfType<PropertyDescriptor>())
                {
                    yield return new PropertyDescriptorMember(desc);
                }
                yield break;
            }

            foreach (var memberInfo in value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).OrderBy(m => m.Name))
            {
                if (!CanGetValue(context, memberInfo))
                    continue;

                yield return new MemberInfoMember(memberInfo);
            }
        }

        protected virtual bool CanGetValue(ToStringVisitorContext context, MemberInfo info)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(info);
            var att = info.GetCustomAttribute<BrowsableAttribute>();
            if (att != null && !att.Browsable)
                return false;

            var tsa = info.GetCustomAttribute<ToStringVisitorAttribute>();
            if (tsa != null && tsa.Ignore)
                return false;

            if (info is PropertyInfo prop)
                return prop.CanRead;

            return true;
        }

        protected virtual IEnumerable? GetEnumerable(ToStringVisitorContext context, object value)
        {
            ArgumentNullException.ThrowIfNull(context);
            if (value is string || value is byte[] || value is char[])
                return null;

            return value as IEnumerable;
        }

        protected class PropertyDescriptorMember : Member
        {
            public PropertyDescriptorMember(PropertyDescriptor prop)
            {
                ArgumentNullException.ThrowIfNull(prop);
                PropertyDescriptor = prop;
            }

            public PropertyDescriptor PropertyDescriptor { get; }
            public override string Name => PropertyDescriptor.Name;
            public override string DisplayName
            {
                get
                {
                    var tsa = PropertyDescriptor.Attributes.OfType<ToStringVisitorAttribute>().FirstOrDefault();
                    if (tsa != null && tsa.DisplayName != null)
                        return tsa.DisplayName;

                    var dn = PropertyDescriptor.Attributes.OfType<DisplayNameAttribute>().FirstOrDefault();
                    if (dn != null && dn.DisplayName != null)
                        return dn.DisplayName;

                    return DisplayName;
                }
            }

            public override bool WriteIfEmpty
            {
                get
                {
                    var tsa = PropertyDescriptor.Attributes.OfType<ToStringVisitorAttribute>().FirstOrDefault();
                    if (tsa != null)
                        return !tsa.DontWriteIfEmpty;

                    return base.WriteIfEmpty;
                }
            }

            public override object? GetValue(object? instance) => PropertyDescriptor.GetValue(instance);
        }

        protected class MemberInfoMember : Member
        {
            public MemberInfoMember(MemberInfo info)
            {
                ArgumentNullException.ThrowIfNull(info);
                MemberInfo = info;
            }

            public MemberInfo MemberInfo { get; }
            public override string Name => MemberInfo.Name;
            public override string DisplayName
            {
                get
                {
                    var tsa = MemberInfo.GetCustomAttribute<ToStringVisitorAttribute>();
                    if (tsa != null && tsa.DisplayName != null)
                        return tsa.DisplayName;

                    var dn = MemberInfo.GetCustomAttribute<DisplayNameAttribute>();
                    if (dn != null && dn.DisplayName != null)
                        return dn.DisplayName;

                    if (MemberInfo is FieldInfo)
                    {
                        // DisplayNameAttribute is not usable for fields so we use DescriptionAttribute
                        var da = MemberInfo.GetCustomAttribute<DescriptionAttribute>();
                        if (da != null && da.Description != null)
                            return da.Description;
                    }

                    return base.DisplayName;
                }
            }

            public override bool WriteIfEmpty
            {
                get
                {
                    var tsa = MemberInfo.GetCustomAttribute<ToStringVisitorAttribute>();
                    if (tsa != null)
                        return !tsa.DontWriteIfEmpty;

                    return base.WriteIfEmpty;
                }
            }

            public override object? GetValue(object? instance)
            {
                if (MemberInfo is PropertyInfo pi)
                {
                    ArgumentNullException.ThrowIfNull(instance);
                    return pi.GetValue(instance);
                }

                if (MemberInfo is FieldInfo fi)
                {
                    ArgumentNullException.ThrowIfNull(instance);
                    return fi.GetValue(instance);
                }

                throw new NotImplementedException();
            }
        }

        protected abstract class Member
        {
            public abstract string Name { get; }
            public virtual string DisplayName => Name.Decamelize();

            public virtual bool WriteIfEmpty => true;
            public abstract object? GetValue(object? instance);

            public override string ToString() => Name;
        }
    }
}
