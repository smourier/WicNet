using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace DirectN
{
    public static class ID2D1DeviceContextExtensions
    {
        public static IComObject<ID2D1Bitmap> CreateBitmap(this IComObject<ID2D1DeviceContext> context, D2D_SIZE_U size, D2D1_BITMAP_PROPERTIES1 properties) => CreateBitmap<ID2D1Bitmap>(context?.Object, size, IntPtr.Zero, 0, properties);
        public static IComObject<ID2D1Bitmap> CreateBitmap(this IComObject<ID2D1DeviceContext> context, D2D_SIZE_U size, IntPtr srcData, uint pitch, D2D1_BITMAP_PROPERTIES1 properties) => CreateBitmap<ID2D1Bitmap>(context?.Object, size, srcData, pitch, properties);
        public static IComObject<T> CreateBitmap<T>(this IComObject<ID2D1DeviceContext> context, D2D_SIZE_U size, D2D1_BITMAP_PROPERTIES1 properties) where T : ID2D1Bitmap => CreateBitmap<T>(context?.Object, size, IntPtr.Zero, 0, properties);
        public static IComObject<T> CreateBitmap<T>(this IComObject<ID2D1DeviceContext> context, D2D_SIZE_U size, IntPtr srcData, uint pitch, D2D1_BITMAP_PROPERTIES1 properties) where T : ID2D1Bitmap => CreateBitmap<T>(context?.Object, size, srcData, pitch, properties);
        public static IComObject<T> CreateBitmap<T>(this ID2D1DeviceContext context, D2D_SIZE_U size, IntPtr srcData, uint pitch, D2D1_BITMAP_PROPERTIES1 properties) where T : ID2D1Bitmap
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.CreateBitmap(size, srcData, pitch, ref properties, out var bmp).ThrowOnError();
            return new ComObject<T>((T)bmp);
        }

        public static IComObject<ID2D1Bitmap1> CreateBitmapFromWicBitmap(this IComObject<ID2D1DeviceContext> context, IComObject<IWICBitmapSource> source, D2D1_BITMAP_PROPERTIES1? properties = null) => CreateBitmapFromWicBitmap<ID2D1Bitmap1>(context?.Object, source?.Object, properties);
        public static IComObject<T> CreateBitmapFromWicBitmap<T>(this IComObject<ID2D1DeviceContext> context, IComObject<IWICBitmapSource> source, D2D1_BITMAP_PROPERTIES1? properties = null) where T : ID2D1Bitmap1 => CreateBitmapFromWicBitmap<T>(context?.Object, source?.Object, properties);
        public static IComObject<T> CreateBitmapFromWicBitmap<T>(this ID2D1DeviceContext context, IWICBitmapSource source, D2D1_BITMAP_PROPERTIES1? properties = null) where T : ID2D1Bitmap1
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            using (var mem = properties.StructureToMemory())
            {
                context.CreateBitmapFromWicBitmap(source, mem.Pointer, out ID2D1Bitmap1 bmp).ThrowOnError();
                return new ComObject<T>((T)bmp);
            }
        }

        public static void WithPrimitiveBlend(this IComObject<ID2D1DeviceContext> context, D2D1_PRIMITIVE_BLEND mode, Action action) => WithPrimitiveBlend(context?.Object, mode, action);
        public static void WithPrimitiveBlend(this ID2D1DeviceContext context, D2D1_PRIMITIVE_BLEND mode, Action action)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var old = context.GetPrimitiveBlend();
            try
            {
                context.SetPrimitiveBlend(mode);
                action();
            }
            finally
            {
                context.SetPrimitiveBlend(old);
            }
        }

        public static IComObject<ID2D1Device> GetDevice(this IComObject<ID2D1DeviceContext> context) => GetDevice<ID2D1Device>(context?.Object);
        public static IComObject<T> GetDevice<T>(this IComObject<ID2D1DeviceContext> context) where T : ID2D1Device => GetDevice<T>(context?.Object);
        public static IComObject<T> GetDevice<T>(this ID2D1DeviceContext context) where T : ID2D1Device
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.GetDevice(out var device);
            return device != null ? new ComObject<T>((T)device) : null;
        }

        public static IComObject<ID2D1Image> GetTarget(this IComObject<ID2D1DeviceContext> context) => GetTarget<ID2D1Image>(context?.Object);
        public static IComObject<T> GetTarget<T>(this IComObject<ID2D1DeviceContext> context) where T : ID2D1Image => GetTarget<T>(context?.Object);
        public static IComObject<T> GetTarget<T>(this ID2D1DeviceContext context) where T : ID2D1Image
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.GetTarget(out var device);
            return device != null ? new ComObject<T>((T)device) : null;
        }

        public static D2D_RECT_F GetImageLocalBounds(this IComObject<ID2D1DeviceContext> context, IComObject<ID2D1Image> image) => GetImageLocalBounds(context?.Object, image?.Object);
        public static D2D_RECT_F GetImageLocalBounds(this ID2D1DeviceContext context, ID2D1Image image)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.GetImageLocalBounds(image, out var bounds).ThrowOnError();
            return bounds;
        }

        public static D2D_RECT_F GetImageWorldBounds(this IComObject<ID2D1DeviceContext> context, IComObject<ID2D1Image> image) => GetImageWorldBounds(context?.Object, image?.Object);
        public static D2D_RECT_F GetImageWorldBounds(this ID2D1DeviceContext context, ID2D1Image image)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.GetImageWorldBounds(image, out var bounds).ThrowOnError();
            return bounds;
        }

        public static void SetTarget(this IComObject<ID2D1DeviceContext> context, IComObject<ID2D1Image> target) => SetTarget(context?.Object, target?.Object);
        public static void SetTarget(this ID2D1DeviceContext context, ID2D1Image target)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (target == null)
                throw new ArgumentNullException(nameof(context));

            context.SetTarget(target);
        }

        public static IComObject<ID2D1Effect> CreateEffect(this IComObject<ID2D1DeviceContext> context, Guid id) => CreateEffect(context?.Object, id);
        public static IComObject<ID2D1Effect> CreateEffect(this ID2D1DeviceContext context, Guid id)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.CreateEffect(id, out var effect).ThrowOnError();
            return effect != null ? new ComObject<ID2D1Effect>(effect) : null;
        }

        public static IComObject<ID2D1ColorContext> CreateColorContextFromWicColorContext(this IComObject<ID2D1DeviceContext> context, IComObject<IWICColorContext> colorContext) => CreateColorContextFromWicColorContext(context?.Object, colorContext?.Object);
        public static IComObject<ID2D1ColorContext> CreateColorContextFromWicColorContext(this ID2D1DeviceContext context, IWICColorContext colorContext)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (colorContext == null)
                throw new ArgumentNullException(nameof(colorContext));

            context.CreateColorContextFromWicColorContext(colorContext, out var d2dColorContext).ThrowOnError();
            return new ComObject<ID2D1ColorContext>(d2dColorContext);
        }

        public static IComObject<ID2D1ColorContext> CreateColorContext(this IComObject<ID2D1DeviceContext> context, D2D1_COLOR_SPACE space, byte[] profile = null) => CreateColorContext(context?.Object, space, profile);
        public static IComObject<ID2D1ColorContext> CreateColorContext(this ID2D1DeviceContext context, D2D1_COLOR_SPACE space, byte[] profile = null)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.CreateColorContext(space, profile, (profile?.Length).GetValueOrDefault(), out var d2dColorContext).ThrowOnError();
            return new ComObject<ID2D1ColorContext>(d2dColorContext);
        }

        public static IComObject<ID2D1ColorContext> CreateColorContextFromFilename(this IComObject<ID2D1DeviceContext> context, string filePath) => CreateColorContextFromFilename(context?.Object, filePath);
        public static IComObject<ID2D1ColorContext> CreateColorContextFromFilename(this ID2D1DeviceContext context, string filePath)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.CreateColorContextFromFilename(filePath, out var d2dColorContext).ThrowOnError();
            return new ComObject<ID2D1ColorContext>(d2dColorContext);
        }

        public static IComObject<ID2D1ColorContext1> CreateColorContextFromDxgiColorSpace(this IComObject<ID2D1DeviceContext5> context, DXGI_COLOR_SPACE_TYPE colorSpace) => CreateColorContextFromDxgiColorSpace(context?.Object, colorSpace);
        public static IComObject<ID2D1ColorContext1> CreateColorContextFromDxgiColorSpace(this ID2D1DeviceContext5 context, DXGI_COLOR_SPACE_TYPE colorSpace)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.CreateColorContextFromDxgiColorSpace(colorSpace, out var d2dColorContext).ThrowOnError();
            return new ComObject<ID2D1ColorContext1>(d2dColorContext);
        }

        public static void DrawBitmap(this IComObject<ID2D1DeviceContext> context,
            IComObject<ID2D1Bitmap1> bitmap,
            float opacity = 1,
            D2D1_INTERPOLATION_MODE interpolationMode = D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_NEAREST_NEIGHBOR,
            D2D_RECT_F? destinationRectangle = null,
            D2D_RECT_F? sourceRectangle = null,
            D2D_MATRIX_4X4_F? perspectiveTransform = null) => DrawBitmap(context?.Object, bitmap?.Object, opacity, interpolationMode, destinationRectangle, sourceRectangle, perspectiveTransform);

        public static void DrawBitmap(this IComObject<ID2D1DeviceContext> context,
            IComObject<ID2D1Bitmap> bitmap,
            float opacity = 1,
            D2D1_INTERPOLATION_MODE interpolationMode = D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_NEAREST_NEIGHBOR,
            D2D_RECT_F? destinationRectangle = null,
            D2D_RECT_F? sourceRectangle = null,
            D2D_MATRIX_4X4_F? perspectiveTransform = null) => DrawBitmap(context?.Object, bitmap?.Object, opacity, interpolationMode, destinationRectangle, sourceRectangle, perspectiveTransform);

        public static void DrawBitmap(this ID2D1DeviceContext context,
            ID2D1Bitmap bitmap,
            float opacity = 1,
            D2D1_INTERPOLATION_MODE interpolationMode = D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_NEAREST_NEIGHBOR,
            D2D_RECT_F? destinationRectangle = null,
            D2D_RECT_F? sourceRectangle = null,
            D2D_MATRIX_4X4_F? perspectiveTransform = null)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            using (var drc = destinationRectangle.StructureToMemory())
            {
                using (var src = sourceRectangle.StructureToMemory())
                {
                    if (perspectiveTransform.HasValue || (int)interpolationMode > (int)D2D1_BITMAP_INTERPOLATION_MODE.D2D1_BITMAP_INTERPOLATION_MODE_LINEAR)
                    {
                        using (var per = perspectiveTransform.StructureToMemory())
                        {
                            context.DrawBitmap(bitmap, drc.Pointer, opacity, interpolationMode, src.Pointer, per.Pointer);
                        }
                    }
                    else
                    {
                        context.DrawBitmap(bitmap, drc.Pointer, opacity, (D2D1_BITMAP_INTERPOLATION_MODE)interpolationMode, src.Pointer);
                    }
                }
            }
        }

        public static void DrawImage(this IComObject<ID2D1DeviceContext> context,
            IComObject<ID2D1Image> image,
            D2D1_INTERPOLATION_MODE interpolationMode = D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_LINEAR,
            D2D1_COMPOSITE_MODE compositeMode = D2D1_COMPOSITE_MODE.D2D1_COMPOSITE_MODE_SOURCE_OVER,
            D2D_POINT_2F? targetOffset = null,
            D2D_RECT_F? imageRectangle = null) => DrawImage(context?.Object, image?.Object, interpolationMode, compositeMode, targetOffset, imageRectangle);

        public static void DrawImage(this ID2D1DeviceContext context,
            ID2D1Image image,
            D2D1_INTERPOLATION_MODE interpolationMode = D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_LINEAR,
            D2D1_COMPOSITE_MODE compositeMode = D2D1_COMPOSITE_MODE.D2D1_COMPOSITE_MODE_SOURCE_OVER,
            D2D_POINT_2F? targetOffset = null,
            D2D_RECT_F? imageRectangle = null)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (image == null)
                throw new ArgumentNullException(nameof(image));

            using (var irc = imageRectangle.StructureToMemory())
            {
                using (var to = targetOffset.StructureToMemory())
                {
                    context.DrawImage(image, to.Pointer, irc.Pointer, interpolationMode, compositeMode);
                }
            }
        }

        public static void DrawImage(this IComObject<ID2D1DeviceContext> context,
            IComObject<ID2D1Effect> effect,
            D2D1_INTERPOLATION_MODE interpolationMode = D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_LINEAR,
            D2D1_COMPOSITE_MODE compositeMode = D2D1_COMPOSITE_MODE.D2D1_COMPOSITE_MODE_SOURCE_OVER,
            D2D_POINT_2F? targetOffset = null,
            D2D_RECT_F? imageRectangle = null) => DrawImage(context?.Object, effect?.Object, interpolationMode, compositeMode, targetOffset, imageRectangle);

        public static void DrawImage(this ID2D1DeviceContext context,
            ID2D1Effect effect,
            D2D1_INTERPOLATION_MODE interpolationMode = D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_LINEAR,
            D2D1_COMPOSITE_MODE compositeMode = D2D1_COMPOSITE_MODE.D2D1_COMPOSITE_MODE_SOURCE_OVER,
            D2D_POINT_2F? targetOffset = null,
            D2D_RECT_F? imageRectangle = null)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (effect == null)
                throw new ArgumentNullException(nameof(effect));

            using (var irc = imageRectangle.StructureToMemory())
            {
                using (var to = targetOffset.StructureToMemory())
                {
                    effect.GetOutput(out var image);
                    try
                    {
                        context.DrawImage(image, to.Pointer, irc.Pointer, interpolationMode, compositeMode);
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(image);
                    }
                }
            }
        }

        public static void PushLayer(this IComObject<ID2D1DeviceContext> context, D2D1_LAYER_PARAMETERS1 parameters) => PushLayer(context?.Object, parameters);
        public static void PushLayer(this ID2D1DeviceContext context, D2D1_LAYER_PARAMETERS1 parameters)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.PushLayer(ref parameters, null);
        }

        public static IComObject<ID2D1SvgDocument> CreateSvgDocument(this IComObject<ID2D1DeviceContext5> context, IStream stream, D2D_SIZE_F viewPortSize) => CreateSvgDocument(context?.Object, stream, viewPortSize);
        public static IComObject<ID2D1SvgDocument> CreateSvgDocument(this ID2D1DeviceContext5 context, IStream stream, D2D_SIZE_F viewPortSize)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            context.CreateSvgDocument(stream, viewPortSize, out var doc).ThrowOnError();
            return new ComObject<ID2D1SvgDocument>(doc);
        }

        public static void DrawSvgDocument(this ID2D1DeviceContext5 context, ID2D1SvgDocument document)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (document == null)
                throw new ArgumentNullException(nameof(document));

            context.DrawSvgDocument(document);
        }
    }
}