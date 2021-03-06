﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace WicNet.Interop
{
    public class ComObject : IComObject
    {
        private object _object;

        public ComObject(object comObject)
        {
            if (comObject == null)
                throw new ArgumentNullException(nameof(comObject));

            if (!Marshal.IsComObject(comObject))
                throw new ArgumentException("Argument is not a COM object", nameof(comObject));

            _object = comObject;

#if DEBUG
            Id = Interlocked.Increment(ref _id);
            ConstructorThreadId = Environment.CurrentManagedThreadId;
            Trace("+");
#endif
        }

        public bool IsDisposed => _object == null;
        public object Object
        {
            get
            {
                var obj = _object;
                if (obj == null)
                {
#if DEBUG
                    Trace("!!!", "Already disposed");
#endif
                    throw new ObjectDisposedException(nameof(Object));
                }

                return obj;
            }
        }

        public static ComObject<T> From<T>(T comObject) => comObject == null ? null : new ComObject<T>(comObject);

        public static object Unwrap(object obj)
        {
            if (obj is ComObject co)
                return co.Object;

            if (!Marshal.IsComObject(obj))
                throw new ArgumentException("Argument is not a COM object", nameof(obj));

            return obj;
        }

        public static T Unwrap<T>(object obj)
        {
            if (obj is ComObject co)
                return (T)co.Object;

            if (!Marshal.IsComObject(obj))
                throw new ArgumentException("Argument is not a COM object", nameof(obj));

            return (T)obj;
        }

        public T As<T>(bool throwOnError = false) where T : class
        {
            if (throwOnError)
                return (T)Object; // will throw

            return Object as T;
        }

        public static ComObject WrapAsGeneric(Type comType, object instance)
        {
            if (comType == null)
                throw new ArgumentNullException(nameof(comType));

            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (!comType.IsAssignableFrom(instance.GetType()))
                throw new ArgumentNullException(nameof(instance));

            var type = typeof(ComObject<>).MakeGenericType(comType);
            var ctor = type.GetConstructor(new[] { comType });
            return (ComObject)ctor.Invoke(new[] { instance });
        }

        public static bool IsGenericComObjectType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ComObject<>);
        }

        public static Type GetGenericComObjectComType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(ComObject<>))
                return null;

            return type.GetGenericArguments()[0];
        }

        protected virtual void Dispose(bool disposing)
        {
            //#if DEBUG
            //            Trace("~", "disposing: " + disposing + " duration: " + Duration.Milliseconds);
            //#endif
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                var obj = Interlocked.Exchange(ref _object, null);
                if (obj != null)
                {
#if DEBUG
                    //var typeName = GetType().FullName;
                    //if (typeName.IndexOf("textlayout", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    //    typeName.IndexOf("textformat", StringComparison.OrdinalIgnoreCase) >= 0)
                    //    return;

                    var count = Marshal.ReleaseComObject(obj);
                    Trace("~", "disposing: " + disposing + " count: " + count);
#else
                    Marshal.ReleaseComObject(obj);
#endif

                }
            }
        }

        ~ComObject() { Dispose(false); }
        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }

        public static int Release(object comObject)
        {
            if (comObject == null)
                return 0;

            if (!Marshal.IsComObject(comObject))
                throw new ArgumentException("Argument is not a COM object", nameof(comObject));

            return Marshal.ReleaseComObject(comObject);
        }

#if DEBUG
        protected virtual string ObjectTypeName => null;
        private static long _id;
        private static readonly Stopwatch _sw = new Stopwatch();

        static ComObject()
        {
            _sw.Start();
        }

        protected void Trace(string methodName, string message = null)
        {
            // many COM objects (like DXGI ones) dont' like to be used on different threads
            // so we tracks calls on different threads
            var s = Id.ToString();

            var tid = Thread.CurrentThread.ManagedThreadId;
            if (tid != ConstructorThreadId)
            {
                s += "!" + ConstructorThreadId + "!";
            }

            var tn = ObjectTypeName;
            if (tn != null)
            {
                s += "<" + tn + ">";
            }

            if (message != null)
            {
                s += " " + message;
            }
            //System.Diagnostics.Trace.WriteLine(s, methodName);
        }

        public long Id { get; }
        public int ConstructorThreadId { get; }
        public TimeSpan Duration => _sw.Elapsed;

        public override string ToString()
        {
            string s = null;
            if (IsDisposed)
            {
                s = "<disposed>";
            }

            var ot = ObjectTypeName;
            if (ot != null)
            {
                if (s != null)
                {
                    s += " ";
                }
                s += ot;
            }

            if (s != null)
                return Id + " " + s;

            return Id.ToString();
        }
#endif
    }

    public class ComObject<T> : ComObject, IComObject<T>
    {
        public ComObject(T comObject)
            : base(comObject)
        {
        }

        public new T Object => (T)base.Object;

#if DEBUG
        protected override string ObjectTypeName => typeof(T).Name;
#endif

        //public static implicit operator ComObject<T>(T value) => new ComObject<T>(value);
        //public static implicit operator T(ComObject<T> value) => value.Object;
    }

    public interface IComObject : IDisposable
    {
        bool IsDisposed { get; }
        object Object { get; }
        I As<I>(bool throwOnError = false) where I : class;
    }

    public interface IComObject<out T> : IComObject
    {
        new T Object { get; }
    }

    public sealed class ComObjectWrapper<T> : IDisposable
    {
        private readonly IComObject<T> _cot;

        public ComObjectWrapper(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            _cot = obj as IComObject<T>;
            if (_cot == null)
            {
                if (obj is T t)
                {
                    _cot = new ComObject<T>(t);
                }
                else
                {
                    if (obj is IComObject co)
                    {
                        if (co.IsDisposed)
                            throw new ArgumentException("Input of type '" + obj.GetType() + "' is disposed.", nameof(obj));

                        if (co.Object is T t2)
                        {
                            _cot = new ComObject<T>(t2);
                        }
                    }

                    if (_cot == null)
                        throw new ArgumentException("Input of type '" + obj.GetType() + "' must be assignable to type '" + typeof(T) + "'.", nameof(obj));
                }
            }

            if (_cot.IsDisposed)
                throw new ArgumentException("Input of type '" + obj.GetType() + "' is disposed.", nameof(obj));
        }

        public T Object => _cot.Object;
        public IComObject<T> ComObject => _cot;

        public void Dispose()
        {
            if (_cot.IsDisposed)
                return;

            _cot.Dispose();
        }
    }
}
