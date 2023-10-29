using System.Reflection;
using System.Text;

namespace ElementSql
{
    internal class ColumnBuilder<T>
    {
        public static string GetColumns()
        {
            var sb = new StringBuilder();

            var t = typeof(T);
            var properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
                sb.Append($"{property.Name},");

            return sb.ToString().TrimEnd(',');
        }
    }
}
