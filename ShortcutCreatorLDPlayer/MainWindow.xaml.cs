//Default
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

//Imported
using System.IO;
using System.Net;
using HtmlAgilityPack;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using ImageMagick;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;


namespace ShortcutCreatorLDPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string Path, IName, AName, SName, CustomShortcutDirectory, IconsDirectory;
        string doSet = "\"MY_VAR =%% I\"";
        Dictionary<string, string> Apps = new Dictionary<string, string>();
        public MainWindow()
        {
            InitializeComponent();
            CheckIfRunAsAdmin();
            InstallationPathFinder();
            FindInstances();
            CheckIfFoldersExist();
            FindInstalledApps();
            GC.Collect();
        }

        // Events

        private void CreateShortcut_Click(object sender, RoutedEventArgs e)
        {
            if (Instances.SelectedIndex < 0)
            {
                MessageBox.Show("You have to choose the instance.");
                return;
            }
            if (InstalledApps.SelectedIndex < 0)
            {
                MessageBox.Show("You have to choose the app you want to create shortcut for.");
                return;
            }
            try
            {
                Path = InstallationPath.Text;
                IName = Instances.SelectedItem.ToString();
                AName = InstalledApps.SelectedItem.ToString();
                SName = ShortcutName.Text;
            }
            catch
            {
                return;
            }
            if (IName.Length <= 0)
            {
                IName = "LDPlayer";
            }
            string tempSave = CustomShortcutDirectory + @"\" + SName + ".bat";
            using (StreamWriter sw = new StreamWriter(tempSave))
            {
                if (Path.StartsWith("C:"))
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
                else
                {
                    sw.Write("@echo off" +
                        "\ncd /d " + Path +
                        "\ndnconsole.exe launch --name " + IName +
                        "\n:waitt" +
                        "\nTimeout 15" +
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
            }
            IconGrabber();
            object Desktop = (object)"Desktop";
            WshShell shell = new WshShell();
            string ShortcutAddress = (string)shell.SpecialFolders.Item(ref Desktop) + @"\" + SName + ".lnk";
            IWshShortcut Shortcut = (IWshShortcut)shell.CreateShortcut(ShortcutAddress);
            Shortcut.Description = "Shortcut for " + SName + " installed on LDPlayer";
            Shortcut.TargetPath = tempSave;
            Shortcut.IconLocation = IconsDirectory + @"\" + SName + ".ico";
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

        private void CheckIfRunAsAdmin()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                MessageBox.Show("Run as administrator!");
                Environment.Exit(0);
            }
        }

        private void InstallationPathFinder()
        {
            RegistryKey InstallPathLocation = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\XuanZhi\LDPlayer9");
            string location = InstallPathLocation.GetValue("InstallDir").ToString();
            InstallationPath.Text = location;
            CustomShortcutDirectory = location + "CustomShortcuts";
            IconsDirectory = CustomShortcutDirectory + @"\Icons";
            Path = InstallationPath.Text;
        }

        private void CheckIfFoldersExist()
        {
            string IconsDirectory = CustomShortcutDirectory + @"\Icons";
            if (!Directory.Exists(CustomShortcutDirectory))
            {
                Directory.CreateDirectory(CustomShortcutDirectory);
            }
            if (!Directory.Exists(IconsDirectory))
            {
                Directory.CreateDirectory(IconsDirectory);
            }
        }

        private void FindInstalledApps()
        {
            string AppList = Path + "appNames.text";
            if (!System.IO.File.Exists(AppList))
            {
                MessageBox.Show("You need to run your applications first in LDPlayer to be able to create shortcuts");
                Environment.Exit(0);
            }

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

        private void IconGrabber()
        {
            if (!System.IO.File.Exists(IconsDirectory + @"\" + ShortcutName.Text + ".ico"))
            {
                WebClient wClient = new WebClient();
                var src = "https://play.google.com/store/apps/details?id=" + InstalledApps.SelectedItem.ToString();
                string srcHTML = wClient.DownloadString(src);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(srcHTML);
                var ImageURL = doc.DocumentNode.SelectSingleNode("//img").Attributes["src"].Value; //img[@alt='Icon Image']
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(new Uri(ImageURL), IconsDirectory + @"\" + ShortcutName.Text + ".png");
                }
                using (MagickImage convertImage = new MagickImage(IconsDirectory + @"\" + ShortcutName.Text + ".png"))
                {
                    convertImage.Write(IconsDirectory + @"\" + ShortcutName.Text + ".ico");
                }
                System.IO.File.Delete(IconsDirectory + @"\" + ShortcutName.Text + ".png");
            }
        }

        private void FindInstances()
        {

            string console = Path;
            Process findInstance = new Process();
            findInstance.StartInfo.UseShellExecute = false;
            findInstance.StartInfo.FileName = Path + "dnconsole.exe";
            findInstance.StartInfo.RedirectStandardOutput = true;
            findInstance.StartInfo.CreateNoWindow = true;
            MessageBox.Show(Path + "dnconsole.exe");
            findInstance.StartInfo.Arguments = "list";
            findInstance.Start();
            string output = findInstance.StandardOutput.ReadToEnd();
            findInstance.WaitForExit();
            string[] outputSorted = output.Split(Environment.NewLine.ToCharArray());
            outputSorted = Array.FindAll(outputSorted, i => i!= "").ToArray();
            foreach (string item in outputSorted)
            {
                Instances.Items.Add(item);
            }
        }
    }
}
