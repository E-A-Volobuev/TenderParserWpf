using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenderParserWpf.Models.Dto
{
    public class DtoCheckBoxes
    {
        public DtoCheckBoxes(bool? checkBox1, bool? checkBox2, bool? checkBox3, bool? checkBox4, bool? checkBox5,
                             bool? checkBoxSite1, bool? checkBoxSite2, bool? checkBoxSite3, bool? checkBoxSite4,
                             bool? checkBoxSite5, bool? checkBoxSite6, bool? checkBoxSite7, bool? checkBoxSite8,
                             string text, string path)
        {
            CheckBox1 = checkBox1;
            CheckBox2 = checkBox2;
            CheckBox3 = checkBox3;
            CheckBox4 = checkBox4;
            CheckBox5 = checkBox5;
            CheckBoxSite1 = checkBoxSite1;
            CheckBoxSite2 = checkBoxSite2;
            CheckBoxSite3 = checkBoxSite3;
            CheckBoxSite4 = checkBoxSite4;
            CheckBoxSite5 = checkBoxSite5;
            CheckBoxSite6 = checkBoxSite6;
            CheckBoxSite7 = checkBoxSite7;
            CheckBoxSite8 = checkBoxSite8;
            SearchWord = text;
            Path = path;
        }
        /// <summary>
        /// текст в комбобоксе 
        /// </summary>
        public string SearchWord { get; set; }

        /// <summary>
        /// путь к папке , куда сохранить отчёт
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// чекбокс ПИР
        /// </summary>
        public bool? CheckBox1 { get; set; }

        /// <summary>
        /// чекбокс СИПОЭ
        /// </summary>
        public bool? CheckBox2 { get; set; }

        /// <summary>
        /// чекбокс СИ по ТИТЭЭ
        /// </summary>
        public bool? CheckBox3 { get; set; }

        /// <summary>
        /// чекбокс И и ИД
        /// </summary>
        public bool? CheckBox4 { get; set; }

        /// <summary>
        /// чекбокс ГИР и ГРР
        /// </summary>
        public bool? CheckBox5 { get; set; }


        /// <summary>
        /// чекбокс etpgpb.ru
        /// </summary>
        public bool? CheckBoxSite1 { get; set; }

        /// <summary>
        /// чекбокс b2b-center.ru
        /// </summary>
        public bool? CheckBoxSite2 { get; set; }

        /// <summary>
        /// чекбокс zakupki.gazprom-neft.ru
        /// </summary>
        public bool? CheckBoxSite3 { get; set; }

        /// <summary>
        /// чекбокс lukoil.ru
        /// </summary>
        public bool? CheckBoxSite4 { get; set; }

        /// <summary>
        /// чекбокс neftisa.ru
        /// </summary>
        public bool? CheckBoxSite5 { get; set; }

        /// <summary>
        /// чекбокс fabrikant.ru
        /// </summary>
        public bool? CheckBoxSite6 { get; set; }

        /// <summary>
        /// чекбокс etp.spbex.ru
        /// </summary>
        public bool? CheckBoxSite7 { get; set; }

        /// <summary>
        /// чекбокс novatek.ru
        /// </summary>
        public bool? CheckBoxSite8 { get; set; }


    }
}
