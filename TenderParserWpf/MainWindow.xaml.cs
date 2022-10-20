using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TenderParserWpf.Models.Dto;
using TenderParserWpf.Models.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using LightInject;
using TenderParserWpf.BLL.Interfaces;
using TenderParserWpf.BLL.Services;
using System.Net;

namespace TenderParserWpf
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IExcelService _iExcelService;
        private readonly IProcessService _iProcessService;
        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();

            var container = new ServiceContainer();
            container.Register<IExcelService, ExcelService>();
            container.Register<IProcessService, ProcessService>();

            _iExcelService = container.GetInstance<IExcelService>();
            _iProcessService = container.GetInstance<IProcessService>();

            WordList.Add("ПИР");
            WordList.Add("ГИР");
            WordList.Add("МИР");

            wordList.ItemsSource = WordList;
        }

        private ObservableCollection<string> WordList { get; set; } = new ObservableCollection<string>();

        private async void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            DtoCheckBoxes checkDto = new DtoCheckBoxes(checkBox1.IsChecked, checkBox2.IsChecked, checkBox3.IsChecked, checkBox4.IsChecked,
                                                       checkBox5.IsChecked, checkBoxSite1.IsChecked, checkBoxSite2.IsChecked, checkBoxSite3.IsChecked,
                                                       checkBoxSite4.IsChecked, checkBoxSite5.IsChecked, checkBoxSite6.IsChecked,
                                                       checkBoxSite7.IsChecked, checkBoxSite8.IsChecked, wordList.Text, textBoxSelectFile.Text);

            string pathCatalog = textBoxSelectFile.Text;
            string searchWord = wordList.Text;
            string[] searchDirectory = GetSearchDirections(checkDto);
            string[] sites = GetSites(checkDto);

            RequestDto dto = new RequestDto(pathCatalog, searchWord, searchDirectory, sites);

            progressBar.Value = 0;

            Action action = () => { progressBar.Value++; };
            Action actionEnd = () => { progressBar.Value = 100; };
            var task = new Task(() =>
            {
                ProgressBarHelper(progressBar, action);
            });
            task.Start();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            await Starting(checkDto, dto);

            await ProgressBarEnded(progressBar, actionEnd);

            await CreateNotify(pathCatalog);
        }

        private async Task CreateNotify(string pathCatalog)
        {
            await Task.Run(() =>
            {
                MessageBox.Show($"отчёт сформирован:{pathCatalog}" + "\\отчёт.xlsx");
            });
        }

        private async Task ProgressBarEnded(ProgressBar bar, Action action)
        {
            await Task.Run(() => bar.Dispatcher.Invoke(action));
        }

        /// <summary>
        /// создаем массив директорий поиска,которые указал пользователь
        /// </summary>
        /// <param name="checkDto"></param>
        /// <returns></returns>
        private string[] GetSearchDirections(DtoCheckBoxes checkDto)
        {
            List<string> searchDirectories = new List<string>();

            if (checkDto.CheckBox1 == true)
                searchDirectories.Add("ПИР");
            if (checkDto.CheckBox2 == true)
                searchDirectories.Add("СИПОЭ");
            if (checkDto.CheckBox3 == true)
                searchDirectories.Add("СИ по ТИТЭЭ");
            if (checkDto.CheckBox4 == true)
                searchDirectories.Add("И и ИД");
            if (checkDto.CheckBox5 == true)
                searchDirectories.Add("ГИР и ГРР");

            return searchDirectories.ToArray();
        }

        /// <summary>
        /// получение значений сайтов для поиска,которые указал пользователь
        /// </summary>
        /// <param name="checkDto"></param>
        /// <returns></returns>
        private string[] GetSites(DtoCheckBoxes checkDto)
        {
            List<string> searchSites = new List<string>();

            if (checkDto.CheckBoxSite1 == true)
                searchSites.Add("etpgpb.ru");
            if (checkDto.CheckBoxSite2 == true)
                searchSites.Add("b2b-center.ru");
            if (checkDto.CheckBoxSite3 == true)
                searchSites.Add("zakupki.gazprom-neft.ru");
            if (checkDto.CheckBoxSite4 == true)
                searchSites.Add("lukoil.ru");
            if (checkDto.CheckBoxSite5 == true)
                searchSites.Add("neftisa.ru");
            if (checkDto.CheckBoxSite6 == true)
                searchSites.Add("fabrikant.ru");
            if (checkDto.CheckBoxSite7 == true)
                searchSites.Add("etp.spbex.ru");
            if (checkDto.CheckBoxSite8 == true)
                searchSites.Add("novatek.ru");

            return searchSites.ToArray();
        }


        /// <summary>
        /// проверка на заполнение формы и запуск
        /// </summary>
        /// <param name="checkDto"></param>
        private async Task Starting(DtoCheckBoxes checkDto, RequestDto dto)
        {
            bool flagSearchDirection = CheckSearchDirection(checkDto);
            bool flagSelectedSites = CheckSelectedSites(checkDto);

            if (!flagSearchDirection)
                MessageBox.Show("Не указано направление поиска");
            else if (string.IsNullOrEmpty(checkDto.SearchWord))
                MessageBox.Show("Не указаны ключевые слова для поиска");
            else if (!flagSelectedSites)
                MessageBox.Show("Не выбраны площадки для поиска");
            else if (string.IsNullOrEmpty(checkDto.Path))
                MessageBox.Show("Не укахана папка для сохранения отчёта");
            else
                await CreateReport(dto);
        }
        private void ProgressBarHelper(ProgressBar bar, Action action)
        {
            for (var i = 0; i < 40; i++)
            {
                bar.Dispatcher.Invoke(action);
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// проверка на указание директорий поиска
        /// </summary>
        /// <param name="checkDto"></param>
        /// <returns></returns>
        private bool CheckSearchDirection(DtoCheckBoxes checkDto)
        {
            bool flag = true;

            if ((checkDto.CheckBox1 == false) && (checkDto.CheckBox2 == false) && (checkDto.CheckBox3 == false) && (checkDto.CheckBox4 == false) && (checkDto.CheckBox5 == false))
                flag = false;

            return flag;
        }

        /// <summary>
        /// проверка на указание сайтов для поиска
        /// </summary>
        /// <param name="checkDto"></param>
        /// <returns></returns>
        private bool CheckSelectedSites(DtoCheckBoxes checkDto)
        {
            bool flag = true;

            if ((checkDto.CheckBoxSite1 == false) && (checkDto.CheckBoxSite2 == false) && (checkDto.CheckBoxSite3 == false) &&
                (checkDto.CheckBoxSite4 == false) && (checkDto.CheckBoxSite5 == false) && (checkDto.CheckBoxSite6 == false) &&
                (checkDto.CheckBoxSite7 == false) && (checkDto.CheckBoxSite8 == false))
                flag = false;

            return flag;
        }


        private void buttonSelectFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();

            try
            {
                textBoxSelectFile.Text = dialog.FileName;
            }
            catch
            {
                MessageBox.Show("Не выбрана папка");
            }
        }

        public async Task CreateReport(RequestDto dto)
        {
            string path = GetTemplateFile();

            List<ProcedureTender> list = await _iProcessService.Start(dto);

            string tempFile = await Task.Run(() => _iExcelService.GetExcelResult(list, dto, path));

        }

        private string GetTemplateFile()
        {
            string fileTemplate = Path.GetTempFileName() + "отчёт.xlsx";
            File.WriteAllBytes(fileTemplate, Properties.Resources.отчет);

            return fileTemplate;
        }
    }
}
