
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ME.ControlLibrary.View
{
    /// <summary>
    /// ExportCommand.xaml 的交互逻辑
    /// </summary>
    public partial class ExportCommandView : Window
    {
        #region Private Fields

        #endregion

        #region Properties
        public string CommandInfoFileName { get; set; }
        #endregion

        #region Events

        #endregion

        #region Constructors

        public ExportCommandView()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        #endregion

        #region Methods

        #endregion

        private void Confirm(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CommandFileName.Text))
            {
                return;
            }
            CommandInfoFileName = CommandFileName.Text;
            DialogResult = true;
        }
    }
}
