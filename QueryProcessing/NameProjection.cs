using System.Collections.Generic;

namespace QueryProcessing
{
    public class NameProjection
    {
        public string SourcePropertyName { get; set; }
        public string TargetPropertyName { get; set; }

        public List<NameProjection> ChildProjections { get; set; }

        public NameProjection(string sourceAndTarget) : this(sourceAndTarget, sourceAndTarget)
        { }

        public NameProjection(string sourceAndTarget, List<NameProjection> childProjections) : this(sourceAndTarget, sourceAndTarget, childProjections)
        { }

        public NameProjection(string source, string target)
        {
            SourcePropertyName = source;
            TargetPropertyName = target;
        }

        public NameProjection(string source, string target, List<NameProjection> childProjections)
            : this(source, target)
        {
            ChildProjections = childProjections;
        }
    }
}
