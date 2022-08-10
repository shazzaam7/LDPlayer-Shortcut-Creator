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
using Microsoft.Win32;

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

        string Path, IName, AName, SName;

        string doSet = "\"MY_VAR =%% I\"";

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Path = LDPlayerPath.Text.ToString();
                IName = InstanceName.Text.ToString();
                AName = AppName.Text.ToString();
                SName = ShortcutName.Text.ToString() + ".bat";
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

            using (StreamWriter sw = new StreamWriter(SName))
            {
                sw.Write("@echo off" +
                    "\ncd " + Path +
                    "\ndnconsole.exe launch --name " + IName +
                    "\n:waitt" +
                    "\nTimeout 1" +
                    "\n@SET MY_VAR=" +
                    "\nFOR /F %%I IN ('ldconsole.exe runninglist') DO @SET " + doSet +
                    "\n@REM" +
                    "\necho %MY_VAR% FIND /I" + IName + ">Nul && (\n ldconsole runapp --name Main --packagename " + AName +
                    "\n goto :end" + "\n) || (" +
                    "\n goto :waitt" +
                    "\n)" +
                    "\n:end" +
                    "\nbreak;"
              );
            }
        }
    }
}
