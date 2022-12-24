using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenderParserWpf.Models.Models;

namespace TenderParserWpf.BLL.Interfaces
{
    public interface IWordByIiIdRepository
    {
        Task Create(WordByIiId word);
        Task<List<WordByIiId>> Get();

        event Repositories.WordByIiIdRepository.WordByIiIdHandler Notify;
        Task<WordByIiId> GetById(Guid id);
        Task DeleteHelper(List<WordKey> list);
        Task UpdateHelper(WordByIiId word);
    }
}
