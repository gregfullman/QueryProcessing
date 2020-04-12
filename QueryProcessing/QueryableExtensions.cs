using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;

namespace QueryProcessing
{
    public static class QueryableExtensions
    {
        public static IQueryable ApplyPropertiesExistFilter(this IQueryable queryable, params string[] propertyNames)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < propertyNames.Length; i++)
            {
                // TODO: need to handle nested properties (one-to-one and one-to-many)
                // Probably best to handle that in the ExpandObjectHelper class
                if (i > 0)
                    sb.Append(" && ");
                sb.Append($"ExpandoObjectHelper.HasProperty(it, @{i})");
            }
            return queryable.Where(sb.ToString(), propertyNames.Cast<object>().ToArray());
        }
    }
}
