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

        string Path, IName, AName;

        string echo1 = "\"Waiting For Launch Complete\"";
        string echo2 = "\"define MY_VAR variable and set it to get running ldplayer emu device list and get first line\"";
        string doSet = "\"MY_VAR =%% I\"";

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Path = LDPlayerPath.Text.ToString();
                IName = InstanceName.Text.ToString();
                AName = AppName.Text.ToString();
            }
            catch 
            {
                MessageBox.Show("Needs to be a string");
                return;
            }
            if (Path.Length <= 0)
            {
                Path = "\"C:\\LDPlayer\\LDPlayer9\\\"";
            }
            
            using (StreamWriter sw = new StreamWriter("test.bat"))
            {
                sw.WriteLine("@echo off");
                sw.WriteLine("cd " + Path);
                sw.WriteLine("dnconsole.exe launch --name " + IName);
                sw.WriteLine("echo " + echo1);
                sw.WriteLine(":waitt");
                sw.WriteLine("Timeout 1");
                sw.WriteLine("@SET MY_VAR=");
                sw.WriteLine("FOR /F %%I IN ('ldconsole.exe runninglist') DO @SET " + doSet);
                sw.WriteLine("@REM");
                sw.WriteLine("echo %MY_VAR%");
                sw.WriteLine("ldconsole runapp --name Main --packagename " + AName);
                sw.WriteLine(":end");
                sw.WriteLine("break;");
            }
        }
    }
}
