using System;

namespace DirectN
{
    public static class ID2D1ResourceExtensions
    {
        public static IComObject<ID2D1Factory> GetFactory(this IComObject<ID2D1Resource> resource) => GetFactory(resource?.Object);
        public static IComObject<ID2D1Factory> GetFactory(this ID2D1Resource resource)
        {
            if (resource == null)
                throw new ArgumentNullException(nameof(resource));

            resource.GetFactory(out var factory);
            return factory != null ? new ComObject<ID2D1Factory>(factory) : null;
        }
    }
}
