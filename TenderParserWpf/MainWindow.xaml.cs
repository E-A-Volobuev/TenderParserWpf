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
using TenderParserWpf.BLL.Repositories;
using System.Linq;
using System.Data.SQLite;
using System.Data;

namespace TenderParserWpf
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IExcelService _iExcelService;
        private readonly IProcessService _iProcessService;
        private readonly IWordByPirRepository _iWordRepositoryByPir;
        private readonly IWordBySipoeRepository _iWordRepositoryBySipoe;
        private readonly IWordBySiPoTiteeRepository _iWordRepositoryBySiPoTitee;
        private readonly IWordByIiIdRepository _iWordRepositoryByIiId;
        private readonly IWordByGirIGrrRepository _iWordRepositoryByGirIGrr;

        private SQLiteConnection m_dbConn;
        private SQLiteCommand m_sqlCmd;
        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();

            var container = new ServiceContainer();
            container.Register<IExcelService, ExcelService>();
            container.Register<IProcessService, ProcessService>();
            container.Register<IWordByPirRepository, WordByPirRepository>();
            container.Register<IWordBySipoeRepository, WordBySipoeRepository>();
            container.Register<IWordBySiPoTiteeRepository, WordBySiPoTiteeRepository>();
            container.Register<IWordByIiIdRepository, WordByIiIdRepository>();
            container.Register<IWordByGirIGrrRepository, WordByGirIGrrRepository>();

            _iExcelService = container.GetInstance<IExcelService>();
            _iProcessService = container.GetInstance<IProcessService>();
            _iWordRepositoryByPir = container.GetInstance<IWordByPirRepository>();
            _iWordRepositoryBySipoe = container.GetInstance<IWordBySipoeRepository>();
            _iWordRepositoryBySiPoTitee = container.GetInstance<IWordBySiPoTiteeRepository>();
            _iWordRepositoryByIiId = container.GetInstance<IWordByIiIdRepository>();
            _iWordRepositoryByGirIGrr = container.GetInstance<IWordByGirIGrrRepository>();


            m_dbConn = new SQLiteConnection();
            m_sqlCmd = new SQLiteCommand();


            //wordList.ItemsSource = WordList;
        }

        private ObservableCollection<string> WordList { get; set; } = new ObservableCollection<string>();

        private  async void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            DtoCheckBoxes checkDto = new DtoCheckBoxes(checkBox1.IsChecked, checkBox2.IsChecked, checkBox3.IsChecked, checkBox4.IsChecked,
                                                       checkBox5.IsChecked, checkBoxSite1.IsChecked, checkBoxSite2.IsChecked, checkBoxSite3.IsChecked,
                                                       checkBoxSite5.IsChecked, checkBoxSite6.IsChecked,
                                                        wordList.Text, textBoxSelectFile.Text);

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
            if (checkDto.CheckBoxSite5 == true)
                searchSites.Add("neftisa.ru");
            if (checkDto.CheckBoxSite6 == true)
                searchSites.Add("fabrikant.ru");

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
                (checkDto.CheckBoxSite5 == false) && (checkDto.CheckBoxSite6 == false))
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

        private void link_Click(object sender, RoutedEventArgs e)
        {
            WordEditView view = new WordEditView();
            view.Show();

            this.Close();
        }

        private async void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var words = await _iWordRepositoryByPir.Get();
                if (words != null)
                    AddWordHelper(words.Select(x => x.Value).ToList());

                wordList.ItemsSource = WordList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private async void checkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            var words = await _iWordRepositoryByPir.Get();
            if (words != null)
                RemovedWordHelper(words.Select(x => x.Value).ToList());

            wordList.ItemsSource = WordList;
        }

        private async void checkBox2_Checked(object sender, RoutedEventArgs e)
        {
            var words = await _iWordRepositoryBySipoe.Get();
            if (words != null)
                AddWordHelper(words.Select(x => x.Value).ToList());

            wordList.ItemsSource = WordList;
        }

        private async void checkBox2_Unchecked(object sender, RoutedEventArgs e)
        {
            var words = await _iWordRepositoryBySipoe.Get();
            if (words != null)
                RemovedWordHelper(words.Select(x => x.Value).ToList());

            wordList.ItemsSource = WordList;
        }

        private async void checkBox3_Checked(object sender, RoutedEventArgs e)
        {
            var words = await _iWordRepositoryBySiPoTitee.Get();
            if (words != null)
                AddWordHelper(words.Select(x => x.Value).ToList());

            wordList.ItemsSource = WordList;
        }

        private async void checkBox3_Unchecked(object sender, RoutedEventArgs e)
        {
            var words = await _iWordRepositoryBySiPoTitee.Get();
            if (words != null)
                RemovedWordHelper(words.Select(x => x.Value).ToList());
            wordList.ItemsSource = WordList;
        }
        private async void checkBox4_Checked(object sender, RoutedEventArgs e)
        {
            var words = await _iWordRepositoryByIiId.Get();
            if (words != null)
                AddWordHelper(words.Select(x => x.Value).ToList());
            wordList.ItemsSource = WordList;
        }

        private async void checkBox4_Unchecked(object sender, RoutedEventArgs e)
        {
            var words = await _iWordRepositoryByIiId.Get();
            if (words != null)
                RemovedWordHelper(words.Select(x => x.Value).ToList());

            wordList.ItemsSource = WordList;
        }
        private async void checkBox5_Checked(object sender, RoutedEventArgs e)
        {
            var words = await _iWordRepositoryByGirIGrr.Get();
            if (words != null)
                AddWordHelper(words.Select(x => x.Value).ToList());

            wordList.ItemsSource = WordList;
        }

        private async void checkBox5_Unchecked(object sender, RoutedEventArgs e)
        {
            var words = await _iWordRepositoryByGirIGrr.Get();
            if (words != null)
                RemovedWordHelper(words.Select(x => x.Value).ToList());

            wordList.ItemsSource = WordList;
        }

        private void AddWordHelper(List<string> words)
        {
            foreach (var word in words)
            {
                if (!WordList.Contains(word))
                    WordList.Add(word);
            }
        }

        private void RemovedWordHelper(List<string> words)
        {
            foreach (var word in words)
            {
                if (WordList.Contains(word))
                    WordList.Remove(word);
            }
        }
    }
}
