using System.Collections.Generic;
using System;

namespace IngameScript.VectorThrustOS.Extensions
{
    internal static class CollectionExtensions
    {
        public static bool Empty<T>(this List<T> list) => 
            list.Count == 0;

        public static double Round(this double val, int num = 0) =>  
            Math.Round(val, num);
    }
}