using System.Collections.Generic;

namespace TenderParserWpf.Models.Dto
{
    public class DtoExcelHelper
    {
        public int IndexSheet { get; set; }
        public int IndexRowStart { get; set; }
        public List<int> IndexColumns { get; set; }
        public int IndexRowEnd { get; set; }
    }
}
