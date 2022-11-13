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
        public const int ArrayMaxDumpSize = 32;

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
                var svalue = GetValueDisplayString(value);
                Writer.WriteLine(svalue);
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
                        var svalue = GetValueDisplayString(item);
                        Writer.WriteLine("[" + i + "]: " + svalue);
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

                var name = member.DisplayName;
                if (IsValue(context, memberValue))
                {
                    var svalue = GetValueDisplayString(memberValue);
                    Writer.WriteLine(name + ": " + svalue);
                }
                else
                {
                    Writer.WriteLine(name + ":");
                    Visit(context, memberValue, value);
                }
                anyMember = true;
            }

            if (!anyMember)
            {
                var svalue = GetValueDisplayString(value);
                Writer.WriteLine(svalue);
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

        protected virtual string GetValueDisplayString(object? value)
        {
            if (value == null)
                return string.Empty;

            if (value is IValueProvider valueProvider)
            {
                value = valueProvider.Value;
            }

            if (value is byte[] bytes)
            {
                var max = ArrayMaxDumpSize;
                if (bytes.Length > max)
                    return bytes.ToHexa(max) + "... (size: " + bytes.Length + ")";

                return bytes.ToHexa();
            }

            var enumerable = getEnumerable();
            if (enumerable != null)
            {
                var s = string.Join(", ", enumerable.OfType<object>().Take(32).Select(o => o?.ToString()));
                if (value is Array array && array.Rank == 1)
                {
                    var max = ArrayMaxDumpSize;
                    if (array.Length > max)
                    {
                        s += "... (size: " + array.Length + ")";
                    }
                }
                return s;
            }

            return value.ToString()!;

            IEnumerable? getEnumerable()
            {
                if (value is string || value is char[])
                    return null;

                return value as IEnumerable;
            }
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
                    var dn = PropertyDescriptor.Attributes.OfType<DisplayNameAttribute>().FirstOrDefault();
                    if (dn != null && !string.IsNullOrWhiteSpace(dn.DisplayName))
                        return dn.DisplayName;

                    return DisplayName;
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
                    var dn = MemberInfo.GetCustomAttribute<DisplayNameAttribute>();
                    if (dn != null && !string.IsNullOrWhiteSpace(dn.DisplayName))
                        return dn.DisplayName;

                    if (MemberInfo is FieldInfo)
                    {
                        // DisplayNameAttribute is not usable for fields so we use DescriptionAttribute
                        var da = MemberInfo.GetCustomAttribute<DescriptionAttribute>();
                        if (da != null && !string.IsNullOrWhiteSpace(da.Description))
                            return da.Description;
                    }

                    return base.DisplayName;
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

            public abstract object? GetValue(object? instance);

            public override string ToString() => Name;
        }
    }
}
