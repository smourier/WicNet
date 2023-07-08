using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace DirectN
{
    public abstract class ComObject : IComObject
    {
        private object _object;
        private readonly bool _dispose;

        public ComObject(object comObject, bool dispose = true)
        {
            if (comObject == null)
                throw new ArgumentNullException(nameof(comObject));

            if (!Marshal.IsComObject(comObject))
                throw new ArgumentException("Argument is not a COM object", nameof(comObject));

            _object = comObject;
            _dispose = dispose;

#if DEBUG
            Id = Interlocked.Increment(ref _id);
            LiveObjects[Id] = this;
            ConstructorThreadId = Environment.CurrentManagedThreadId;
            Trace(null, "+");
#endif
        }

        public abstract Type InterfaceType { get; }
        public bool IsDisposed => _object == null;
        public object Object
        {
            get
            {
                var obj = _object;
                if (obj == null)
                {
#if DEBUG
                    Trace("!!! Already disposed");
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

        public IntPtr GetInterfacePointer<T>(bool throwOnError = false)
        {
            try
            {
                return Marshal.GetComInterfaceForObject(Object, typeof(T));
            }
            catch
            {
                if (throwOnError)
                    throw;
            }
            return IntPtr.Zero;
        }

        public T As<T>(bool throwOnError = false) where T : class
        {
            if (throwOnError)
                return (T)Object; // will throw

            return Object as T;
        }

        public IComObject<T> AsComObject<T>(bool throwOnError = false) where T : class
        {
            if (throwOnError)
                return new ComObject<T>((T)Object, false); // will throw

            if (!(Object is T obj))
                return null;

            return new ComObject<T>(obj, false);
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
            if (!_dispose)
                return;

            //#if DEBUG
            //            Trace("~disposing: " + disposing + " duration: " + Duration.Milliseconds);
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
                    Trace("disposing: " + disposing + " count: " + count, "~");
                    LiveObjects.TryRemove(Id, out _);
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
        public static void TraceLiveObjects()
        {
            var array = LiveObjects.ToArray();
            foreach (var kv in array)
            {
                kv.Value.Trace(null, "? ");
            }
        }

        public static readonly System.Collections.Concurrent.ConcurrentDictionary<long, ComObject> LiveObjects = new System.Collections.Concurrent.ConcurrentDictionary<long, ComObject>();
        protected virtual string ObjectTypeName => null;
        private static long _id;
        private static readonly Stopwatch _sw = new Stopwatch();

        static ComObject()
        {
            _sw.Start();
        }

#pragma warning disable IDE0060 // Remove unused parameter
        protected void Trace(string message, [CallerMemberName] string methodName = null)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            return;
            //// many COM objects (like DXGI ones) dont' like to be used on different threads
            //// so we tracks calls on different threads
            //var s = Id.ToString();

            //var tid = Thread.CurrentThread.ManagedThreadId;
            //if (tid != ConstructorThreadId)
            //{
            //    s += "!" + ConstructorThreadId + "!";
            //}

            //var tn = ObjectTypeName;
            //if (tn != null)
            //{
            //    s += "<" + tn + ">";
            //}

            //if (message != null)
            //{
            //    s += " " + message;
            //}

            //if (methodName != null)
            //{
            //    s = methodName + s;
            //}

            //EventProvider.Default.WriteMessageEvent(s);
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
        public ComObject(T comObject, bool dispose = true)
            : base(comObject, dispose)
        {
        }

        public new T Object => (T)base.Object;
        public override Type InterfaceType => typeof(T);

#if DEBUG
        protected override string ObjectTypeName => typeof(T).Name;
#endif
    }

    public interface IComObject : IDisposable
    {
        bool IsDisposed { get; }
        object Object { get; }
        Type InterfaceType { get; }
        I As<I>(bool throwOnError = false) where I : class;
        IntPtr GetInterfacePointer<T>(bool throwOnError = false);
        IComObject<I> AsComObject<I>(bool throwOnError = false) where I : class;
    }

    public interface IComObject<out T> : IComObject
    {
        new T Object { get; }
    }

    public static class ComObjectExtensions
    {
        public static void SafeDispose(this IComObject comObject)
        {
            if (comObject == null || comObject.IsDisposed)
                return;

            comObject.Dispose();
        }
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
