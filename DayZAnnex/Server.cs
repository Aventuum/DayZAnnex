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
                int maxThreads = mainWin.settings.maxThreads;
                while (runningThreads >= maxThreads && !(maxThreads <= 0))
                {
                    
                }
                runningThreads++;
                GetInfoThread(endp);
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
            var t = new Thread(() => GetServerInfo(host));
            t.Start();
            return t;
        }

        int count = 0;
        int nullservers = 0;

        private object Dispatcher { get; set; }

        private void GetServerInfo(IPEndPoint host)
        {
            uint appId = 33930; // A2
            string address = host.Address.ToString();
            ushort port = ushort.Parse(host.Port.ToString());
            int retries = 1;
            count++;
            SetStatus(string.Format("Getting info for servers ({0}/{1}) ({2} Threads running, {3} max)", count.ToString(), ServerEndP.Count().ToString(), runningThreads.ToString(), mainWin.settings.maxThreads.ToString()));
            using (var server = ServerQuery.GetServerInstance((Game)appId, address, port, throwExceptions: false, retries: retries, sendTimeout: 1000, receiveTimeout: 1000))
            {
                //Get Server Information
                var serverInfo = server.GetInfo(x => Console.WriteLine("Fetching Server Information, Attempt " + x));
                var serverRule = server.GetRules(x => Console.WriteLine("Fetching Server Information, Attempt " + x));
                var serverPlayers = server.GetPlayers(x => Console.WriteLine("Fetching Server Information, Attempt " + x));

                if (serverInfo != null && serverRule != null && serverPlayers != null)
                {
                    mainWin.Dispatcher.Invoke((Action)(() =>
                    {
                        AddServerItem(serverInfo, serverRule, serverPlayers);
                    }));
                }
                else
                {
                    mainWin.Dispatcher.Invoke((Action)(() =>
                    {
                        AddNullServerItem(host);
                    }));
                    nullservers++;
                }
                mainWin.Dispatcher.Invoke((Action)(() =>
                {
                    mainWin.ProgressRect.Width = (mainWin.StatusGrid.ActualWidth / ServerEndP.Count()) * count;
                }));
            }
            runningThreads--;
        }

        private void SetStatus(string Text)
        {
            mainWin.Dispatcher.Invoke((Action)(() =>
            {
                mainWin.LoadingText.Text = Text;
            }));
        }

        private void AddServerItem(ServerInfo ServerDetails, QueryMasterCollection<Rule> ServerRules, QueryMasterCollection<PlayerInfo> PlayerList)
        {
            ServerListInfo ServerItem = new ServerListInfo();
            ServerItem.Host = ServerDetails.Address.Split(':')[0];
            ServerItem.Port = ServerDetails.ExtraInfo.Port;
            ServerItem.Name = ServerDetails.Name;
            ServerItem.Map = ServerDetails.Map;
            ServerItem.Players = ServerDetails.Players + "/" + ServerDetails.MaxPlayers;
            ServerItem.CurrentPlayers = ServerDetails.Players;
            ServerItem.MaxPlayers = ServerDetails.MaxPlayers;
            ServerItem.Passworded = ServerDetails.IsPrivate;
            ServerItem.Ping = ServerDetails.Ping;
            ServerItem.BattleEye = true;
            ServerItem.PlayerList = PlayerList;
            ServerItem.ServerRules = ServerRules;

            string modString = "";
            foreach (Rule serverrule in ServerRules)
            {
                if (serverrule.Name.StartsWith("modNames"))
                {
                    modString += serverrule.Value;
                }
            }
            if (modString.EndsWith(";")) { modString.Take(modString.Length - 1); }
            ServerItem.ModInfo = modString;

            MainServerList.Add(ServerItem);

            if (IsFiltered(ServerItem))
            {
                AddItem(ServerItem);
            }
        }

        private void AddNullServerItem(IPEndPoint host)
        {
            ServerListInfo ServerItem = new ServerListInfo();
            ServerItem.Host = host.ToString();
            ServerItem.Port = int.Parse(host.ToString().Split(':')[1]);
            ServerItem.Name = host.ToString();
            ServerItem.Map = "";
            ServerItem.Players = "0/0";
            ServerItem.CurrentPlayers = 0;
            ServerItem.MaxPlayers = 0;
            ServerItem.Passworded = false;
            ServerItem.Ping = 9999;
            ServerItem.BattleEye = false;

            MainServerList.Add(ServerItem);
            if (IsFiltered(ServerItem))
            {
                AddItem(ServerItem);
            }
        }

        private string PingIP(string IPAddress)
        {
            Ping pingSender = new Ping();
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 1000;
            PingOptions options = new PingOptions(64, true);
            PingReply reply = pingSender.Send(IPAddress, timeout, buffer, options);
            if (reply.Status == IPStatus.Success)
            {
                return reply.RoundtripTime.ToString();
            }
            return "999";
        }

        public void ReloadDisplay()
        {
            mainWin.MainServerGrid.Items.Clear();
            mainWin.displayedList.Clear();
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
            mainWin.MainServerGrid.Items.Add(serverInfo);
            mainWin.displayedList.Add(serverInfo);
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
