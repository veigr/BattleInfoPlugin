using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleInfoPlugin.Models
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class AliasNameAttribute : Attribute
    {
        public string AliasName { get; private set; }

        public AliasNameAttribute(string aliasName)
        {
            this.AliasName = aliasName;
        }
    }

    public static class AttributeExtensions
    {
        public static T ThrowIf<T>(this T value, Func<T, bool> predicate, Exception exception)
        {
            if (predicate(value)) throw exception;
            return value;
        }

        public static string ToAliasName(this Enum value)
        {
            return value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof (AliasNameAttribute), false)
                .Cast<AliasNameAttribute>()
                .FirstOrDefault()
                .ThrowIf(x => x == null, new ArgumentException("AliasName属性が定義されていません。"))
                .AliasName;
        }
    }
}
