namespace WicNet;

public class WicMetadataKeyValue : IDisposable
{
    private bool _disposedValue;

    public WicMetadataKeyValue(WicMetadataKey key, object? value, VARENUM type)
    {
        ArgumentNullException.ThrowIfNull(key);
        Key = key;
        Value = value;
        Type = type;
    }

    public WicMetadataKey Key { get; }
    public object? Value { get; }

    // keeping the exact type is important for example to differentiate between VT_BLOB and VT_UI1 | VT_VECTOR
    public VARENUM Type { get; }

    public override string ToString() => Key + ": " + Value + " (" + Type + ")";

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // dispose managed state (managed objects)
                if (Value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            _disposedValue = true;
        }
    }

    ~WicMetadataKeyValue() { Dispose(disposing: false); }
    public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }
}
