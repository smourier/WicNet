﻿using System;
using DirectN;

namespace WicNet
{
    public class WicMetadataKeyValue
    {
        public WicMetadataKeyValue(WicMetadataKey key, object value, PropertyType type)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            Key = key;
            Value = value;
            Type = type;
        }

        public WicMetadataKey Key { get; }
        public object Value { get; }

        // keeping the exact type is important for example to differentiate between VT_BLOB and VT_UI1 | VT_VECTOR
        public PropertyType Type { get; }

        public override string ToString() => Key + ": " + Value + " (" + Type + ")";
    }
}
