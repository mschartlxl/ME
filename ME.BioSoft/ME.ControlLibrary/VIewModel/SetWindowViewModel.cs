using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ME.ControlLibrary.VIewModel
{
    public class SetWindowViewModel:ObservableRecipient,IRecipient<string>
    {
        public SetWindowViewModel()
        {

        }

        public void Receive(string message)
        {
            
        }
    }
}
