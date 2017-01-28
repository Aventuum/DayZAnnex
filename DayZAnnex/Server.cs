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
        public ObservableCollection<ServerListInfo> MainServerList = new ObservableCollection<ServerListInfo>();        
        ObservableCollection<ServerListInfo> FilteredList = new ObservableCollection<ServerListInfo>();

        static QueryMaster.MasterServer.Server masterServer;
        static AutoResetEvent resetEventObj = new AutoResetEvent(false);

        static List<IPEndPoint> ServerEndP = new List<IPEndPoint>();
        static List<ServerInfo> ServerInfoList = new List<ServerInfo>();
        
        MainWindow mainWin;

        public void LoadServers(MainWindow mw)
        {
            mainWin = mw;
            Thread thread = new Thread(LoadServersThread);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
        
        int runningThreads = 0;

        private void LoadServersThread()
        {
            SetStatus("Fetching server list from master server");
            ServerInfoList.Clear();
            ServerEndP.Clear();

            uint appId = 33930; // A2
            string map = "";
            string hostName = "";
            int retries = 1;

            IpFilter filter = new IpFilter();
            filter.AppId = (Game)appId;
            if (!String.IsNullOrEmpty(map))
                filter.Map = map;
            if (!String.IsNullOrEmpty(hostName))
                filter.HostName = hostName;
            masterServer = MasterQuery.GetServerInstance(MasterQuery.SourceServerEndPoint, retries: retries, attemptCallback: x => Console.WriteLine("\nAttempt " + x + " : "));
            masterServer.GetAddresses(QueryMaster.MasterServer.Region.Rest_of_the_world, recv, filter, batchCount: 1);
            resetEventObj.WaitOne();
            masterServer.Dispose();

            foreach (IPEndPoint endp in ServerEndP)
            {
                //Limit the number of servers loaded. Used for dev purposes
                //if (count > 5)
                //{
                //    continue;
                //}

                SetStatus(string.Format("Getting info for servers ({0}/{1}) ({2} Threads running, {3} max)", count.ToString(), ServerEndP.Count().ToString(), runningThreads.ToString(), mainWin.settings.maxThreads.ToString()));

                int maxThreads = mainWin.settings.maxThreads;
                while (runningThreads >= maxThreads && !(maxThreads <= 0))
                {
                    
                }
                (new Thread(() => { GetInfoThread(endp); })).Start();
            }

            while (runningThreads != 0)
            {
                //Lets the last few threads finish loading before continuing
            }
            SetStatus(string.Format("{0} servers loaded, {1} timed out", (count - nullservers).ToString(), nullservers.ToString()));

            mainWin.Dispatcher.Invoke((Action)(() =>
            {
                mainWin.ProgressRect.Width = double.NaN;
                mainWin.ProgressRect.HorizontalAlignment = HorizontalAlignment.Stretch;
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

                mainWin.Dispatcher.Invoke((Action)(() =>
                {
                    MainServerList.Add(server);

                    if (IsFiltered(server))
                    {
                        AddItem(server);
                    }
                    runningThreads--;
                }));
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
                SetStatus("Updating server info: " + serverInfo.Name);
                ServerListInfo server = GetServerInfo(serverInfo);

                mainWin.Dispatcher.Invoke((Action)(() =>
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

                    ServerListInfo selectedItem = mainWin.MainServerGrid.SelectedItem as ServerListInfo;
                    int selectedIndex = mainWin.serverCollection.IndexOf(serverInfo);

                    if (mainWin.serverCollection[index] == server)
                        mainWin.ShowDisplayPanel(server);
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
            string Mod = mainWin.Filter_Mod.Text;
            string Map = mainWin.Filter_Map.Text;
            bool HidePassworded = mainWin.Filter_HidePassword.IsChecked.Value;
            
            if (!string.IsNullOrEmpty(Name) && !serverInfo.Name.ToLower().Contains(Name)) { filtered = false; }
            if (MinPing > 0 && serverInfo.Ping < MinPing) { filtered = false; }
            if (MaxPing > 0 && serverInfo.Ping > MaxPing) { filtered = false; }
            if (HideUnresponsive && serverInfo.Ping == 9999) { filtered = false; }
            if (MinPlayers > 0 && long.Parse(serverInfo.Players.Split('/')[0]) < MinPlayers) { filtered = false; }
            if (MaxPlayers > 0 && long.Parse(serverInfo.Players.Split('/')[0]) > MaxPlayers) { filtered = false; }
            if (HideEmpty && long.Parse(serverInfo.Players.Split('/')[0]) == 0) { filtered = false; }
            if (HideFull && serverInfo.Players.Split('/')[0] == serverInfo.Players.Split('/')[1]) { filtered = false; }
            //if (!(Mod == "All" || Mod == "")) { FilteredList = new ObservableCollection<ServerListInfo>(FilteredList.Where(x => x.ModInfo.Split(';').Contains(Mod))); }
            //if (!(Map == "All" || Mod == "")) { FilteredList = new ObservableCollection<ServerListInfo>(FilteredList.Where(x => x.Map == Map)); }
            if (HidePassworded && serverInfo.Passworded) { filtered = false; }

            return filtered;
        }
    }
}
