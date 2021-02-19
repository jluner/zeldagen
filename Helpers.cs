using System.Collections.Generic;

namespace zeldagen
{
    public static class Helpers
    {
        public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> other)
        {
            foreach (var item in other) set.Add(item);
        }
    }
}
