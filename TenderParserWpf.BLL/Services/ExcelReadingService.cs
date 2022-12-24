using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using TenderParserWpf.BLL.Interfaces;
using TenderParserWpf.Models.Dto;
using TenderParserWpf.Models.Models;

namespace TenderParserWpf.BLL.Services
{
    public class ExcelReadingService:IExcelReadingService
    {
        public delegate void ExcelHandler(ExcelReadingService sender, ExcelEventArgs e);
        public event ExcelHandler Notify;
        /// <summary>
        /// книга откуда считываем данные
        /// </summary>
        public IWorkbook workBook = null;

        /// <summary>
        /// словарь собранных данных из ячеек файла excel
        /// </summary>
        private List<Dictionary<int, string>> _rawData { get; set; }

        /// <summary>
        /// считывание из файла ексель 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public List<DtoWordKey> GetRawData(string filePath)
        {
            try
            {
                List<DtoWordKey> listResult = new List<DtoWordKey>();

                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                workBook = new XSSFWorkbook(fs);

                if(CheckFile(workBook))
                {
                    var cols = new List<int>();


                    for (int i = 0; i < 2; i++)
                        cols.Add(i);

                    GetRawDataHelper(cols, listResult);

                    return listResult;
                }
                else
                {
                    Notify?.Invoke(this, new ExcelEventArgs("выбран некорректный файл"));
                    return null;
                }
            }
            catch(Exception ex)
            {
                Notify?.Invoke(this, new ExcelEventArgs(ex.Message));
                return null;
            }          
        }

        private bool CheckFile(IWorkbook wb)
        {
            bool flag = true;
            if (wb.GetSheetAt(0).SheetName != "ПИР")
                flag = false;
            if (wb.NumberOfSheets != 5)
                flag = false;

            return flag;

        }

        private void GetRawDataHelper(List<int> cols, List<DtoWordKey> listResult)
        {
            for (int i = 0; i < 5; i++)
            {
                List<DtoWordKey> list = GetRawDataHelper(cols, i);
                if (list != null)
                    listResult.AddRange(list);
            }
        }

        private List<DtoWordKey> GetRawDataHelper(List<int> cols,int i)
        {
            List<DtoWordKey> list = new List<DtoWordKey>();

            DtoExcelHelper helper = new DtoExcelHelper() { IndexSheet = i, IndexRowStart = 1, IndexRowEnd = 0, IndexColumns = cols };

            _rawData = GetDataFromSheet(helper);

            foreach (var rawItem in _rawData)
            {
                var info = CreateInfo(rawItem, i);

                if (info != null)
                    list.Add(info);
            }

            return list;
        }
        /// <summary>
        /// создание объекта ключевого слова 
        /// </summary>
        /// <param name="rawItem"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private DtoWordKey CreateInfo(Dictionary<int, string> rawItem,int i)
        {
            DtoWordKey info = new DtoWordKey();
            if (!string.IsNullOrEmpty(rawItem[0]))
            {
                CreateHelper(i, info);
                info.Value = rawItem[1];

                return info;
            }
            else
                return null;
        }
        private void CreateHelper(int i,DtoWordKey info)
        {
            switch (i)
            {
                case 0:
                    info.TypeWord = TypeWord.PIR;
                    break;
                case 1:
                    info.TypeWord = TypeWord.SIPOE;
                    break;
                case 2:
                    info.TypeWord = TypeWord.SI_PO_TITEEB;
                    break;
                case 3:
                    info.TypeWord = TypeWord.I_I_ID;
                    break;
                case 4:
                    info.TypeWord = TypeWord.GIR_I_GRR;
                    break;
            }
        }
        /// <summary>
        /// Считывание данных с листа
        /// </summary>
        /// <param name="indexSheet"></param>
        /// <param name="indexRowStart"></param>
        /// <param name="indexColumns"></param>
        /// <param name="indexRowEnd"></param>
        /// <returns></returns>
        public List<Dictionary<int, string>> GetDataFromSheet(DtoExcelHelper helper)
        {
            var listData = new List<Dictionary<int, string>>();

            if (helper.IndexSheet == -1) return listData;

            var sheet = workBook.GetSheetAt(helper.IndexSheet);
            var rows = sheet.GetRowEnumerator();

            rows.Reset();

            while (rows.MoveNext())
            {
                try
                {
                    var row = (IRow)rows.Current;

                    GetDataFormSheetHelper(row, helper, listData);

                    //если указан индекс последний строки, проверяем и выходим из цикла
                    if (row != null && helper.IndexRowEnd > 0 && row.RowNum == helper.IndexRowEnd)
                        break;

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

            }

            return listData;
        }

        private void GetDataFormSheetHelper(IRow row, DtoExcelHelper helper, List<Dictionary<int, string>> listData)
        {
            if ((row != null) && (row.RowNum >= helper.IndexRowStart))
            {
                var dicRow = new Dictionary<int, string>();

                SwitchCellType(row, helper, dicRow);

                listData.Add(dicRow);
            }
        }

        private void SwitchCellType(IRow row, DtoExcelHelper helper, Dictionary<int, string> dicRow)
        {
            foreach (int indexCol in helper.IndexColumns)
            {
                var cell = row.GetCell(indexCol);
                var cellValue = "";

                if (cell != null)
                {
                    switch (cell.CellType)
                    {
                        case CellType.Numeric:
                            if (DateUtil.IsCellDateFormatted(cell))
                            {
                                DateTime date = cell.DateCellValue;
                                cellValue = date.ToString().Substring(0, 10);
                            }
                            else
                                cellValue = cell.NumericCellValue.ToString();
                            break;
                        case CellType.String:
                            cellValue = cell.StringCellValue;
                            break;
                        case CellType.Formula:
                            if (cell.CachedFormulaResultType == CellType.String)
                            {
                                cellValue = cell.StringCellValue;
                            }
                            else if (cell.CachedFormulaResultType == CellType.Numeric)
                            {
                                cellValue = cell.NumericCellValue.ToString();
                            }
                            break;
                    }
                }

                dicRow.Add(indexCol, cellValue);
            }
        }

    }

    public class ExcelEventArgs
    {
        // Сообщение
        public string Message { get; }

        public ExcelEventArgs(string message)
        {
            Message = message;
        }
    }
   
}
