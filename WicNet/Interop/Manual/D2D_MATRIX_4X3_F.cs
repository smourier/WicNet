using System;
using System.Runtime.InteropServices;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential)]
    public partial struct D2D_MATRIX_4X3_F
    {
        public float _11;
        public float _12;
        public float _13;
        public float _21;
        public float _22;
        public float _23;
        public float _31;
        public float _32;
        public float _33;
        public float _41;
        public float _42;
        public float _43;

        public D2D_MATRIX_4X3_F(
            float m11, float m12, float m13,
            float m21, float m22, float m23,
            float m31, float m32, float m33,
            float m41, float m42, float m43
            )
        {
            _11 = m11;
            _12 = m12;
            _13 = m13;
            _21 = m21;
            _22 = m22;
            _23 = m23;
            _31 = m31;
            _32 = m32;
            _33 = m33;
            _41 = m41;
            _42 = m42;
            _43 = m43;
        }

        public float[] ToArray() => new[] {
            _11, _12, _13,
            _21, _22, _23,
            _31, _32, _33,
            _41, _42, _43,
        };

        public static D2D_MATRIX_4X3_F Identity()
        {
            var m = new D2D_MATRIX_4X3_F();
            m._11 = 1;
            m._22 = 1;
            m._33 = 1;
            return m;
        }
    }
}
