using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenderParserWpf.Models.Dto
{
    public class RequestDto
    {
        public RequestDto(string pathCatalog, string search, string[] selected, string[] sites)
        {
            PathCatalog = pathCatalog;
            Search = search;
            Selected = selected;
            Sites = sites;
        }

        /// <summary>
        /// путь сохранения файла с отчётом
        /// </summary>
        public string PathCatalog { get; set; }

        /// <summary>
        /// ключевое слово для поиска тендера
        /// </summary>
        public string Search { get; set; }

        /// <summary>
        /// выбранные направления поиска тендера
        /// </summary>
        public string[] Selected { get; set; }

        /// <summary>
        /// список площадок для поиска тенедра
        /// </summary>
        public string[] Sites { get; set; }
    }
}
