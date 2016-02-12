using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
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

        public static int GetValuesHashCode<T>(this IEnumerable<T> ie, Func<T, int> valuesHashCodeFunc = null)
        {
            return ie?.Aggregate(new StringBuilder(), (b, v) => b.Append(valuesHashCodeFunc?.Invoke(v) ?? v?.ToString().GetHashCode() ?? 0))
            .ToString().GetHashCode()
            ?? 0;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary == null) return default(TValue);
            TValue p;
            return dictionary.TryGetValue(key, out p) ? p : default(TValue);
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
                merged.AddOrUpdate(newKvp.Key, newKvp.Value, (k, v) => updateValueFactory(v, newKvp.Value));
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

        public static List<TElement> Merge<TElement, TKey>(this List<TElement> c1, List<TElement> c2, Func<TElement,TKey> keySelector)
        {
            var e = c2
                .Where(x => !c1.Any(y => keySelector(x).Equals(keySelector(y))))
                .ToArray();
            c1.AddRange(e);
            return c1;
        }
        
        private static readonly object serializeLoadLock = new object();
        public static void Serialize<T>(this T target, string fileName)
        {
            Debug.WriteLine("Start Serialize");
            var serializer = new DataContractJsonSerializer(typeof(T));
            lock (serializeLoadLock)
            {
                var i = 0;
                string tempPath;
                do
                {
                    tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"_{i++}_{fileName}");
                } while (File.Exists(tempPath));
                using (var stream = Stream.Synchronized(new FileStream(tempPath, FileMode.Create, FileAccess.Write)))
                {
                    serializer.WriteObject(stream, target);
                }

                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                if (File.Exists(path))
                    File.Delete(path);
                File.Move(tempPath, path);
            }
            Debug.WriteLine("End  Serialize");
        }
        public static T Deserialize<T>(this string fileName)
        {
            Debug.WriteLine("Start Deserialize");
            var serializer = new DataContractJsonSerializer(typeof(T));
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            lock (serializeLoadLock)
            {
                if (!File.Exists(path)) return default(T);
                using (var stream = Stream.Synchronized(new FileStream(path, FileMode.Open, FileAccess.Read)))
                {
                    Debug.WriteLine("End  Deserialize");
                    return (T)serializer.ReadObject(stream);
                }
            }
        }
    }
}
