
using TenderParserWpf.Models.Models;

namespace TenderParserWpf.Models.Dto
{
    /// <summary>
    /// ключевое слово
    /// </summary>
    public class DtoWordKey
    {
        /// <summary>
        /// тип ключевого слова (пир, сипоэ или т.д.)
        /// </summary>
        public TypeWord TypeWord { get; set; }
        /// <summary>
        /// значение
        /// </summary>
        public string Value { get; set; }
    }
}
