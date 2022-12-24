using LightInject;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Threading.Tasks;
using System.Windows;
using TenderParserWpf.BLL.Interfaces;
using TenderParserWpf.BLL.Services;
using TenderParserWpf.BLL.Repositories;
using TenderParserWpf.Models.Models;
using System.Collections.Generic;
using TenderParserWpf.Models.Dto;

namespace TenderParserWpf
{
    /// <summary>
    /// Логика взаимодействия для WordEditView.xaml
    /// </summary>
    public partial class WordEditView : Window
    {
        private readonly IExcelReadingService _iExcelReadingService;
        private readonly IWordByPirRepository _iWordRepositoryByPir;
        private readonly IWordBySipoeRepository _iWordRepositoryBySipoe;
        private readonly IWordBySiPoTiteeRepository _iWordRepositoryBySiPoTitee;
        private readonly IWordByIiIdRepository _iWordRepositoryByIiId;
        private readonly IWordByGirIGrrRepository _iWordRepositoryByGirIGrr;
        public WordEditView()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();

            var container = new ServiceContainer();
            container.Register<IExcelReadingService, ExcelReadingService>();
            container.Register<IWordByPirRepository, WordByPirRepository>();
            container.Register<IWordBySipoeRepository, WordBySipoeRepository>();
            container.Register<IWordBySiPoTiteeRepository, WordBySiPoTiteeRepository>();
            container.Register<IWordByIiIdRepository, WordByIiIdRepository>();
            container.Register<IWordByGirIGrrRepository, WordByGirIGrrRepository>();


            _iExcelReadingService = container.GetInstance<IExcelReadingService>();
            _iWordRepositoryByPir = container.GetInstance<IWordByPirRepository>();
            _iWordRepositoryBySipoe = container.GetInstance<IWordBySipoeRepository>();
            _iWordRepositoryBySiPoTitee = container.GetInstance<IWordBySiPoTiteeRepository>();
            _iWordRepositoryByIiId = container.GetInstance<IWordByIiIdRepository>();
            _iWordRepositoryByGirIGrr = container.GetInstance<IWordByGirIGrrRepository>();

            _iExcelReadingService.Notify += ExcelReadingMessage;
            _iWordRepositoryByPir.Notify += PirMessage;
            _iWordRepositoryBySipoe.Notify += SipoeMessage;
            _iWordRepositoryBySiPoTitee .Notify+= SiPoMessage;
            _iWordRepositoryByIiId.Notify+= IiIdMessage;
            _iWordRepositoryByGirIGrr.Notify+= GirIGrrMessage;
        }

        private void buttonSelectFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.Filters.Add(new CommonFileDialogFilter("Excel Files", "xls,xlsx,xlsm"));
            CommonFileDialogResult result = dialog.ShowDialog();

            try
            {
                textBoxSelectFile.Text = dialog.FileName;
            }
            catch
            {
                MessageBox.Show("Не выбран файл");
            }
        }

        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow view = new MainWindow();
            view.Show();

            this.Close();
        }
        private void buttonDownloadTemplate_Click(object sender, RoutedEventArgs e)
        {
            DownloadingTemplateView view = new DownloadingTemplateView();
            view.Show();
        }

        private async void buttonUpdate_Click(object sender, RoutedEventArgs e)
        {
            string filePath = textBoxSelectFile.Text;
            if (!string.IsNullOrEmpty(filePath))
            {
                var listsObjects = await Task.Run(() => _iExcelReadingService.GetRawData(filePath));
                if (listsObjects != null)
                {
                    SaveWord(listsObjects);
                    MessageBox.Show("В базу данных добавлен " + listsObjects.Count + " объект");
                }
            }

        }
        private void SaveWord(List<DtoWordKey> listObjects)
        {
            foreach (var obj in listObjects)
            {
                switch (obj.TypeWord)
                {
                    case TypeWord.PIR:
                        WordByPir pir = new WordByPir() { Value = obj.Value, TypeWord = obj.TypeWord };
                        _iWordRepositoryByPir.Create(pir);
                        break;
                    case TypeWord.SIPOE:
                        WordBySipoe sipoe = new WordBySipoe() { Value = obj.Value, TypeWord = obj.TypeWord };
                        _iWordRepositoryBySipoe.Create(sipoe);
                        break;
                    case TypeWord.SI_PO_TITEEB:
                        WordBySiPoTitee sip = new WordBySiPoTitee() { Value = obj.Value, TypeWord = obj.TypeWord };
                        _iWordRepositoryBySiPoTitee.Create(sip);
                        break;
                    case TypeWord.I_I_ID:
                        WordByIiId word = new WordByIiId() { Value = obj.Value, TypeWord = obj.TypeWord };
                        _iWordRepositoryByIiId.Create(word);
                        break;
                    case TypeWord.GIR_I_GRR:
                        WordByGirIGrr grr = new WordByGirIGrr() { Value = obj.Value, TypeWord = obj.TypeWord };
                        _iWordRepositoryByGirIGrr.Create(grr);
                        break;
                }
            }  
        }

        private void buttonWordView_Click(object sender, RoutedEventArgs e)
        {
            WordView view = new WordView();
            view.Show();
        }

        private void ExcelReadingMessage(ExcelReadingService sender, ExcelEventArgs e)
        {
            MessageBox.Show(e.Message);
        }

        private void PirMessage(WordByPirRepository sender, ExcelEventArgs e)
        {
            MessageBox.Show(e.Message);
        }

        private void SipoeMessage(WordBySipoeRepository sender, ExcelEventArgs e)
        {
            MessageBox.Show(e.Message);
        }

        private void SiPoMessage(WordBySiPoTiteeRepository sender, ExcelEventArgs e)
        {
            MessageBox.Show(e.Message);
        }

        private void IiIdMessage(WordByIiIdRepository sender, ExcelEventArgs e)
        {
            MessageBox.Show(e.Message);
        }

        private void GirIGrrMessage(WordByGirIGrrRepository sender, ExcelEventArgs e)
        {
            MessageBox.Show(e.Message);
        }

    }
}
