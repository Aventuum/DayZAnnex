using QueryMaster;
using QueryMaster.GameServer;
using QueryMaster.MasterServer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace DayZAnnex
{
    public class ServerListInfo
    {
        public string Host { get; set; }
        public string Map { get; set; }
        public string Name { get; set; }
        public string Players { get; set; }
        public int CurrentPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public int Port { get; set; }
        public int GamePort { get; set; }
        public long Ping { get; set; }
        public string GameVer { get; set; }
        public string ModInfo { get; set; }
        public bool BattleEye { get; set; }
        public bool Passworded { get; set; }
        public QueryMasterCollection<PlayerInfo> PlayerList { get; set; }
        public QueryMasterCollection<Rule> ServerRules { get; set; }
    }

    class Server
    {
        // All servers retrieved from the master server query are kept here in a ServerListInfo collection
        public List<ServerListInfo> MainServerList = new List<ServerListInfo>();

        // Temporary list used to store servers while loading, is copied to MainServerList and MainWindow.ServerCollection
        List<ServerListInfo> tmpList = new List<ServerListInfo>();

        static QueryMaster.MasterServer.Server masterServer;
        static AutoResetEvent resetEventObj = new AutoResetEvent(false);

        // Endpoints from master server query are kept here and used for game server query
        static List<IPEndPoint> ServerEndP = new List<IPEndPoint>();
        
        // Maybe not best practise... But MainWindow is passed to Server through LoadServers, and set here. That way, all the methods in Server can access MainWindow
        // can access methods and controls from MainWindow
        MainWindow mainWin;

        bool isUpdating = false;

        public void LoadServers(MainWindow mw)
        {
            mainWin = mw;
            LoadServerInfo();
            //Thread thread = new Thread(LoadServersThread);
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start();
        }
        
        int runningThreads = 0;

        private List<IPEndPoint> GetServerEndPoints()
        {
            SetStatus("Fetching server list from master server");
            ServerEndP.Clear();
            isUpdating = true;

            uint appId = 33930;
            int retries = 1;
            IpFilter filter = new IpFilter() { AppId = (Game)appId };

            masterServer = MasterQuery.GetServerInstance(MasterQuery.SourceServerEndPoint, retries: retries, attemptCallback: x => Console.WriteLine("\nAttempt " + x + " : "));
            masterServer.GetAddresses(QueryMaster.MasterServer.Region.Rest_of_the_world, recv, filter, batchCount: 1);
            resetEventObj.WaitOne();
            masterServer.Dispose();

            return ServerEndP;
        }

        private void LoadServersThread()
        {
            List<IPEndPoint> endPoints = GetServerEndPoints();
            tmpList.Clear();
            foreach (IPEndPoint endp in endPoints)
            {
                //if (count > 5) { break; } // Stops loading servers. Useful for testing

                SetStatus(string.Format("Getting info for servers ({0}/{1}) ({2} Threads running, {3} max)", count.ToString(), endPoints.Count().ToString(), runningThreads.ToString(), mainWin.settings.maxThreads.ToString()));

                // Do nothing while maximum threads are running
                int maxThreads = mainWin.settings.maxThreads;
                while (runningThreads >= maxThreads && !(maxThreads <= 0)) { }

                // Start new thread to query server for information
                (new Thread(() => { GetInfoThread(endp); })).Start();
            }

            // Do nothing while there are still threads running
            while (runningThreads != 0) { }

            SetStatus(string.Format("{0} servers loaded, {1} timed out", (count - nullservers).ToString(), nullservers.ToString()));
            isUpdating = false;
            
            mainWin.Dispatcher.Invoke((Action)(() =>
            {
                mainWin.serverCollection.Clear();
                MainServerList.Clear();
                foreach (ServerListInfo serverInfo in tmpList)
                {
                    mainWin.serverCollection.Add(serverInfo);
                    MainServerList.Add(serverInfo);
                }

                mainWin.ProgressRect.Width = double.NaN;
                mainWin.ProgressRect.HorizontalAlignment = HorizontalAlignment.Stretch;
                SaveServerInfo();
                ReloadDisplay();
            }));
        }
        
        static void recv(BatchInfo info)
        {
            bool exit = false;
            Console.WriteLine("Received " + info.ReceivedEndpoints.Count + " ip(s).");
            ServerEndP.AddRange(info.ReceivedEndpoints);
            Console.WriteLine(info.ReceivedEndpoints);
            if (info.IsLastBatch)
            {
                Console.WriteLine("Received all ip(s)");
                exit = true;
            }
            else
            {
                masterServer.GetNextBatch(1);
            }
            if (exit)
            {
                resetEventObj.Set();
            }
        }

        private Thread GetInfoThread(IPEndPoint host)
        {
            runningThreads++;

            var t = new Thread(() => 
            {
                ServerListInfo server = GetServerInfo(new ServerListInfo() { Host = host.Address.ToString(), Port = host.Port });

                // Check if server already exists in serverCollection. If it does, update the item, if not then add it
                if (MainServerList.ToList().Any(x => x.Host == server.Host && x.Port == host.Port))
                {
                    mainWin.Dispatcher.Invoke((Action)(() =>
                    {
                        tmpList.Add(server);

                        int index = mainWin.serverCollection.IndexOf(mainWin.serverCollection.Where(x => x.Host == server.Host && x.Port == host.Port).FirstOrDefault());
                        if (index != -1)
                        {
                            UpdateItem(server, index);
                        }
                    }));
                }
                else
                {
                    mainWin.Dispatcher.Invoke((Action)(() =>
                    {
                        tmpList.Add(server);

                        //if (IsFiltered(server))
                        //    AddItem(server);
                    }));
                }
                runningThreads--;
            });
            t.Start();
            return t;
        }

        int count = 0;
        int nullservers = 0;

        public ServerListInfo GetServerInfo(ServerListInfo serverListInfo)
        {
            uint appId = 33930; // A2
            string address = serverListInfo.Host;
            ushort port = ushort.Parse(serverListInfo.Port.ToString());
            int retries = 1;
            count++;
            ServerListInfo ServerItem = serverListInfo;
            using (var server = ServerQuery.GetServerInstance((Game)appId, address, port, throwExceptions: false, retries: retries, sendTimeout: 1000, receiveTimeout: 1000))
            {
                var serverInfo = server.GetInfo(x => Console.WriteLine("Fetching Server Information, Attempt " + x));
                var serverRule = server.GetRules(x => Console.WriteLine("Fetching Server Information, Attempt " + x));
                var serverPlayers = server.GetPlayers(x => Console.WriteLine("Fetching Server Information, Attempt " + x));

                if (serverInfo != null && serverRule != null && serverPlayers != null)
                {
                    ServerItem.Host = serverInfo.Address.Split(':')[0];
                    ServerItem.Port = int.Parse(serverInfo.Address.Split(':')[1]);
                    ServerItem.GamePort = serverInfo.ExtraInfo.Port;
                    ServerItem.Name = serverInfo.Name;
                    ServerItem.Map = serverInfo.Map;
                    ServerItem.Players = serverInfo.Players + "/" + serverInfo.MaxPlayers;
                    ServerItem.CurrentPlayers = serverInfo.Players;
                    ServerItem.MaxPlayers = serverInfo.MaxPlayers;
                    ServerItem.Passworded = serverInfo.IsPrivate;
                    ServerItem.Ping = serverInfo.Ping;
                    ServerItem.BattleEye = true;
                    ServerItem.PlayerList = serverPlayers;
                    ServerItem.ServerRules = serverRule;

                    string modString = "";
                    foreach (Rule serverrule in serverRule)
                    {
                        if (serverrule.Name.StartsWith("modNames"))
                        {
                            modString += serverrule.Value;
                        }
                    }
                    if (modString.EndsWith(";")) { modString.Take(modString.Length - 1); }
                    ServerItem.ModInfo = modString;

                    //if (!mainWin.Filter_Map.Items.Contains(serverInfo.Map))
                    //{
                    //    mainWin.Dispatcher.Invoke((Action)(() =>
                    //    {
                    //        mainWin.Filter_Map.Items.Add(serverInfo.Map);
                    //    }));
                    //}
                }
                else
                {
                    ServerItem.Host = address;
                    ServerItem.Port = int.Parse(port.ToString());
                    ServerItem.GamePort = int.Parse(port.ToString());
                    ServerItem.Name = address;
                    ServerItem.Map = "";
                    ServerItem.Players = "0/0";
                    ServerItem.CurrentPlayers = 0;
                    ServerItem.MaxPlayers = 0;
                    ServerItem.Passworded = false;
                    ServerItem.Ping = 9999;
                    ServerItem.BattleEye = false;
                    nullservers++;
                }
            }

            return ServerItem;
        }

        private void SetStatus(string Text)
        {
            mainWin.Dispatcher.Invoke((Action)(() =>
            {
                mainWin.LoadingText.Text = Text;
            }));
        }

        public void ReloadDisplay()
        {
            mainWin.serverCollection.Clear();
            foreach (ServerListInfo serverInfo in MainServerList)
            {
                if (IsFiltered(serverInfo))
                {
                    AddItem(serverInfo);
                }
            }
        }

        public void AddItem(ServerListInfo serverInfo)
        {
            mainWin.serverCollection.Add(serverInfo);
        }

        public void UpdateItem(ServerListInfo serverInfo, int index)
        {
            IPEndPoint endp = new IPEndPoint(IPAddress.Parse(serverInfo.Host), serverInfo.Port);

            var t = new Thread(() =>
            {
                if(!isUpdating)
                    SetStatus("Updating server info: " + serverInfo.Name);

                ServerListInfo server = GetServerInfo(serverInfo);

                mainWin.Dispatcher.Invoke((Action)(() =>
                {
                    if (IsFiltered(server))
                    {
                        mainWin.serverCollection[index].BattleEye = server.BattleEye;
                        mainWin.serverCollection[index].CurrentPlayers = server.CurrentPlayers;
                        mainWin.serverCollection[index].GamePort = server.GamePort;
                        mainWin.serverCollection[index].GameVer = server.GameVer;
                        mainWin.serverCollection[index].Host = server.Host;
                        mainWin.serverCollection[index].Map = server.Map;
                        mainWin.serverCollection[index].MaxPlayers = server.MaxPlayers;
                        mainWin.serverCollection[index].ModInfo = server.ModInfo;
                        mainWin.serverCollection[index].Name = server.Name;
                        mainWin.serverCollection[index].Passworded = server.Passworded;
                        mainWin.serverCollection[index].Ping = server.Ping;
                        mainWin.serverCollection[index].PlayerList = server.PlayerList;
                        mainWin.serverCollection[index].Players = server.Players;
                        mainWin.serverCollection[index].Port = server.Port;
                        mainWin.serverCollection[index].ServerRules = server.ServerRules;
                        mainWin.serverCollectionView.Refresh();
                    }

                    int mainServerIndex = MainServerList.IndexOf(MainServerList.Where(x => x.Host == server.Host && x.Port == server.Port).FirstOrDefault());
                    if (mainServerIndex != -1)
                        MainServerList[mainServerIndex] = server;

                    ServerListInfo selectedItem = mainWin.MainServerGrid.SelectedItem as ServerListInfo;
                    int selectedIndex = mainWin.serverCollection.IndexOf(serverInfo);

                    if (mainWin.MainServerGrid.SelectedIndex != -1)
                        mainWin.UpdateDisplayPanel();

                    if (!isUpdating)
                        SetStatus("");
                }));
            });
            t.Start();
        }

        public bool IsFiltered(ServerListInfo serverInfo)
        {
            bool filtered = true;

            string Name = mainWin.Filter_Name.Text.ToLower();
            long MinPing = long.Parse(string.IsNullOrEmpty(mainWin.Filter_PingMin.Text) ? "0" : mainWin.Filter_PingMin.Text);
            long MaxPing = long.Parse(string.IsNullOrEmpty(mainWin.Filter_PingMax.Text) ? "0" : mainWin.Filter_PingMax.Text);
            bool HideUnresponsive = mainWin.Filter_HideUnresponsive.IsChecked.Value;
            long MinPlayers = long.Parse(string.IsNullOrEmpty(mainWin.Filter_PlayersMin.Text) ? "0" : mainWin.Filter_PlayersMin.Text);
            long MaxPlayers = long.Parse(string.IsNullOrEmpty(mainWin.Filter_PlayersMax.Text) ? "0" : mainWin.Filter_PlayersMax.Text);
            bool HideEmpty = mainWin.Filter_HidePlayersEmpty.IsChecked.Value;
            bool HideFull = mainWin.Filter_HidePlayersFull.IsChecked.Value;
            //string Mod = mainWin.Filter_Mod.Text;
            //string Map = mainWin.Filter_Map.Text.ToLower();
            bool HidePassworded = mainWin.Filter_HidePassword.IsChecked.Value;
            
            if (!string.IsNullOrEmpty(Name) && !serverInfo.Name.ToLower().Contains(Name)) { filtered = false; }
            if (MinPing > 0 && serverInfo.Ping < MinPing) { filtered = false; }
            if (MaxPing > 0 && serverInfo.Ping > MaxPing) { filtered = false; }
            if (HideUnresponsive && serverInfo.Ping == 9999) { filtered = false; }
            if (MinPlayers > 0 && long.Parse(serverInfo.Players.Split('/')[0]) < MinPlayers) { filtered = false; }
            if (MaxPlayers > 0 && long.Parse(serverInfo.Players.Split('/')[0]) > MaxPlayers) { filtered = false; }
            if (HideEmpty && long.Parse(serverInfo.Players.Split('/')[0]) == 0) { filtered = false; }
            if (HideFull && serverInfo.Players.Split('/')[0] == serverInfo.Players.Split('/')[1]) { filtered = false; }
            //if (!(Map == "All" || Map == "") && serverInfo.Map.ToLower() != Map) { filtered = false; }
            if (HidePassworded && serverInfo.Passworded) { filtered = false; }

            return filtered;
        }

        public void SaveServerInfo()
        {
            string serverdoc = mainWin.settings.annexAppFolder + "\\servers.xml";
            XDocument xdoc = new XDocument(new XElement("servers"));
            foreach (ServerListInfo serverInfo in mainWin.serverCollection)
            {
                XElement serverElement = new XElement("server",
                        new XElement("host", serverInfo.Host),
                        new XElement("map", serverInfo.Map),
                        new XElement("port", serverInfo.Port),
                        new XElement("gameport", serverInfo.GamePort.ToString()),
                        new XElement("name", serverInfo.Name),
                        new XElement("players", serverInfo.Players),
                        new XElement("currentplayers", serverInfo.CurrentPlayers.ToString()),
                        new XElement("maxplayers", serverInfo.MaxPlayers.ToString()),
                        new XElement("ping", serverInfo.Ping.ToString()),
                        new XElement("gamever", serverInfo.GameVer),
                        new XElement("modinfo", serverInfo.ModInfo),
                        new XElement("battleye", serverInfo.BattleEye.ToString()),
                        new XElement("passworded", serverInfo.Passworded.ToString()));

                if (serverInfo.Ping != 9999 && serverInfo.PlayerList != null)
                {
                    XElement playerlistElement = new XElement("playerlist");

                    foreach (PlayerInfo pInfo in serverInfo.PlayerList)
                    {
                        playerlistElement.Add(new XElement("player",
                            new XElement("name", pInfo.Name),
                            new XElement("score", pInfo.Score),
                            new XElement("time", pInfo.Time.ToString())
                            ));
                    }

                    XElement rulesElement = new XElement("rules");

                    foreach (Rule rule in serverInfo.ServerRules)
                    {
                        rulesElement.Add(new XElement("rule",
                            new XElement("name", rule.Name),
                            new XElement("value", rule.Value)
                            ));
                    }

                    serverElement.Add(playerlistElement);
                    serverElement.Add(rulesElement);
                }
                xdoc.Element("servers").Add(serverElement);
            }

            xdoc.Save(serverdoc);
        }

        public void LoadServerInfo()
        {
            if (!System.IO.File.Exists(mainWin.settings.annexAppFolder + "\\servers.xml"))
                return;
            
            ObservableCollection<ServerListInfo> newServerCollection = new ObservableCollection<ServerListInfo>();
            XDocument xdoc = XDocument.Parse(System.IO.File.ReadAllText(mainWin.settings.annexAppFolder + "\\servers.xml"));

            mainWin.serverCollection.Clear();

            foreach (XElement element in xdoc.Element("servers").Descendants("server"))
            {
                try
                {
                    ServerListInfo server = new ServerListInfo();
                    server.Host = element.Element("host").Value;
                    server.Map = element.Element("map").Value;
                    server.Port = int.Parse(element.Element("port").Value);
                    server.GamePort = int.Parse(element.Element("gameport").Value);
                    server.Name = element.Element("name").Value;
                    server.Players = element.Element("players").Value;
                    server.CurrentPlayers = int.Parse(element.Element("currentplayers").Value);
                    server.MaxPlayers = int.Parse(element.Element("maxplayers").Value);
                    server.Ping = int.Parse(element.Element("ping").Value);
                    server.GameVer = element.Element("gamever").Value;
                    server.ModInfo = element.Element("modinfo").Value;
                    server.BattleEye = bool.Parse(element.Element("battleye").Value);
                    server.Passworded = bool.Parse(element.Element("passworded").Value);

                    if (element.Elements("playerlist").Any())
                    {
                        List<PlayerInfo> players = new List<PlayerInfo>();
                        foreach (XElement playerElement in element.Element("playerlist").Descendants("player"))
                        {
                            players.Add(new PlayerInfo
                            {
                                Name = playerElement.Element("name").Value,
                                Score = long.Parse(playerElement.Element("score").Value),
                                Time = TimeSpan.Parse(playerElement.Element("time").Value)
                            });
                        }

                        if (!(players.Count == 0))
                            server.PlayerList = new QueryMasterCollection<PlayerInfo>(players);
                    }

                    if (element.Elements("rules").Any())
                    {
                        List<Rule> rules = new List<Rule>();
                        foreach (XElement ruleElement in element.Element("rules").Descendants("rule"))
                        {
                            rules.Add(new Rule
                            {
                                Name = ruleElement.Element("name").Value,
                                Value = ruleElement.Element("value").Value
                            });
                        }

                        if (!(rules.Count == 0))
                            server.ServerRules = new QueryMasterCollection<Rule>(rules);
                    }
                    mainWin.serverCollection.Add(server);
                    MainServerList.Add(server);
                }
                catch (NullReferenceException)
                {
                }
            }
        }
    }
}
