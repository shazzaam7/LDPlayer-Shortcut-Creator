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
using IWshRuntimeLibrary;


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

        string Path, IName, AName, SaveDirectory, SName;
        string doSet = "\"MY_VAR =%% I\"";

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Path = LDPlayerPath.Text.ToString();
                IName = InstanceName.Text.ToString();
                AName = AppName.Text.ToString();
                SName = ShortcutName.Text.ToString();
                SaveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\XuanZhi9\";
            }
            catch 
            {
                MessageBox.Show("Needs to be a string");
                return;
            }
            if (Path.Length <= 0)
            {
                //Path = "\"C:\\LDPlayer\\LDPlayer9\\\"";
                Path = @"C:\LDPlayer\LDPlayer9\";
            }

            string temp = SaveDirectory + SName + ".bat";

            using (StreamWriter sw = new StreamWriter(temp))
            {
                sw.Write("@echo off" +
                    "\ncd " + Path +
                    "\ndnconsole.exe launch --name " + IName +
                    "\n:waitt" +
                    "\nTimeout 10" +
                    "\n@SET MY_VAR=" +
                    "\nFOR /F %%I IN ('ldconsole.exe runninglist') DO @SET " + doSet +
                    "\n@REM" +
                    "\necho %MY_VAR% FIND /I" + IName + ">Nul && (\n ldconsole runapp --name " + IName + " --packagename " + AName +
                    "\n goto :end" + "\n) || (" +
                    "\n goto :waitt" +
                    "\n)" +
                    "\n:end" +
                    "\nbreak;"
              );
            }

            object shDesktop = (object)"Desktop";
            WshShell shell = new WshShell();
            string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\" + SName + ".lnk";
            IWshShortcut ShortCut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            ShortCut.Description = "Shortcut for " + SName;
            ShortCut.TargetPath = temp;
            ShortCut.IconLocation = @"C:\LDPlayer\LDPlayer9\apk_icon.ico";
            ShortCut.Save();
        }
    }
}
