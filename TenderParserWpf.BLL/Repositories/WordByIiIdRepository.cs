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
   public  class WordByIiIdRepository : IWordByIiIdRepository
    {
        public delegate void WordByIiIdHandler(WordByIiIdRepository sender, ExcelEventArgs e);
        public event WordByIiIdHandler Notify;

        private ApplicationContext _context = new ApplicationContext();
        private DbSet<WordByIiId> _dbSet;

        public WordByIiIdRepository()
        {
            try
            {
                // загружаем данные из БД
                _context.WordByIiIdTable.Load();
                _dbSet = _context.Set<WordByIiId>();
            }
            catch (Exception ex)
            {
                Notify?.Invoke(this, new ExcelEventArgs(ex.Message));
            }
        }

        public async Task Create(WordByIiId item)
        {
            try
            {
                Predicate<WordByIiId> isWord = (WordByIiId x) => x.Value == item.Value && x.TypeWord == item.TypeWord;
                WordByIiId word = _dbSet.FirstOrDefault(x => isWord(x));
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

        public async Task<List<WordByIiId>> Get()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        public async Task<WordByIiId> GetById(Guid id)
        {
            var word = await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
            return word;
        }
        public async Task DeleteHelper(List<WordKey> list)
        {
            var collection = new List<WordByIiId>();

            foreach (var word in list)
            {
                var result = (WordByIiId)word;
                collection.Add(result);
            }

            if (collection.Count > 0)
            {
                _dbSet.RemoveRange(collection);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateHelper(WordByIiId word)
        {
            _dbSet.Update(word);
            await _context.SaveChangesAsync();
        }
    }
}
