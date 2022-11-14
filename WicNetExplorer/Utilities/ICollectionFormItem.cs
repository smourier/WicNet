namespace WicNetExplorer.Utilities
{
    public interface ICollectionFormItem
    {
        string TypeName { get; }
        string Name { get; }
        object Value { get; }
    }
}
