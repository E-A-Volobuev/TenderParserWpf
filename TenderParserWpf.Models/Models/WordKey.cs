using System;
using System.ComponentModel.DataAnnotations;

namespace TenderParserWpf.Models.Models
{
    public abstract class WordKey
    {
        public Guid Id { get; set; }

        /// <summary>
        /// тип ключевого слова (пир, сипоэ или т.д.)
        /// </summary>
        public TypeWord TypeWord { get; set; }
        /// <summary>
        /// значение
        /// </summary>
        public string Value { get; set; }
    }

    /// <summary>
    /// тип ключевого слова
    /// </summary>
    public enum TypeWord
    {
        [Display(Name = "ПИР")]
        PIR,
        [Display(Name = "СИПОЭ")]
        SIPOE,
        [Display(Name = "СИ по ТИТЭЭ")]
        SI_PO_TITEEB,
        [Display(Name = "И и ИД")]
        I_I_ID,
        [Display(Name = "ГИР и ГРР")]
        GIR_I_GRR
    }
}
