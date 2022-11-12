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
                foreach (var item in enumerable)
                {
                    if (IsValue(context, item))
                    {
                        Writer.Indent++;
                        var svalue = GetValueDisplayString(item);
                        Writer.WriteLine("[" + i + "]: " + svalue);
                    }
                    else
                    {
                        Writer.WriteLine("[" + i + "]:");
                        Writer.Indent++;
                        Visit(context, item, value);
                    }
                    Writer.Indent--;
                    i++;
                }
                return;
            }

            foreach (var member in GetMembers(context, value.GetType()))
            {
                if (!CanGetValue(context, member))
                    continue;

                object? memberValue;
                try
                {
                    memberValue = GetValue(context, member, value);
                }
                catch
                {
                    if (context.ThrowOnGetValue)
                        throw;

                    continue;
                }

                var name = GetMemberDisplayName(member);
                if (IsValue(context, memberValue))
                {
                    var svalue = GetValueDisplayString(memberValue);
                    Writer.WriteLine(name + ": " + svalue);
                }
                else
                {
                    Writer.Indent++;
                    Writer.WriteLine(name + ":");
                    Visit(context, memberValue, value);
                    Writer.Indent--;
                }
            }
        }

        protected virtual string GetValueDisplayString(object? value)
        {
            if (value == null)
                return string.Empty;

            if (value is byte[] bytes)
            {
                var max = 32;
                if (bytes.Length > max)
                    return bytes.ToHexa(max) + "... (size: " + bytes.Length + ")";
                
                return bytes.ToHexa();
            }

            return value.ToString()!;
        }

        protected virtual string GetMemberDisplayName(MemberInfo member)
        {
            ArgumentNullException.ThrowIfNull(member);
            return member.Name.Decamelize();
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

            return false;
        }

        protected virtual IEnumerable<MemberInfo> GetMembers(ToStringVisitorContext context, Type type) => type.GetProperties(BindingFlags.Instance | BindingFlags.Public).OrderBy(m => m.Name);
        protected virtual bool CanGetValue(ToStringVisitorContext context, MemberInfo info)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(info);
            var att = info.GetCustomAttribute<BrowsableAttribute>();
            if (att != null && !att.Browsable)
                return false;

            if (info is PropertyInfo prop)
                return prop.CanRead;

            return true;
        }

        protected virtual object? GetValue(ToStringVisitorContext context, MemberInfo info, object value)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(info);
            ArgumentNullException.ThrowIfNull(value);
            if (info is PropertyInfo prop)
                return prop.GetValue(value);

            return true;
        }

        protected virtual IEnumerable? GetEnumerable(ToStringVisitorContext context, object value)
        {
            ArgumentNullException.ThrowIfNull(context);
            if (value is string || value is byte[] || value is char[])
                return null;

            return value as IEnumerable;
        }
    }
}
