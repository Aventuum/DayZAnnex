using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using QueryMaster;
using QueryMaster.GameServer;
using QueryMaster.MasterServer;
using System.Globalization;
using System.ComponentModel;
using System.Xml;
using System.IO;
using System.Collections.ObjectModel;

namespace DayZReborn
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    
    /*  Some notes to myself
     *  
     *      02/11/16
     *  
     *      Added Server.cs and moved some functions there, still needs some cleanup and MainWindow.xaml.cs needs updating
     *      
     *  
     *      01/11/16
     *      
     *      Project is at the point where I need to move some functions to their own classes as to avoid spaghettification
     *      
     *      Slight overhaul of the GUI is neccessary to add 'theme' functionality/greater customization options
     *      Perhaps iterate through every object and set visual aspects based on some tag that would be set in XAML
     *      Add this function in its own class. Maybe 'Theme' called by 'Theme.Load(string ThemeName)'
     *      Themes should be saved in a seperate xdoc-readable file
     *      
     *      Still no functionality to actually launch ArmA2OA
     *      
     *      Add mods tab and functionality to add mods
     *      Add option to download 'internally' (in-app) or 'externally' (open link in browser or torrent program)
     *      Internally downloaded mods will be automatically added
     *      mods can be kept in an xdoc-readable file, containing information like Name, creator, website, DL link, magnet link, isInstalled
     *      
     *      Filter out mods with 'server' in the name when a servers mods are listed
     *      
     *      Classes:
     *          MainWindow
     *              Contains functions and handlers for XAML objects
     *          Themes
     *              Contains functions for loading, editing and saving themes
     *              GetThemes() returns names of saved themes
     *              SaveTheme(string Name) saves the current theme
     *              LoadTheme(string Name) loads a theme
     *          Server
     *              List<ServerInfo> MainList
     *              GetServerList() gets the main server list
     *              QueryServers() begins the server query and updates the main server list
     *          Settings
     *              get/set functions for settings
     *              Load() loads settings
     *              Save() saves settings
     */



    //Class containing information on a server. Is used in a list.

    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
            server.LoadServers(this);
            MainServerListBox.ItemsSource = server.getServerList();
            ChangeToTab(0);
            Load();
        }

        Server server = new Server();

        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.BorderThickness = new Thickness(8);
            }
            else
            {
                this.BorderThickness = new Thickness(0);
            }
        }

        private void ChangeToTab(int tabIndex) // 0 = Servers, 1 = Favourites, 2 = Friends, 3 = History, 4 = Settings
        {
            //Make all the tabs look unselected
            MenuItem_Servers_Selected.Visibility = Visibility.Hidden;
            MenuItem_Servers_Text.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFromString("#6c7481"));
            MenuItem_Favourites_Selected.Visibility = Visibility.Hidden;
            MenuItem_Favourites_Text.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFromString("#6c7481"));
            MenuItem_Friends_Selected.Visibility = Visibility.Hidden;
            MenuItem_Friends_Text.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFromString("#6c7481"));
            MenuItem_History_Selected.Visibility = Visibility.Hidden;
            MenuItem_History_Text.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFromString("#6c7481"));
            MenuItem_Settings_Selected.Visibility = Visibility.Hidden;
            MenuItem_Settings_Text.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFromString("#6c7481"));

            Content_Servers.Visibility = Visibility.Hidden;
            Content_Settings.Visibility = Visibility.Hidden;
            //Unused stackpanels
            //ServerStackPanel.Visibility = Visibility.Hidden;
            //FavouriteStackPanel.Visibility = Visibility.Hidden;
            //HistoryStackPanel.Visibility = Visibility.Hidden;

            //Define the animation
            DoubleAnimation SlideIn = new DoubleAnimation() { From = 0, To = 4, Duration = new Duration(TimeSpan.FromSeconds(.15)) };
            DoubleAnimation Fadein = new DoubleAnimation() { From = .5, To = 1, Duration = new Duration(TimeSpan.FromSeconds(.15)) };

            //Make the selected tab look selected by setting the foreground and highlight visibility
            switch (tabIndex)
            {
                case 0:
                    Content_Servers.Visibility = Visibility.Visible;
                    MenuItem_Servers_Selected.Visibility = Visibility.Visible;
                    MenuItem_Servers_Selected.BeginAnimation(Rectangle.HeightProperty, SlideIn);
                    MenuItem_Servers_Text.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFromString("#FFFFFF"));
                    break;
                case 1:
                    Content_Servers.Visibility = Visibility.Visible;
                    MenuItem_Favourites_Selected.Visibility = Visibility.Visible;
                    MenuItem_Favourites_Selected.BeginAnimation(Rectangle.HeightProperty, SlideIn);
                    MenuItem_Favourites_Text.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFromString("#FFFFFF"));
                    break;
                case 2:
                    MenuItem_Friends_Selected.Visibility = Visibility.Visible;
                    MenuItem_Friends_Selected.BeginAnimation(Rectangle.HeightProperty, SlideIn);
                    MenuItem_Friends_Text.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFromString("#FFFFFF"));
                    break;
                case 3:
                    Content_Servers.Visibility = Visibility.Visible;
                    MenuItem_History_Selected.Visibility = Visibility.Visible;
                    MenuItem_History_Selected.BeginAnimation(Rectangle.HeightProperty, SlideIn);
                    MenuItem_History_Text.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFromString("#FFFFFF"));
                    break;
                case 4:
                    Content_Settings.Visibility = Visibility.Visible;
                    MenuItem_Settings_Selected.Visibility = Visibility.Visible;
                    MenuItem_Settings_Selected.BeginAnimation(Rectangle.HeightProperty, SlideIn);
                    MenuItem_Settings_Text.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFromString("#FFFFFF"));
                    break;
            }

        }
        
        

        private void MenuItem_Servers_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ChangeToTab(0);
        }

        private void MenuItem_Favourites_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //ChangeToTab(1);
        }

        private void MenuItem_Friends_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //ChangeToTab(2);
        }

        private void MenuItem_History_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //ChangeToTab(3);
        }

        private void MenuItem_Settings_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ChangeToTab(4);
        }

        

        public static bool AutoLoadServers { get; set; }
        public static bool AutoRefreshServers { get; set; }
        public static List<string> ColourSchemeFiles { get; set; }
        public static string ColourScheme { get; set; }
        public static XDocument XmlSettings { get; set; }
        public static string SettingsFile = "settings.xml";

        public void Load()
        {

            if (!File.Exists(SettingsFile))
            {
                MessageBox.Show("Cannot find settings file");
                //GenerateDefaultSettings();
            }
            else
            {
                XmlSettings = XDocument.Parse(File.ReadAllText(SettingsFile));

                AutoLoadServers = bool.Parse(XmlSettings.Element("settings").Element("startup").Element("autofetchservers").Attribute("value").Value);
                //Setting_AutoFetch.IsChecked = AutoLoadServers;

                AutoRefreshServers = bool.Parse(XmlSettings.Element("settings").Element("startup").Element("autorefreshservers").Attribute("value").Value);
                //Setting_AutoRefresh.IsChecked = AutoRefreshServers;

                // Get history list
                if (File.Exists(XmlSettings.Element("settings").Element("serverlists").Element("historylist").Attribute("value").Value))
                {

                }
            }
            /* //Currently unused
            if (File.Exists("history.txt"))
            {
                foreach(string line in File.ReadAllLines("history.txt"))
                {
                    HistoryEndP.Add(line);
                }
            }
            */
        }

        /* //Currently unused
        public void SaveFavourites()
        {
            string FavList = XmlSettings.Element("settings").Element("serverlists").Element("favouritelist").Attribute("value").Value;
            if (File.Exists(FavList))
            {
                File.Delete(FavList);
                using(StreamWriter sr = new StreamWriter(FavList))
                {
                    foreach(string fEndP in FavouriteEndP)
                    {
                        sr.WriteLine(fEndP);
                    }
                }
            }
        }
        */

        public void Save()
        {
            if (!File.Exists(SettingsFile))
            {
                MessageBox.Show("Cannot find settings file");
            }
            else
            {
                XmlSettings = XDocument.Parse(File.ReadAllText(SettingsFile));

                //XmlSettings.Element("settings").Element("startup").Element("autofetchservers").Attribute("value").Value = Setting_AutoFetch.IsChecked.ToString();
                //XmlSettings.Element("settings").Element("startup").Element("autorefreshservers").Attribute("value").Value = Setting_AutoRefresh.IsChecked.ToString();
            }
        }

        //public string Read(string setting) // Returns value of requested settings, example call Settings.Read("Startup\\autofetchservers"); will return the value
        //{

        //    return "";
        //}

        //public void GenerateDefaultSettings()
        //{
        //    using (StreamWriter sw = new StreamWriter(SettingsFile))
        //    {

        //    }
        //}

        private void Settings_Save_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("");
        }

        private void sideinfo_MouseEnter(object sender, MouseEventArgs e)
        {
            info_icon.Opacity = .1;
            info_icon.Margin = new Thickness(0, 25, 0, 0);
            info_text.Opacity = 0;
            info_text.Margin = new Thickness(0, 30, 0, 0);

            CubicEase easing = new CubicEase();
            easing.EasingMode = EasingMode.EaseInOut;
            ThicknessAnimation icon_slide = new ThicknessAnimation() { From = new Thickness(0, 25, 0, 0), To = new Thickness(0, 0, 0, 0), Duration = new Duration(TimeSpan.FromSeconds(.3)), EasingFunction = easing };
            DoubleAnimation icon_fade = new DoubleAnimation() { From = .1, To = 1, Duration = new Duration(TimeSpan.FromSeconds(.3)), EasingFunction = easing };
            ThicknessAnimation text_slide = new ThicknessAnimation() { From = new Thickness(0, 30, 0, 0), To = new Thickness(0, 40, 0, 0), Duration = new Duration(TimeSpan.FromSeconds(.3)), EasingFunction = easing };
            DoubleAnimation text_fade = new DoubleAnimation() { From = 0, To = 1, Duration = new Duration(TimeSpan.FromSeconds(.3)), EasingFunction = easing };

            info_icon.BeginAnimation(Image.MarginProperty, icon_slide);
            info_icon.BeginAnimation(Image.OpacityProperty, icon_fade);
            info_text.BeginAnimation(TextBlock.MarginProperty, text_slide);
            info_text.BeginAnimation(TextBlock.OpacityProperty, text_fade);
        }

        private void sideinfo_MouseLeave(object sender, MouseEventArgs e)
        {
            info_icon.Opacity = 1;
            info_icon.Margin = new Thickness(0, 0, 0, 0);
            info_text.Opacity = 1;
            info_text.Margin = new Thickness(0, 40, 0, 0);

            CubicEase easing = new CubicEase();
            easing.EasingMode = EasingMode.EaseInOut;
            ThicknessAnimation icon_slide = new ThicknessAnimation() { From = new Thickness(0, 0, 0, 0), To = new Thickness(0, 25, 0, 0), Duration = new Duration(TimeSpan.FromSeconds(.3)), EasingFunction = easing};
            DoubleAnimation icon_fade = new DoubleAnimation() { From = 1, To = .1, Duration = new Duration(TimeSpan.FromSeconds(.3)), EasingFunction = easing };
            ThicknessAnimation text_slide = new ThicknessAnimation() { From = new Thickness(0, 40, 0, 0), To = new Thickness(0, 30, 0, 0), Duration = new Duration(TimeSpan.FromSeconds(.3)), EasingFunction = easing };
            DoubleAnimation text_fade = new DoubleAnimation() { From = 1, To = 0, Duration = new Duration(TimeSpan.FromSeconds(.3)), EasingFunction = easing };

            info_icon.BeginAnimation(Image.MarginProperty, icon_slide);
            info_icon.BeginAnimation(Image.OpacityProperty, icon_fade);
            info_text.BeginAnimation(TextBlock.MarginProperty, text_slide);
            info_text.BeginAnimation(TextBlock.OpacityProperty, text_fade);
        }


        private void Column_Name_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            server.sortBy = "Name";
            if(Column_Name_Arrow.Points[0].X == 0)
            {
                server.sortAsc = false;
            }
            else
            {
                server.sortAsc = true;
            }
            MainServerListBox.ItemsSource = server.getServerList();
        }

        private void Column_Map_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            server.sortBy = "Map";
            if (Column_Map_Arrow.Points[0].X == 0)
            {
                server.sortAsc = false;
            }
            else
            {
                server.sortAsc = true;
            }
            MainServerListBox.ItemsSource = server.getServerList();
        }

        private void Column_Players_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            server.sortBy = "Players";
            if (Column_Players_Arrow.Points[0].X == 0)
            {
                server.sortAsc = false;
            }
            else
            {
                server.sortAsc = true;
            }
            MainServerListBox.ItemsSource = server.getServerList();
        }

        private void Column_Ping_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            server.sortBy = "Ping";
            if (Column_Ping_Arrow.Points[0].X == 0)
            {
                server.sortAsc = false;
            }
            else
            {
                server.sortAsc = true;
            }
            MainServerListBox.ItemsSource = server.getServerList();
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Content_Servers.IsEnabled = true;
            Content_Servers.Opacity = 1;
            Grid_ServerInfo.Visibility = Visibility.Hidden;
        }

        private void Image_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            Environment.Exit(0);
        }


        private void Grid_ServerInfo_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Grid_ServerInfo.Visibility == Visibility.Visible)
            {
                Thread thread = new Thread(PingThread);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }

        public void PingThread()
        {
            string ip = "";
            this.Dispatcher.Invoke((Action)(() =>
            {
                ip = ServerInfo_IP.Text;
            }));
            while(Grid_ServerInfo.Visibility == Visibility.Visible)
            {
                Ping pingSender = new Ping();
                string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                int timeout = 1000;
                PingOptions options = new PingOptions(64, true);
                PingReply reply = pingSender.Send(ip.Split(':')[0], timeout, buffer, options);
                string strReply = reply.RoundtripTime.ToString();
                this.Dispatcher.Invoke((Action)(() =>
                {
                    if(reply.Status == IPStatus.Success)
                    {
                        ServerInfo_Ping.Text = strReply + "ms";
                    }
                    else
                    {
                        ServerInfo_Ping.Text = "//Couldn't reach host";
                    }
                }));
                Thread.Sleep(1000);
            }
        }

        private void MainServerListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            List<ServerListInfo> selectedList = server.getServerList();
            if(MainServerListBox.SelectedIndex != -1 && selectedList[MainServerListBox.SelectedIndex].Ping != 9999)
            {
                ServerListInfo item = selectedList[MainServerListBox.SelectedIndex];
                ServerInfo_Name.Text = item.Name;
                ServerInfo_IP.Text = item.Host;
                ServerInfo_Map.Text = item.Map;
                ServerInfo_GameVersion.Text = item.GameVer;
                ServerInfo_Passworded.Text = item.Passworded.ToString().Replace("True", "Yes").Replace("False", "No");
                ServerInfo_BattleEye.Text = "unknown"; // TODO Find BattleEye state
                ServerInfo_LastJoined.Text = "unknown"; // TODO History
                ServerInfo_Players.Text = string.Format("Players - {0}", item.Players);
                ServerInfo_PlayerList.Items.Clear();
                ServerInfo_ModList.Items.Clear();
                foreach (string moditem in item.ModInfo.Split(';'))
                {
                    ServerInfo_ModList.Items.Add(moditem);
                }

                foreach (PlayerInfo player in item.PlayerList)
                {
                    ServerInfo_PlayerList.Items.Add(player.Name);
                }

                Grid_ServerInfo.Visibility = Visibility.Visible;
            }
            else if(MainServerListBox.SelectedIndex != -1 && selectedList[MainServerListBox.SelectedIndex].Ping == 9999)
            {
                ServerListInfo item = selectedList[MainServerListBox.SelectedIndex];
                ServerInfo_Name.Text = item.Host;
                ServerInfo_IP.Text = item.Host;
                ServerInfo_Map.Text = "unknown";
                ServerInfo_GameVersion.Text = "unknown";
                ServerInfo_Passworded.Text = "unknown";
                ServerInfo_BattleEye.Text = "unknown"; // TODO Find BattleEye state
                ServerInfo_LastJoined.Text = "unknown"; // TODO History
                ServerInfo_Players.Text = "Players - unknown";
                ServerInfo_PlayerList.Items.Clear();
                ServerInfo_ModList.Items.Clear();

                Grid_ServerInfo.Visibility = Visibility.Visible;
            }
        }

        
        private void PreviewCharFilter(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }

        private void FilterTextChanged(object sender, TextChangedEventArgs e)
        {
            server.ApplyFilters();
        }

        private void FilterCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            server.ApplyFilters();
        }
    }
}
