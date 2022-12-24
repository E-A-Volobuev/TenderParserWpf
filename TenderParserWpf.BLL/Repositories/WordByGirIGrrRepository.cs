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
    public class WordByGirIGrrRepository: IWordByGirIGrrRepository
    {
        public delegate void WordByGirIGrrHandler(WordByGirIGrrRepository sender, ExcelEventArgs e);
        public event WordByGirIGrrHandler Notify;

        private ApplicationContext _context = new ApplicationContext();
        private DbSet<WordByGirIGrr> _dbSet;

        public WordByGirIGrrRepository()
        {
            try
            {
                // загружаем данные из БД
                _context.WordByGirIGrrTable.Load();
                _dbSet = _context.Set<WordByGirIGrr>();
            }
            catch (Exception ex)
            {
                Notify?.Invoke(this, new ExcelEventArgs(ex.Message));
            }
        }

        public async Task Create(WordByGirIGrr item)
        {
            try
            {
                Predicate<WordByGirIGrr> isWord = (WordByGirIGrr x) => x.Value == item.Value && x.TypeWord == item.TypeWord;
                WordByGirIGrr word = _dbSet.FirstOrDefault(x => isWord(x));
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

        public async Task<List<WordByGirIGrr>> Get()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        public async Task<WordByGirIGrr> GetById(Guid id)
        {
            var word = await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
            return word;
        }
        public async Task DeleteHelper(List<WordKey> list)
        {
            var collection = new List<WordByGirIGrr>();

            foreach (var word in list)
            {
                var result = (WordByGirIGrr)word;
                collection.Add(result);
            }

            if (collection.Count > 0)
            {
                _dbSet.RemoveRange(collection);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateHelper(WordByGirIGrr word)
        {
            _dbSet.Update(word);
            await _context.SaveChangesAsync();
        }
    }
}
