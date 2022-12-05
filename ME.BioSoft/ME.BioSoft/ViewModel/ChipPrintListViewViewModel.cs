using CommunityToolkit.Mvvm.Input;
using ME.ControlLibrary.Model;
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
        public ObservableCollection<MessageCmd> Messages { get; set; } = new ObservableCollection<MessageCmd>();
        private void InitListViewField()
        {
            ListViewClearCmd = new RelayCommand(ListViewClear);
            ListViewCopyCmd = new RelayCommand(ListViewCopy);
        }

        public void ListViewClear()
        {

        }
        public void ListViewCopy()
        {

        }
        public ICommand ListViewClearCmd { get; private set; }
        public ICommand ListViewCopyCmd { get; private set; }
    }
}
