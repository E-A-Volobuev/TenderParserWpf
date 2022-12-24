using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
            bool isNeftisa = false;//  флаг площадки neftisa.ru
            bool isFabrikant = false;//  флаг площадки fabrikant.ru

            FlagHelper(ref isEtp, ref isB2b, ref isGazProm, ref isNeftisa, ref isFabrikant, dto);

            List<ProcedureTender> tenderList = new List<ProcedureTender>();

            await SiteRead(isEtp, isB2b, isGazProm, isNeftisa, isFabrikant,  dto, tenderList);

            return tenderList;
        }

        private void FlagHelper(ref bool isEtp, ref bool isB2b, ref bool isGazProm, ref bool isNeftisa, ref bool isFabrikant, RequestDto dto)
        {
            if (dto.Sites.Contains("etpgpb.ru"))
                isEtp = true;
            if (dto.Sites.Contains("b2b-center.ru"))
                isB2b = true;
            if (dto.Sites.Contains("zakupki.gazprom-neft.ru"))
                isGazProm = true;
            if (dto.Sites.Contains("neftisa.ru"))
                isNeftisa = true;
            if (dto.Sites.Contains("fabrikant.ru"))
                isFabrikant = true;
        }


        /// <summary>
        /// /парсим сайт по его наличию
        /// </summary>
        /// <param name="isEtp"></param>
        /// <param name="isB2b"></param>
        /// <param name="isGazProm"></param>
        /// <param name="isNeftisa"></param>
        /// <param name="isFabrikant"></param>
        /// <param name="dto"></param>
        /// <param name="tenderList"></param>
        /// <returns></returns>
        private async Task SiteRead(bool isEtp, bool isB2b, bool isGazProm, bool isNeftisa, bool isFabrikant, RequestDto dto, List<ProcedureTender> tenderList)
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

            if (isNeftisa)
            {
                List<ProcedureTender> listFour = await HtmlAgilityPackByNeftisa(dto.Search); ///для площадки www.neftisa.ru
                tenderList.AddRange(listFour);
            }

            if (isFabrikant)
            {
                List<ProcedureTender> listFive = await HtmlAgilityPackByFabrikant(dto.Search); ///для площадки www.fabrikant.ru
                tenderList.AddRange(listFive);
            }

        }
        #region html agility pack by fabrikant.ru
        private async Task<List<ProcedureTender>> HtmlAgilityPackByFabrikant(string text)
        {
            string code = HttpUtility.UrlEncode(text).ToUpper();

            List<ProcedureTender> list = new List<ProcedureTender>();

            if (code.Length > 2) //для этого сайта минимальное количество символов для поиска это 2 шт.
            {
                string baseUrl = $"https://www.fabrikant.ru";
                string url = $"https://www.fabrikant.ru/trades/procedure/search/?query=" + code + $"&type=0&org_type=org&currency=0&date_type=date_publication&ensure=all&okpd2_embedded=1&okdp_embedded=1&count_on_page=40&order_direction=1&type_hash=1561441166";

                list = await PaginationHelperByFabrikant(baseUrl, url, text, code);
            }

            return list;
        }
        /// <summary>
        /// проходим по каждой странице результата поиска (если на сайте есть пагинация)
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="url"></param>
        /// <param name="text"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        private async Task<List<ProcedureTender>> PaginationHelperByFabrikant(string baseUrl, string url, string text, string code)
        {
            List<ProcedureTender> list = new List<ProcedureTender>();

            HtmlDocument doc = await LoadDocHelper(url);

            var div = doc.DocumentNode.SelectSingleNode("//ul[@class='pagination__lt']");

            await PaginationFabrikant(text, list, doc, div,code);

            var resultList = list.Distinct().ToList();

            return resultList;
        }

        private async Task PaginationFabrikant(string text, List<ProcedureTender> list, HtmlDocument doc, HtmlNode div,string code)
        {
            if (div != null)
                await SearchLinksByPage(text, list, doc,code);
            else
            {
                var tenders = await ReadAllPagesByFabrikant(doc, text, list);
                list.AddRange(tenders);
            }
        }

        private async Task SearchLinksByPage(string text, List<ProcedureTender> list, HtmlDocument doc, string code)
        {
            var links = doc.DocumentNode.SelectNodes("//a[@class='pagination__lt__ref pagination__link']");

            if (links != null)
            {
                for (int i = 1; i < links.Count() + 1; i++)
                {
                    string urlPagin = $"https://www.fabrikant.ru/trades/procedure/search/?type=0&procedure_stage=0&price_from=&price_to=&currency=0&date_type=date_publication&date_from=&date_to=&ensure=all&count_on_page=40&order_direction=1&type_hash=1561441166&query=" + code + $"&page=" + i;

                    HtmlDocument newDoc = await LoadDocHelper(urlPagin);

                    var tendersFromPage = await ReadAllPagesByFabrikant(newDoc, text, list);
                    list.AddRange(tendersFromPage);
                }
            }
        }

        private async Task<List<ProcedureTender>> ReadAllPagesByFabrikant(HtmlDocument doc, string text, List<ProcedureTender> list)
        {
            var headers = doc.DocumentNode.SelectNodes("//h4[@class='marketplace-unit__title']");
            var tenderNumbers= doc.DocumentNode.SelectNodes("//div[@class='marketplace-unit__info__name']");

            if (headers != null)
                await ReadPageByFabrikant( list, headers, tenderNumbers);

            return list;
        }

        private async Task ReadPageByFabrikant(List<ProcedureTender> list,HtmlNodeCollection headers, HtmlNodeCollection tenderNumbers)
        {
            for (int i = 0; i < headers.Count(); i++)
            {
                var url = headers.ElementAt(i).ChildNodes.Where(x => x.Name == "a").FirstOrDefault().Attributes["href"].Value;
                string tenderNumber = GetNumberTenderByFabrikant(tenderNumbers.ElementAt(i).InnerText, i);

                if (!string.IsNullOrEmpty(url))
                    await GetObjectByFabrikant(url, tenderNumber, list);
            }
        }

        private async Task GetObjectByFabrikant(string url, string tenderNumber, List<ProcedureTender> list)
        {
            try
            {
                HtmlDocument newDoc = GetHtml(url);

                ProcedureTender tender = await Task.Run(() => FindParametrByLinkByFabrikant(newDoc, url));
                tender.NumberProcedure = tenderNumber;
                if (tender != null)
                    list.Add(tender);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private HtmlDocument GetHtml(string url)
        {
            HtmlDocument doc = new HtmlDocument();
            StreamReader reader = new StreamReader(WebRequest.Create(url).GetResponse().GetResponseStream(), Encoding.Default);
            doc.Load(reader);

            return doc;
        }

        /// <summary>
        /// считываем значения с карточки объявления
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="currentLink"></param>
        /// <returns></returns>
        private ProcedureTender FindParametrByLinkByFabrikant(HtmlDocument doc, string currentLink)
        {
            ProcedureTender tender = new ProcedureTender();
            GetValuesByFabrikant(doc, tender);

            tender.Url = currentLink;


            return tender;
        }

        private void GetValuesByFabrikant(HtmlDocument doc, ProcedureTender tender)
        {
            var tables = doc.DocumentNode.SelectNodes("//table[@class='blank']");

            if (tables != null)
            {
                var table = tables.FirstOrDefault();

                if (table != null)
                {
                    var body = table.SelectNodes("//tr[@class='c1']");
                    var twoBody = table.SelectNodes("//tr[@class='c2']");

                    tender.Customer = ValueStringHelper(body, twoBody, "Организатор:");
                    tender.ProcedureName = ValueStringHelper(body, twoBody, "Общее наименование закупки:");
                    string dateStart = ValueStringHelper(body, twoBody, "Начало подачи заявок:");

                    string datePublic = ValueStringHelper(body, twoBody, "Дата публикации:");

                    if (!string.IsNullOrEmpty(datePublic))
                        tender.DatePubliсation = Convert.ToDateTime(datePublic);
                    else
                        tender.DatePubliсation = Convert.ToDateTime(dateStart);

                    string dateEnd = ValueStringHelper(body, twoBody, "Окончание подачи заявок:");

                    if(!string.IsNullOrEmpty(dateEnd))
                         tender.DateEnd = Convert.ToDateTime(dateEnd);

                    tender.Price = ValueStringHelper(body, twoBody, "Общая цена:");
                }
            }
            
        }

        private string ValueStringHelper(HtmlNodeCollection nodesOne, HtmlNodeCollection nodesTwo, string text)
        {
            string result = string.Empty;

            if (nodesOne.FirstOrDefault(x => x.InnerText.Contains(text)) != null)
                result = CheckValues(nodesOne, text);

            if (nodesTwo.FirstOrDefault(x => x.InnerText.Contains(text)) != null)
                result = CheckValues(nodesTwo, text);

            return result;
        }

        private string CheckValues(HtmlNodeCollection nodes,string text)
        {
            string val = nodes.FirstOrDefault(x => x.InnerText.Contains(text)).InnerText;
            string result = val.Replace("\n", string.Empty).Replace("\t", string.Empty).Replace(text, "").Replace("&quot;","");

            return result;
        }

        private string GetNumberTenderByFabrikant(string text,int i)
        {
            string result = "";

            if (!string.IsNullOrEmpty(text))
            {
                result = text.Replace("\n", string.Empty).Replace("\t", string.Empty).Replace(" ", "").Replace("&quot;", "").Replace("&nbsp","");
                int index = result.IndexOf("№");
                int indexEnd = result.IndexOf(";");

                if((indexEnd>0) && (indexEnd>index))
                    result = result.Substring(index, result.Length-indexEnd-1).Replace(";","");
                else
                    result = result.Substring(index, result.Length - index);
            }
              

            return result;
        }

        #endregion


        #region html agility pack by neftisa.ru
        private async Task<List<ProcedureTender>> HtmlAgilityPackByNeftisa(string text)
        {
            string code = HttpUtility.UrlEncode(text).ToUpper();

            string baseUrl = $"https://neftisa.ru";
            string url = $"https://neftisa.ru/tenders/?set_filter=Y&is_ajax=y&arrFilterTenders_ff%5BNAME%5D=" + code;

            List<ProcedureTender> list = await PaginationHelper(baseUrl, url, text, code);

            return list;
        }

        /// <summary>
        /// находим таблицу с результатами по запросу
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="baseUrl"></param>
        /// <param name="text"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private async Task<List<ProcedureTender>> ReadAllPagesByNeftisa(HtmlDocument doc, string baseUrl, string text, List<ProcedureTender> list)
        {

            string html = doc.ParsedText;

            var div = doc.DocumentNode.SelectSingleNode("//div[@class='tenders__items']");

            if (div != null)
                await GetTendersByNeftisa(baseUrl, doc,  list);

            return list;
        }

        private async Task GetTendersByNeftisa(string baseUrl, HtmlDocument doc, List<ProcedureTender> list)
        {
            var links = doc.DocumentNode.SelectNodes("//a[@class='tenders__item-title']"); ;

            if (links != null)
            {
                foreach (var obj in links)
                {
                    try
                    {
                        ProcedureTender tender = await FindAllLinksByNeftisa(obj, baseUrl);
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
        private async Task<ProcedureTender> FindAllLinksByNeftisa(HtmlNode obj, string baseUrl)
        {
            var urlLink = obj.SelectSingleNode(".").Attributes["href"].Value;
            string currentLink = baseUrl + urlLink;

            HtmlDocument doc = await LoadDocHelper(currentLink);

            return FindParametrByLinkByNeftisa(doc, currentLink);
        }


        /// <summary>
        /// считываем значения с карточки объявления
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="currentLink"></param>
        /// <returns></returns>
        private ProcedureTender FindParametrByLinkByNeftisa(HtmlDocument doc, string currentLink)
        {
            ProcedureTender tender = new ProcedureTender();

            SetNameNeftisa(tender, doc);
            SetDateAndCustomerByNeftisa(tender, doc);

            tender.Url = currentLink;


            return tender;
        }


        /// <summary>
        /// получение названия тендера
        /// </summary>
        /// <param name="tender"></param>
        /// <param name="doc"></param>
        private void SetNameNeftisa(ProcedureTender tender, HtmlDocument doc)
        {
            var div = doc.DocumentNode.SelectSingleNode(".//h1[@class='page-title__text page-title__text--medium']");
            string name = div.InnerText.Replace("\n", string.Empty).Replace("\t", string.Empty);

            tender.ProcedureName = name;

        }

        /// <summary>
        /// получение дат и заказчика
        /// </summary>
        /// <param name="tender"></param>
        /// <param name="doc"></param>
        private void SetDateAndCustomerByNeftisa(ProcedureTender tender, HtmlDocument doc)
        {
            var div = doc.DocumentNode.SelectSingleNode(".//div[@class='tender-detail__info']");

            foreach (var childNode in div.ChildNodes)
            {
                if (childNode.InnerText.Contains("Организатор"))
                    tender.Customer = GetCustomerOrNumberByNeftisa(childNode);
                if (childNode.InnerText.Contains("Прием заявок до"))
                    tender.DateEnd = GetDateByNeftisa(childNode);
                if (childNode.InnerText.Contains("Номер тендера"))
                    tender.NumberProcedure = GetCustomerOrNumberByNeftisa(childNode);
            }

            tender.DatePubliсation = null; //на этой торговой площадке не указаны даты публикации тендера
            tender.Price = null; //на этой торговой площадке не указаны цены тендера
        }

        /// <summary>
        /// получение заказчика тендера
        /// </summary>
        /// <param name="childNode"></param>
        /// <returns></returns>
        private string GetCustomerOrNumberByNeftisa(HtmlNode childNode)
        {
            var obj = childNode.SelectSingleNode(".//p[@class='tender-detail__prop-value']");
            string text = obj.InnerText.Replace(" ", "").Replace("\n", string.Empty).Replace("\t", string.Empty);

            return text;
        }

        /// <summary>
        /// получение дат начала и окончания
        /// </summary>
        /// <param name=" childNode"></param>
        /// <returns></returns>
        private DateTime GetDateByNeftisa(HtmlNode childNode)
        {
            var obj = childNode.SelectSingleNode(".//p[@class='tender-detail__date-value']");

            string text = obj.InnerText.Replace(" ", "");
            DateTime dateTime = ConverterHelperDate(text);

            return dateTime;
        }


        private async Task<List<ProcedureTender>> PaginationHelper(string baseUrl, string url, string text, string code)
        {
            List<ProcedureTender> list = new List<ProcedureTender>();

            HtmlDocument doc = await LoadDocHelper(url);

            var div = doc.DocumentNode.SelectSingleNode("//div[@class='pagination pagination--margin']");

            if (div != null)
            {
                var links = doc.DocumentNode.SelectNodes("//a[@class='pagination__item-link js-pag-ajax']");

                if (links != null)
                    await GetObjectBySelectLink(links, baseUrl, text,  code,  list);
            }
            else
                await ReadAllPagesByNeftisa(doc, baseUrl, text, list);

            return list;
        }

        private async Task GetObjectBySelectLink(HtmlNodeCollection links,string baseUrl, string text, string code,List<ProcedureTender> list)
        {
            int i = 1;
            foreach (var obj in links)
            {
                string urlPagin = $"https://neftisa.ru/tenders/?set_filter=Y&is_ajax=y&arrFilterTenders_ff%5BNAME%5D=" + code + $"&arrFilterTenders_pf%5BORGANIZER%5D=&arrFilterTenders_pf%5BSUBJECT%5D=&arrFilterTenders_pf%5BREGION%5D=&arrFilterTenders_pf%5BTYPE%5D=&STAGE=132&PAGEN_1=" + i + $"&is_ajax=y";

                i++;

                HtmlDocument newDoc = await LoadDocHelper(urlPagin);

                await ReadAllPagesByNeftisa(newDoc, baseUrl, text, list);

            }
        }

        #endregion



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
                await GazpromHelper(div, baseUrl, list);

            return list;
        }
        /// <summary>
        /// получение объекта из html кода страницы сайта газпрома
        /// </summary>
        /// <param name="div"></param>
        /// <param name="baseUrl"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private async Task GazpromHelper(HtmlNode div,string baseUrl, List<ProcedureTender> list)
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
            string name = div.InnerText.Replace("\n", string.Empty).Replace("\t", string.Empty).Replace("&quot;","");

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
            string number = numberTenderDiv.InnerText.Replace(" ", "").Replace("\n", string.Empty).Replace("\t", string.Empty).Replace("&quot;", "");

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
            string text = obj.InnerText.Replace(" ", "").Replace("\n", string.Empty).Replace("\t", string.Empty).Replace("&quot;", "");

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
                await ReadByB2b(countActual, baseUrl,  count,  list,  text);
        }

        /// <summary>
        /// если есть пагинация на странице
        /// </summary>
        /// <param name="countActual"></param>
        /// <param name="baseUrl"></param>
        /// <param name="count"></param>
        /// <param name="list"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private async Task ReadByB2b(int countActual, string baseUrl, double count, List<ProcedureTender> list, string text)
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
                foreach (var link in links)
                {
                    await FindBySiteHelper(baseUrl,list,link);
                }
            }
        }
        /// <summary>
        /// создаем объект тендера из объектов кода страницы
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="list"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private async Task FindBySiteHelper(string baseUrl, List<ProcedureTender> list, HtmlNode node)
        {
            try
            {
                ProcedureTender tender = await FindAllLinks(node, baseUrl);
                if (tender != null)
                    list.Add(tender);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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

            string currentLink = "";

            if (urlLink.Contains("https"))
                currentLink = urlLink;
            else
                currentLink = baseUrl + urlLink;

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

            //IWebProxy proxy = WebRequest.GetSystemWebProxy();
            //proxy.Credentials = CredentialCache.DefaultCredentials;
            //request.Proxy = proxy;
            request.CookieContainer = new CookieContainer();
            request.Method = "GET";
            //request.CookieContainer = new CookieContainer();
            //request.AllowAutoRedirect = false;

            HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync();

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
                ProcedureTender tender = GetTenderObject(item,baseUrl);

                if (tender != null)
                {
                    if (tender.DateEnd > DateTime.Now)
                        tenderList.Add(tender);
                }
            }

            return tenderList;
        }


        private ProcedureTender GetTenderObject(dynamic item, string baseUrl)
        {
            ProcedureTender tender = new ProcedureTender();

            tender.NumberProcedure = item.registry_number;
            tender.DatePubliсation = item.created_at;
            tender.Url = baseUrl + item.truncated_path;
            tender.DateEnd = item.end_registration;
            tender.ProcedureName = item.title;
            tender.Customer = item.company_name;
            tender.Price = item.amount;

            return tender;
        }

        #endregion
    }
}
