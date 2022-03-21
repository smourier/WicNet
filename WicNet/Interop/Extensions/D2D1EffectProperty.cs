namespace DirectN
{
    public class D2D1EffectProperty
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }
        public byte[] Data { get; set; }
        public D2D1_PROPERTY_TYPE Type { get; set; }

        public override string ToString() => Name + ": " + Value;
    }
}
