using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenderParserWpf.Models.Models;

namespace TenderParserWpf.BLL.Interfaces
{
    public interface IWordBySiPoTiteeRepository
    {
        Task Create(WordBySiPoTitee word);
        Task<List<WordBySiPoTitee>> Get();

        event Repositories.WordBySiPoTiteeRepository.WordBySiPoTiteeHandler Notify;
        Task<WordBySiPoTitee> GetById(Guid id);
        Task DeleteHelper(List<WordKey> list);
        Task UpdateHelper(WordBySiPoTitee word);
    }
}
