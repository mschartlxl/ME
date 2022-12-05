using AutoMapper;
using CommunityToolkit.Mvvm.Input;
using ME.BaseCore;
using ME.BioSoft.AutoMapper;
using ME.BioSoft.Model;
using ME.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ME.BioSoft.ViewModel
{
    public partial class ChipPrintViewModel
    {
        private void InitDataGrid()
        {
            InitDataGridField();
            InitData();
        }
        private void InitDataGridField()
        {
            PlatformActionList = new ObservableCollection<PlatformActionDTO>();
            AllCheckedCmd = new RelayCommand<bool>(AllChecked);
            DataGridAddCmd = new RelayCommand(DataGridAdd);
            DataGridDelCmd = new RelayCommand(DataGridDel);
            MouseDoubleCmd = new RelayCommand<object>(MouseDouble);
            DataGridEditCmd = new RelayCommand(DataGridEdit);
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<OrginalProfile>();
            });
            mapper = config.CreateMapper();
        }
        private void InitData()
        {
            var alldata = PlatformActionDAL.Instance.SearchMany(null);
            var index = 1;
            foreach (var data in alldata)
            {
                var model = mapper.Map<PlatformActionDTO>(data);
                model.Index = index;
                PlatformActionList.Add(model);
                index++;
            }
        }
        public ICommand AllCheckedCmd { get; private set; }
        public ICommand DataGridAddCmd { get; private set; }
        public ICommand DataGridDelCmd { get; private set; }
        public ICommand MouseDoubleCmd { get; private set; }
        public ICommand DataGridEditCmd { get; private set; }
        private ObservableCollection<PlatformActionDTO> platformActionList;
        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<PlatformActionDTO> PlatformActionList
        {
            get => platformActionList;
            set
            {
                SetProperty(ref platformActionList, value);
            }
        }
        private PlatformActionDTO currentDataGridItem = new PlatformActionDTO();
        public PlatformActionDTO CurrentDataGridItem
        {
            get => currentDataGridItem;
            set
            {
                SetProperty(ref currentDataGridItem, value);
            }
        }
        public void AllChecked(bool flag)
        {
            foreach (var item in PlatformActionList)
            {
                item.IsChecked = flag;
            }

        }
        private void DataGridAdd()
        {
            var z = "";
            foreach (var item in ZAxisItems)
            {
                z += $"{item.CkContent}:{item.CkTxt},";
            }
            CurrentDataGridItem.Id = UtilsFun.GetNewGuid();
            CurrentDataGridItem.Z = z.TrimEnd(',');
            CurrentDataGridItem.Index = (PlatformActionList.Count == 0) ? 1 : (PlatformActionList.Max(t => t.Index) + 1);
            var newModel = (PlatformActionDTO)CurrentDataGridItem.Clone();
            PlatformActionList.Add(newModel);
            var newModelDTO = mapper.Map<PlatformAction>(newModel);
            PlatformActionDAL.Instance.Add(newModelDTO);
            CurrentDataGridItem = new PlatformActionDTO();

        }
        private void MouseDouble(object o)
        {
            var selectItem = o as PlatformActionDTO;
            if (selectItem != null)
            {
                CurrentDataGridItem = selectItem.Clone() as PlatformActionDTO;
                var zlist = CurrentDataGridItem.Z.ToString().Split(',');
                foreach (var item in ZAxisItems)
                {
                    foreach (var zstr in zlist)
                    {
                        var z = zstr.Split(':');
                        if (z[0] == item.CkContent)
                        {
                            item.CkTxt = z[1];
                        }
                    }

                }
            }
        }
        private void DataGridEdit()
        {
            var modeledit = PlatformActionList.FirstOrDefault(t => t.Id == CurrentDataGridItem.Id);
            modeledit.Name = CurrentDataGridItem.Name;
            modeledit.X = CurrentDataGridItem.X;
            modeledit.Y = CurrentDataGridItem.Y;
            modeledit.R = CurrentDataGridItem.R;
            var z = "";
            foreach (var item in ZAxisItems)
            {
                z += $"{item.CkContent}:{item.CkTxt},";
            }
            modeledit.Z = z.TrimEnd(',');
            var newModel = mapper.Map<PlatformAction>(modeledit);
            PlatformActionDAL.Instance.Update(newModel);
            CurrentDataGridItem = new PlatformActionDTO();
        }
        private void DataGridDel()
        {
            List<PlatformActionDTO> selectModels = PlatformActionList.Where(t => t.IsChecked == true).ToList();
            for (int t = 0; t < selectModels.Count; t++)
            {
                var newModel = mapper.Map<PlatformAction>(selectModels[t]);
                PlatformActionDAL.Instance.Delete(newModel);
                PlatformActionList.Remove(selectModels[t]);
            }

        }
    }
}
