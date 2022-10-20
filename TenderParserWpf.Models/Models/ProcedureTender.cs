using System;

namespace TenderParserWpf.Models.Models
{
    public class ProcedureTender
    {
        /// <summary>
        /// Номер процедуры
        /// </summary>
        public string NumberProcedure { get; set; }

        /// <summary>
        /// дата публикации
        /// </summary>
        public DateTime DatePubliсation { get; set; }

        /// <summary>
        /// url процедуры
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Сроки подачи заявки
        /// </summary>
        public DateTime DateEnd { get; set; }

        /// <summary>
        /// Предмет
        /// </summary>
        public string ProcedureName { get; set; }

        /// <summary>
        /// Заказчик
        /// </summary>
        public string Customer { get; set; }

        /// <summary>
        /// НМЦ
        /// </summary>
        public string Price { get; set; }
    }
}
