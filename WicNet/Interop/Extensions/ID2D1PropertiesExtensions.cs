using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using WicNet;

namespace DirectN
{
    public static class ID2D1PropertiesExtensions
    {
        public const int D2D1_INVALID_PROPERTY_INDEX = -1;

        public static int GetPropertyCount(this IComObject<ID2D1Properties> properties) => GetPropertyCount(properties?.Object);
        public static int GetPropertyCount(this ID2D1Properties properties)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            return properties.GetPropertyCount();
        }

        public static int GetPropertyIndex(this IComObject<ID2D1Properties> properties, string name) => GetPropertyIndex(properties?.Object, name);
        public static int GetPropertyIndex(this ID2D1Properties properties, string name)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return properties.GetPropertyIndex(name);
        }

        public static D2D1_PROPERTY_TYPE GetType(this IComObject<ID2D1Properties> properties, int index) => GetType(properties?.Object, index);
        public static D2D1_PROPERTY_TYPE GetType(this ID2D1Properties properties, int index)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            return properties.GetType(index);
        }

        public static int GetValueSize(this IComObject<ID2D1Properties> properties, int index) => GetValueSize(properties?.Object, index);
        public static int GetValueSize(this ID2D1Properties properties, int index)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            return properties.GetValueSize(index);
        }

        public static bool TryGetValue(this IComObject<ID2D1Properties> properties, string name, out D2D1_PROPERTY_TYPE type, out byte[] data) => TryGetValue(properties?.Object, name, out type, out data);
        public static bool TryGetValue(this ID2D1Properties properties, string name, out D2D1_PROPERTY_TYPE type, out byte[] data)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var index = properties.GetPropertyIndex(name);
            if (index < 0)
            {
                data = null;
                type = D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_UNKNOWN;
                return false;
            }

            return TryGetValue(properties, index, out type, out data);
        }

        public static bool TryGetValue(this IComObject<ID2D1Properties> properties, int index, out D2D1_PROPERTY_TYPE type, out byte[] data) => TryGetValue(properties?.Object, index, out type, out data);
        public static bool TryGetValue(this ID2D1Properties properties, int index, out D2D1_PROPERTY_TYPE type, out byte[] data)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            data = null;
            type = properties.GetType(index);
            if (type == D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_UNKNOWN)
                return false;

            var size = properties.GetValueSize(index);
            if (size < 0)
                return false;

            var bytes = new byte[size];
            var hr = properties.GetValue(index, type, bytes, bytes.Length);
            if (hr.IsError)
                return false;

            data = bytes;
            return true;
        }

        public static T GetValue<T>(this IComObject<ID2D1Properties> properties, int index, T defaultValue = default)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            if (!TryGetValue(properties, index, out var value))
                return default;

            return Conversions.ChangeType(value, defaultValue);
        }

        public static T GetValue<T>(this IComObject<ID2D1Properties> properties, string name, T defaultValue = default)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (!TryGetValue(properties, name, out var value))
                return default;

            return Conversions.ChangeType(value, defaultValue);
        }

        public static bool TryGetValue(this IComObject<ID2D1Properties> properties, string name, out object value) => TryGetValue(properties?.Object, name, out value);
        public static bool TryGetValue(this ID2D1Properties properties, string name, out object value)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            value = null;
            var index = properties.GetPropertyIndex(name);
            if (index < 0)
                return false;

            return TryGetValue(properties, index, out value);
        }

        public static bool TryGetValue(this IComObject<ID2D1Properties> properties, int index, out object value) => TryGetValue(properties?.Object, index, out value);
        public static bool TryGetValue(this ID2D1Properties properties, int index, out object value)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            value = null;
            var size = properties.GetValueSize(index);
            if (size < 0)
                return false;

            var type = properties.GetType(index);

            var bytes = new byte[size];
            var hr = properties.GetValue(index, type, bytes, bytes.Length);
            if (hr.IsError)
                return false;

            return TryGetValue(type, bytes, out value);
        }

        public static void SetValue(this IComObject<ID2D1Properties> properties, int index, object value) => SetValue(properties?.Object, index, value);
        public static void SetValue(this ID2D1Properties properties, int index, object value)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            if (!TryGetData(value, out var type, out var data))
                throw new NotSupportedException();

            properties.SetValue(index, type, data, data.Length).ThrowOnError();
        }

        public static void SetValue(this IComObject<ID2D1Properties> properties, string name, object value) => SetValue(properties?.Object, name, value);
        public static void SetValue(this ID2D1Properties properties, string name, object value)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (!TryGetData(value, out var type, out var data))
                throw new NotSupportedException();

            properties.SetValueByName(name, type, data, data.Length).ThrowOnError();
        }

        public static void SetValue(this IComObject<ID2D1Properties> properties, int index, D2D1_PROPERTY_TYPE type, byte[] data) => SetValue(properties?.Object, index, type, data);
        public static void SetValue(this ID2D1Properties properties, int index, D2D1_PROPERTY_TYPE type, byte[] data)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            properties.SetValue(index, type, data, data.Length).ThrowOnError();
        }

        public static void SetValue(this IComObject<ID2D1Properties> properties, string name, D2D1_PROPERTY_TYPE type, byte[] data) => SetValue(properties?.Object, name, type, data);
        public static void SetValue(this ID2D1Properties properties, string name, D2D1_PROPERTY_TYPE type, byte[] data)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            properties.SetValueByName(name, type, data, data.Length).ThrowOnError();
        }

        public static string GetPropertyName(this IComObject<ID2D1Properties> properties, int index) => GetPropertyName(properties?.Object, index);
        public static string GetPropertyName(this ID2D1Properties properties, int index)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            var length = properties.GetPropertyNameLength(index);
            if (length == 0)
                return null;

            var name = new string('\0', length);
            properties.GetPropertyName(index, name, length + 1).ThrowOnError();
            return name;
        }

        public static IComObject<ID2D1Properties> GetSubProperties(this IComObject<ID2D1Properties> properties, int index) => GetSubProperties(properties?.Object, index);
        public static IComObject<ID2D1Properties> GetSubProperties(this ID2D1Properties properties, int index)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            properties.GetSubProperties(index, out var sub).ThrowOnError();
            return new ComObject<ID2D1Properties>(sub);
        }

        public static IEnumerable<D2D1EffectProperty> GetProperties(this IComObject<ID2D1Properties> properties) => GetProperties(properties?.Object);
        public static IEnumerable<D2D1EffectProperty> GetProperties(this ID2D1Properties properties)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            var count = properties.GetPropertyCount();
            for (var i = 0; i < count; i++)
            {
                var prop = new D2D1EffectProperty
                {
                    Index = i,
                    Type = properties.GetType(i),
                    Name = properties.GetPropertyName(i)
                };

                var size = properties.GetValueSize(i);
                if (size >= 0)
                {
                    prop.Data = new byte[size];
                    if (properties.GetValue(i, prop.Type, prop.Data, size).IsSuccess && TryGetValue(prop.Type, prop.Data, out var value))
                    {
                        prop.Value = value;
                    }
                }
                yield return prop;
            }
        }

        public static bool TryGetValue(D2D1_PROPERTY_TYPE type, byte[] data, out object value)
        {
            value = null;
            if (data == null || data.Length == 0)
                return false;

            switch (type)
            {
                case D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_BOOL:
                    value = BitConverter.ToInt32(data, 0) != 0;
                    return true;

                case D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_STRING:
                    var s = Encoding.Unicode.GetString(data);
                    if (!string.IsNullOrEmpty(s))
                    {
                        while (s[s.Length - 1] == '0')
                        {
                            s = s.Substring(0, s.Length - 1);
                        }
                    }
                    value = s;
                    return true;

                case D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_UINT32:
                    value = BitConverter.ToUInt32(data, 0);
                    return true;

                case D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_INT32:
                    value = BitConverter.ToInt32(data, 0);
                    return true;

                case D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_FLOAT:
                    value = BitConverter.ToSingle(data, 0);
                    return true;

                case D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_VECTOR2:
                    value = data.BytesToStructure<D2D_VECTOR_2F>();
                    return true;

                case D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_VECTOR3:
                    value = data.BytesToStructure<D2D_VECTOR_3F>();
                    return true;

                case D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_VECTOR4:
                    value = data.BytesToStructure<D2D_VECTOR_4F>();
                    return true;

                case D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_BLOB:
                    value = data;
                    return true;

                case D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_ENUM:
                    value = BitConverter.ToUInt32(data, 0);
                    return true;

                case D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_CLSID:
                    value = new Guid(data);
                    return true;

                case D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_MATRIX_3X2:
                    value = data.BytesToStructure<D2D_MATRIX_3X2_F>();
                    return true;

                case D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_MATRIX_4X3:
                    value = data.BytesToStructure<D2D_MATRIX_4X3_F>();
                    return true;

                case D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_MATRIX_4X4:
                    value = data.BytesToStructure<D2D_MATRIX_4X4_F>();
                    return true;

                case D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_MATRIX_5X4:
                    value = data.BytesToStructure<D2D_MATRIX_5X4_F>();
                    return true;

                case D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_IUNKNOWN:
                    var ptr = data.BytesToIntPtr();
                    if (ptr != IntPtr.Zero)
                    {
                        value = Marshal.GetObjectForIUnknown(ptr);
                    }
                    return true;
            }
            return false;
        }

        public static bool TryGetData(object value, out D2D1_PROPERTY_TYPE type, out byte[] data)
        {
            type = D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_UNKNOWN;
            data = null;
            if (value == null)
                return false;

            var vt = value.GetType();
            if (vt.IsEnum && TryGetData(Convert.ToUInt32(value), out type, out data))
            {
                type = D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_ENUM;
                return true;
            }

            var tc = Type.GetTypeCode(vt);
            switch (tc)
            {
                case TypeCode.Boolean:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Byte:
                case TypeCode.SByte:
                    type = D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_INT32;
                    data = BitConverter.GetBytes(Convert.ToInt32(value));
                    return true;

                case TypeCode.Int32:
                    type = D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_INT32;
                    data = BitConverter.GetBytes((int)value);
                    return true;

                case TypeCode.UInt32:
                    type = D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_UINT32;
                    data = BitConverter.GetBytes((uint)value);
                    return true;

                case TypeCode.Single:
                    type = D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_FLOAT;
                    data = BitConverter.GetBytes((float)value);
                    return true;

                case TypeCode.Double:
                case TypeCode.Decimal:
                    type = D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_FLOAT;
                    data = BitConverter.GetBytes(Convert.ToSingle(value));
                    return true;

                case TypeCode.Char:
                    type = D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_STRING;
                    data = Encoding.Unicode.GetBytes(((char)value).ToString() + '\0');
                    break;

                case TypeCode.String:
                    type = D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_STRING;
                    data = Encoding.Unicode.GetBytes(((string)value) + '\0');
                    break;

                case TypeCode.Object:
                    if (value is Guid guid)
                    {
                        type = D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_CLSID;
                        data = guid.ToByteArray();
                        return true;
                    }

                    if (value is byte[] bytes)
                    {
                        type = D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_BLOB;
                        data = bytes;
                        return true;
                    }

                    if (value is D2D_VECTOR_2F v2)
                    {
                        type = D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_VECTOR2;
                        data = v2.StructureToBytes();
                        return true;
                    }

                    if (value is D2D_VECTOR_3F v3)
                    {
                        type = D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_VECTOR3;
                        data = v3.StructureToBytes();
                        return true;
                    }

                    if (value is D2D_VECTOR_4F v4)
                    {
                        type = D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_VECTOR4;
                        data = v4.StructureToBytes();
                        return true;
                    }

                    if (value is D2D_MATRIX_3X2_F m32)
                    {
                        type = D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_MATRIX_3X2;
                        data = m32.StructureToBytes();
                        return true;
                    }

                    if (value is D2D_MATRIX_4X3_F m43)
                    {
                        type = D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_MATRIX_4X3;
                        data = m43.StructureToBytes();
                        return true;
                    }

                    if (value is D2D_MATRIX_4X4_F m44)
                    {
                        type = D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_MATRIX_4X4;
                        data = m44.StructureToBytes();
                        return true;
                    }

                    if (value is D2D_MATRIX_5X4_F m54)
                    {
                        type = D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_MATRIX_5X4;
                        data = m54.StructureToBytes();
                        return true;
                    }

                    try
                    {
                        var unk = Marshal.GetIUnknownForObject(value);
                        type = D2D1_PROPERTY_TYPE.D2D1_PROPERTY_TYPE_IUNKNOWN;
                        data = unk.IntPtrToBytes();
                        return true;
                    }
                    catch
                    {
                        // continue
                    }
                    break;
            }
            return false;
        }
    }
}
