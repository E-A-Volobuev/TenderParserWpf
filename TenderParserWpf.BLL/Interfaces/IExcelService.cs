using System.Collections.Generic;
using TenderParserWpf.Models.Dto;
using TenderParserWpf.Models.Models;

namespace TenderParserWpf.BLL.Interfaces
{
    public interface IExcelService
    {
        string GetExcelResult(List<ProcedureTender> list, RequestDto dto, string path);
    }
}
