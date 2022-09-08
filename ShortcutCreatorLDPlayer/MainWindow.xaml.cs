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
using Microsoft.Win32;


namespace ShortcutCreatorLDPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string Path, IName, AName, SName, SaveDirectory;
        string doSet = "\"MY_VAR =%% I\"";
        Dictionary<string, string> Apps = new Dictionary<string, string>();

        public MainWindow()
        {
            InitializeComponent();
            InstallationPathFinder();
            FindInstalledApps();
        }

        // Events

        private void CreateShortcut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Path = InstallationPath.Text;
                IName = InstanceName.Text;
                AName = InstalledApps.SelectedItem.ToString();
                SName = ShortcutName.Text;
                SaveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\XuanZhi9\";
            }
            catch
            {
                return;
            }
            if (IName.Length <= 0)
            {
                IName = "Main";
            }

            string tempSave = SaveDirectory + SName + ".bat";
            using (StreamWriter sw = new StreamWriter(tempSave))
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

            object Desktop = (object)"Desktop";
            WshShell shell = new WshShell();
            string ShortcutAddress = (string)shell.SpecialFolders.Item(ref Desktop) + @"\" + SName + ".lnk";
            IWshShortcut Shortcut = (IWshShortcut)shell.CreateShortcut(ShortcutAddress);
            Shortcut.Description = "Shortcut for " + SName + " installed on LDPlayer";
            Shortcut.TargetPath = tempSave;
            Shortcut.IconLocation = @"C:\LDPlayer\LDPlayer9\apk_icon.ico";
            Shortcut.Save();
        }

        private void InstalledApps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedItem;
            try
            {
                selectedItem = InstalledApps.SelectedItem.ToString();
            }
            catch 
            {
                return;
            }

            foreach (string key in Apps.Keys)
            {
                if (Apps[key] == selectedItem)
                {
                    ShortcutName.Text = key;
                }
            }
        }

        // Methods
        private void InstallationPathFinder()
        {
            RegistryKey finder = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            string location = FindByDisplayName(finder, "LDPlayer");
            InstallationPath.Text = location.Remove(location.LastIndexOf('\\')) + "\\";
            Path = InstallationPath.Text;
        }

        private string FindByDisplayName(RegistryKey AllInstalledSoftware, string Name)
        {
            string[] nameList = AllInstalledSoftware.GetSubKeyNames();
            for (int i = 0; i < nameList.Length; i++)
            {
                RegistryKey tempInstallation = AllInstalledSoftware.OpenSubKey(nameList[i]);
                if (tempInstallation.GetValue("DisplayName") == null)
                {
                    continue;
                }
                else
                {
                    try
                    {
                        if (string.Equals(tempInstallation.GetValue("DisplayName").ToString(), Name))
                        {
                            return tempInstallation.GetValue("DisplayIcon").ToString();
                        }
                    }
                    catch { }
                }
            }
            return "";
        }

        private void FindInstalledApps()
        {
            string AppList = Path + "appNames.text";
            using (StreamReader sr = new StreamReader(AppList))
            {
                string line = sr.ReadLine();
                string[] splitLine = line.Split('|');
                splitLine = Array.FindAll(splitLine, i => i != "").ToArray();
                foreach (string item in splitLine)
                {
                    string[] tempSplit = item.Split(':');
                    Apps.Add(tempSplit[1], tempSplit[0]);
                }
                foreach (string key in Apps.Keys)
                {
                    InstalledApps.Items.Add(Apps[key]);
                }
            }
        }
    }
}
