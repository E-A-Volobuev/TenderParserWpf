using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenderParserWpf.BLL.Interfaces;
using TenderParserWpf.Models.Dto;
using TenderParserWpf.Models.Models;

namespace TenderParserWpf.BLL.Services
{
    public class ExcelService : IExcelService
    {
        /// <summary>
        /// книга,куда записываем данные
        /// </summary>
        private IWorkbook _templateWorkbook = null;

        public string GetExcelResult(List<ProcedureTender> list, RequestDto dto, string path)
        {
            var tempFile = GetTempFileName(dto.PathCatalog, path);

            using (FileStream fs = new FileStream(tempFile, FileMode.Open, FileAccess.Read))
            {
                _templateWorkbook = new XSSFWorkbook(fs);
            }

            SelectTypeByWrite(list, dto);

            using (FileStream fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
            {
                _templateWorkbook.Write(fs);
            }

            return tempFile;
        }

        private void SelectTypeByWrite(List<ProcedureTender> list, RequestDto dto)
        {
            foreach (var select in dto.Selected)
            {
                TypeRequest type = TypeRequest.PIR;

                if (select.Contains("ПИР"))
                    ExcelHelper(type, list);
                if (select.Contains("СИПОЭ"))
                {
                    type = TypeRequest.SIPOE;
                    ExcelHelper(type, list);
                }
                if (select.Contains("СИ по ТИТЭЭ"))
                {
                    type = TypeRequest.SI_PO_TITEE;
                    ExcelHelper(type, list);
                }
                if (select.Contains("И и ИД"))
                {
                    type = TypeRequest.I_I_ID;
                    ExcelHelper(type, list);
                }
                if (select.Contains("ГИР ГРР"))
                {
                    type = TypeRequest.GIR_GRR;
                    ExcelHelper(type, list);
                }

            }
        }

        private void ExcelHelper(TypeRequest type, List<ProcedureTender> list)
        {
            ISheet sheet = _templateWorkbook.GetSheetAt((int)type);
            WriteHelper(list, sheet);
        }

        private void WriteHelper(List<ProcedureTender> list, ISheet sheet)
        {
            for (int i = 1; i < list.Count + 1; i++)
            {
                IRow dataRow = sheet.GetRow(i) ?? sheet.CreateRow(i);
                CellSetVal(dataRow, list, i);
            }
        }
        private void CellSetVal(IRow dataRow, List<ProcedureTender> list, int i)
        {
            var styleNum = GetStyleForCell(TypeDateCell.NUMERIC);
            ICell cellNum = dataRow.GetCell(0) ?? dataRow.CreateCell(0);
            cellNum.SetCellValue(i);
            cellNum.CellStyle = styleNum;

            var styleDate = GetStyleForCell(TypeDateCell.DATE_PUBLIC);
            ICell cellDataPublic = dataRow.GetCell(1) ?? dataRow.CreateCell(1);
            cellDataPublic.SetCellValue(list[i - 1].DatePubliсation);
            cellDataPublic.CellStyle = styleDate;

            ICell cellNumberProcedure = dataRow.GetCell(2) ?? dataRow.CreateCell(2);
            cellNumberProcedure.SetCellValue(list[i - 1].NumberProcedure);

            ICell cellResource = dataRow.GetCell(3) ?? dataRow.CreateCell(3);
            cellResource.SetCellValue(list[i - 1].Url);

            var styleDateEnd = GetStyleForCell(TypeDateCell.DATE_END);
            ICell cellDateEnd = dataRow.GetCell(4) ?? dataRow.CreateCell(4);
            cellDateEnd.SetCellValue(list[i - 1].DateEnd);
            cellDateEnd.CellStyle = styleDateEnd;

            ICell cellProcedureName = dataRow.GetCell(5) ?? dataRow.CreateCell(5);
            cellProcedureName.SetCellValue(list[i - 1].ProcedureName);

            ICell cellCustomer = dataRow.GetCell(6) ?? dataRow.CreateCell(6);
            cellCustomer.SetCellValue(list[i - 1].Customer);
        }
        private XSSFCellStyle GetStyleForCell(TypeDateCell type)
        {
            XSSFCellStyle cellStyle = (XSSFCellStyle)_templateWorkbook.CreateCellStyle();

            if (type == TypeDateCell.NUMERIC)
                cellStyle.DataFormat = _templateWorkbook.CreateDataFormat().GetFormat("#,###");
            else if (type == TypeDateCell.DATE_END)
                cellStyle.DataFormat = _templateWorkbook.CreateDataFormat().GetFormat("dd.MM.yyyy hh:mm");
            else
                cellStyle.DataFormat = _templateWorkbook.CreateDataFormat().GetFormat("dd.MM.yyyy");

            return cellStyle;
        }

        private string GetTempFileName(string catalogPath, string fileTemplate)
        {

            string resultFileName = catalogPath + @"\отчёт.xlsx";

            FileInfo fileInf = new FileInfo(fileTemplate);
            if (fileInf.Exists)
            {
                FileInfo file = new FileInfo(resultFileName);
                if (file.Exists)
                    File.Delete(resultFileName);

                File.Copy(fileTemplate, resultFileName, true);
                File.Delete(fileTemplate);
            }

            return resultFileName;
        }
    }
    public enum TypeRequest
    {
        PIR,
        SIPOE,
        SI_PO_TITEE,
        I_I_ID,
        GIR_GRR
    }

    public enum TypeDateCell
    {
        NUMERIC,
        DATE_PUBLIC,
        DATE_END
    }
}
