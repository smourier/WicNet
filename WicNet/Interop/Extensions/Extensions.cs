using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace DirectN
{
    public static class Extensions
    {
        public static bool IsValid(this float value) => !float.IsNaN(value);
        public static bool IsInvalid(this float value) => float.IsNaN(value);
        public static bool IsSet(this float value) => IsValid(value) && !float.IsInfinity(value);
        public static bool IsNotSet(this float value) => IsInvalid(value) || float.IsInfinity(value);
        public static bool IsMinOrMax(this float value) => value == float.MaxValue || value == float.MinValue;
        public static bool IsMax(this float value) => value == float.MaxValue;
        public static bool IsMin(this float value) => value == float.MinValue;
        public static int SignedLOWORD(this IntPtr value) => SignedLOWORD((int)(long)value);
        public static int SignedLOWORD(this int value) => (short)(value & 0xffff);
        public static int SignedHIWORD(this IntPtr value) => SignedHIWORD((int)(long)value);
        public static int SignedHIWORD(this int value) => (short)((value >> 0x10) & 0xffff);
        public static int HIWORD(this int value) => (value >> 0x10) & 0xffff;
        public static int HIWORD(this IntPtr value) => HIWORD((int)(long)value);
        public static int LOWORD(this int value) => value & 0xffff;
        public static int LOWORD(this IntPtr value) => LOWORD((int)(long)value);
        public static float ToZero(this float value) => float.IsNaN(value) ? 0 : value;

        public static float NextFloat(this Random random) => (float)random?.NextDouble();
        public static byte NextByte(this Random random, byte minValue = 0, byte maxValue = 255) => (byte)random?.Next(minValue, maxValue);

        public static float Ceiling(this float value) => (float)Math.Ceiling(value);
        public static int CeilingI(this float value) => Math.Ceiling(value).ToInt32();
        public static uint CeilingU(this float value) => Math.Ceiling(value).ToUInt32();
        public static float Floor(this float value) => (float)Math.Floor(value);
        public static int FloorI(this float value) => Math.Floor(value).ToInt32();
        public static uint FloorU(this float value) => Math.Floor(value).ToUInt32();
        public static float Round(this float value) => (float)Math.Round(value);
        public static int RoundI(this float value) => Math.Round(value).ToInt32();
        public static uint RoundU(this float value) => Math.Round(value).ToUInt32();

        public static double Ceiling(this double value) => Math.Ceiling(value);
        public static int CeilingI(this double value) => Math.Ceiling(value).ToInt32();
        public static uint CeilingU(this double value) => Math.Ceiling(value).ToUInt32();
        public static double Floor(this double value) => Math.Floor(value);
        public static int FloorI(this double value) => Math.Floor(value).ToInt32();
        public static uint FloorU(this double value) => Math.Floor(value).ToUInt32();
        public static double Round(this double value) => Math.Round(value);
        public static int RoundI(this double value) => Math.Round(value).ToInt32();
        public static uint RoundU(this double value) => Math.Round(value).ToUInt32();

        public static float Clamp(this float value, float min, float max = float.MaxValue) => value < min ? min : value > max ? max : value;
        public static byte Clamp(this byte value, byte min, byte max = byte.MaxValue) => value < min ? min : value > max ? max : value;
        public static int Clamp(this int value, int min, int max = int.MaxValue) => value < min ? min : value > max ? max : value;
        public static uint Clamp(this uint value, uint min, uint max = uint.MaxValue) => value < min ? min : value > max ? max : value;
        public static long Clamp(this long value, long min, long max = long.MaxValue) => value < min ? min : value > max ? max : value;
        public static ulong Clamp(this ulong value, ulong min, ulong max = ulong.MaxValue) => value < min ? min : value > max ? max : value;

        public static float ClampMinMax(this float value)
        {
            if (float.IsPositiveInfinity(value))
                return float.MaxValue;

            if (float.IsNegativeInfinity(value))
                return float.MinValue;

            return value;
        }

        public static uint ToUInt32(this float value)
        {
            if (float.IsNaN(value))
                throw new OverflowException();

            if (value <= 0)
                return 0;

            if (value >= uint.MaxValue)
                return uint.MaxValue;

            return (uint)value;
        }

        public static int ToInt32(this float value)
        {
            if (float.IsNaN(value))
                throw new OverflowException();

            if (value <= int.MinValue)
                return int.MinValue;

            if (value >= int.MaxValue)
                return int.MaxValue;

            return (int)value;
        }

        public static uint ToUInt32(this double value)
        {
            if (double.IsNaN(value))
                throw new OverflowException();

            if (value <= 0)
                return 0;

            if (value >= uint.MaxValue)
                return uint.MaxValue;

            return (uint)value;
        }

        public static int ToInt32(this double value)
        {
            if (double.IsNaN(value))
                throw new OverflowException();

            if (value <= int.MinValue)
                return int.MinValue;

            if (value >= int.MaxValue)
                return int.MaxValue;

            return (int)value;
        }

        public static bool IsEmpty(this Array array) => array == null || array.Length == 0;
        public static bool IsEmpty(this IEnumerable enumerable)
        {
            if (enumerable == null)
                return true;

            var enumerator = enumerable.GetEnumerator();
            if (enumerator == null)
                return true;

            var next = enumerator.MoveNext();
            if (enumerator is IDisposable disp)
            {
                disp.Dispose();
            }
            return !next;
        }

        private static readonly Lazy<ConcurrentDictionary<Guid, string>> _guids = new Lazy<ConcurrentDictionary<Guid, string>>(ExtractAllGuids, true);

        public static ComMemory StructureToMemory(this object structure) => new ComMemory(structure);

        public static IntPtr StructureToPtr(this object structure)
        {
            var ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(structure));
            Marshal.StructureToPtr(structure, ptr, false);
            return ptr;
        }

        public static IntPtr StructureToPtr<T>(this T structure) where T : struct
        {
            var ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf<T>());
            Marshal.StructureToPtr<T>(structure, ptr, false);
            return ptr;
        }

        public static byte[] StructureToBytes(this object structure)
        {
            var bytes = new byte[Marshal.SizeOf(structure)];
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            Marshal.StructureToPtr(structure, handle.AddrOfPinnedObject(), false);
            handle.Free();
            return bytes;
        }

        public static byte[] StructureToBytes<T>(this T structure) where T : struct
        {
            var bytes = new byte[Marshal.SizeOf<T>()];
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(structure, handle.AddrOfPinnedObject(), false);
                return bytes;
            }
            finally
            {
                handle.Free();
            }
        }

        public static object BytesToStructure(this byte[] bytes, Type structureType)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            if (structureType == null)
                throw new ArgumentNullException(nameof(structureType));

            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return Marshal.PtrToStructure(handle.AddrOfPinnedObject(), structureType);
            }
            finally
            {
                handle.Free();
            }
        }

        public static T BytesToStructure<T>(this byte[] bytes) where T : struct
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }

        public static IntPtr BytesToIntPtr(this byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            if (bytes.Length != IntPtr.Size)
                throw new ArgumentException(null, nameof(bytes));

            if (IntPtr.Size == 4)
            {
                var i = BitConverter.ToInt32(bytes, 0);
                return new IntPtr(i);
            }

            var l = BitConverter.ToInt64(bytes, 0);
            return new IntPtr(l);
        }

        public static byte[] IntPtrToBytes(this IntPtr ptr)
        {
            if (IntPtr.Size == 4)
                return BitConverter.GetBytes(ptr.ToInt32());

            return BitConverter.GetBytes(ptr.ToInt64());
        }

        public static T[] ToArrayNullify<T>(this IEnumerable<IComObject<T>> enumerable)
        {
            if (enumerable == null)
                return null;

            return enumerable?.Where(e => e != null && e.Object != null)?.Select(e => e.Object)?.ToArray();
        }

        public static T[] ToArrayNullify<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                return null;

            return enumerable?.Where(e => e != null)?.ToArray();
        }

        public static T[] ToArrayOrEmpty<T>(this ICollection<T> collection, bool allowsNull = false)
        {
            if (collection.IsEmpty())
            {
                if (allowsNull)
                    return null;

                return Array.Empty<T>();
            }

            return collection.ToArray();
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

        public static string GetAllMessages(this Exception exception) => GetAllMessages(exception, Environment.NewLine);
        public static string GetAllMessages(this Exception exception, string separator)
        {
            if (exception == null)
                return null;

            var sb = new StringBuilder();
            AppendMessages(sb, exception, separator);
            return sb.ToString().Replace("..", ".");
        }

        private static void AppendMessages(StringBuilder sb, Exception e, string separator)
        {
            if (e == null)
                return;

            // this one is not interesting...
            if (!(e is TargetInvocationException))
            {
                if (sb.Length > 0)
                {
                    sb.Append(separator);
                }
                sb.Append(e.Message);
            }
            AppendMessages(sb, e.InnerException, separator);
        }
    }
}
