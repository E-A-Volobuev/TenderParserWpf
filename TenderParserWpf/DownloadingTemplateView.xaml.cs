using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Windows;

namespace TenderParserWpf
{
    /// <summary>
    /// Логика взаимодействия для DownloadingTemplateView.xaml
    /// </summary>
    public partial class DownloadingTemplateView : Window
    {
        public DownloadingTemplateView()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
        }

        private void buttonSelectFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();

            try
            {
                textBoxSelectCatalog.Text = dialog.FileName;
            }
            catch
            {
                MessageBox.Show("Не выбран файл");
            }
        }

        private void buttonDownload_Click(object sender, RoutedEventArgs e)
        {
            string newPath = textBoxSelectCatalog.Text + @"\ключевые_слова_шаблон.xlsx";
            string fileTemplate = System.IO.Path.GetTempFileName() + "ключевые_слова_шаблон.xlsx";
            File.WriteAllBytes(fileTemplate, Properties.Resources.tempalte);

            FileInfo fileInf = new FileInfo(newPath);
            if (fileInf.Exists)
                File.Delete(newPath);

            FileInfo fileInfTemp = new FileInfo(fileTemplate);
            if (fileInfTemp.Exists)
            {
                File.Move(fileTemplate, newPath);
                File.Delete(fileTemplate);
            }


            MessageBox.Show($"заберите скачанный файл: {newPath}");
        }

    }
}
