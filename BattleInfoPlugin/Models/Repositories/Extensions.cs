using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleInfoPlugin.Models.Repositories
{
    static class Extensions
    {
        public static bool EqualsValue<T>(this T[] array1, T[] array2)
            where T : struct
        {
            if (array1 == array2) return true;
            if (array1 == null || array2 == null) return false;
            if (array1.Length != array2.Length) return false;
            return array1
                .Zip(array2, (x, y) => new { x, y })
                .All(x => x.x.Equals(x.y));
        }
        public static bool EqualsValue<T>(this T[][] array1, T[][] array2)
            where T : struct
        {
            if (array1 == array2) return true;
            if (array1 == null || array2 == null) return false;
            if (array1.Length != array2.Length) return false;
            return array1
                .Zip(array2, (x, y) => new { x, y })
                .All(x => x.x.EqualsValue(x.y));
        }

        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
            this IDictionary<TKey, TValue> dic1,
            IDictionary<TKey, TValue> dic2,
            Func<TValue, TValue, TValue> updateValueFactory)
        {
            if (dic2 == null || !dic2.Any()) return dic1.ToDictionary(x => x.Key, x => x.Value);

            var merged = new ConcurrentDictionary<TKey, TValue>(dic1);
            foreach (var newKvp in dic2)
            {
                merged.AddOrUpdate(newKvp.Key, newKvp.Value, (k, v) => updateValueFactory(merged[k], v));
            }
            return merged.ToDictionary(x => x.Key, x => x.Value);
        }

        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
            this IDictionary<TKey, TValue> dic1,
            IDictionary<TKey, TValue> dic2,
            bool isSelectNewValue = true)
        {
            return dic1.Merge(dic2, (v1, v2) => isSelectNewValue ? v2 : v1);
        }

        public static HashSet<T> Merge<T>(this HashSet<T> h1, HashSet<T> h2)
        {
            h1.UnionWith(h2);
            return h1;
        }

        public static List<T> Merge<T>(this List<T> c1, List<T> c2)
        {
            var e = c1.Except(c2);
            foreach (var item in e)
            {
                c1.Add(item);
            }
            return c1;
        }
    }
}
