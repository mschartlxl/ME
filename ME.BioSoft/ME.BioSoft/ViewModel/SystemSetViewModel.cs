using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ME.BaseCore;
using ME.BaseCore.Instrument;
using ME.BioSoft.AutoMapper;
using ME.BioSoft.Model;
using ME.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MessageBox = ME.ControlLibrary.View.UMessageBox;

namespace ME.BioSoft.ViewModel
{
    public class SystemSetViewModel : ObservableObject
    {
        public ICommand SystemSetSaveCmd { get; private set; }
        public SystemSetViewModel()
        {
            SystemSetSaveCmd = new RelayCommand(SystemSetSave);
            var model = MEGlobal.SystemSet;
            CurrentModel = GlobalMapper.Instance.mapper.Map<SystemSetDTO>(model);
        }
        public void SystemSetSave()
        {
            var model= GlobalMapper.Instance.mapper.Map<SystemSet>(CurrentModel);
            var flag=SystemSetDAL.Instance.Update(model);
            if (flag)
            {
                ObjectCache cache = MemoryCache.Default;
                cache.Remove("SystemSet");
                MessageBox.Show("更新成功!");

            }
            else 
            {
                MessageBox.Show("更新失败!");

            }
        }
        private SystemSetDTO currentModel;
        /// <summary>
        /// 
        /// </summary>
        public SystemSetDTO CurrentModel
        {
            get => currentModel;
            set
            {
                SetProperty(ref currentModel, value);
            }
        }

    }
}
