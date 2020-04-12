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
                if (i > 0)
                    sb.Append(" && ");
                sb.Append($"ExpandoObjectHelper.HasProperty(it, @{i})");
            }
            return queryable.Where(sb.ToString(), propertyNames.Cast<object>().ToArray());
        }

        public static IQueryable SelectProperties(this IQueryable queryable, params NameProjection[] propertyProjections)
        {
            var sb = new StringBuilder("new(");
            for (int i = 0; i < propertyProjections.Length; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append($"ExpandoObjectHelper.SelectProperty(it, @{i}) as {propertyProjections[i].TargetPropertyName}");
            }
            sb.Append(")");
            return queryable.Select(sb.ToString(), propertyProjections.Cast<object>().ToArray());
        }
    }
}
