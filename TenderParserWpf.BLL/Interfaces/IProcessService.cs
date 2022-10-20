using System.Collections.Generic;
using System.Threading.Tasks;
using TenderParserWpf.Models.Dto;
using TenderParserWpf.Models.Models;

namespace TenderParserWpf.BLL.Interfaces
{
    public interface IProcessService
    {
        Task<List<ProcedureTender>> Start(RequestDto dto);
    }
}
