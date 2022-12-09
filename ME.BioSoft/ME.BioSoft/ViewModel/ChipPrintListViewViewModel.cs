using CommunityToolkit.Mvvm.Input;
using ME.ControlLibrary.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;

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
            Messages.Clear();
        }
        public void ListViewCopy()
        {
            var list = Messages.Where(t => t.IsSelect == true).ToList();
            string jsonStr = JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.Indented);
            System.Windows.Clipboard.Clear();
            System.Windows.Clipboard.SetText(jsonStr);
        }
        public ICommand ListViewClearCmd { get; private set; }
        public ICommand ListViewCopyCmd { get; private set; }
    }
}
