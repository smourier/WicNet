using System.Collections.Generic;

namespace WicNetExplorer.Utilities
{
    public class ToStringVisitorContext
    {
        public virtual ISet<object> VisitedObjects { get; } = new HashSet<object>();
        public virtual bool ThrowOnGetValue { get; set; }
    }
}
