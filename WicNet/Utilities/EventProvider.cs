using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace WicNet.Utilities
{
    public sealed class EventProvider : IDisposable
    {
        public static readonly EventProvider Default = new EventProvider(new Guid("964d4572-adb9-4f3a-8170-fcbecec27467"));

        private long _handle;
        public Guid Id { get; }

        public EventProvider(Guid id)
        {
            Id = id;
            var hr = EventRegister(id, IntPtr.Zero, IntPtr.Zero, out _handle);
            if (hr != 0)
                throw new Win32Exception(hr);
        }

        public bool WriteMessageEvent(string text, byte level = 0, long keywords = 0) => EventWriteString(_handle, level, keywords, text) == 0;

        public void Dispose()
        {
            var handle = Interlocked.Exchange(ref _handle, 0);
            if (handle != 0)
            {
                _ = EventUnregister(handle);
            }
        }

        [DllImport("advapi32")]
        private static extern int EventRegister([MarshalAs(UnmanagedType.LPStruct)] Guid ProviderId, IntPtr EnableCallback, IntPtr CallbackContext, out long RegHandle);

        [DllImport("advapi32")]
        private static extern int EventUnregister(long RegHandle);

        [DllImport("advapi32")]
        private static extern int EventWriteString(long RegHandle, byte Level, long Keyword, [MarshalAs(UnmanagedType.LPWStr)] string String);
    }
}
