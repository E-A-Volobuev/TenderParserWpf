using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TenderParserWpf.BLL.Interfaces;
using TenderParserWpf.Models.Dto;
using TenderParserWpf.Models.Models;

namespace TenderParserWpf.BLL.Services
{
    public class ProcessService : IProcessService
    {
        /// <summary>
        /// запуск парсинга
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<List<ProcedureTender>> Start(RequestDto dto)
        {
            bool isEtp = false; // флаг площадки etpgpb.ru
            bool isB2b = false;//  флаг площадки b2b-center.ru
            bool isGazProm = false;//  флаг площадки zakupki.gazprom-neft.ru

            if (dto.Sites.Contains("etpgpb.ru"))
                isEtp = true;
            if (dto.Sites.Contains("b2b-center.ru"))
                isB2b = true;
            if (dto.Sites.Contains("zakupki.gazprom-neft.ru"))
                isGazProm = true;

            List<ProcedureTender> tenderList = new List<ProcedureTender>();
            await SiteRead(isEtp, isB2b, isGazProm, dto, tenderList);


            return tenderList;
        }


        /// <summary>
        /// парсим сайт по его наличию
        /// </summary>
        /// <param name="isEtp"></param>
        /// <param name="isB2b"></param>
        /// <param name="dto"></param>
        /// <param name="tenderList"></param>
        /// <returns></returns>
        private async Task SiteRead(bool isEtp, bool isB2b, bool isGazProm, RequestDto dto, List<ProcedureTender> tenderList)
        {
            if (isEtp)
            {
                List<ProcedureTender> listOne = await GetRequest(dto.Search); ///для площадки etpgpb.ru
                tenderList.AddRange(listOne);
            }

            if (isB2b)
            {
                List<ProcedureTender> listTwo = await HtmlAgilityPack(dto.Search); ///для площадки www.b2b-center.ru
                tenderList.AddRange(listTwo);
            }

            if (isGazProm)
            {
                List<ProcedureTender> listThree = await HtmlAgilityPackByGazProm(dto.Search); ///для площадки www.zakupki.gazprom-neft.ru
                tenderList.AddRange(listThree);
            }
        }
        #region html agility pack by zakupki.gazprom-neft.ru

        private async Task<List<ProcedureTender>> HtmlAgilityPackByGazProm(string text)
        {
            string baseUrl = $"https://zakupki.gazprom-neft.ru";
            string url = $"https://zakupki.gazprom-neft.ru/tenderix/?FILTER[SEARCH]=" + text + "&LIMIT=100";

            List<ProcedureTender> list = await ReadAllPagesByGazprom(url, baseUrl, text);

            return list;
        }

        /// <summary>
        /// находим таблицу с результатами по запросу
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="baseUrl"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private async Task<List<ProcedureTender>> ReadAllPagesByGazprom(string url, string baseUrl, string text)
        {
            List<ProcedureTender> list = new List<ProcedureTender>();

            HtmlDocument doc = await LoadDocHelper(url);

            string html = doc.ParsedText;

            var div = doc.DocumentNode.SelectSingleNode("//div[@class='purchases-list']");

            if (div != null)
            {
                var links = div.Descendants("a").Where(x => x.InnerHtml.Contains("№")).ToList();

                foreach (var obj in links)
                {
                    try
                    {
                        ProcedureTender tender = await FindAllLinksByGazprom(obj, baseUrl);
                        if (tender != null)
                            list.Add(tender);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// получаем url каждой ссылки (каждого результата поиска)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        private async Task<ProcedureTender> FindAllLinksByGazprom(HtmlNode obj, string baseUrl)
        {
            var urlLink = obj.SelectSingleNode(".").Attributes["href"].Value;
            string currentLink = baseUrl + urlLink;

            HtmlDocument doc = await LoadDocHelper(currentLink);

            return FindParametrByLinkByGazprom(doc, currentLink);
        }


        /// <summary>
        /// считываем значения с карточки объявления
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="currentLink"></param>
        /// <returns></returns>
        private ProcedureTender FindParametrByLinkByGazprom(HtmlDocument doc, string currentLink)
        {
            ProcedureTender tender = new ProcedureTender();

            SetNameTender(tender, doc);
            SetDateAndCustomer(tender, doc);
            SetNumberTender(tender, doc);

            tender.Url = currentLink;


            return tender;
        }
        /// <summary>
        /// получение названия тендера
        /// </summary>
        /// <param name="tender"></param>
        /// <param name="doc"></param>
        private void SetNameTender(ProcedureTender tender, HtmlDocument doc)
        {
            var div = doc.DocumentNode.SelectSingleNode(".//div[@class='info-title tender-desktop']");
            string name = div.InnerText.Replace("\n", string.Empty).Replace("\t", string.Empty);

            tender.ProcedureName = name;

        }

        /// <summary>
        /// получение дат и заказчика
        /// </summary>
        /// <param name="tender"></param>
        /// <param name="doc"></param>
        private void SetDateAndCustomer(ProcedureTender tender, HtmlDocument doc)
        {
            var divs = doc.DocumentNode.SelectNodes(".//div[@class='col-sm-12 col-md-6']");

            foreach (var div in divs)
            {
                if (div.InnerText.Contains("Дата начала приема"))
                    tender.DatePubliсation = GetDate(div);
                else if (div.InnerText.Contains("Дата окончания приема"))
                    tender.DateEnd = GetDate(div);
                else if (div.InnerText.Contains("Организатор"))
                    tender.Customer = GetCustomer(div);
            }
        }

        /// <summary>
        /// получение номера тендера
        /// </summary>
        /// <param name="tender"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private void SetNumberTender(ProcedureTender tender, HtmlDocument doc)
        {
            var numberTenderDiv = doc.DocumentNode.SelectSingleNode(".//div[@class='info-number tender-desktop']");
            string number = numberTenderDiv.InnerText.Replace(" ", "").Replace("\n", string.Empty).Replace("\t", string.Empty);

            tender.NumberProcedure = number;
        }

        /// <summary>
        /// получение заказчика тендера
        /// </summary>
        /// <param name="div"></param>
        /// <returns></returns>
        private string GetCustomer(HtmlNode div)
        {
            var obj = div.SelectSingleNode(".//div[@class='text']");
            string text = obj.InnerText.Replace(" ", "").Replace("\n", string.Empty).Replace("\t", string.Empty);

            return text;
        }

        /// <summary>
        /// получение дат начала и окончания
        /// </summary>
        /// <param name="div"></param>
        /// <returns></returns>
        private DateTime GetDate(HtmlNode div)
        {
            var obj = div.SelectSingleNode(".//div[@class='text']");

            string dateString = "";
            string text = obj.InnerText.Replace(" ", "");

            string result = "";

            if (text.Contains('\n'))
                result = text.Substring(1, text.Length - 1);
            else
                result = text;

            if (result.Contains(','))
            {
                int index = result.IndexOf(',');
                if (index > 0)
                    dateString = result.Substring(0, index);
            }
            else
                dateString = result;

            DateTime dateTime = ConverterHelperDate(dateString);

            return dateTime;
        }

        #endregion

        #region html agility pack by b2b-center.ru

        private async Task<List<ProcedureTender>> HtmlAgilityPack(string text)
        {
            string baseUrl = $"https://www.b2b-center.ru";
            string url = $"https://www.b2b-center.ru/market/?f_keyword=" + text + "&searching=1&company_type=2&price_currency=0&date=1&trade=all&from=" + "0" + "#search-result";

            List<ProcedureTender> list = await ReadAllPages(url, baseUrl, text);

            return list;
        }

        /// <summary>
        /// находим таблицу с результатами по запросу
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="baseUrl"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private async Task<List<ProcedureTender>> ReadAllPages(string url, string baseUrl, string text)
        {
            HtmlDocument doc = await LoadDocHelper(url);

            HtmlNode countAd = doc.DocumentNode.SelectSingleNode("//a[@class='active btn btn-default']");

            int countActual = GetNumberAd(countAd); ///количество актуальных объявлений

            double count = 0; //количество объявлений на странице (для вывода в запрос)

            List<ProcedureTender> list = new List<ProcedureTender>();

            await ReadAllHelper(countActual, url, baseUrl, count, list, text);

            return list;
        }
        /// <summary>
        /// считываем актуальное объявление и открывем карточку этого объявления
        /// </summary>
        /// <param name="countActual"></param>
        /// <param name="url"></param>
        /// <param name="baseUrl"></param>
        /// <param name="count"></param>
        /// <param name="list"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private async Task ReadAllHelper(int countActual, string url, string baseUrl, double count, List<ProcedureTender> list, string text)
        {
            if ((countActual / 20 > 0) && (countActual / 20 < 1) || (countActual / 20 == 1) || (countActual / 20 == 0))
                await HtmlReadHelper(url, baseUrl, list);
            else
            {
                count = Convert.ToDouble(countActual / 20);
                int result = Convert.ToInt32(Math.Round(count));

                for (int i = 1; i < result; i++)
                {
                    int num = 20 * i; //на этом сайте вывод объявлений всегда кратен 20
                    string currentUrl = $"https://www.b2b-center.ru/market/?f_keyword=" + text + "&searching=1&company_type=2&price_currency=0&date=1&trade=all&from=" + num + "#search-result";

                    await HtmlReadHelper(currentUrl, baseUrl, list);
                }
            }
        }
        /// <summary>
        /// получаем количество актуальных объявлений
        /// </summary>
        /// <returns></returns>
        private int GetNumberAd(HtmlNode countAd)
        {
            if (countAd != null)
            {
                string count = countAd.InnerText;
                if (count.Contains("Актуально &bull; "))
                {
                    count = count.Replace("Актуально &bull; ", "");

                    try
                    {
                        int num = Convert.ToInt32(count);
                        return num;
                    }
                    catch
                    {
                        return 0;
                    }
                }
                return 0;
            }
            else
                return 0;
        }

        /// <summary>
        /// преобразуем скачанную страницу в json
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task<HtmlDocument> LoadDocHelper(string url)
        {
            HttpWebResponse response = await GetRersponceAsync(url);
            var result = GetJsonResponce(response);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(result);

            return doc;
        }

        private async Task HtmlReadHelper(string url, string baseUrl, List<ProcedureTender> list)
        {
            HtmlDocument doc = await LoadDocHelper(url);

            HtmlNode table = doc.DocumentNode.SelectSingleNode("//table[@class='table table-hover table-filled search-results']");

            await FindBySite(table, baseUrl, list);
        }
        private async Task FindBySite(HtmlNode table, string baseUrl, List<ProcedureTender> list)
        {
            if (table != null)
            {
                var links = table.Descendants("a").Where(x => x.InnerHtml.Contains("search-results-title-desc")).ToList();
                foreach (var obj in links)
                {
                    try
                    {
                        ProcedureTender tender = await FindAllLinks(obj, baseUrl);
                        if (tender != null)
                            list.Add(tender);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        /// <summary>
        /// получаем url каждой ссылки (каждого результата поиска)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        private async Task<ProcedureTender> FindAllLinks(HtmlNode obj, string baseUrl)
        {
            var urlLink = obj.SelectSingleNode(".").Attributes["href"].Value;
            string currentLink = baseUrl + urlLink;

            HtmlDocument doc = await LoadDocHelper(currentLink);

            return FindParametrByLink(doc, currentLink);
        }

        /// <summary>
        /// считываем значения с карточки объявления
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="currentLink"></param>
        /// <returns></returns>
        private ProcedureTender FindParametrByLink(HtmlDocument doc, string currentLink)
        {
            ProcedureTender tender = new ProcedureTender();

            HtmlNode trNumber = doc.DocumentNode.SelectSingleNode("//h1[@class='h3']");
            tender.NumberProcedure = GetNumberProcedure(trNumber);

            HtmlNode trDatePublic = doc.DocumentNode.SelectSingleNode("//tr[@id='trade_info_date_begin']");
            DateTime datePublic = GetDatePublic(trDatePublic);
            tender.DatePubliсation = datePublic;

            HtmlNode trDateEnd = doc.DocumentNode.SelectSingleNode("//tr[@id='trade_info_date_end']");
            DateTime dateEnd = GetDatePublic(trDateEnd);
            tender.DateEnd = dateEnd;

            tender.Url = currentLink;

            HtmlNode trName = doc.DocumentNode.SelectSingleNode("//title");
            tender.ProcedureName = trName.InnerText;

            HtmlNode trCustomer = doc.DocumentNode.SelectSingleNode("//tr[@id='trade-info-organizer-name']");
            tender.Customer = GetNameOwner(trCustomer);

            HtmlNode trPrice = doc.DocumentNode.SelectSingleNode("//tr[@id='trade-info-lot-price']");
            tender.Price = GetPrice(trPrice);


            return tender;
        }

        private string GetPrice(HtmlNode trPrice)
        {
            if (trPrice == null)
                return "Без указания цены";
            else
            {
                string price = trPrice.InnerText;
                if (price.Contains("Общая стоимость закупки:"))
                    price = price.Replace("Общая стоимость закупки:", "");

                return price;
            }
        }

        private string GetNameOwner(HtmlNode trCustomer)
        {
            if (trCustomer != null)
            {
                string name = trCustomer.InnerText;
                if (name.Contains("&quot;"))
                    name = name.Replace("&quot;", "\"");

                return name;
            }
            else
            {
                return "Без названия";
            }



        }
        /// <summary>
        /// номер закупки
        /// </summary>
        /// <param name="trNumber"></param>
        /// <returns></returns>
        private string GetNumberProcedure(HtmlNode trNumber)
        {
            if (trNumber != null)
            {
                string numberPublic = trNumber.InnerText;
                int index = numberPublic.IndexOf('№') + 1;
                int indexEnd = numberPublic.IndexOf("&nbsp");

                numberPublic = numberPublic.Substring(index, indexEnd - index);

                return numberPublic;
            }
            else
            {
                return "номер не указан";
            }
        }

        /// <summary>
        /// преобразуем считанное значение в дату публикации
        /// </summary>
        /// <param name="tr"></param>
        /// <returns></returns>
        private DateTime GetDatePublic(HtmlNode tr)
        {
            if (tr != null)
            {
                string datePublic = tr.InnerText;
                int index = datePublic.IndexOf(':') + 1;
                datePublic = datePublic.Substring(index, datePublic.Length - index);

                DateTime time = ConverterHelperDate(datePublic);
                return time;
            }
            else
            {
                DateTime time = new DateTime();
                return time;
            }
        }

        private DateTime ConverterHelperDate(string date)
        {
            try
            {
                if (date.Contains('('))
                {
                    int index = date.IndexOf('(');
                    date = date.Substring(0, index - 1);
                }

                DateTime time = Convert.ToDateTime(date);
                return time;

            }
            catch
            {
                DateTime time = new DateTime();
                return time;
            }
        }

        #endregion


        private async Task<HttpWebResponse> GetRersponceAsync(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            IWebProxy proxy = WebRequest.GetSystemWebProxy();
            proxy.Credentials = CredentialCache.DefaultCredentials;
            request.Proxy = proxy;
            request.CookieContainer = new CookieContainer();

            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();

            return response;
        }
        #region json parse

        private async Task<List<ProcedureTender>> GetRequest(string text)
        {
            string url = $"https://etpgpb.ru/procedures.json/?page=1&per=1000&search=" + text;
            string baseUrl = $"https://etpgpb.ru";

            HttpWebResponse response = await GetRersponceAsync(url);

            var result = GetJsonResponce(response);

            List<ProcedureTender> list = CreateTenderList(result, baseUrl);

            return list;
        }

        /// <summary>
        /// парсим ответ json 
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private string GetJsonResponce(HttpWebResponse response)
        {
            string jsonResponce = "";

            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    jsonResponce = reader.ReadToEnd();
                }
            }
            response.Close();

            return jsonResponce;

        }

        private List<ProcedureTender> CreateTenderList(string result, string baseUrl)
        {
            List<ProcedureTender> tenderList = new List<ProcedureTender>();

            dynamic obj = JObject.Parse(result);

            foreach (dynamic item in obj.procedures)
            {
                ProcedureTender tender = new ProcedureTender();

                tender.NumberProcedure = item.registry_number;
                tender.DatePubliсation = item.created_at;
                tender.Url = baseUrl + item.truncated_path;
                tender.DateEnd = item.end_registration;
                tender.ProcedureName = item.title;
                tender.Customer = item.company_name;
                tender.Price = item.amount;

                if (tender != null)
                {
                    if (tender.DateEnd > DateTime.Now)
                        tenderList.Add(tender);
                }
            }

            return tenderList;
        }

        #endregion
    }
}
