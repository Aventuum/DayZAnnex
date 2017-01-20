using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DayZAnnex
{
    class LaunchParams
    {
        public bool noSplash { get; set; }
        public bool noLogs { get; set; }
        public bool noPause { get; set; }
        public bool windowMode { get; set; }
    }
    class Settings
    {
        
        public bool autoLoadServers { get; set; }
        public bool autoRefreshServers { get; set; }
        public string modPath { get; set; }
        public string armaPath { get; set; }
        public string oaPath { get; set; }
        public LaunchParams launchOptions { get; set; }
        public string profile { get; set; }

        string annexAppFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\DayZAnnex";

        public void LoadSettings()
        {
            if (!Directory.Exists(annexAppFolder))
                Directory.CreateDirectory(annexAppFolder);

            if (!File.Exists(annexAppFolder + "\\config.xml"))
                LoadDefaults(true);

            string config = annexAppFolder + "\\config.xml";

            XDocument xdoc = XDocument.Parse(File.ReadAllText(config));

            autoLoadServers = bool.Parse(xdoc.Element("settings").Element("autoloadservers").Value);
            autoRefreshServers = bool.Parse(xdoc.Element("settings").Element("autorefreshservers").Value);
            modPath = xdoc.Element("settings").Element("modpath").Value;
            armaPath = xdoc.Element("settings").Element("armapath").Value;
            oaPath = xdoc.Element("settings").Element("oapath").Value;
            LaunchParams lparams = new LaunchParams();
            lparams.noSplash = bool.Parse(xdoc.Element("settings").Element("launchoptions").Element("nosplash").Value);
            lparams.noLogs = bool.Parse(xdoc.Element("settings").Element("launchoptions").Element("nologs").Value);
            lparams.noPause = bool.Parse(xdoc.Element("settings").Element("launchoptions").Element("nopause").Value);
            lparams.windowMode = bool.Parse(xdoc.Element("settings").Element("launchoptions").Element("windowmode").Value);
            launchOptions = lparams;
            profile = xdoc.Element("settings").Element("profile").Value;
        }

        public void LoadDefaults(bool saveDefaults = false)
        {
            autoLoadServers = true;
            autoRefreshServers = true;
            LocatePaths();
            //modPath = "";
            //armaPath = "";
            //oaPath = "";
            LaunchParams lparams = new LaunchParams();
            lparams.noLogs = false;
            lparams.noPause = false;
            lparams.noSplash = false;
            lparams.windowMode = false;
            launchOptions = lparams;
            profile = "";

            if (saveDefaults)
                SaveSettings();
        }

        public void SaveSettings()
        {
            string config = annexAppFolder + "\\config.xml";
            XDocument xdoc =
                new XDocument(
                    new XElement("settings",
                        new XElement("autoloadservers", autoLoadServers.ToString()),
                        new XElement("autorefreshservers", autoRefreshServers.ToString()),
                        new XElement("modpath", modPath),
                        new XElement("armapath", armaPath),
                        new XElement("oapath", oaPath),
                        new XElement("launchoptions", 
                            new XElement("nologs", launchOptions.noLogs),
                            new XElement("nopause", launchOptions.noPause),
                            new XElement("nosplash", launchOptions.noSplash),
                            new XElement("windowmode", launchOptions.windowMode)
                            ),
                        new XElement("profile", profile)
                        )
                    );

            xdoc.Save(config);
        }

        private void LocatePaths()
        {
            List<string> locations = new List<string>
            {
                "{drive}Games\\{game}\\",
                "{drive}SteamApps\\common\\{game}\\",
                "{drive}Steam\\SteamApps\\common\\{game}\\",
                "{drive}Program Files (x86)\\Steam\\SteamApps\\common\\{game}\\"
            };

            List<string> games = new List<string>
            {
                "Arma 2",
                "Arma 2 Operation Arrowhead"
            };

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
    }
}
