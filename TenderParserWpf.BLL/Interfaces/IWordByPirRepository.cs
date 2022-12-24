using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenderParserWpf.Models.Models;

namespace TenderParserWpf.BLL.Interfaces
{
    public interface IWordByPirRepository
    {
        Task Create(WordByPir word);
        Task<List<WordByPir>> Get();

        event Repositories.WordByPirRepository.WordByPirHandler Notify;
        Task<WordByPir> GetById(Guid id);
        Task DeleteHelper(List<WordKey> list);
        Task UpdateHelper(WordByPir word);

    }
}
