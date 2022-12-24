using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenderParserWpf.Models.Models;

namespace TenderParserWpf.BLL.Interfaces
{
    public interface IWordBySipoeRepository
    {
        Task Create(WordBySipoe word);
        Task<List<WordBySipoe>> Get();

        event Repositories.WordBySipoeRepository.WordBySipoeHandler Notify;
        Task<WordBySipoe> GetById(Guid id);
        Task DeleteHelper(List<WordKey> list);
        Task UpdateHelper(WordBySipoe word);
    }
}
