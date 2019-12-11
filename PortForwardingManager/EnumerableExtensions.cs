using System.Collections.Generic;
using System.Linq;

namespace PortForwardingManager
{
    internal static class EnumerableExtensions
    {
        internal static IEnumerable<T> splice<T>(this IEnumerable<T> source, int start, int count, IEnumerable<T> newElements = null)
        {
            IEnumerable<T> enumerated = source.ToList();
            IEnumerable<T> before = enumerated.Take(start);
            IEnumerable<T> after = enumerated.Skip(start + count);
            return before.Concat(newElements ?? Enumerable.Empty<T>()).Concat(after);
        }
    }
}