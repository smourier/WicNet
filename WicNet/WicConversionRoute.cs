using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace WicNet
{
    public class WicConversionRoute
    {
        private readonly List<Guid> _formats = new List<Guid>();

        public WicConversionRoute(Guid startPixelFormatGuid)
        {
            StartPixelFormatGuid = startPixelFormatGuid;
        }

        public Guid StartPixelFormatGuid { get; }
        public IReadOnlyList<Guid> Formats => _formats.AsReadOnly();

        public override string ToString() => string.Join(" => ", AllFormats);

        private IEnumerable<WicPixelFormat> AllFormats
        {
            get
            {
                yield return WicPixelFormat.FromClsid(StartPixelFormatGuid);
                foreach (var format in _formats)
                {
                    yield return WicPixelFormat.FromClsid(format);
                }
            }
        }

        private IEnumerable<Guid> AllFormatsGuids
        {
            get
            {
                yield return StartPixelFormatGuid;
                foreach (var format in _formats)
                {
                    yield return format;
                }
            }
        }

        public static IReadOnlyDictionary<Guid, IReadOnlyList<WicConversionRoute>> EnumerateAllConversionRoutes()
        {
            var dic = new ConcurrentDictionary<Guid, IReadOnlyList<WicConversionRoute>>();
            var formats = WicImagingComponent.AllComponents.OfType<WicPixelFormat>().Select(f => f.Guid).ToArray();
            var converters = WicImagingComponent.AllComponents.OfType<WicPixelFormatConverter>().ToArray();
            foreach (var converter in converters)
            {
                foreach (var from in formats)
                {
                    foreach (var to in formats)
                    {
                        if (from == to)
                            continue;

                        if (converter.CanConvert(from, to))
                        {
                            if (!dic.TryGetValue(from, out var list))
                            {
                                list = new List<WicConversionRoute>();
                                dic[from] = list;
                            }

                            if (!list.Any(r => r.Formats.Any(f => f == to)))
                            {
                                var route = new WicConversionRoute(from);
                                route._formats.Add(to);
                                ((IList<WicConversionRoute>)list).Add(route);
                            }
                        }
                    }
                }
            }

            //bool changed;
            //do
            //{
            //    changed = false;
            //    foreach (var kv in dic)
            //    {
            //        var fmts = formats.Except(kv.Value.AllFormatsGuids).ToArray();
            //        foreach (var converter in converters)
            //        {
            //            foreach (var to in fmts)
            //            {
            //                if (converter.CanConvert(kv.Value.Formats.Last(), to))
            //                {
            //                    kv.Value._formats.Add(to);
            //                    changed = true;
            //                }
            //            }
            //        }
            //    }
            //}
            //while (changed);
            return dic;
        }
    }
}
