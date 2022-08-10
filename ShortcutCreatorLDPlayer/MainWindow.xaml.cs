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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace ShortcutCreatorLDPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            string Path, IName, AName;
            try
            {
                Path = LDPlayerPath.Text;
                IName = InstanceName.Text;
                AName = InstanceName.Text;
            }
            catch (Exception)
            {
                MessageBox.Show("Inputs are not strings.");
                return;
            }

        }
    }
}
