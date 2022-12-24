using System.Collections.Generic;
using TenderParserWpf.Models.Dto;

namespace TenderParserWpf.BLL.Interfaces
{
    public interface IExcelReadingService
    {
        /// <summary>
        /// считывание из файла ексель 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        List<DtoWordKey> GetRawData(string filePath);

        /// <summary>
        /// Событие для информирования пользователя о типе ошибки, при чтении excel файла
        /// </summary>
        event Services.ExcelReadingService.ExcelHandler Notify;
    }
}
