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
using System.Security.Principal;


namespace ShortcutCreatorLDPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string Path, IName, AName, SName, loadingTime, CustomShortcutDirectory, IconsDirectory;
        string doSet = "\"MY_VAR =%% I\"";
        Dictionary<string, string> Apps = new Dictionary<string, string>();
        public MainWindow()
        {
            InitializeComponent();
            //Check to see if the program is running as administrator, if not then it shows a MessageBox and exits
            if (!CheckIfRunAsAdmin())
            {
                MessageBox.Show("Run as administrator!");
                Environment.Exit(0);
            }
            InstallationPathFinder(); //Finds installation folder of LDPlayer
            CheckIfFoldersExist(); //Checks if all of the folders necessary for this program to work exist
            if (!System.IO.File.Exists("firstrun.fr"))
            {
                MessageBoxResult result = MessageBox.Show("Do you want to run First Run setup to make sure the program works properly?"," ", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    FirstRun();
                } else
                {
                    System.IO.File.Create("firstrun.fr");
                }
            }         
            FindInstances(); //Finds all of the instances in LDPlayer (Requires Administrator)
            FindInstalledApps(); //Finds all of the apps installed in LDPlayer
            GC.Collect(); //Collects unnecessary garbage
        }

        // Events

        private void CreateShortcut_Click(object sender, RoutedEventArgs e)
        {
            //Checks if an Instance has been selected
            if (Instances.SelectedIndex < 0)
            {
                MessageBox.Show("You have to choose the instance.");
                return;
            }
            //Checks if a Installed App has been selected
            if (InstalledApps.SelectedIndex < 0)
            {
                MessageBox.Show("You have to choose the app you want to create shortcut for.");
                return;
            }

            //Tries to assign everything to their variable
            try
            {
                Path = InstallationPath.Text;
                IName = Instances.SelectedItem.ToString();
                AName = InstalledApps.SelectedItem.ToString();
                SName = ShortcutName.Text;
                loadingTime = LoadingTime.Text;
            }
            catch
            {
                return;
            }

            //If Instance is somehow empty string, it defaults to default value
            if (IName.Length <= 0)
            {
                IName = "LDPlayer";
            }

            string tempSave = CustomShortcutDirectory + @"\" + SName + ".bat"; //Where .bat script is stored
            using (StreamWriter sw = new StreamWriter(tempSave))
            {
                //.bat script creation
                sw.Write("@echo off" +
                    "\ncd /d " + Path +
                    "\ndnconsole.exe launch --name " + IName +
                    "\n:waitt" +
                    "\nTimeout " + loadingTime +
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
            try
            {
                IconGrabber(); //Method used to grab icons from Play Store
            }
            catch
            {

            }

            string search = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + @"\Programs\LDPlayer9\" + SName + ".lnk";
            object Desktop = (object)"Desktop";
            WshShell shell = new WshShell(); //New shell
            string ShortcutLocation = (string)shell.SpecialFolders.Item(ref Desktop) + @"\" + SName + ".lnk"; //Location of the Shortcut (Desktop)
            IWshShortcut Shortcut = (IWshShortcut)shell.CreateShortcut(ShortcutLocation); //Creating new Shortcut
            Shortcut.Description = "Shortcut for " + SName + " installed on LDPlayer"; //Description of the Shortcut
            Shortcut.TargetPath = tempSave; //Where .bat script is saved
            if (System.IO.File.Exists(IconsDirectory + @"\" + SName + ".ico"))
            {
                Shortcut.IconLocation = IconsDirectory + @"\" + SName + ".ico";
            } else
            {
                Shortcut.IconLocation = Path + "apk_icon.ico";
            }
            Shortcut.Save(); //Save the Shortcut
            if (ShortcutAsAdmin.IsChecked == true)
            {
                using (FileStream fs = new FileStream(ShortcutLocation, FileMode.Open, FileAccess.ReadWrite))
                {
                    fs.Seek(21, SeekOrigin.Begin);
                    fs.WriteByte(0x22);
                }
            }
            if (AddShortcutToSearch.IsChecked == true)
            {
                System.IO.File.Copy(ShortcutLocation, search, true);
            };
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
        private void FirstRun()
        {
            if (!System.IO.File.Exists("firstrun.fr"))
            {
                System.IO.File.Delete(Path + "appNames.text");
                MessageBox.Show("First Run only: Current appNames.text has been deleted. Now open LDPlayer and launch all of the applications that you want to create shortcut off");
                System.IO.File.Create("firstrun.fr");
                Environment.Exit(0);
            }
        }

        private bool CheckIfRunAsAdmin()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void InstallationPathFinder()
        {
            //Opens RegistryKey where LDPlayer9 is located
            RegistryKey InstallPathLocation = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\XuanZhi\LDPlayer9");
            //Grabs the value of InstallDir in LDPlayer9 registry folder
            string location = InstallPathLocation.GetValue("InstallDir").ToString();
            //Shows the value of InstallDir on InstallationPath TextBox
            InstallationPath.Text = location;
            //Giving values to variables that are needed in the future
            CustomShortcutDirectory = location + "CustomShortcuts";
            IconsDirectory = CustomShortcutDirectory + @"\Icons";
            Path = InstallationPath.Text;
        }

        private void FindInstances()
        {
            //Uses Process to open dnconsole.exe with arguments needed to list all of the instances and then stores them in a array
            string console = Path;
            Process findInstance = new Process();
            findInstance.StartInfo.UseShellExecute = false;
            findInstance.StartInfo.FileName = Path + "dnconsole.exe";
            findInstance.StartInfo.RedirectStandardOutput = true;
            findInstance.StartInfo.CreateNoWindow = true;
            findInstance.StartInfo.Arguments = "list";
            findInstance.Start();
            findInstance.WaitForExit();
            string output = findInstance.StandardOutput.ReadToEnd();
            string[] outputSorted = output.Split(Environment.NewLine.ToCharArray()); //Converts string to array (Instances are stored in rows)
            outputSorted = Array.FindAll(outputSorted, i => i != "").ToArray(); //Removes all of the empty strings in the array
            foreach (string item in outputSorted)
            {
                Instances.Items.Add(item); //Stores all of the instances in Instances ComboBox
            }
        }

        private void CheckIfFoldersExist()
        {
            string IconsDirectory = CustomShortcutDirectory + @"\Icons"; //Icons direcotry where program stores all of the downloaded icons
            //Checks if CustomShortcutDirecotry exists and creates one if it doesn't already exist
            if (!Directory.Exists(CustomShortcutDirectory))
            {
                Directory.CreateDirectory(CustomShortcutDirectory);
            }
            //Checks if IconsDirecotry exists and creates one if it doesn't already exist
            if (!Directory.Exists(IconsDirectory))
            {
                Directory.CreateDirectory(IconsDirectory);
            }
        }

        private void FindInstalledApps()
        {
            string AppList = Path + "appNames.text"; //Directory of the text file that stores all of the apps installed on LDPlayer
            //Checks if the text file exists.
            if (!System.IO.File.Exists(AppList))
            {
                MessageBox.Show("You need to run your applications first in LDPlayer to be able to create shortcuts");
                Environment.Exit(0);
            }

            //StreamReader reads the text file
            using (StreamReader sr = new StreamReader(AppList))
            {
                string line = sr.ReadLine(); //Text File has everything stored in 1 line
                string[] splitLine = line.Split('|'); //Apps are split with this character
                splitLine = Array.FindAll(splitLine, i => i != "").ToArray(); //Removing empty strings
                //Now that all apps are split, we need to split package name and app name from each other
                foreach (string item in splitLine)
                {
                    string[] tempSplit = item.Split(':'); //Splits Package Name and App Name
                    if (!Apps.ContainsKey(tempSplit[1]))
                    {
                        if (!Apps.ContainsValue(tempSplit[0]))
                        {
                            Apps.Add(tempSplit[1], tempSplit[0]); //Storing all of the App Name and Package Name in a Dictionary
                        }
                    } else
                    {
                        Apps.Add(tempSplit[1] + " (2)", tempSplit[0]); //Storing all of the App Name and Package Name in a Dictionary
                    }
                }
                foreach (string key in Apps.Keys)
                {
                    InstalledApps.Items.Add(Apps[key]); //Showing all of the Package names in the InstalledApps ComboBox
                }
            }
        }

        private void IconGrabber()
        {
            //Checks if the icon is already downloaded so it doesn't download it again
            if (!System.IO.File.Exists(IconsDirectory + @"\" + ShortcutName.Text + ".ico"))
            {
                WebClient wClient = new WebClient(); //WebClient
                var src = "https://play.google.com/store/apps/details?id=" + InstalledApps.SelectedItem.ToString(); //Link of the selected app in Play Store
                string srcHTML = wClient.DownloadString(src); //Downloads the HTML of the site
                HtmlDocument doc = new HtmlDocument(); //Instancing new doc
                doc.LoadHtml(srcHTML); //Loading the downloaded HTML
                var ImageURLFinder = doc.DocumentNode.SelectNodes("//img"); //Trying to find img in the HTML
                var ImageURL = "";
                foreach (var item in ImageURLFinder)
                {
                    if (item.Attributes["src"].Value.StartsWith("https://play-lh.googleusercontent.com/"))
                    {
                        Console.WriteLine(item.Attributes["src"].Value);
                        ImageURL = item.Attributes["src"].Value;
                        break;
                    }
                }
                //WebClient downloads the icon as a .png
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(new Uri(ImageURL), IconsDirectory + @"\" + ShortcutName.Text + ".png"); //Downloads the Icon in the correct spot
                }
                //MagickImage converts the icon from .png to .ico
                using (MagickImage convertImage = new MagickImage(IconsDirectory + @"\" + ShortcutName.Text + ".png"))
                {
                    convertImage.Write(IconsDirectory + @"\" + ShortcutName.Text + ".ico");
                }
                //Deletes old .png file
                System.IO.File.Delete(IconsDirectory + @"\" + ShortcutName.Text + ".png");
            }
        }
    }
}
