using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using ME.ControlLibrary.View;
using MessageBox = ME.ControlLibrary.View.UMessageBox;
namespace ME.ControlLibrary.Model
{
    public class CustomMessageBox
    {
        public CustomMessageBox()
        {
            WeakReferenceMessenger.Default.Register<CustomMessageBox>(this, (th, me) =>
            {
                MessageBox.Show(Title, "提示", 1);
            });
        }
        public string Title { get; set; }

    }
}
