using System;
using DirectN;

namespace WicNet
{
    public sealed class WicColorContext : IDisposable
    {
        private readonly IComObject<IWICColorContext> _comObject;
        private readonly Lazy<ColorProfile> _profile;

        public WicColorContext(IWICColorContext palette)
            : this((object)palette)
        {
        }

        public WicColorContext(IComObject<IWICColorContext> palette)
            : this((object)palette)
        {
        }

        public WicColorContext(uint colorSpace)
            : this((object)colorSpace)
        {
        }

        public WicColorContext(string fileName)
            : this((object)fileName)
        {
        }

        public WicColorContext(byte[] bytes)
            : this((object)bytes)
        {
        }

        public WicColorContext()
            : this((object)null)
        {
        }

        public WicColorContext(object source)
        {
            if (source == null)
            {
                _comObject = WICImagingFactory.CreateColorContext();
            }
            else if (source is IWICColorContext p)
            {
                _comObject = new ComObject<IWICColorContext>(p);
            }
            else if (source is uint colorSpace)
            {
                _comObject = WICImagingFactory.CreateColorContext();
                _comObject.Object.InitializeFromExifColorSpace(colorSpace);
            }
            else if (source is string fileName)
            {
                _comObject = WICImagingFactory.CreateColorContext();
                _comObject.Object.InitializeFromFilename(fileName);
            }
            else if (source is byte[] memory)
            {
                _comObject = WICImagingFactory.CreateColorContext();
                _comObject.Object.InitializeFromMemory(memory, memory.Length);
            }
            else
            {
                _comObject = source as IComObject<IWICColorContext>;
                if (_comObject == null)
                    throw new ArgumentException("Source must be an " + nameof(IWICColorContext) + ".", nameof(source));
            }
            _profile = new Lazy<ColorProfile>(() => ColorProfile.FromMemory(ProfileBytes), true);
        }

        public IComObject<IWICColorContext> ComObject => _comObject;

        public ColorProfile Profile => _profile.Value;

        public uint ExifColorSpace
        {
            get
            {
                _comObject.Object.GetExifColorSpace(out var value).ThrowOnError();
                return value;
            }
        }

        public WICColorContextType Type
        {
            get
            {
                _comObject.Object.GetType(out var value).ThrowOnError();
                return value;
            }
        }

        public byte[] ProfileBytes
        {
            get
            {
                _comObject.Object.GetProfileBytes(0, null, out var count).ThrowOnError();
                var bytes = new byte[count];
                _comObject.Object.GetProfileBytes(count, bytes, out _).ThrowOnError();
                return bytes;
            }
        }

        public void Dispose()
        {
            if (_comObject == null || _comObject.IsDisposed)
                return;

            _comObject.Dispose();
        }
    }
}
