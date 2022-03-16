using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace DirectN
{
    public static class Extensions
    {
        private static readonly Lazy<ConcurrentDictionary<Guid, string>> _guids = new Lazy<ConcurrentDictionary<Guid, string>>(ExtractAllGuids, true);

        public static ComMemory StructureToMemory(this object structure) => new ComMemory(structure);

        public static IntPtr StructureToPtr(this object structure)
        {
            if (structure == null)
                return IntPtr.Zero;

            int size = Marshal.SizeOf(structure);
            var ptr = Marshal.AllocCoTaskMem(size);
            Marshal.StructureToPtr(structure, ptr, false);
            return ptr;
        }

        public static IntPtr StructureToPtr<T>(this T structure)
        {
            if (structure == null)
                return IntPtr.Zero;

            int size = Marshal.SizeOf<T>();
            var ptr = Marshal.AllocCoTaskMem(size);
            Marshal.StructureToPtr<T>(structure, ptr, false);
            return ptr;
        }

        public static string ToName(this Guid guid, string formatIfNotFound = null)
        {
            if (guid == Guid.Empty)
                return nameof(Guid.Empty);

            if (_guids.Value.TryGetValue(guid, out string name))
                return name;

            if (FourCC.IsFourCC(guid))
            {
                name = FourCC.ToString(guid);
                _guids.Value.AddOrUpdate(guid, name, (k, old) => old);
                return name;
            }

            return guid.ToString(formatIfNotFound);
        }

        private static ConcurrentDictionary<Guid, string> ExtractAllGuids()
        {
            var dic = new ConcurrentDictionary<Guid, string>();
            foreach (var type in typeof(Extensions).Assembly.GetTypes())
            {
                // ids
                var att = type.GetCustomAttribute<InterfaceTypeAttribute>();
                if (att != null)
                {
                    dic[type.GUID] = "IID_" + type.Name;
                }

                // constants
                foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public))
                {
                    if (field.FieldType == typeof(Guid))
                    {
                        dic[(Guid)field.GetValue(null)] = field.Name;
                    }
                }
            }
            return dic;
        }

        public static string ToStringUni(this IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return null;

            return Marshal.PtrToStringUni(ptr);
        }

        public static string ToStringUni(this IntPtr ptr, int len)
        {
            if (ptr == IntPtr.Zero)
                return null;

            return Marshal.PtrToStringUni(ptr, len);
        }

        public static DateTimeOffset ToDateTimeOffset(this FILETIME fileTime)
        {
            var ft = (((long)fileTime.dwHighDateTime) << 32) + fileTime.dwLowDateTime;
            return DateTimeOffset.FromFileTime(ft);
        }

        public static long CopyTo(this Stream input, Stream output, long count = long.MaxValue, int bufferSize = 0x14000)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (output == null)
                throw new ArgumentNullException(nameof(output));

            if (count <= 0)
                throw new ArgumentException(null, nameof(count));

            if (bufferSize <= 0)
                throw new ArgumentException(null, nameof(bufferSize));

            if (count < bufferSize)
            {
                bufferSize = (int)count;
            }

            var bytes = new byte[bufferSize];
            var total = 0;
            do
            {
                var max = (int)Math.Min(count - total, bytes.Length);
                var read = input.Read(bytes, 0, max);
                if (read == 0)
                    break;

                output.Write(bytes, 0, read);
                total += read;
                if (total == count)
                    break;
            }
            while (true);
            return total;
        }
    }
}
