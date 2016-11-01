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

namespace DayZReborn
{
    public class ServerListInfo
    {
        public string Host { get; set; }
        public string Map { get; set; }
        public string Name { get; set; }
        public string Players { get; set; }
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
        //Contains all servers
        public ObservableCollection<ServerListInfo> MainServerList = new ObservableCollection<ServerListInfo>();

        //Contains only servers matching the current filter
        ObservableCollection<ServerListInfo> FilteredList = new ObservableCollection<ServerListInfo>();

        static QueryMaster.MasterServer.Server masterServer;
        static AutoResetEvent resetEventObj = new AutoResetEvent(false);

        //List containing IP's of favourited and joined servers. Functionality removed but will be re-added.
        //static List<string> HistoryEndP = new List<string>();   // Format: ip,lastjoined
        //static List<string> FavouriteEndP = new List<string>(); // Example: 192.168.1.1:5656,19/08/2016 02-49
        // lastjoined format must be DD/MM/YYYY HH-MM

        static List<IPEndPoint> ServerEndP = new List<IPEndPoint>();    // The Master Server query appends IPs here, so that
                                                                        // we can iterate through and use Server Query on each IP.

        // Server Lists
        static List<ServerInfo> ServerInfoList = new List<ServerInfo>();
        //Unused lists
        //static List<ServerInfo> ServerHistoryList = new List<ServerInfo>();
        //static List<ServerInfo> ServerFavouritesList = new List<ServerInfo>();

        MainWindow mainWin = new MainWindow();

        public List<ServerListInfo> getServerList(bool filtered = true)
        {
            if (filtered)
            {
                ApplyFilters();
                SortList();
                return FilteredList.ToList();
            }
            else
            {
                return MainServerList.ToList();
            }
        }

        private void LoadServers()
        {
            //Load servers on another thread, so that it doesnt freaze the UI.
            Thread thread = new Thread(LoadServersThread);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        //Track running threads, used for debug purposes usually
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
                //if (count > 50)
                //{
                //    continue;
                //}
                while (runningThreads >= 10)
                {
                    //Wait if there are more than 10 running threads
                    //Im not sure what the disadvantages are of running too many threads, but 10 sounds like a safe number and loads 2000 servers reletively fast
                    //Might add a setting to change max threads
                }
                runningThreads++;
                GetInfoThread(endp);
            }
            while (runningThreads != 0)
            {
                //Lets the last few threads finish loading before continuing
            }

            mainWin.Dispatcher.Invoke((Action)(() =>
            {
                SortList();
            }));
            SetStatus(string.Format("{0} servers loaded, {1} timed out", (count - nullservers).ToString(), nullservers.ToString()));
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

        //Sorting options
        public string sortBy = "Name";
        public bool sortAsc = true;

        private void SortList()
        {
            mainWin.Column_Name_Arrow.Visibility = Visibility.Hidden;
            mainWin.Column_Map_Arrow.Visibility = Visibility.Hidden;
            mainWin.Column_Players_Arrow.Visibility = Visibility.Hidden;
            mainWin.Column_Ping_Arrow.Visibility = Visibility.Hidden;

            PointCollection ascending = new PointCollection() { new Point(x: 0, y: 0), new Point(x: 5, y: 8), new Point(x: 10, y: 0) };
            PointCollection descending = new PointCollection() { new Point(x: 5, y: 0), new Point(x: 0, y: 8), new Point(x: 10, y: 8) };

            switch (sortBy)
            {
                case "Name":
                    if (sortAsc)
                    {
                        FilteredList = new ObservableCollection<ServerListInfo>(FilteredList.OrderBy(i => i.Name));
                        mainWin.Column_Name_Arrow.Points = ascending;
                        mainWin.Column_Name_Arrow.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        FilteredList = new ObservableCollection<ServerListInfo>(FilteredList.OrderByDescending(i => i.Name));
                        mainWin.Column_Name_Arrow.Points = descending;
                        mainWin.Column_Name_Arrow.Visibility = Visibility.Visible;
                    }
                    break;
                case "Map":
                    if (sortAsc)
                    {
                        FilteredList = new ObservableCollection<ServerListInfo>(FilteredList.OrderBy(i => i.Map));
                        mainWin.Column_Map_Arrow.Points = ascending;
                        mainWin.Column_Map_Arrow.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        FilteredList = new ObservableCollection<ServerListInfo>(FilteredList.OrderByDescending(i => i.Map));
                        mainWin.Column_Map_Arrow.Points = descending;
                        mainWin.Column_Map_Arrow.Visibility = Visibility.Visible;
                    }
                    break;
                case "Players":
                    if (sortAsc)
                    {
                        FilteredList = new ObservableCollection<ServerListInfo>(FilteredList.OrderBy(i => int.Parse(i.Players.Split('/')[0])));
                        mainWin.Column_Players_Arrow.Points = ascending;
                        mainWin.Column_Players_Arrow.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        FilteredList = new ObservableCollection<ServerListInfo>(FilteredList.OrderByDescending(i => int.Parse(i.Players.Split('/')[0])));
                        mainWin.Column_Players_Arrow.Points = descending;
                        mainWin.Column_Players_Arrow.Visibility = Visibility.Visible;
                    }
                    break;
                case "Ping":
                    if (sortAsc)
                    {
                        FilteredList = new ObservableCollection<ServerListInfo>(FilteredList.OrderBy(i => i.Ping));
                        mainWin.Column_Ping_Arrow.Points = ascending;
                        mainWin.Column_Ping_Arrow.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        FilteredList = new ObservableCollection<ServerListInfo>(FilteredList.OrderByDescending(i => i.Ping));
                        mainWin.Column_Ping_Arrow.Points = descending;
                        mainWin.Column_Ping_Arrow.Visibility = Visibility.Visible;
                    }
                    break;
                default:
                    FilteredList = new ObservableCollection<ServerListInfo>(FilteredList.OrderBy(i => i.Name));
                    mainWin.Column_Ping_Arrow.Points = ascending;
                    mainWin.Column_Ping_Arrow.Visibility = Visibility.Visible;
                    break;
            }
            mainWin.MainServerListBox.ItemsSource = FilteredList;
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
            SetStatus(string.Format("Getting info for servers ({0}/{1}) ({2} Threads running)", count.ToString(), ServerEndP.Count().ToString(), runningThreads.ToString()));
            using (var server = ServerQuery.GetServerInstance((Game)appId, address, port, throwExceptions: false, retries: retries, sendTimeout: 1000, receiveTimeout: 1000))
            {
                //Get Server Information
                var serverInfo = server.GetInfo(x => Console.WriteLine("Fetching Server Information, Attempt " + x));
                var serverRule = server.GetRules(x => Console.WriteLine("Fetching Server Information, Attempt " + x));
                var serverPlayers = server.GetPlayers(x => Console.WriteLine("Fetching Server Information, Attempt " + x));

                if (serverInfo != null && serverRule != null && serverPlayers != null)
                {
                    //return (new object[] { serverInfo, serverRule, serverPlayers });

                    //ServerInfo serverInfo = (ServerInfo)thisServer[0];
                    //QueryMasterCollection<Rule> ServerRuleCollection = (QueryMasterCollection<Rule>)thisServer[1];
                    //QueryMasterCollection<PlayerInfo> PlayerInfoCollection = (QueryMasterCollection<PlayerInfo>)thisServer[2];

                    mainWin.Dispatcher.Invoke((Action)(() =>
                    {
                        AddServerItem(serverInfo, serverRule, serverPlayers);
                    }));
                }
                else
                {
                    mainWin.Dispatcher.Invoke((Action)(() =>
                    {
                        // TODO: Add null item to list, maybe create AddNullServerItem() method to do this
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
            ServerItem.Host = ServerDetails.Address;
            ServerItem.Port = int.Parse(ServerDetails.Address.Split(':')[1]);
            ServerItem.Name = ServerDetails.Name;
            ServerItem.Map = ServerDetails.Map;
            ServerItem.Players = ServerDetails.Players + "/" + ServerDetails.MaxPlayers;
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
            ApplyFilters();
        }

        private void AddNullServerItem(IPEndPoint host)
        {
            ServerListInfo ServerItem = new ServerListInfo();
            ServerItem.Host = host.ToString();
            ServerItem.Port = int.Parse(host.ToString().Split(':')[1]);
            ServerItem.Name = host.ToString();
            ServerItem.Map = "";
            ServerItem.Players = "0/0";
            ServerItem.Passworded = false;
            ServerItem.Ping = 9999;
            ServerItem.BattleEye = false;

            MainServerList.Add(ServerItem);
            ApplyFilters();
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

        private void ApplyFilters()
        {
            FilteredList = new ObservableCollection<ServerListInfo>(MainServerList);
            try
            {
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
                //bool HideLocked = mainWin.Filter_HideLocked.IsChecked.Value;

                if (!string.IsNullOrEmpty(Name)) { FilteredList = new ObservableCollection<ServerListInfo>(FilteredList.Where(x => x.Name.ToLower().Contains(Name))); }
                if (MinPing > 0) { FilteredList = new ObservableCollection<ServerListInfo>(FilteredList.Where(x => x.Ping >= MinPing)); }
                if (MaxPing > 0) { FilteredList = new ObservableCollection<ServerListInfo>(FilteredList.Where(x => x.Ping <= MaxPing)); }
                if (HideUnresponsive) { FilteredList = new ObservableCollection<ServerListInfo>(FilteredList.Where(x => x.Ping != 9999)); }
                if (MinPlayers > 0) { FilteredList = new ObservableCollection<ServerListInfo>(FilteredList.Where(x => long.Parse(x.Players.Split('/')[0]) >= MinPlayers)); }
                if (MaxPlayers > 0) { FilteredList = new ObservableCollection<ServerListInfo>(FilteredList.Where(x => long.Parse(x.Players.Split('/')[0]) <= MaxPlayers)); }
                if (HideEmpty) { FilteredList = new ObservableCollection<ServerListInfo>(FilteredList.Where(x => long.Parse(x.Players.Split('/')[0]) != 0)); }
                if (HideFull) { FilteredList = new ObservableCollection<ServerListInfo>(FilteredList.Where(x => long.Parse(x.Players.Split('/')[0]) != long.Parse(x.Players.Split('/')[1]))); }
                if (!(Mod == "All" || Mod == "")) { FilteredList = new ObservableCollection<ServerListInfo>(FilteredList.Where(x => x.ModInfo.Split(';').Contains(Mod))); }
                if (!(Map == "All" || Mod == "")) { FilteredList = new ObservableCollection<ServerListInfo>(FilteredList.Where(x => x.Map == Map)); }
                if (HidePassworded) { FilteredList = new ObservableCollection<ServerListInfo>(FilteredList.Where(x => x.Passworded != true)); }

                SortList();
                mainWin.MainServerListBox.ItemsSource = FilteredList;
            }
            catch (Exception e)
            {
                mainWin.Filter_Name.Text = "";
                mainWin.Filter_PingMin.Text = "";
                mainWin.Filter_PingMax.Text = "";
                mainWin.Filter_PlayersMin.Text = "";
                mainWin.Filter_PlayersMax.Text = "";
                mainWin.Filter_Mod.Text = "";
                mainWin.Filter_Map.Text = "";
                mainWin.Filter_HidePassword.IsChecked = false;
                mainWin.Filter_HidePlayersEmpty.IsChecked = false;
                mainWin.Filter_HidePlayersFull.IsChecked = false;
                MessageBox.Show("One of the filters threw an exception! Dont do that again.\n\n" + e.StackTrace);
            }
        }


    }
}
