using System;
using System.Linq;


namespace TenderParserWpf.Models.Extensions
{
    public static class EnumExtension
    {
        /// <summary>
        /// Метод получения значения атрибута у элемента перечисления
        /// </summary>
        /// <typeparam name="TEnum">Тип перечисления</typeparam>
        /// <typeparam name="TAttribute">Тип атрибута</typeparam>
        /// <param name="value">Элемент перечисления</param>
        public static TAttribute GetAttribute<TEnum, TAttribute>(this TEnum value)
            where TEnum : Enum
        {
            var enumValueInfo = value.GetType().GetMember(value.ToString())
                                               .SingleOrDefault();

            return (TAttribute)enumValueInfo?.GetCustomAttributes(typeof(TAttribute), false)
                                             .SingleOrDefault();
        }
    }
}
