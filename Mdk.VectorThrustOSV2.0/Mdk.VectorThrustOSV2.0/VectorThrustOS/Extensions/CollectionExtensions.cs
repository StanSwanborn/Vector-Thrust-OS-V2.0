using System.Collections.Generic;

namespace IngameScript.VectorThrustOS.Extensions
{
    internal static class CollectionExtensions
    {
        public static bool Empty<T>(this List<T> list) => list.Count == 0;
    }
}