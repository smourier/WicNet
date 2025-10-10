namespace WicNet.Utilities;

[SupportedOSPlatform("windows6.0.6000")]
public static class IconUtilities
{
    private readonly static ConcurrentDictionary<string, (string?, int?)> _iconFilePathByExtension = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> _knownDlls = [
        "imageres",
        "netcenter",
        "shell32",
        "ddores",
        "mmores",
        "gameux",
        "netshell",
        "networkexplorer",
        "sensorscpl",
        "setupapi",
        "wpdshext",
        "wmploc",
    ];

    public const string FileExtension = ".ico";
    public const int MaxIconSize = 256;

    private const string _openWithProgIdsToken = "OpenWithProgIds";
    private const string _shellToken = "shell";
    private const string _openToken = "open";
    private const string _readToken = "read";
    private const string _browseToken = "browse";
    private const string _commandToken = "command";
    private const string _defaultIconToken = "DefaultIcon";
    private const string _imageResDll = @"%SystemRoot%\System32\imageres.dll";
    private const string _dynamicIcon = "%1";

    private delegate bool EnumResNameProc(nint hModule, nint lpszType, nint lpszName, nint lParam);

#pragma warning disable IDE1006 // Naming Styles
    private const int PNG_SIG = 0x474e5000;
    private const int LOAD_LIBRARY_AS_DATAFILE = 0x2;
    private const int LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x20;

    private const int ERROR_RESOURCE_TYPE_NOT_FOUND = 1813;
    private const int ERROR_RESOURCE_DATA_NOT_FOUND = 1812;
    private const int ERROR_FILE_NOT_FOUND = 2;
    private const int ERROR_PATH_NOT_FOUND = 3;
    private const int ERROR_BAD_EXE_FORMAT = 193;

    private const int BI_RGB = 0;

    private const int RT_ICON = 3;
    private const int RT_GROUP_ICON = RT_ICON + 11;
#pragma warning restore IDE1006 // Naming Styles

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct GRPICONDIRENTRY
    {
        public byte bWidth;
        public byte bHeight;
        public byte bColorCount;
        public byte bReserved;
        public ushort wPlanes;
        public ushort wBitCount;
        public uint dwBytesInRes;
        public ushort nId;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct ICONDIRENTRY
    {
        public byte bWidth;
        public byte bHeight;
        public byte bColorCount;
        public byte bReserved;
        public ushort wPlanes;
        public ushort wBitCount;
        public uint dwBytesInRes;
        public uint dwImageOffset;
    }

#pragma warning disable CS0649
    private struct BITMAPINFOHEADER
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage; // equals to GetStride(width/bitCount)*height+(height*GetStride(width/8)) (XOR size + AND size)
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }
#pragma warning restore CS0649

    private static void ResizeForIcon(WicBitmapSource source, uint size)
    {
        if (size > MaxIconSize)
        {
            size = MaxIconSize;
        }
        source.Scale((int)size, mode: WICBitmapInterpolationMode.WICBitmapInterpolationModeHighQualityCubic);
    }

    private static void WriteIcons(Stream iconFileStream, IEnumerable<WicBitmapSource> bitmaps)
    {
        var positions = new List<long>();
        var lens = new List<int>();
        var offsets = new List<int>();

        using var writer = new BinaryWriter(iconFileStream);
        writer.Write((short)0); // reserved
        writer.Write((short)1); // image type (1 for .ico)
        writer.Write((short)bitmaps.Count());
        foreach (var bitmap in bitmaps)
        {
            writer.Write(bitmap.Width == MaxIconSize ? (byte)0 : (byte)bitmap.Width);
            writer.Write(bitmap.Height == MaxIconSize ? (byte)0 : (byte)bitmap.Height);
            writer.Write((byte)0); // number of colors in the color palette
            writer.Write((byte)0); // reserved
            writer.Write((short)1); // color planes
            writer.Write((short)32); // bits per pixel (ARGB=32)
            positions.Add(iconFileStream.Position);
            writer.Write(0); // size of image data, we'll rewrite that later
            writer.Write(0); // offset of data from beginning;
        }

        foreach (var bitmap in bitmaps)
        {
            var bmpPos = iconFileStream.Position;
            offsets.Add((int)bmpPos);

            bitmap.Save(iconFileStream, WicCodec.GUID_ContainerFormatPng);

            var len = (int)(iconFileStream.Position - bmpPos);
            lens.Add(len);
        }

        for (var i = 0; i < positions.Count; i++)
        {
            iconFileStream.Seek(positions[i], SeekOrigin.Begin);
            writer.Write(lens[i]);
            writer.Write(offsets[i]);
        }
    }

    private static void EnumIconResources(string filePath, HashSet<ushort> groups, HashSet<int> icons)
    {
        var h = Functions.LoadLibraryExW(PWSTR.From(filePath), 0, LOAD_LIBRARY_FLAGS.LOAD_LIBRARY_AS_DATAFILE | LOAD_LIBRARY_FLAGS.LOAD_LIBRARY_AS_IMAGE_RESOURCE);
        if (h == 0)
        {
            var gle = Marshal.GetLastWin32Error();
            throw new Win32Exception(gle);
        }

        try
        {
            if (!Functions.EnumResourceNamesExW(h, new PWSTR(RT_GROUP_ICON), (m, t, n, lp) =>
            {
                groups.Add((ushort)(long)n.Value);
                return true;
            }, 0, 0, 0))
            {
                var gle = Marshal.GetLastWin32Error();
                if (gle != ERROR_RESOURCE_TYPE_NOT_FOUND && gle != ERROR_RESOURCE_DATA_NOT_FOUND)
                    throw new Win32Exception(gle);
            }

            if (!Functions.EnumResourceNamesExW(h, new PWSTR(RT_ICON), (m, t, n, lp) =>
            {
                icons.Add((ushort)(long)n.Value);
                return true;
            }, 0, 0, 0))
            {
                var gle = Marshal.GetLastWin32Error();
                if (gle != ERROR_RESOURCE_TYPE_NOT_FOUND && gle != ERROR_RESOURCE_DATA_NOT_FOUND)
                    throw new Win32Exception(gle);
            }
        }
        finally
        {
            Functions.FreeLibrary(h);
        }
    }

    private static bool SaveBaseDll(string filePath, bool overwrite, Architecture architecture)
    {
        if (!overwrite && File.Exists(filePath))
            return false;

        if (architecture != Architecture.X64 && architecture != Architecture.X86 && architecture != Architecture.Arm64)
            throw new NotSupportedException();

        var asm = Assembly.GetExecutingAssembly();
        // note the stream is case sensitive
        var name = $"WicNet.Resources.base.{architecture.ToString().ToLowerInvariant()}.dll";
        using var basedll = asm.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"'{name}' stream is missing from assembly.");

        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        basedll.CopyTo(stream);
        return true;
    }

    public static void SaveAsResourceDll(string icoFilePath, string filePath, int resourceId, int language, bool deleteExisting, Architecture? architecture = null)
    {
        ArgumentNullException.ThrowIfNull(icoFilePath);
        ArgumentNullException.ThrowIfNull(filePath);

        if (!IsBinaryPath(filePath))
            throw new ArgumentException("File path must be a binary (.dll) path.", nameof(filePath));

        if (language > ushort.MaxValue || language < 0)
            throw new ArgumentException(null, nameof(language));

        // note we use this method here because it can detect binary binary as .ico (pdf...)
        var bitmaps = LoadIcons(icoFilePath);
        try
        {
            architecture ??= RuntimeInformation.ProcessArchitecture;
            WriteBitmaps(bitmaps, filePath, resourceId, language, deleteExisting, architecture.Value);
        }
        finally
        {
            foreach (var bmp in bitmaps)
            {
                try
                {
                    bmp.Dispose();
                }
                catch
                {
                    // do nothing
                }
            }
        }
    }

    public static void SaveAsResourceDll(Assembly assembly, string resourcePath, string filePath, int resourceId, int language, bool deleteExisting)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(resourcePath);

        using var stream = assembly.GetManifestResourceStream(resourcePath) ?? throw new ArgumentException("Stream name '" + resourcePath + "' was not found in assembly.", nameof(resourcePath));
        SaveAsResourceDll(stream, filePath, resourceId, language, deleteExisting);
    }

    public static void SaveAsResourceDll(Stream iconStream, string filePath, int resourceId, int language, bool deleteExisting, Architecture? architecture = null)
    {
        ArgumentNullException.ThrowIfNull(iconStream);
        ArgumentNullException.ThrowIfNull(filePath);

        if (!IsBinaryPath(filePath))
            throw new ArgumentException("File path must be a binary (.dll) path.", nameof(filePath));

        if (language > ushort.MaxValue || language < 0)
            throw new ArgumentException(null, nameof(language));

        var bitmaps = LoadIconsFromIco(iconStream);
        try
        {
            architecture ??= RuntimeInformation.ProcessArchitecture;
            WriteBitmaps(bitmaps, filePath, resourceId, language, deleteExisting, architecture.Value);
        }
        finally
        {
            foreach (var bmp in bitmaps)
            {
                try
                {
                    bmp.Dispose();
                }
                catch
                {
                    // do nothing
                }
            }
        }
    }

    // The format of icon resources
    // https://blogs.msdn.microsoft.com/oldnewthing/20120720-00/?p=7083
    public static void SaveAsResourceDll(this WicBitmapSource bitmap, string filePath, IEnumerable<uint> sizes, int resourceId, int language, bool deleteExisting, Architecture? architecture = null)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentNullException.ThrowIfNull(sizes);

        if (!IsBinaryPath(filePath))
            throw new ArgumentException("File path must be a binary (.dll) path.", nameof(filePath));

        if (language > ushort.MaxValue || language < 0)
            throw new ArgumentException(null, nameof(language));

        var bitmaps = BuildSizedBitmaps(bitmap, sizes);
        try
        {
            architecture ??= RuntimeInformation.ProcessArchitecture;
            WriteBitmaps(bitmaps, filePath, resourceId, language, deleteExisting, architecture.Value);
        }
        finally
        {
            bitmap.Dispose();
        }
    }

    private static unsafe void WriteBitmaps(IReadOnlyList<WicBitmapSource> bitmaps, string filePath, int resourceId, int language, bool deleteExisting, Architecture architecture)
    {
        SaveBaseDll(filePath, deleteExisting, architecture);

        var groups = new HashSet<ushort>();
        var icons = new HashSet<int>();
        EnumIconResources(filePath, groups, icons);

        // icons ids are automatically computed
        var currentIconId = icons.Count == 0 ? 0 : icons.Max() + 1;

        // group ids are computed if required by caller
        if (resourceId < 0)
        {
            resourceId = groups.Count == 0 ? 0 : groups.Max() + 1;
        }

        var h = Functions.BeginUpdateResourceW(PWSTR.From(filePath), false);
        if (h == 0)
        {
            var gle = Marshal.GetLastWin32Error();
            throw new Win32Exception(gle);
        }

        try
        {
            byte[] groupBytes;
            var iconsBytes = new List<byte[]>();
            var iconsIds = new List<int>();
            using (var groupMs = new MemoryStream())
            {
                var bw = new BinaryWriter(groupMs);

                // GRPICONDIR
                bw.Write((ushort)0); // idReserved
                bw.Write((ushort)1); // 1 for Icon
                bw.Write((ushort)bitmaps.Count);

                foreach (var bm in bitmaps)
                {
                    byte[] iconBytes;
                    using (var iconMs = new MemoryStream())
                    {
                        var bwb = new BinaryWriter(iconMs);
                        bm.Save(iconMs, WicCodec.GUID_ContainerFormatPng);
                        iconBytes = iconMs.ToArray();
                        iconsBytes.Add(iconBytes);
                    }

                    // GRPICONDIRENTRY
                    bw.Write((byte)bm.Width);
                    bw.Write((byte)bm.Height);
                    bw.Write((byte)0); // we write only PNGs
                    bw.Write((byte)0);
                    bw.Write((ushort)1); // color planes
                    bw.Write((ushort)32); // bits per pixel (ARGB=32)
                    bw.Write(iconBytes.Length);
                    bw.Write((ushort)currentIconId); // id
                    iconsIds.Add(currentIconId);
                    currentIconId++;
                }

                groupBytes = groupMs.ToArray();
            }

            fixed (byte* p = groupBytes)
            {
                if (!Functions.UpdateResourceW(h, new PWSTR(RT_GROUP_ICON), new PWSTR(resourceId), (ushort)language, (nint)p, groupBytes.Length()))
                {
                    var gle = Marshal.GetLastWin32Error();
                    throw new Win32Exception(gle);
                }
            }

            for (var i = 0; i < iconsBytes.Count; i++)
            {
                fixed (byte* p = iconsBytes[i])
                {
                    if (!Functions.UpdateResourceW(h, new PWSTR(RT_ICON), new PWSTR(iconsIds[i]), (ushort)language, (nint)p, iconsBytes[i].Length()))
                    {
                        var gle = Marshal.GetLastWin32Error();
                        throw new Win32Exception(gle);
                    }
                }
            }

            if (!Functions.EndUpdateResourceW(h, false))
            {
                var gle = Marshal.GetLastWin32Error();
                throw new Win32Exception(gle);
            }
        }
        finally
        {
            Functions.EndUpdateResourceW(h, true);
        }
    }

    private static bool IsBinaryPath(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext == ".dll";
    }

    public static void SaveAsIcon(this WicBitmapSource bitmap, string filePath)
    {
        var max = Math.Min(MaxIconSize, Math.Max(bitmap.Width, bitmap.Height));
        SaveAsIcon(bitmap, filePath, [max]);
    }

    public static void SaveAsIcon(this WicBitmapSource bitmap, string filePath, IEnumerable<uint> sizes)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentNullException.ThrowIfNull(sizes);
        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        SaveAsIcon(bitmap, stream, sizes);
    }

    private static List<WicBitmapSource> BuildSizedBitmaps(WicBitmapSource bitmap, IEnumerable<uint> sizes)
    {
        var count = sizes.Count();
        if (count > 0)
        {
            foreach (var size in sizes)
            {
                if (size <= 0 || size > MaxIconSize)
                    throw new ArgumentException(null, nameof(sizes));
            }
        }

        var bitmaps = new List<WicBitmapSource>();
        if (count > 0)
        {
            foreach (var size in sizes)
            {
                var clone = bitmap.Clone();
                ResizeForIcon(clone, size);
                bitmaps.Add(clone);
            }
        }
        else
        {
            var clone = bitmap.Clone();
            if (bitmap.Width > MaxIconSize || bitmap.Height > MaxIconSize)
            {
                ResizeForIcon(clone, MaxIconSize);
            }

            bitmaps.Add(clone);
        }
        return bitmaps;
    }

    public static void SaveAsIcon(this WicBitmapSource bitmap, Stream stream, IEnumerable<uint> sizes)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var bitmaps = BuildSizedBitmaps(bitmap, sizes);
        try
        {
            SaveAsIcon(bitmaps, stream);
        }
        finally
        {
            bitmaps.Dispose();
        }
    }

    public static void SaveAsIcon(IEnumerable<WicBitmapSource> bitmaps, string filePath)
    {
        ArgumentNullException.ThrowIfNull(bitmaps);
        ArgumentNullException.ThrowIfNull(filePath);

        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        SaveAsIcon(bitmaps, stream);
    }

    public static void SaveAsIcon(IEnumerable<WicBitmapSource> bitmaps, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(bitmaps);
        ArgumentNullException.ThrowIfNull(stream);

        var owned = new List<WicBitmapSource>();
        var resizedBitmaps = new List<WicBitmapSource>();
        foreach (var bm in bitmaps)
        {
            if (bm.Width > MaxIconSize || bm.Height > MaxIconSize)
            {
                var resized = bm.Clone();
                owned.Add(resized);

                ResizeForIcon(resized, MaxIconSize);
                resizedBitmaps.Add(resized);
            }
            else
            {
                resizedBitmaps.Add(bm);
            }
        }

        try
        {
            WriteIcons(stream, resizedBitmaps);
        }
        finally
        {
            foreach (var bm in owned)
            {
                try
                {
                    bm.Dispose();
                }
                catch
                {
                    // do nothing
                }
            }
        }
    }

    // see https://msdn.microsoft.com/en-us/library/ms997538.aspx
    // and http://blogs.msdn.com/b/oldnewthing/archive/2012/07/20/10331787.aspx
    public static WicBitmapSource? LoadBestIcon(string iconFilePath, int? indexOrResourceId = null, int? preferredSize = null, IconComparer? iconComparer = null)
    {
        ArgumentNullException.ThrowIfNull(iconFilePath);

        // happens on some registry errors
        if (string.IsNullOrWhiteSpace(iconFilePath))
            return null;

        List<WicBitmapSource> bmps;
        var ext = Path.GetExtension(iconFilePath).ToLowerInvariant();
        if (ext == FileExtension)
        {
            if (indexOrResourceId.HasValue && indexOrResourceId.Value < 0)
                throw new ArgumentException(null, nameof(indexOrResourceId));

            bmps = LoadIconsFromIco(iconFilePath);
        }
        else
        {
            bmps = LoadIconsFromBinary(iconFilePath, indexOrResourceId);
        }

        iconComparer ??= new IconComparer();
        bmps.Sort(iconComparer);

        if (preferredSize.HasValue)
        {
            var sized = bmps.LastOrDefault(b => b.Width == preferredSize.Value && b.Height == preferredSize.Value);
            if (sized != null)
                return sized;
        }

        var bmp = bmps.Count > 0 ? bmps[^1] : null;
        return bmp;
    }

    public static IReadOnlyList<WicBitmapSource> LoadIconsForExtension(string extension) => LoadIconsForExtension(extension, out _);
    public static IReadOnlyList<WicBitmapSource> LoadIconsForExtension(string extension, out string? iconFilePath)
    {
        iconFilePath = GetIconFilePathForExtension(extension, out var index);
        if (iconFilePath == null)
            return [];

        return LoadIcons(iconFilePath, index);
    }

    public static IReadOnlyList<WicBitmapSource> LoadIcons(string iconFilePath, int? byIndexOrResourceId = null)
    {
        ArgumentNullException.ThrowIfNull(iconFilePath);
        var ext = Path.GetExtension(iconFilePath).ToLowerInvariant();
        if (ext == FileExtension)
            return LoadIconsFromIco(iconFilePath);

        return LoadIconsFromBinary(iconFilePath, byIndexOrResourceId);
    }

    private static unsafe void ExtractIconGroupEntries(nint module, nint name, nint type, int index, Dictionary<ushort, GRPICONDIRENTRY> entries, Dictionary<ushort, int> groupIndices, Dictionary<ushort, string> groupIds)
    {
        var hres = Functions.FindResourceW(module, new PWSTR(name), new PWSTR(type));
        if (hres == 0)
            return;

        var size = Functions.SizeofResource(module, hres);
        if (size == 0)
            return;

        var res = Functions.LoadResource(module, hres);
        if (res == 0)
            return;

        var ptr = Functions.LockResource(res);
        if (ptr == 0)
            return;

        using var stream = new System.IO.UnmanagedMemoryStream(new IntPtrBuffer(ptr, size), 0, size);
        var reader = new BinaryReader(stream);
        // GRPICONDIR
        reader.ReadInt16(); // idReserved
        if (reader.ReadInt16() != 1) // idType, 1 for ICO
            return;

        Span<byte> span = stackalloc byte[sizeof(GRPICONDIRENTRY)];
        var count = reader.ReadInt16();
        for (var i = 0; i < count; i++)
        {
            if (stream.Read(span) != span.Length)
                return;

            var entry = MemoryMarshal.Read<GRPICONDIRENTRY>(span);
            entries[entry.nId] = entry;

            // is it a string or an id?
            groupIndices[entry.nId] = index;
            if (name.ToInt64() > ushort.MaxValue)
            {
                var id = Marshal.PtrToStringUni(name);
                groupIds[entry.nId] = id!;
            }
            else
            {
                groupIds[entry.nId] = "#" + name.ToInt32();
            }
        }
    }

    private static bool IsPngHeader(int sig) => ((uint)sig & 0xFFFFFF00) == PNG_SIG; // 'PNG' signature

    private static WicBitmapSource? ExtractIcon(nint module, PWSTR name, PWSTR type, Dictionary<ushort, GRPICONDIRENTRY> entries, out int colorCount)
    {
        colorCount = 0;
        var id = (ushort)(name.Value.ToInt64());
        if (!entries.TryGetValue(id, out _))
            return null;

        var hres = Functions.FindResourceExW(module, type, name, 0);
        if (hres == 0)
            return null;

        var size = Functions.SizeofResource(module, hres);
        if (size == 0)
            return null;

        var res = Functions.LoadResource(module, hres);
        if (res == 0)
            return null;

        var ptr = Functions.LockResource(res);
        if (ptr == 0)
            return null;

        using var stream = new System.IO.UnmanagedMemoryStream(new IntPtrBuffer(ptr, size), 0, size);
        var sig = Marshal.ReadInt32(ptr);
        return IsPngHeader(sig) ? LoadPngIcon(stream) : LoadBmpIcon(stream, out colorCount);
    }

    private static WicBitmapSource LoadPngIcon(Stream stream) => WicBitmapSource.Load(stream);
    private static unsafe WicBitmapSource? LoadBmpIcon(Stream stream, out int colorCount)
    {
        colorCount = 0;
        Span<byte> span = stackalloc byte[sizeof(BITMAPINFOHEADER)];
        if (stream.Read(span) != span.Length)
            return null;

        var bih = MemoryMarshal.Read<BITMAPINFOHEADER>(span);

        // we don't support BI_RLExx schemes, BI_BITFIELDS, PNG is handled elsewhere, JPG should not happen in icons
        if (bih.biCompression != BI_RGB)
            return null;

        // for non PNG, header.biHeight is combined height of XOR and AND mask, so we need to divide height by 2
        var height = bih.biHeight == 0 ? MaxIconSize : bih.biHeight / 2;
        var width = bih.biWidth == 0 ? MaxIconSize : bih.biWidth;
        if (height < 0 || width < 0)
            return null;

        // stride is rounded up to a four-byte boundary
        var stride = GetStride((uint)(bih.biBitCount * width / 8));

        if (bih.biBitCount < 16)
        {
            // we don't use bColorCount as it can be wrong per http://blogs.msdn.com/b/oldnewthing/archive/2010/10/18/10077133.aspx
            colorCount = 1 << bih.biBitCount * bih.biPlanes;
            return LoadIndexedBmpIcon(stream, (uint)width, (uint)height, colorCount, stride);
        }

        Guid format;
        switch (bih.biBitCount)
        {
            case 32:
                format = WicPixelFormat.GUID_WICPixelFormat32bppBGRA;
                break;

            case 24:
                format = WicPixelFormat.GUID_WICPixelFormat24bppBGR;
                break;

            case 16:
                format = WicPixelFormat.GUID_WICPixelFormat16bppBGR555;
                break;

            default: // huh?
                return null;
        }

        var bmp = new WicBitmapSource((uint)width, (uint)height, format);
        bmp.WithLock(WICBitmapLockFlags.WICBitmapLockWrite, data =>
        {
            var bmpPtr = data.DataPointer;

            // for some reason, the WicBitmapSource is bottom-up
            bmpPtr += (nint)((height - 1) * stride);
            var bytes = new byte[stride];
            for (var i = 0; i < height; i++)
            {
                var read = stream.Read(bytes, 0, bytes.Length);
                if (read == 0)
                    break;

                Marshal.Copy(bytes, 0, bmpPtr, read);
                bmpPtr -= (nint)stride;
            }
        });
        return bmp;
    }

    private static WicBitmapSource? LoadIndexedBmpIcon(Stream stream, uint width, uint height, int colorCount, uint stride)
    {
        if (colorCount != 2 && colorCount != 16 && colorCount != 256)
            return null;

        var reader = new BinaryReader(stream);
        var palette = new int[colorCount];
        for (var i = 0; i < colorCount; i++)
        {
            var color = reader.ReadInt32();
            if ((color & 0xFF000000) == 0) // no opacity set in the palette, force to 1.0 (255)
            {
                color = (int)((uint)color | 0xFF000000);
            }
            palette[i] = color;
        }

        // we want an alpha channel
        var bmp = new WicBitmapSource(width, height, WicPixelFormat.GUID_WICPixelFormat32bppBGRA);
        bmp.WithLock(WICBitmapLockFlags.WICBitmapLockWrite, data =>
        {
            LoadIndexedBmpIcon(stream, colorCount, width, height, stride, data.DataPointer, palette);
        });
        return bmp;
    }

    private static void LoadIndexedBmpIcon(Stream stream, int colorCount, uint width, uint height, uint stride, nint ptr, int[] palette)
    {
        var ptrStride = width * 4;
        // use XOR (color) bitmap
        var bmpPtr = ptr + (nint)((height - 1) * ptrStride);
        var br = new BitReader(stream, false);
        switch (colorCount)
        {
            case 2:
                for (var i = 0; i < height; i++)
                {
                    var linePtr = bmpPtr;
                    for (var j = 0; j < width; j++)
                    {
                        var color = br.ReadBit();
                        if (color < 0)
                            return;

                        Marshal.WriteInt32(linePtr, palette[color]);
                        linePtr += 4;
                    }

                    // read padding
                    for (var j = 0; j < stride - width / 8; j++)
                    {
                        if (stream.ReadByte() < 0)
                            return;
                    }
                    bmpPtr -= (nint)ptrStride;
                }
                break;

            case 16:
                for (var i = 0; i < height; i++)
                {
                    var linePtr = bmpPtr;
                    for (var j = 0; j < width / 2; j++)
                    {
                        var color = stream.ReadByte();
                        if (color < 0)
                            return;

                        Marshal.WriteInt32(linePtr, palette[color >> 4]);
                        linePtr += 4;
                        Marshal.WriteInt32(linePtr, palette[color & 0xF]);
                        linePtr += 4;
                    }

                    // read padding
                    for (var j = 0; j < stride - width / 2; j++)
                    {
                        if (stream.ReadByte() < 0)
                            return;
                    }
                    bmpPtr -= (nint)ptrStride;
                }
                break;

            case 256:
                for (var i = 0; i < height; i++)
                {
                    var linePtr = bmpPtr;
                    for (var j = 0; j < width; j++)
                    {
                        var color = stream.ReadByte();
                        if (color < 0)
                            return;

                        Marshal.WriteInt32(linePtr, palette[color]);
                        linePtr += 4;
                    }

                    // read padding
                    for (var j = 0; j < stride - width; j++)
                    {
                        if (stream.ReadByte() < 0)
                            return;
                    }
                    bmpPtr -= (nint)ptrStride;
                }
                break;
        }

        // use AND (mask) WicBitmapSource as transparency (assuming 1bpp)
        bmpPtr = ptr + (nint)((height - 1) * ptrStride);
        var andStride = GetStride(width / 8); // and mask also has a stride
        for (var i = 0; i < height; i++)
        {
            var linePtr = bmpPtr;
            for (var j = 0; j < width; j++)
            {
                var color = br.ReadBit();
                if (color < 0)
                    return;

                var opacity = Marshal.ReadByte(linePtr + 3);
                if (opacity == 255) // not set
                {
                    Marshal.WriteByte(linePtr + 3, (byte)(color > 0 ? 0 : 0xFF));
                }
                linePtr += 4;
            }

            // read padding
            for (var j = 0; j < andStride * 8 - width; j++)
            {
                if (br.ReadBit() < 0)
                    return;
            }
            bmpPtr -= (nint)ptrStride;
        }
    }

    private static uint GetStride(uint pixelsBytes)
    {
        var mod4 = pixelsBytes % 4;
        if (mod4 != 0)
        {
            pixelsBytes += 4 - mod4;
        }
        return pixelsBytes;
    }

    private static List<WicBitmapSource> LoadIconsFromIco(string iconFilePath)
    {
        if (!File.Exists(iconFilePath))
            return [];

        using var stream = new FileStream(iconFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var ret = LoadIconsFromIco(stream, out var isExe);
        if (isExe)
        {
            stream.Dispose();
            return LoadIconsFromBinary(iconFilePath, null);
        }
        return ret;
    }

    public static IReadOnlyList<WicBitmapSource> LoadIcons(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        return LoadIconsFromIco(stream);
    }

    private static List<WicBitmapSource> LoadIconsFromIco(Stream stream) => LoadIconsFromIco(stream, out _);
    private static unsafe List<WicBitmapSource> LoadIconsFromIco(Stream stream, out bool isExe)
    {
        isExe = false;
        var list = new List<WicBitmapSource>();
        var reader = new BinaryReader(stream);
        var reserved = reader.ReadInt16(); // idReserved
        const short MZ = 0x5A4D;
        if (reserved == MZ)
        {
            // some ICO have in fact an EXE format
            // like C:\Windows\Installer\{AC76BA86-7AD7-1033-7B44-AC0F074E4100}\PDFFile_8.ico
            isExe = true;
            return [];
        }

        var type = reader.ReadInt16();
        if (type == 1) // idType
        {
            var count = reader.ReadInt16(); // idCount
            var entries = new List<ICONDIRENTRY>(count);
            var indices = new List<int>();
            Span<byte> span = stackalloc byte[sizeof(ICONDIRENTRY)];
            for (var i = 0; i < count; i++)
            {
                if (stream.Read(span) != span.Length)
                    break;

                var entry = MemoryMarshal.Read<ICONDIRENTRY>(span);
                entries.Add(entry);
                indices.Add(i);
            }

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                stream.Seek(entry.dwImageOffset, SeekOrigin.Begin);
                var sig = reader.ReadInt32();
                stream.Seek(entry.dwImageOffset, SeekOrigin.Begin);

                WicBitmapSource? bmp;
                var colorCount = 0;
                if (IsPngHeader(sig))
                {
                    var ws = new WindowedStream(stream, entry.dwBytesInRes);
                    bmp = LoadPngIcon(ws);
                }
                else
                {
                    bmp = LoadBmpIcon(stream, out colorCount);
                }

                if (bmp != null)
                {
                    bmp.IconInformation = new IconInformation
                    {
                        ColorCount = colorCount,
                        Index = indices[i],
                        Id = indices[i].ToString()
                    };
                    list.Add(bmp);
                }
            }
        }
        return list;
    }

    public static WicBitmapSource? ToBitmap(this HICON icon)
    {
        if (icon.Value == 0)
            return null;

        return WicBitmapSource.FromHIcon(icon);
    }

    public static WicBitmapSource? ToBitmap(this Icon? icon)
    {
        if (icon == null)
            return null;

        return ToBitmap(icon.Handle);
    }

    public static IReadOnlyList<WicBitmapSource> LoadIconsUsingShell(string iconFilePath, int? byIndexOrResourceId = null, uint size = 0)
    {
        var list = new List<WicBitmapSource>();
        if (byIndexOrResourceId.HasValue)
        {
            var bmp = ToBitmap(Icon.ExtractIcon(iconFilePath, byIndexOrResourceId.Value, size, throwOnError: false));
            if (bmp != null)
            {
                var ii = new IconInformation();
                if (byIndexOrResourceId.Value < 0)
                {
                    ii.GroupId = "#" + byIndexOrResourceId.Value;
                }
                else
                {
                    ii.Index = byIndexOrResourceId.Value;
                }
                bmp.IconInformation = ii;
                list.Add(bmp);
            }
        }
        else
        {
            var count = Icon.ExtractIconsCount(iconFilePath);
            for (var i = 0; i < count; i++)
            {
                var bmp = ToBitmap(Icon.ExtractIcon(iconFilePath, i, size, throwOnError: false));
                if (bmp != null)
                {
                    bmp.IconInformation = new IconInformation
                    {
                        GroupId = "#" + i,
                        Index = i
                    };
                    list.Add(bmp);
                }
            }
        }
        return list;
    }

    private static List<WicBitmapSource> LoadIconsFromBinary(string iconFilePath, int? byIndexOrResourceId)
    {
        var list = new List<WicBitmapSource>();
        var h = Functions.LoadLibraryExW(PWSTR.From(iconFilePath), 0, LOAD_LIBRARY_FLAGS.LOAD_LIBRARY_AS_DATAFILE | LOAD_LIBRARY_FLAGS.LOAD_LIBRARY_AS_IMAGE_RESOURCE);
        if (h == 0)
        {
            var gle = Marshal.GetLastWin32Error();
            if (gle == ERROR_FILE_NOT_FOUND || gle == ERROR_PATH_NOT_FOUND)
                return list;

            // used as a last resort when our bitness is incompatible with LoadLibrary
            if (gle == ERROR_BAD_EXE_FORMAT)
                return (List<WicBitmapSource>)LoadIconsUsingShell(iconFilePath, byIndexOrResourceId);

            throw new Win32Exception(gle);
        }

        try
        {
            list = LoadIconsFromBinary(h, byIndexOrResourceId, out var hasIcons);
            if (list.Count == 0 && hasIcons)
            {
                // take first found
                list = LoadIconsFromBinary(h, 0, out hasIcons);
            }
        }
        finally
        {
            Functions.FreeLibrary(h);
        }
        return list;
    }

    private static List<WicBitmapSource> LoadIconsFromBinary(nint h, int? byIndexOrResourceId, out bool hasIcons)
    {
        var list = new List<WicBitmapSource>();
        int gle;
        var entries = new Dictionary<ushort, GRPICONDIRENTRY>();
        var groupIndices = new Dictionary<ushort, int>();
        var groupIds = new Dictionary<ushort, string>();
        var groupIndex = 0;
        var hi = false;
        if (!Functions.EnumResourceNamesExW(h, new PWSTR(RT_GROUP_ICON), (m, t, n, lp) =>
        {
            hi = true;
            if (byIndexOrResourceId.HasValue && byIndexOrResourceId.Value >= 0 && byIndexOrResourceId.Value != groupIndex)
            {
                groupIndex++;
                return true;
            }

            string name;
            if (n.Value > ushort.MaxValue)
            {
                name = n.ToString()!;
            }
            else
            {
                name = n.Value.ToString();
            }

            if (byIndexOrResourceId.HasValue && byIndexOrResourceId.Value < 0 && (-byIndexOrResourceId.Value).ToString() != name)
            {
                groupIndex++;
                return true;
            }

            try
            {
                ExtractIconGroupEntries(h, n.Value, t.Value, groupIndex, entries, groupIndices, groupIds);
                groupIndex++;
            }
            catch
            {
                // do nothing
            }
            return true;
        }, 0, 0, 0))
        {
            gle = Marshal.GetLastWin32Error();
            if (gle == ERROR_RESOURCE_TYPE_NOT_FOUND || gle == ERROR_RESOURCE_DATA_NOT_FOUND)
            {
                hasIcons = hi;
                return list;
            }

            throw new Win32Exception(gle);
        }

        if (!Functions.EnumResourceNamesExW(h, new PWSTR(RT_ICON), (m, t, n, lp) =>
        {
            try
            {
                var bmp = ExtractIcon(h, n, t, entries, out var colorCount);
                if (bmp != null)
                {
                    bmp.IconInformation = new IconInformation
                    {
                        ColorCount = colorCount,
                        Id = n.ToString()!,
                        Index = (int)(long)n.Value - 1,
                        GroupId = groupIds[(ushort)(long)n.Value],
                        GroupIndex = groupIndices[(ushort)(long)n.Value]
                    };
                    list.Add(bmp);
                }
            }
            catch
            {
                // do nothing
            }
            return true;
        }, 0, 0, 0))
        {
            gle = Marshal.GetLastWin32Error();
            if (gle == ERROR_RESOURCE_TYPE_NOT_FOUND || gle == ERROR_RESOURCE_DATA_NOT_FOUND)
            {
                hasIcons = hi;
                return list;
            }

            throw new Win32Exception(gle);
        }

        hasIcons = hi;
        return list;
    }

    public static WicBitmapSource? LoadBestIconForExtension(string extension, string? fallbackExtension = null, int? preferredSize = null)
    {
        ArgumentNullException.ThrowIfNull(extension);
        var path = GetIconFilePathForExtension(extension, out var index);
        if (path != null)
        {
            var bm = LoadBestIcon(path, index, preferredSize);
            if (bm != null)
                return bm;
        }

        if (fallbackExtension == null || extension.EqualsIgnoreCase(fallbackExtension))
            return null;

        return LoadBestIconForExtension(fallbackExtension, null, preferredSize);
    }

    private static string? ParseIconPath(string? path, out int? indexOrResourceId)
    {
        indexOrResourceId = null;
        path = path.Nullify();
        if (path == null)
            return null;

        // appx/windows store, we don't support it right now
        if (path.StartsWith("@{"))
            return null;

        // see https://msdn.microsoft.com/en-us/library/windows/desktop/cc144122.aspx
        // %1 is for custom icon handlers, we can't get it
        if (path == _dynamicIcon || path == "\"" + _dynamicIcon + "\"")
            return null;

        string file;
        var pos = path.LastIndexOf(',');
        if (pos < 0)
        {
            file = path;
        }
        else
        {
            if (int.TryParse(path[(pos + 1)..], out var i))
            {
                indexOrResourceId = i;
            }
            file = path[..pos];
        }
        return file;
    }

    private static string? GetIconFilePathFromProgid(string progid, out int? indexOrResourceId, out bool isCommand)
    {
        indexOrResourceId = null;
        isCommand = false;
        if (string.IsNullOrWhiteSpace(progid))
            return null;

        string? path;
        using (var key = Registry.ClassesRoot.OpenSubKey(progid + "\\" + _defaultIconToken, false))
        {
            if (key != null)
            {
                path = ParseIconPath(key.GetValue(null, null) as string, out indexOrResourceId);
                if (path != null)
                    return path;
            }
        }

        var names = GetSubKeyNames(progid);
        if (names.Contains(_shellToken, StringComparer.OrdinalIgnoreCase))
        {
            path = GetPathFromShellKey(progid);
            if (path != null)
            {
                isCommand = true;
                return path;
            }
        }

        return null;
    }

    private static string? GetIconFilePathFromExt(string ext, out int? indexOrResourceId, out bool isCommand)
    {
        string? path;
        indexOrResourceId = null;
        isCommand = false;
        using (var key = Registry.ClassesRoot.OpenSubKey(ext + "\\" + _defaultIconToken, false))
        {
            if (key != null)
            {
                path = ParseIconPath(key.GetValue(null, null) as string, out indexOrResourceId);
                if (path != null)
                    return path;
            }
        }

        var names = GetSubKeyNames(ext);
        if (names.Contains(_shellToken, StringComparer.OrdinalIgnoreCase))
        {
            path = GetPathFromShellKey(ext);
            if (path != null)
            {
                isCommand = true;
                return path;
            }
        }

        var progid = GetKeyDefaultValue(ext);
        if (progid != null && !progid.EqualsIgnoreCase(ext))
        {
            path = GetIconFilePathFromProgid(progid, out indexOrResourceId, out isCommand);
            if (path != null)
                return path;
        }

        if (names.Contains(_openWithProgIdsToken, StringComparer.OrdinalIgnoreCase))
        {
            var progids = GetValuesNames(ext + "\\" + _openWithProgIdsToken);
            foreach (var p in progids)
            {
                path = GetIconFilePathFromProgid(p, out indexOrResourceId, out isCommand);
                if (path != null)
                    return path;
            }
        }

        return null;
    }

    private static string? GetPerceivedType(string keyPath)
    {
        using var key = Registry.ClassesRoot.OpenSubKey(keyPath, false);
        if (key != null)
            return (key.GetValue("PerceivedType", null) as string).Nullify();
        return null;
    }

    public static void Dispose(this IEnumerable<WicBitmapSource> bitmaps)
    {
        if (bitmaps == null)
            return;

        foreach (var bmp in bitmaps)
        {
            try
            {
                bmp.Dispose();
            }
            catch
            {
                // do nothing
            }
        }
    }

    public static WicBitmapSource? GetPreferredSizeBitmap(this IEnumerable<WicBitmapSource> bitmaps, int? preferredWidth = null, int? preferredHeight = null, bool exactMatch = false, bool disposeOthers = true)
    {
        if (!preferredWidth.HasValue && !preferredHeight.HasValue)
            throw new ArgumentException(null, nameof(preferredWidth));

        if (bitmaps == null)
            return null;

        // note: we favor width
        List<WicBitmapSource> ordered;
        if (preferredWidth.HasValue)
        {
            ordered = [.. bitmaps.OrderByDescending(w => w.Width)];
        }
        else
        {
            ordered = [.. bitmaps.OrderByDescending(w => w.Height)];
        }
        if (ordered.Count == 0)
            return null;

        // just one or largest
        if (ordered.Count == 1 || preferredWidth == int.MaxValue || preferredHeight == int.MaxValue)
            return ordered[0];

        // smallest
        if (preferredWidth <= 0 || preferredHeight <= 0)
            return ordered[^1];

        WicBitmapSource? result = null;
        if (preferredWidth.HasValue)
        {
            if (preferredHeight.HasValue)
            {
                result = ordered.FirstOrDefault(b => b.Width == preferredWidth.Value && b.Height == preferredHeight.Value);
            }
            else
            {
                result = ordered.FirstOrDefault(b => b.Width == preferredWidth.Value);
            }
        }
        else
        {
            result = ordered.FirstOrDefault(b => b.Height == preferredHeight);
        }

        if (result == null && !exactMatch)
        {
            // lower down our expectations
            if (preferredWidth.HasValue & preferredHeight.HasValue)
            {
                // note: we favor width again
                result = ordered.FirstOrDefault(b => b.Width == preferredWidth);
                result ??= ordered.FirstOrDefault(b => b.Height == preferredHeight);
            }

            if (result == null)
            {
                // get the one with the bigger size
                // start with first
                result = ordered[0];
                for (var i = 1; i < ordered.Count; i++)
                {
                    // note: we favor width again
                    if (preferredWidth.HasValue)
                    {
                        if (ordered[i].Width < preferredWidth.Value)
                            break;
                    }
                    else
                    {
                        if (ordered[i].Height < preferredHeight)
                            break;
                    }
                    result = ordered[i];
                }
            }
        }

        if (disposeOthers)
        {
            foreach (var bmp in ordered)
            {
                if (bmp != result)
                {
                    try
                    {
                        bmp.Dispose();
                    }
                    catch
                    {
                        // do nothing
                    }
                }
            }
        }
        return result;
    }

    public static WicBitmapSource? GetLargestBitmap(this IEnumerable<WicBitmapSource> bitmaps, bool disposeOthers = true)
    {
        if (bitmaps == null)
            return null;

        var ordered = bitmaps.OrderByDescending(w => w.Width).ToList();
        if (ordered.Count == 0)
            return null;

        if (disposeOthers)
        {
            foreach (var bitmap in ordered.Skip(1)) // don't dispose the one we send
            {
                try
                {
                    bitmap.Dispose();
                }
                catch
                {
                    // do nothing
                }
            }
        }
        return ordered.First();
    }

    public static void ClearIconFilePathByExtensionCache() => _iconFilePathByExtension.Clear();
    public static string? GetIconFilePathForExtension(string extension) => GetIconFilePathForExtension(extension, out _);
    public static string? GetIconFilePathForExtension(string extension, out int? indexOrResourceId, bool useCache = true)
    {
        ArgumentNullException.ThrowIfNull(extension);

        if (!useCache)
            return GetIconFilePathForExtensionNoCache(extension, out indexOrResourceId);

        if (!_iconFilePathByExtension.TryGetValue(extension, out var tuple))
        {
            var path = GetIconFilePathForExtensionNoCache(extension, out var index);
            tuple = (path, index);
            _iconFilePathByExtension[extension] = tuple;
        }

        indexOrResourceId = tuple.Item2;
        return tuple.Item1;
    }

    private static string? GetIconFilePathForExtensionNoCache(string extension, out int? indexOrResourceId)
    {
        var path = GetIconRawPathForExtension(extension, out indexOrResourceId, out var isCommand).Nullify();
        if (path == null)
            return null;

        if (path.StartsWith('"'))
        {
            var pos = path.IndexOf('"', 2);
            if (pos > 0)
            {
                path = path[1..pos].Nullify();
            }
        }
        else if (isCommand)
        {
            var pos = path.IndexOf(' ');
            if (pos > 0)
            {
                path = path[..pos].Nullify();
            }
        }

        if (path != null && path.StartsWith('@'))
        {
            path = path[1..].Nullify();
        }

        if (path != null)
        {
            path = Environment.ExpandEnvironmentVariables(path);
        }

        if (path != null)
        {
            path = path.Replace(@"\\", @"\");
        }
        return path;
    }

    private static bool TryParseWellKnownIcons(string text, out string? dll, out int index)
    {
        dll = null;
        index = 0;
        if (text == null)
            return false;

        var s = text.Split('#');
        if (s.Length < 2)
            return false;

        var name = s[0].Nullify();
        if (name == null)
            return false;

        if (!_knownDlls.Contains(name))
            return false;

        if (!int.TryParse(s[1], out index))
            return false;

        dll = name;
        return true;
    }

    private static string? GetIconRawPathForExtension(string extensionOrProgid, out int? indexOrResourceId, out bool isCommand)
    {
        string? path;
        if (!extensionOrProgid.StartsWith('.'))
        {
            path = GetIconFilePathFromProgid(extensionOrProgid, out indexOrResourceId, out isCommand);
            if (path != null)
                return path;

            if (extensionOrProgid.IndexOf('.') > 0)
            {
                extensionOrProgid = Path.GetExtension(extensionOrProgid);
            }
        }

        path = GetIconFilePathFromExt(extensionOrProgid, out indexOrResourceId, out isCommand);
        if (path != null)
            return path;

        // last resort, hardcoded
        var pt = GetPerceivedType(extensionOrProgid);
        if (string.IsNullOrWhiteSpace(pt))
        {
            pt = extensionOrProgid;
        }

        if (pt != null)
        {
            if (TryParseWellKnownIcons(pt, out var dll, out var dllIndex))
            {
                indexOrResourceId = dllIndex;
                return @"%SystemRoot%\System32\" + dll + ".dll";
            }

            pt = pt.ToLowerInvariant();
            switch (pt)
            {
                case "image":
                    path = _imageResDll;
                    indexOrResourceId = 67;
                    break;

                case "text":
                    if (!extensionOrProgid.EqualsIgnoreCase(".txt"))
                    {
                        path = GetIconRawPathForExtension(".txt", out indexOrResourceId, out isCommand);
                    }
                    break;

                case "audio":
                    path = _imageResDll;
                    indexOrResourceId = 125;
                    break;

                case "video":
                    path = _imageResDll;
                    indexOrResourceId = 127;
                    break;

                case "compressed":
                    if (!extensionOrProgid.EqualsIgnoreCase(".zip"))
                    {
                        path = GetIconRawPathForExtension(".zip", out indexOrResourceId, out isCommand);
                    }
                    break;

                case "document":
                    path = _imageResDll;
                    indexOrResourceId = 85;
                    break;

                case "system":
                    path = _imageResDll;
                    indexOrResourceId = 62;
                    break;

                case "application":
                    path = _imageResDll;
                    indexOrResourceId = 11;
                    break;
            }
        }

        return path;
    }

    private static bool IsValidShellCommand(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        if (path.StartsWith("\"" + _dynamicIcon + "\""))
            return false;

        if (path.StartsWith(_dynamicIcon))
            return false;

        return true;
    }

    private static string? GetPathFromShellKey(string keyName)
    {
        string? path;
        var keyPath = Path.Combine(keyName, _shellToken);
        var names = GetSubKeyNames(keyPath);

        // 1. favor open command
        if (names.Contains(_openToken, StringComparer.OrdinalIgnoreCase))
        {
            path = GetKeyDefaultValue(Path.Combine(keyPath, _openToken, _commandToken));
            if (IsValidShellCommand(path))
                return path;

            names.RemoveAll(n => n.EqualsIgnoreCase(_openToken));
        }

        // 2. favor openxxx command
        foreach (var name in names)
        {
            if (name.StartsWith(_openToken, StringComparison.OrdinalIgnoreCase))
            {
                path = GetKeyDefaultValue(Path.Combine(keyPath, name, _commandToken));
                if (IsValidShellCommand(path))
                    return path;

            }
        }
        names.RemoveAll(n => n.StartsWith(_openToken, StringComparison.OrdinalIgnoreCase));

        // 3. favor readxxx command
        foreach (var name in names)
        {
            if (name.StartsWith(_readToken, StringComparison.OrdinalIgnoreCase))
            {
                path = GetKeyDefaultValue(Path.Combine(keyPath, name, _commandToken));
                if (IsValidShellCommand(path))
                    return path;
            }
        }
        names.RemoveAll(n => n.StartsWith(_readToken, StringComparison.OrdinalIgnoreCase));

        // 4. get (almost) any command
        foreach (var name in names)
        {
            if (name.Contains(_browseToken, StringComparison.OrdinalIgnoreCase))
                continue;

            path = GetKeyDefaultValue(Path.Combine(keyPath, name, _commandToken));
            if (IsValidShellCommand(path))
                return path;
        }

        return null;
    }

    private static string? GetKeyDefaultValue(string keyPath)
    {
        using var key = Registry.ClassesRoot.OpenSubKey(keyPath, false);
        if (key != null)
            return (key.GetValue(null, null) as string).Nullify();

        return null;
    }

    private static List<string> GetSubKeyNames(string path)
    {
        using var key = Registry.ClassesRoot.OpenSubKey(path, false);
        if (key != null)
            return [.. key.GetSubKeyNames()];

        return [];
    }

    private static string[] GetValuesNames(string path)
    {
        using var key = Registry.ClassesRoot.OpenSubKey(path, false);
        if (key != null)
            return key.GetValueNames();

        return [];
    }

    private class WindowedStream : Stream
    {
        private readonly Stream _stream;
        private readonly long _windowSize;

        public WindowedStream(Stream stream, long windowSize)
        {
            _stream = stream;
            InitialPosition = _stream.Position; // not all stream support this
            _windowSize = windowSize;
        }

        public long InitialPosition { get; private set; }
        public override bool CanRead => _stream.CanRead;
        public override bool CanSeek => _stream.CanSeek;
        public override bool CanWrite => false;
        public override long Length => _windowSize;
        public long Left => Length - Position;

        public override long Position
        {
            get => _stream.Position - InitialPosition;
            set
            {
                if (value < 0 || value >= Length)
                    throw new ArgumentException(null, nameof(value));

                _stream.Position = InitialPosition + value;
            }
        }

        public override void Flush() => _stream.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            var left = Math.Min(count, (int)Left);
            var read = _stream.Read(buffer, offset, left);
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;

                case SeekOrigin.End:
                    Position = Length + offset;
                    break;

                case SeekOrigin.Current:
                    Position += offset;
                    break;
            }
            return Position;
        }

        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    private class BitReader
    {
        private byte _current;
        private int _index = 8;

        public BitReader(Stream stream)
            : this(stream, false)
        {
        }

        public BitReader(Stream stream, bool nativeByteOrder)
        {
            ArgumentNullException.ThrowIfNull(stream);

            BaseStream = stream;
            NativeByteOrder = nativeByteOrder;
        }

        public Stream BaseStream { get; }
        public bool NativeByteOrder { get; }

        public int ReadBit()
        {
            if (_index == 8)
            {
                var i = BaseStream.ReadByte();
                if (i < 0)
                    return i;

                _current = (byte)i;
                _index = 0;
            }

            if (NativeByteOrder)
                return _current >> _index++ & 1;

            return _current >> 7 - _index++ & 1;
        }
    }
}
