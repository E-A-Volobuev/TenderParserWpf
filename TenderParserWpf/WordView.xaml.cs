using AutoMapper;
using LightInject;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using TenderParserWpf.BLL.Interfaces;
using TenderParserWpf.BLL.Repositories;
using TenderParserWpf.Models.Dto;
using TenderParserWpf.Models.Models;
using TenderParserWpf.Models.Extensions;
using System.Windows.Controls;
using System;
using System.Threading.Tasks;

namespace TenderParserWpf
{
    /// <summary>
    /// Логика взаимодействия для WordView.xaml
    /// </summary>
    public partial class WordView : Window
    {
        private readonly IWordByPirRepository _iWordRepositoryByPir;
        private readonly IWordBySipoeRepository _iWordRepositoryBySipoe;
        private readonly IWordBySiPoTiteeRepository _iWordRepositoryBySiPoTitee;
        private readonly IWordByIiIdRepository _iWordRepositoryByIiId;
        private readonly IWordByGirIGrrRepository _iWordRepositoryByGirIGrr;

        private List<DtoWordForDataGrid> GridPirCollection { get; set; }
        private List<DtoWordForDataGrid> GridSipoeCollection { get; set; }
        private List<DtoWordForDataGrid> GridSiPoTiCollection { get; set; }
        private List<DtoWordForDataGrid> GridIiIdCollection { get; set; }
        private List<DtoWordForDataGrid> GridGirCollection { get; set; }
        private List<DtoWordForDataGrid> RemoveList { get; set; }

        public WordView()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();

            var container = DepencyInjectionHelper();

            _iWordRepositoryByPir = container.GetInstance<IWordByPirRepository>();
            _iWordRepositoryBySipoe = container.GetInstance<IWordBySipoeRepository>();
            _iWordRepositoryBySiPoTitee = container.GetInstance<IWordBySiPoTiteeRepository>();
            _iWordRepositoryByIiId = container.GetInstance<IWordByIiIdRepository>();
            _iWordRepositoryByGirIGrr = container.GetInstance<IWordByGirIGrrRepository>();

            RemoveList = new List<DtoWordForDataGrid>();
            new Action(async () => await InitializeHelper())();
        }
        private ServiceContainer DepencyInjectionHelper()
        {
            var container = new ServiceContainer();

            container.Register<IWordByPirRepository, WordByPirRepository>();
            container.Register<IWordBySipoeRepository, WordBySipoeRepository>();
            container.Register<IWordBySiPoTiteeRepository, WordBySiPoTiteeRepository>();
            container.Register<IWordByIiIdRepository, WordByIiIdRepository>();
            container.Register<IWordByGirIGrrRepository, WordByGirIGrrRepository>();

            return container;
        }
        private async Task InitializeHelper()
        {
            GridPirCollection = await GetWords(TypeWord.PIR);
            wordGridPir.ItemsSource = GridPirCollection;

            GridSipoeCollection= await GetWords(TypeWord.SIPOE);
            wordGridSipoe.ItemsSource = GridSipoeCollection;

            GridSiPoTiCollection= await GetWords(TypeWord.SI_PO_TITEEB);
            wordGridSiPoTi.ItemsSource = GridSiPoTiCollection;

            GridIiIdCollection =await GetWords(TypeWord.I_I_ID);
            wordGridIiId.ItemsSource = GridIiIdCollection;

            GridGirCollection= await GetWords(TypeWord.GIR_I_GRR);
            wordGridGir.ItemsSource = GridGirCollection;


            wordGridPir.CanUserAddRows = false;
            wordGridSipoe.CanUserAddRows = false;
            wordGridSiPoTi.CanUserAddRows = false;
            wordGridIiId.CanUserAddRows = false;
            wordGridGir.CanUserAddRows = false;

        }

        private async Task<List<DtoWordForDataGrid>> GetWords(TypeWord type)
        {
            List<DtoWordForDataGrid> dtoList = new List<DtoWordForDataGrid>();
            List<WordKey> listWordKey = new List<WordKey>();

            var config = new MapperConfiguration(cfg =>
                    cfg.CreateMap<WordKey, DtoWordForDataGrid>()
                    .ForMember(dto => dto.Id, word => word.MapFrom(src => src.Id))
                    .ForMember(dto => dto.Number, word => word.MapFrom(src => 0))
                    .ForMember(dto => dto.Value, word => word.MapFrom(src => src.Value))
                    .ForMember(dto => dto.Description, word => word.MapFrom(src => src.TypeWord.GetAttribute<TypeWord, DisplayAttribute>().Name))
                );

            await CreateListEntyties(listWordKey, type);

            CreateListDto(listWordKey, dtoList, config);

            return dtoList;
        }

        private async Task CreateListEntyties(List<WordKey> listKey, TypeWord type)
        {
            switch (type)
            {
                case TypeWord.PIR:
                    listKey.AddRange(await _iWordRepositoryByPir.Get());
                    break;
                case TypeWord.SIPOE:
                    listKey.AddRange(await _iWordRepositoryBySipoe.Get());
                    break;
                case TypeWord.SI_PO_TITEEB:
                    listKey.AddRange(await _iWordRepositoryBySiPoTitee.Get());
                    break;
                case TypeWord.I_I_ID:
                    listKey.AddRange(await _iWordRepositoryByIiId.Get());
                    break;
                case TypeWord.GIR_I_GRR:
                    listKey.AddRange(await _iWordRepositoryByGirIGrr.Get());
                    break;

                default: break;
            }
        }

        private void CreateListDto(List<WordKey> listWordKey, List<DtoWordForDataGrid> dtoList,MapperConfiguration config)
        {
            int counter = 1;
            foreach (var word in listWordKey)
            {
                var dto = config.CreateMapper().Map<DtoWordForDataGrid>(word);
                if (dto != null)
                {
                    dto.Number = counter;
                    dtoList.Add(dto);
                    ++counter;            
                }
                   
            }
        }


        #region deleteButton_region
        private void DeleteValueByPir(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = wordGridPir;
            string cellValueTwo = SelectCellHelper(dataGrid);

            DeleteValueByPirHelper(cellValueTwo);
        }

        private void DeleteValueByPirHelper(string cellValueTwo)
        {
            var dto = GridPirCollection.FirstOrDefault(x => x.Value == cellValueTwo);
            GridPirCollection.Remove(dto);
            RemoveList.Add(dto);

            CounterValueHelper(GridPirCollection);

            wordGridPir.ItemsSource = null;
            wordGridPir.ItemsSource = GridPirCollection;
        }
        private void DeleteValueBySipoe(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = wordGridSipoe;
            string cellValueTwo = SelectCellHelper(dataGrid); 

            DeleteValueBySipoeHelper(cellValueTwo);
        }

        private void DeleteValueBySipoeHelper(string cellValueTwo)
        {
            var dto = GridSipoeCollection.FirstOrDefault(x => x.Value == cellValueTwo);
            GridSipoeCollection.Remove(dto);
            RemoveList.Add(dto);

            CounterValueHelper(GridSipoeCollection);

            wordGridSipoe.ItemsSource = null;
            wordGridSipoe.ItemsSource = GridSipoeCollection;
        }

        private void DeleteValueBySiPoTi(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = wordGridSiPoTi;
            string cellValueTwo = SelectCellHelper(dataGrid);

            DeleteValueBySiPoTiHelper(cellValueTwo);
        }

        private void DeleteValueBySiPoTiHelper(string cellValueTwo)
        {
            var dto = GridSiPoTiCollection.FirstOrDefault(x => x.Value == cellValueTwo);
            GridSiPoTiCollection.Remove(dto);
            RemoveList.Add(dto);

            CounterValueHelper(GridSiPoTiCollection);

            wordGridSiPoTi.ItemsSource = null;
            wordGridSiPoTi.ItemsSource = GridSiPoTiCollection;
        }

        private void DeleteValueByIiId(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = wordGridIiId;
            string cellValueTwo = SelectCellHelper(dataGrid);

            DeleteValueByIiIdHelper(cellValueTwo);
        }

        private void DeleteValueByIiIdHelper(string cellValueTwo)
        {
            var dto = GridIiIdCollection.FirstOrDefault(x => x.Value == cellValueTwo);
            GridIiIdCollection.Remove(dto);
            RemoveList.Add(dto);

            CounterValueHelper(GridIiIdCollection);

            wordGridIiId.ItemsSource = null;
            wordGridIiId.ItemsSource = GridIiIdCollection;
        }

        private void DeleteValueByGir(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = wordGridGir;
            string cellValueTwo = SelectCellHelper(dataGrid);

            DeleteValueByGirHelper(cellValueTwo);
        }

        private void DeleteValueByGirHelper(string cellValueTwo)
        {
            var dto = GridGirCollection.FirstOrDefault(x => x.Value == cellValueTwo);
            GridGirCollection.Remove(dto);
            RemoveList.Add(dto);

            CounterValueHelper(GridGirCollection);

            wordGridGir.ItemsSource = null;
            wordGridGir.ItemsSource = GridGirCollection;
        }
        /// <summary>
        /// создание порядкового номера слов в датагрид
        /// </summary>
        /// <param name="collection"></param>
        private void CounterValueHelper(List<DtoWordForDataGrid> collection)
        {
            int counter = 1;
            foreach (var obj in collection)
            {
                obj.Number = counter;
                counter++;
            }
        }

        /// <summary>
        /// хелпер считывания значения слова из строки,в которой кликнули кнопку удалить
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <returns></returns>
        private string SelectCellHelper(DataGrid dataGrid)
        {
            DataGridRow Row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(dataGrid.SelectedIndex);
            DataGridCell RowAndColumnOne = (DataGridCell)dataGrid.Columns[2].GetCellContent(Row).Parent;
            string cellValueTwo = ((TextBlock)RowAndColumnOne.Content).Text;

            return cellValueTwo;
        }
        #endregion


        private async void SaveValues(object sender, RoutedEventArgs e)
        {
            try
            {
                await DeleteObjBySource();

                await UpdateObjBySource();

                MessageBox.Show("Изменения сохранены");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// удаляем из бд
        /// </summary>
        /// <returns></returns>
        private async Task DeleteObjBySource()
        {
            List<WordKey> removeListByPir = await GetRemoveList(TypeWord.PIR);
            List<WordKey> removeListBySipoe = await GetRemoveList(TypeWord.SIPOE);
            List<WordKey> removeListBySiPoTi = await GetRemoveList(TypeWord.SI_PO_TITEEB);
            List<WordKey> removeListByGir = await GetRemoveList(TypeWord.GIR_I_GRR);
            List<WordKey> removeListByIiId= await GetRemoveList(TypeWord.I_I_ID);

            if (removeListByPir.Count > 0)
                await _iWordRepositoryByPir.DeleteHelper(removeListByPir);
            if (removeListBySipoe.Count > 0)
                await _iWordRepositoryBySipoe.DeleteHelper(removeListBySipoe);
            if (removeListBySiPoTi.Count > 0)
                await _iWordRepositoryBySiPoTitee.DeleteHelper(removeListBySiPoTi);
            if (removeListByGir.Count > 0)
                await _iWordRepositoryByGirIGrr.DeleteHelper(removeListByGir);
            if (removeListByIiId.Count > 0)
                await _iWordRepositoryByIiId.DeleteHelper(removeListByIiId);
        }
        /// <summary>
        /// находим удаляемые слова со всех вкладок
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private async Task<List<WordKey>> GetRemoveList(TypeWord type)
        {
            List<WordKey> removeList = new List<WordKey>();

            foreach (var pirDelete in RemoveList)
            {
                object obj = await RemoveListHelper(pirDelete, type);

                if (obj != null)
                {
                    var word = obj as WordKey;
                    removeList.Add(word);
                }                  
            }

            return removeList;
        }

        private async Task<object> RemoveListHelper(DtoWordForDataGrid pirDelete, TypeWord type)
        {
            object obj = new object();

            switch (type)
            {
                case TypeWord.PIR:
                    obj = await _iWordRepositoryByPir.GetById(pirDelete.Id);
                    break;
                case TypeWord.SIPOE:
                    obj = await _iWordRepositoryBySipoe.GetById(pirDelete.Id);
                    break;
                case TypeWord.GIR_I_GRR:
                    obj = await _iWordRepositoryByGirIGrr.GetById(pirDelete.Id);
                    break;
                case TypeWord.I_I_ID:
                    obj = await _iWordRepositoryByIiId.GetById(pirDelete.Id);
                    break;
                case TypeWord.SI_PO_TITEEB:
                    obj = await _iWordRepositoryBySiPoTitee.GetById(pirDelete.Id);
                    break;
                default:
                    obj = null;
                    break;
            }
            return obj;
        }


        #region update data base
        /// <summary>
        /// обновляем БД
        /// </summary>
        /// <returns></returns>
        private async Task UpdateObjBySource()
        {
            await PirDataBaseUpdate();
            await SipoeDataBaseUpdate();
            await SiPoTiDataBaseUpdate();
            await IiIdDataBaseUpdate();
            await GirDataBaseUpdate();
        }

        private async Task PirDataBaseUpdate()
        {
            foreach (var pir in GridPirCollection)
            {
                var pirSave = await _iWordRepositoryByPir.GetById(pir.Id);
                if (pirSave != null)
                {
                    pirSave.Value = pir.Value;
                    await _iWordRepositoryByPir.UpdateHelper(pirSave);
                }
            }
        }

        private async Task SipoeDataBaseUpdate()
        {
            foreach (var sipoe in GridSipoeCollection)
            {
                var sipoeSave = await _iWordRepositoryBySipoe.GetById(sipoe.Id);
                if (sipoeSave != null)
                {
                    sipoeSave.Value = sipoe.Value;
                    await _iWordRepositoryBySipoe.UpdateHelper(sipoeSave);
                }
            }
        }
        private async Task SiPoTiDataBaseUpdate()
        {
            foreach (var siPoTi in GridSiPoTiCollection)
            {
                var siPoTiSave = await _iWordRepositoryBySiPoTitee.GetById(siPoTi.Id);
                if (siPoTiSave != null)
                {
                    siPoTiSave.Value = siPoTi.Value;
                    await _iWordRepositoryBySiPoTitee.UpdateHelper(siPoTiSave);
                }
            }
        }

        private async Task IiIdDataBaseUpdate()
        {
            foreach (var iIid in GridIiIdCollection)
            {
                var iIidSave = await _iWordRepositoryByIiId.GetById(iIid.Id);
                if (iIidSave != null)
                {
                    iIidSave.Value = iIid.Value;
                    await _iWordRepositoryByIiId.UpdateHelper(iIidSave);
                }
            }
        }

        private async Task GirDataBaseUpdate()
        {
            foreach (var gir in GridGirCollection)
            {
                var girSave = await _iWordRepositoryByGirIGrr.GetById(gir.Id);
                if (girSave != null)
                {
                    girSave.Value = gir.Value;
                    await _iWordRepositoryByGirIGrr.UpdateHelper(girSave);
                }
            }
        }
        #endregion


    }
}
