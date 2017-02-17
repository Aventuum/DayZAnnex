using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DayZAnnex
{
    public class LaunchParams
    {
        public bool noSplash { get; set; }
        public bool noLogs { get; set; }
        public bool noPause { get; set; }
        public bool windowMode { get; set; }
        public bool scriptErrors { get; set; }
        public string profile { get; set; }
    }
    public class Settings
    {
        
        public bool refreshInBackground { get; set; }
        public bool autoRefreshServers { get; set; }
        public string modPath { get; set; }
        public string armaPath { get; set; }
        public string oaPath { get; set; }
        public LaunchParams launchOptions { get; set; }
        public int maxThreads { get; set; }

        public string annexAppFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\DayZAnnex";

        public void LoadSettings()
        {
            if (!Directory.Exists(annexAppFolder))
                Directory.CreateDirectory(annexAppFolder);

            if (!File.Exists(annexAppFolder + "\\config.xml"))
                LoadDefaults(true);

            string config = annexAppFolder + "\\config.xml";

            XDocument xdoc = XDocument.Parse(File.ReadAllText(config));

            try
            {
                refreshInBackground = bool.Parse(xdoc.Element("settings").Element("refreshinbackground").Value);
                autoRefreshServers = bool.Parse(xdoc.Element("settings").Element("autorefreshservers").Value);
                modPath = xdoc.Element("settings").Element("modpath").Value;
                armaPath = xdoc.Element("settings").Element("armapath").Value;
                oaPath = xdoc.Element("settings").Element("oapath").Value;
                LaunchParams lparams = new LaunchParams();
                lparams.noSplash = bool.Parse(xdoc.Element("settings").Element("launchoptions").Element("nosplash").Value);
                lparams.noLogs = bool.Parse(xdoc.Element("settings").Element("launchoptions").Element("nologs").Value);
                lparams.noPause = bool.Parse(xdoc.Element("settings").Element("launchoptions").Element("nopause").Value);
                lparams.windowMode = bool.Parse(xdoc.Element("settings").Element("launchoptions").Element("windowmode").Value);
                lparams.scriptErrors = bool.Parse(xdoc.Element("settings").Element("launchoptions").Element("scripterrors").Value);
                lparams.profile = xdoc.Element("settings").Element("launchoptions").Element("profile").Value;
                launchOptions = lparams;
                maxThreads = int.Parse(xdoc.Element("settings").Element("maxthreads").Value);
            }
            catch (NullReferenceException e)
            {
                if(System.Windows.MessageBox.Show("NullReferenceException loading settings, using defaults. Overwrite broken settings file with defaults?\n\n" + e.StackTrace, "NullReferenceException", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Error) == System.Windows.MessageBoxResult.Yes)
                {
                    LoadDefaults(true);
                }
                else
                {
                    LoadDefaults();
                }
            }
        }

        public void LoadDefaults(bool saveDefaults = false)
        {
            refreshInBackground = true;
            autoRefreshServers = true;
            LocatePaths();
            LaunchParams lparams = new LaunchParams();
            lparams.noLogs = false;
            lparams.noPause = false;
            lparams.noSplash = false;
            lparams.windowMode = false;
            lparams.scriptErrors = false;
            lparams.profile = "";
            launchOptions = lparams;
            maxThreads = 10;

            if (saveDefaults)
                SaveSettings();
        }

        public void SaveSettings()
        {
            string config = annexAppFolder + "\\config.xml";
            XDocument xdoc =
                new XDocument(
                    new XElement("settings",
                        new XElement("refreshinbackground", refreshInBackground.ToString()),
                        new XElement("autorefreshservers", autoRefreshServers.ToString()),
                        new XElement("modpath", modPath),
                        new XElement("armapath", armaPath),
                        new XElement("oapath", oaPath),
                        new XElement("launchoptions", 
                            new XElement("nologs", launchOptions.noLogs),
                            new XElement("nopause", launchOptions.noPause),
                            new XElement("nosplash", launchOptions.noSplash),
                            new XElement("windowmode", launchOptions.windowMode),
                            new XElement("scripterrors", launchOptions.scriptErrors),
                            new XElement("profile", launchOptions.profile)
                            ),
                        new XElement("maxthreads", maxThreads)
                        )
                    );

            xdoc.Save(config);
        }

        private void LocatePaths()
        {
            List<string> locations = new List<string>
            {
                "{drive}Games\\{game}",
                "{drive}SteamApps\\common\\{game}",
                "{drive}Steam\\SteamApps\\common\\{game}",
                "{drive}Program Files (x86)\\Steam\\SteamApps\\common\\{game}"
            };

            List<string> games = new List<string>
            {
                "Arma 2",
                "Arma 2 Operation Arrowhead"
            };

            armaPath = "";
            oaPath = "";

            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach(DriveInfo drive in drives)
            {
                foreach(string location in locations)
                {
                    foreach (string game in games)
                    {
                        string fullPath = location.Replace("{drive}", drive.Name).Replace("{game}", game);
                        if(Directory.Exists(fullPath))
                        {
                            switch (game)
                            {
                                case "Arma 2":
                                    armaPath = fullPath;
                                    System.Windows.MessageBox.Show("Arma 2 path found:\n" + fullPath);
                                    break;
                                case "Arma 2 Operation Arrowhead":
                                    oaPath = fullPath;
                                    System.Windows.MessageBox.Show("Arma 2 Operation Arrowhead path found:\n" + fullPath);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            if(string.IsNullOrEmpty(armaPath))
                System.Windows.MessageBox.Show("Arma 2 path not automatically detected, please browse for it under the Settings tab");

            if (string.IsNullOrEmpty(oaPath))
                System.Windows.MessageBox.Show("Arma 2 Operation Arrowhead path not automatically detected, please browse for it under the Settings tab");
        }

        public List<string> GetProfiles()
        {
            string profileDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ArmA 2";
            if (Directory.Exists(profileDir))
            {
                List<string> profiles = Directory.GetFiles(profileDir).Where(x => x.EndsWith("ArmA2OAProfile")).Select(x => x.Split('.')[0]).Select(x => Path.GetFileName(x)).Distinct().ToList();
                profiles.Insert(0, "Default");
                return profiles;
            }

            return null;
        }
    }
}
