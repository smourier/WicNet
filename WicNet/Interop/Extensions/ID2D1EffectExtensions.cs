using System;

namespace DirectN
{
    public static class ID2D1EffectExtensions
    {
        public static IComObject<ID2D1Image> GetInput(this IComObject<ID2D1Effect> effect, int index) => GetInput(effect?.Object, index);
        public static IComObject<ID2D1Image> GetInput(this ID2D1Effect effect, int index)
        {
            if (effect == null)
                throw new ArgumentNullException(nameof(effect));

            effect.GetInput(index, out var image);
            return new ComObject<ID2D1Image>(image);
        }

        public static int GetInputCount(this IComObject<ID2D1Effect> effect) => GetInputCount(effect?.Object);
        public static int GetInputCount(this ID2D1Effect effect)
        {
            if (effect == null)
                throw new ArgumentNullException(nameof(effect));

            return effect.GetInputCount();
        }

        public static void SetInput(this IComObject<ID2D1Effect> effect, int index, IComObject<ID2D1Bitmap> input = null, bool invalidate = false) => SetInput(effect?.Object, index, input?.Object, invalidate);
        public static void SetInput(this ID2D1Effect effect, int index, ID2D1Bitmap input = null, bool invalidate = false)
        {
            if (effect == null)
                throw new ArgumentNullException(nameof(effect));

            effect.SetInput(index, input, invalidate);
        }

        public static void SetInputCount(this IComObject<ID2D1Effect> effect, int count) => SetInputCount(effect?.Object, count);
        public static void SetInputCount(this ID2D1Effect effect, int count)
        {
            if (effect == null)
                throw new ArgumentNullException(nameof(effect));

            effect.SetInputCount(count).ThrowOnError();
        }

        public static IComObject<ID2D1Image> GetOutput(this IComObject<ID2D1Effect> effect) => GetOutput(effect?.Object);
        public static IComObject<ID2D1Image> GetOutput(this ID2D1Effect effect)
        {
            if (effect == null)
                throw new ArgumentNullException(nameof(effect));

            effect.GetOutput(out var image);
            return new ComObject<ID2D1Image>(image);
        }
    }
}
