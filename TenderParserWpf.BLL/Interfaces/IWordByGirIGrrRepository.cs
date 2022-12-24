using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenderParserWpf.Models.Models;

namespace TenderParserWpf.BLL.Interfaces
{
    public interface IWordByGirIGrrRepository
    {
        Task Create(WordByGirIGrr word);
        Task<List<WordByGirIGrr>> Get();

        event Repositories.WordByGirIGrrRepository.WordByGirIGrrHandler Notify;
        Task<WordByGirIGrr> GetById(Guid id);
        Task DeleteHelper(List<WordKey> list);
        Task UpdateHelper(WordByGirIGrr word);
    }
}
