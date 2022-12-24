using System;

namespace TenderParserWpf.Models.Dto
{
    public class DtoWordForDataGrid
    {
        public Guid Id { get; set; }

        /// <summary>
        /// порядковый номер
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// направление , для которого назначено слово
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// значение
        /// </summary>
        public string Value { get; set; }
    }
}
