using System;
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
    }
}
