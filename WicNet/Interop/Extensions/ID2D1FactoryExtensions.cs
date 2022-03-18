using System;
using System.Collections.Generic;
using System.Linq;

namespace DirectN
{
    public static class ID2D1FactoryExtensions
    {
        public static IComObject<ID2D1RenderTarget> CreateWicBitmapRenderTarget(this IComObject<ID2D1Factory> factory, IComObject<IWICBitmap> target, D2D1_RENDER_TARGET_PROPERTIES? renderTargetProperties = null) => CreateWicBitmapRenderTarget<ID2D1RenderTarget>(factory?.Object, target?.Object, renderTargetProperties);
        public static IComObject<T> CreateWicBitmapRenderTarget<T>(this IComObject<ID2D1Factory> factory, IComObject<IWICBitmap> target, D2D1_RENDER_TARGET_PROPERTIES? renderTargetProperties = null) where T : ID2D1RenderTarget => CreateWicBitmapRenderTarget<T>(factory?.Object, target?.Object, renderTargetProperties);
        public static IComObject<T> CreateWicBitmapRenderTarget<T>(this ID2D1Factory factory, IWICBitmap target, D2D1_RENDER_TARGET_PROPERTIES? renderTargetProperties = null) where T : ID2D1RenderTarget
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var props = renderTargetProperties ?? new D2D1_RENDER_TARGET_PROPERTIES();
            factory.CreateWicBitmapRenderTarget(target, ref props, out var renderTarget).ThrowOnError();
            return new ComObject<T>((T)renderTarget);
        }

        public static IComObject<ID2D1EllipseGeometry> CreateEllipseGeometry(this IComObject<ID2D1Factory> factory, D2D1_ELLIPSE ellipse) => CreateEllipseGeometry(factory?.Object, ellipse);
        public static IComObject<ID2D1EllipseGeometry> CreateEllipseGeometry(this ID2D1Factory factory, D2D1_ELLIPSE ellipse)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            factory.CreateEllipseGeometry(ref ellipse, out var geometry).ThrowOnError();
            return new ComObject<ID2D1EllipseGeometry>(geometry);
        }

        public static IComObject<ID2D1RectangleGeometry> CreateRectangleGeometry(this IComObject<ID2D1Factory> factory, D2D_RECT_F rectangle) => CreateRectangleGeometry(factory?.Object, rectangle);
        public static IComObject<ID2D1RectangleGeometry> CreateRectangleGeometry(this ID2D1Factory factory, D2D_RECT_F rectangle)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            factory.CreateRectangleGeometry(ref rectangle, out var geometry).ThrowOnError();
            return new ComObject<ID2D1RectangleGeometry>(geometry);
        }

        public static IComObject<ID2D1RoundedRectangleGeometry> CreateRoundedRectangleGeometry(this IComObject<ID2D1Factory> factory, D2D1_ROUNDED_RECT rectangle) => CreateRoundedRectangleGeometry(factory?.Object, rectangle);
        public static IComObject<ID2D1RoundedRectangleGeometry> CreateRoundedRectangleGeometry(this ID2D1Factory factory, D2D1_ROUNDED_RECT rectangle)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            factory.CreateRoundedRectangleGeometry(ref rectangle, out var geometry).ThrowOnError();
            return new ComObject<ID2D1RoundedRectangleGeometry>(geometry);
        }

        public static IComObject<ID2D1PathGeometry> CreatePathGeometry(this IComObject<ID2D1Factory> factory) => CreatePathGeometry(factory?.Object);
        public static IComObject<ID2D1PathGeometry> CreatePathGeometry(this ID2D1Factory factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            factory.CreatePathGeometry(out ID2D1PathGeometry geometry).ThrowOnError();
            return new ComObject<ID2D1PathGeometry>(geometry);
        }

        public static IComObject<ID2D1GeometryGroup> CreateGeometryGroup(this IComObject<ID2D1Factory> factory, D2D1_FILL_MODE fillMode, IReadOnlyList<ID2D1Geometry> geometries) => CreateGeometryGroup(factory?.Object, fillMode, geometries);
        public static IComObject<ID2D1GeometryGroup> CreateGeometryGroup(this ID2D1Factory factory, D2D1_FILL_MODE fillMode, IReadOnlyList<ID2D1Geometry> geometries)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            if (geometries == null)
                throw new ArgumentNullException(nameof(geometries));

            if (geometries.IsEmpty())
                throw new ArgumentException(null, nameof(geometries));

            factory.CreateGeometryGroup(fillMode, geometries.ToArray(), geometries.Count, out var geometry).ThrowOnError();
            return new ComObject<ID2D1GeometryGroup>(geometry);
        }

        public static IComObject<ID2D1DCRenderTarget> CreateDCRenderTarget(this IComObject<ID2D1Factory> factory, D2D1_RENDER_TARGET_PROPERTIES properties) => CreateDCRenderTarget<ID2D1DCRenderTarget>(factory?.Object, properties);
        public static IComObject<T> CreateDCRenderTarget<T>(this IComObject<ID2D1Factory> factory, D2D1_RENDER_TARGET_PROPERTIES properties) where T : ID2D1DCRenderTarget => CreateDCRenderTarget<T>(factory?.Object, properties);
        public static IComObject<T> CreateDCRenderTarget<T>(this ID2D1Factory factory, D2D1_RENDER_TARGET_PROPERTIES properties) where T : ID2D1DCRenderTarget
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            factory.CreateDCRenderTarget(ref properties, out var target).ThrowOnError();
            return new ComObject<T>((T)target);
        }

        public static IComObject<ID2D1StrokeStyle> CreateStrokeStyle(this IComObject<ID2D1Factory> factory, D2D1_STROKE_STYLE_PROPERTIES properties, IEnumerable<float> dashes = null) => CreateStrokeStyle<ID2D1StrokeStyle>(factory?.Object, properties, dashes);
        public static IComObject<T> CreateStrokeStyle<T>(this IComObject<ID2D1Factory> factory, D2D1_STROKE_STYLE_PROPERTIES properties, IEnumerable<float> dashes = null) where T : ID2D1StrokeStyle => CreateStrokeStyle<T>(factory?.Object, properties, dashes);
        public static IComObject<T> CreateStrokeStyle<T>(this ID2D1Factory factory, D2D1_STROKE_STYLE_PROPERTIES properties, IEnumerable<float> dashes = null) where T : ID2D1StrokeStyle
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            factory.CreateStrokeStyle(ref properties, dashes?.ToArray(), (dashes?.Count()).GetValueOrDefault(), out var style).ThrowOnError();
            return new ComObject<T>((T)style);
        }
    }
}
