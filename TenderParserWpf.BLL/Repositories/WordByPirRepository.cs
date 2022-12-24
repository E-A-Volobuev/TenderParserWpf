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
    public class WordByPirRepository: IWordByPirRepository
    {
        public delegate void WordByPirHandler(WordByPirRepository sender, ExcelEventArgs e);
        public event WordByPirHandler Notify;

        private ApplicationContext _context = new ApplicationContext();
        private DbSet<WordByPir> _dbSet;

        public WordByPirRepository()
        {
            try
            {
                // загружаем данные из БД
                _context.WordByPirTable.Load();
                _dbSet = _context.Set<WordByPir>();
            }
            catch(Exception ex)
            {
                Notify?.Invoke(this, new ExcelEventArgs(ex.Message));
            }
        }

        public async Task Create(WordByPir item)
        {
            try
            {
                Predicate<WordByPir> isWord = (WordByPir x) => x.Value== item.Value && x.TypeWord== item.TypeWord;
                WordByPir word = _dbSet.FirstOrDefault(x => isWord(x));
                if(word == null)
                {
                    _dbSet.Add(item);
                    await  _context.SaveChangesAsync();
                }               
            }
            catch(Exception ex)
            {
                Notify?.Invoke(this, new ExcelEventArgs(ex.Message));
            }
           
        }

        public async Task<List<WordByPir>> Get()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        public async Task<WordByPir> GetById(Guid id)
        {
            var word =await  _dbSet.FirstOrDefaultAsync(x => x.Id == id);
            return word;
        }
        public async Task DeleteHelper(List<WordKey> list)
        {
            var collection = new List<WordByPir>();

            foreach(var word in list)
            {
                var result = (WordByPir)word;
                collection.Add(result);
            }

            if (collection.Count > 0)
            {
                _dbSet.RemoveRange(collection);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateHelper(WordByPir word)
        {
            _dbSet.Update(word);
            await _context.SaveChangesAsync();
        }
    }
}
