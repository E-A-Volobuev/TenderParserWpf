using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenderParserWpf.BLL.Interfaces;
using TenderParserWpf.BLL.Services;
using TenderParserWpf.Models;
using TenderParserWpf.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace TenderParserWpf.BLL.Repositories
{
   public class WordBySipoeRepository: IWordBySipoeRepository
    {
        public delegate void WordBySipoeHandler(WordBySipoeRepository sender, ExcelEventArgs e);
        public event WordBySipoeHandler Notify;

        private ApplicationContext _context = new ApplicationContext();
        private DbSet<WordBySipoe> _dbSet;

        public WordBySipoeRepository()
        {
            try
            {
                // загружаем данные из БД
                _context.WordBySipoeTable.Load();
                _dbSet = _context.Set <WordBySipoe> ();
            }
            catch (Exception ex)
            {
                Notify?.Invoke(this, new ExcelEventArgs(ex.Message));
            }
        }

        public async Task Create(WordBySipoe item)
        {
            try
            {
                Predicate<WordBySipoe> isWord = (WordBySipoe x) => x.Value == item.Value && x.TypeWord == item.TypeWord;
                WordBySipoe word = _context.WordBySipoeTable.FirstOrDefault(x => isWord(x));
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

        public async Task<List<WordBySipoe>> Get()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        public async Task<WordBySipoe> GetById(Guid id)
        {
            var word = await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
            return word;
        }
        public async Task DeleteHelper(List<WordKey> list)
        {
            var collection = new List<WordBySipoe>();

            foreach (var word in list)
            {
                var result = (WordBySipoe)word;
                collection.Add(result);
            }

            if (collection.Count > 0)
            {
                _dbSet.RemoveRange(collection);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateHelper(WordBySipoe word)
        {
            _dbSet.Update(word);
            await _context.SaveChangesAsync();
        }
    }
}
