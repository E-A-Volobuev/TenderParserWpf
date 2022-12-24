
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenderParserWpf.BLL.Interfaces;
using TenderParserWpf.BLL.Services;
using TenderParserWpf.Models;
using TenderParserWpf.Models.Models;

namespace TenderParserWpf.BLL.Repositories
{
    public class WordBySiPoTiteeRepository: IWordBySiPoTiteeRepository
    {
        public delegate void WordBySiPoTiteeHandler(WordBySiPoTiteeRepository sender, ExcelEventArgs e);
        public event WordBySiPoTiteeHandler Notify;

        private ApplicationContext _context = new ApplicationContext();
        private DbSet<WordBySiPoTitee> _dbSet;

        public WordBySiPoTiteeRepository()
        {
            try
            {
                // загружаем данные из БД
                _context.WordBySiPoTiteeTable.Load();
                _dbSet = _context.Set<WordBySiPoTitee>();
            }
            catch (Exception ex)
            {
                Notify?.Invoke(this, new ExcelEventArgs(ex.Message));
            }
        }

        public async Task Create(WordBySiPoTitee item)
        {
            try
            {
                Predicate<WordBySiPoTitee> isWord = (WordBySiPoTitee x) => x.Value == item.Value && x.TypeWord == item.TypeWord;
                WordBySiPoTitee word = _dbSet.FirstOrDefault(x => isWord(x));
                if (word == null)
                {
                    _dbSet.Add(item);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Notify?.Invoke(this, new ExcelEventArgs(ex.Message));
            }

        }

        public async Task<List<WordBySiPoTitee>> Get()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        public async Task<WordBySiPoTitee> GetById(Guid id)
        {
            var word = await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
            return word;
        }
        public async Task DeleteHelper(List<WordKey> list)
        {
            var collection = new List<WordBySiPoTitee>();

            foreach (var word in list)
            {
                var result = (WordBySiPoTitee)word;
                collection.Add(result);
            }

            if (collection.Count > 0)
            {
                _dbSet.RemoveRange(collection);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateHelper(WordBySiPoTitee word)
        {
            _dbSet.Update(word);
            await _context.SaveChangesAsync();
        }
    }
}
