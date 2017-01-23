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

namespace DayZAnnex
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
            server.LoadServers(this);
            MainServerListBox.ItemsSource = server.getServerList();
            ChangeToTab(0);
            settings.LoadSettings();
        }
        
        Server server = new Server();
        public Settings settings = new Settings();

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
                    ReloadSettingsUI();
                    break;
            }

        }
        
        private void ReloadSettingsUI()
        {
            settings_Arma2Path.Text = settings.armaPath;
            settings_OAPath.Text = settings.oaPath;
            settings_ModsPath.Text = settings.modPath;
            settings_LaunchParam_NoLogs.IsChecked = settings.launchOptions.noLogs;
            settings_LaunchParam_NoPause.IsChecked = settings.launchOptions.noPause;
            settings_LaunchParam_NoSplash.IsChecked = settings.launchOptions.noSplash;
            settings_LaunchParam_WindowMode.IsChecked = settings.launchOptions.windowMode;
            settings_Profile.Text = settings.profile;
            settings_AutoLoadServers.IsChecked = settings.autoLoadServers;
            settings_AutoRefreshServers.IsChecked = settings.autoRefreshServers;
            settings_MaxThreads.Text = settings.maxThreads.ToString();
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
            //ServerInfoPanelContainer.Visibility = Visibility.Hidden;

            if (ServerInfoPanelContainer.Visibility == Visibility.Visible)
            {
                QuarticEase easing = new QuarticEase();
                easing.EasingMode = EasingMode.EaseInOut;
                ThicknessAnimation panel_slide = new ThicknessAnimation() { From = new Thickness(0, 0, ServerInfoPanelContainer.Margin.Right, 0), To = new Thickness(0, 0, -(ServerInfoPanelContainer.ActualWidth), 0), Duration = new Duration(TimeSpan.FromSeconds(.3)), EasingFunction = easing };
                panel_slide.Completed += (q, a) => { ServerInfoPanelContainer.Visibility = Visibility.Hidden; };

                ServerInfoPanelContainer.BeginAnimation(Grid.MarginProperty, panel_slide);
            }
        }

        private void Image_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            Environment.Exit(0);
        }

        private void ServerInfoPanelContainer_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ServerInfoPanelContainer.Visibility == Visibility.Visible)
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
            while (ServerInfoPanelContainer.Visibility == Visibility.Visible)
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
                    if (reply.Status == IPStatus.Success)
                    {
                        ServerInfo_Ping.Text = strReply + "ms";
                    }
                    else
                    {
                        ServerInfo_Ping.Text = "999ms";
                    }
                }));
                Thread.Sleep(1000);
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

        private void MainServerListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<ServerListInfo> selectedList = server.getServerList();

            List<string> modBlackList = new List<string>()
            {
                "Arma 2",
                "Arma 2: Operation Arrowhead",
                "Arma 2: British Armed Forces",
                "Arma 2: British Armed Forces (Lite)",
                "Arma 2: Private Military Company",
                "Arma 2: Private Military Company (Lite)",
            };

            bool hideServerMods = true;

            if (MainServerListBox.SelectedIndex != -1 && selectedList[MainServerListBox.SelectedIndex].Ping != 9999)
            {
                ServerListInfo item = selectedList[MainServerListBox.SelectedIndex];
                ServerInfo_Name.Text = item.Name;
                ServerInfo_IP.Text = item.Host.Split(':')[0];
                ServerInfo_Port.Text = item.Host.Split(':')[1];
                ServerInfo_Map.Text = item.Map;
                ServerInfo_Version.Text = item.GameVer;
                ServerInfo_Passworded.Text = item.Passworded.ToString();
                ServerInfo_BattlEye.Text = "null"; // TODO Find BattleEye state
                ServerInfo_LastJoined.Text = "never"; // TODO History
                ServerInfo_Players.Text = item.Players;

                ServerInfo_PlayerList.Items.Clear();
                ServerInfo_ModList.Items.Clear();

                foreach (string moditem in item.ModInfo.Split(';'))
                {
                    if (!modBlackList.Any(moditem.Contains) && (!moditem.Contains("Server") && hideServerMods) && !string.IsNullOrWhiteSpace(moditem))
                        ServerInfo_ModList.Items.Add(moditem);
                }

                foreach (PlayerInfo player in item.PlayerList)
                {
                    ServerInfo_PlayerList.Items.Add(player.Name);
                }

                if (ServerInfoPanelContainer.Visibility == Visibility.Hidden)
                {
                    ServerInfoPanelContainer.Margin = new Thickness(0, 0, -(ServerInfoPanelContainer.ActualWidth), 0);
                    ServerInfoPanelContainer.Visibility = Visibility.Visible;

                    QuarticEase easing = new QuarticEase();
                    easing.EasingMode = EasingMode.EaseInOut;
                    ThicknessAnimation panel_slide = new ThicknessAnimation() { From = new Thickness(0, 0, -(ServerInfoPanelContainer.ActualWidth), 0), To = new Thickness(0, 0, 0, 0), Duration = new Duration(TimeSpan.FromSeconds(.3)), EasingFunction = easing };

                    ServerInfoPanelContainer.BeginAnimation(Grid.MarginProperty, panel_slide);
                }
            }
        }

        private void ServerInfo_Name_MouseEnter(object sender, MouseEventArgs e)
        {
            if(ServerInfo_Name.ActualWidth > ServerInfoPanel.ActualWidth)
            {
                ThicknessAnimation name_slide = new ThicknessAnimation() { From = new Thickness(0, 0, 0, 0), To = new Thickness(-(ServerInfo_Name.ActualWidth - ServerInfoPanel.ActualWidth), 0, 0, 0), Duration = new Duration(TimeSpan.FromSeconds(1.5)) };
                ServerInfo_Name.BeginAnimation(TextBlock.MarginProperty, name_slide);
            }
        }

        private void ServerInfo_Name_MouseLeave(object sender, MouseEventArgs e)
        {
            ServerInfo_Name.BeginAnimation(TextBlock.MarginProperty, null);
            ServerInfo_Name.Margin = new Thickness(0, 0, 0, 0);
        }
    }
}
