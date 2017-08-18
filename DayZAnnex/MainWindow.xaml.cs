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
using System.Text.RegularExpressions;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Reflection;

namespace DayZAnnex
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public ObservableCollection<ServerListInfo> serverCollection; 
        public ListCollectionView serverCollectionView { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            serverCollection = new ObservableCollection<ServerListInfo>();
            serverCollectionView = new ListCollectionView(serverCollection);
            MainServerGrid.ItemsSource = serverCollectionView;

            server.LoadServers(this);
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

        private void ChangeToTab(int tabIndex) // 0 = Servers, 1 = Favourites, 2 = History, 3 = Mods, 4 = Settings
        {
            //Make all the tabs look unselected
            MenuItem_Servers_Selected.Visibility = Visibility.Hidden;
            MenuItem_Servers_Text.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFromString("#6c7481"));
            MenuItem_Favourites_Selected.Visibility = Visibility.Hidden;
            MenuItem_Favourites_Text.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFromString("#6c7481"));
            MenuItem_Mods_Selected.Visibility = Visibility.Hidden;
            MenuItem_Mods_Text.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFromString("#6c7481"));
            MenuItem_History_Selected.Visibility = Visibility.Hidden;
            MenuItem_History_Text.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFromString("#6c7481"));
            MenuItem_Settings_Selected.Visibility = Visibility.Hidden;
            MenuItem_Settings_Text.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFromString("#6c7481"));

            Content_Servers.Visibility = Visibility.Hidden;
            Content_Settings.Visibility = Visibility.Hidden;

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
                    MenuItem_History_Selected.Visibility = Visibility.Visible;
                    MenuItem_History_Selected.BeginAnimation(Rectangle.HeightProperty, SlideIn);
                    MenuItem_History_Text.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFromString("#FFFFFF"));
                    break;
                case 3:
                    Content_Mods.Visibility = Visibility.Visible;
                    MenuItem_Mods_Selected.Visibility = Visibility.Visible;
                    MenuItem_Mods_Selected.BeginAnimation(Rectangle.HeightProperty, SlideIn);
                    MenuItem_Mods_Text.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFromString("#FFFFFF"));
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
            settings_LaunchParam_ScriptErrors.IsChecked = settings.launchOptions.scriptErrors;
            settings_LaunchParam_WindowMode.IsChecked = settings.launchOptions.windowMode;
            settings_Profile.Text = settings.launchOptions.profile;
            settings_RefreshInBackground.IsChecked = settings.refreshInBackground;
            settings_AutoRefreshServers.IsChecked = settings.autoRefreshServers;
            settings_MaxThreads.Text = settings.maxThreads.ToString();
            List<string> profiles = settings.GetProfiles();
            settings_Profile.ItemsSource = profiles;
            if (settings.launchOptions.profile == "")
            {
                settings_Profile.SelectedIndex = 0;
            }
            else
            {
                settings_Profile.SelectedItem = settings.launchOptions.profile;
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

        private void MenuItem_History_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //ChangeToTab(2);
        }

        private void MenuItem_Friends_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ChangeToTab(3);
        }

        private void MenuItem_Settings_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ChangeToTab(4);
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
            ThicknessAnimation icon_slide = new ThicknessAnimation() { From = new Thickness(0, 0, 0, 0), To = new Thickness(0, 25, 0, 0), Duration = new Duration(TimeSpan.FromSeconds(.3)), EasingFunction = easing };
            DoubleAnimation icon_fade = new DoubleAnimation() { From = 1, To = .1, Duration = new Duration(TimeSpan.FromSeconds(.3)), EasingFunction = easing };
            ThicknessAnimation text_slide = new ThicknessAnimation() { From = new Thickness(0, 40, 0, 0), To = new Thickness(0, 30, 0, 0), Duration = new Duration(TimeSpan.FromSeconds(.3)), EasingFunction = easing };
            DoubleAnimation text_fade = new DoubleAnimation() { From = 1, To = 0, Duration = new Duration(TimeSpan.FromSeconds(.3)), EasingFunction = easing };

            info_icon.BeginAnimation(Image.MarginProperty, icon_slide);
            info_icon.BeginAnimation(Image.OpacityProperty, icon_fade);
            info_text.BeginAnimation(TextBlock.MarginProperty, text_slide);
            info_text.BeginAnimation(TextBlock.OpacityProperty, text_fade);
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Content_Servers.IsEnabled = true;
            Content_Servers.Opacity = 1;
            MainServerGrid.SelectedIndex = -1;

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
                Thread.Sleep(2000);
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
            server.ReloadDisplay();
        }

        private void FilterCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            server.ReloadDisplay();
        }

        private void Filter_Map_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            server.ReloadDisplay();
        }

        private void ServerInfo_Name_MouseEnter(object sender, MouseEventArgs e)
        {
            if (ServerInfo_Name.ActualWidth > ServerInfoPanel.ActualWidth)
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

        private void ServerInfo_Join_Click(object sender, RoutedEventArgs e)
        {
            string parameters = "";
            string path = settings.oaPath.TrimEnd('\\') + "\\ArmA2OA_BE.exe";

            if (settings.launchOptions.noLogs)
                parameters += "-noLogs ";

            if (settings.launchOptions.noPause)
                parameters += "-noPause ";

            if (settings.launchOptions.noSplash)
                parameters += "-nosplash ";

            if (settings.launchOptions.scriptErrors)
                parameters += "-showScriptErrors ";

            if (settings.launchOptions.windowMode)
                parameters += "-window ";

            if (settings.launchOptions.profile != "")
                parameters += string.Format("-name={0} ", settings.launchOptions.profile);

            parameters += string.Format("-connect={0} -port={1} ", ServerInfo_IP.Text, ServerInfo_Port.Text);

            string modParam = "-mod=";
            modParam += settings.armaPath.TrimEnd('\\') + ";Expansion;";

            foreach (string mod in ServerInfo_ModList.Items)
            {
                string[] modFolders = Directory.GetDirectories(settings.modPath);

                if (modFolders.Select(x => x = System.IO.Path.GetFileName(x)).Any(mod.Contains))
                {
                    modParam += string.Format("{0}\\{1};", settings.modPath.TrimEnd('\\'), mod);
                }
                else
                {
                    foreach (string dir in Directory.GetDirectories(settings.modPath).Where(x => File.Exists(x + "\\mod.cpp")))
                    {
                        if (Regex.Match(File.ReadAllLines(dir + "\\mod.cpp").Where(x => x.StartsWith("name")).FirstOrDefault(), "\"([^)]*)\"").Groups[1].Value == mod)
                        {
                            modParam += dir + ";";
                        }
                    }
                }
            }

            modParam = modParam.TrimEnd(';');
            LaunchGame(path, parameters + "\"" + modParam + "\"");
        }

        /// <summary>
        /// Launches arma 2 oa with the mod parameters
        /// </summary>
        /// <param name="exePath">Path of the arma 2 oa executable</param>
        /// <param name="cParams">The parameters to launch the executable with</param>
        private void LaunchGame(string exePath, string cParams)
        {
            if (!File.Exists(exePath))
                return;

            var proc = System.Diagnostics.Process.Start(exePath, cParams);
        }

        /// <summary>
        /// Shows the folder browser to browse for the arma 2 folder
        /// </summary>
        private void settings_BrowseArma2_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            if (Directory.Exists(settings_Arma2Path.Text)) { dialog.InitialDirectory = settings_Arma2Path.Text; } else { dialog.InitialDirectory = "C:\\Users"; }
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (!File.Exists(dialog.FileName + "\\arma2.exe"))
                {
                    MessageBox.Show("arma2.exe was not found in this directory.");
                }
                else
                {
                    settings.armaPath = dialog.FileName;
                    settings.SaveSettings();
                    ReloadSettingsUI();
                }
            }
        }

        /// <summary>
        /// Shows the folder browser to browse for the arma 2 oa folder
        /// </summary>
        private void settings_BrowseArma2OA_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            if (Directory.Exists(settings_OAPath.Text)) { dialog.InitialDirectory = settings_OAPath.Text; } else { dialog.InitialDirectory = "C:\\Users"; }
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (!File.Exists(dialog.FileName + "\\ArmA2OA.exe"))
                {
                    MessageBox.Show("ArmA2OA.exe was not found in this directory.");
                }
                else
                {
                    settings.oaPath = dialog.FileName;
                    settings.SaveSettings();
                    ReloadSettingsUI();
                }
            }
        }

        /// <summary>
        /// Shows the folder browser to browse for the mod folder
        /// </summary>
        private void settings_BrowseMods_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            if (Directory.Exists(settings_ModsPath.Text)) { dialog.InitialDirectory = settings_ModsPath.Text; } else { dialog.InitialDirectory = "C:\\Users"; }
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                settings.modPath = dialog.FileName;
                settings.SaveSettings();
                ReloadSettingsUI();
            }
        }

        /// <summary>
        /// Changes the selected profile and saves the setting change
        /// </summary>
        private void settings_Profile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (settings_Profile.SelectedValue == null)
                return;

            if (settings_Profile.SelectedIndex == 0)
            {
                settings.launchOptions.profile = "";
            }
            else
            {
                settings.launchOptions.profile = settings_Profile.SelectedItem.ToString();
            }

            settings.SaveSettings();
            ReloadSettingsUI();
        }

        /// <summary>
        /// Update all settings options, save them, and reload the settings display
        /// </summary>
        private void settings_checkbox_Click(object sender, RoutedEventArgs e)
        {
            settings.launchOptions.noLogs = settings_LaunchParam_NoLogs.IsChecked.Value;
            settings.launchOptions.noPause = settings_LaunchParam_NoPause.IsChecked.Value;
            settings.launchOptions.noSplash = settings_LaunchParam_NoSplash.IsChecked.Value;
            settings.launchOptions.windowMode = settings_LaunchParam_WindowMode.IsChecked.Value;
            settings.launchOptions.scriptErrors = settings_LaunchParam_ScriptErrors.IsChecked.Value;
            settings.SaveSettings();
            ReloadSettingsUI();
        }

        /// <summary>
        /// List of blacklisted mods that should not be showen in the mod lists
        /// </summary>
        List<string> modBlackList = new List<string>()
            {
                "Arma 2",
                "Arma 2: Operation Arrowhead",
                "Arma 2: British Armed Forces",
                "Arma 2: British Armed Forces (Lite)",
                "Arma 2: Private Military Company",
                "Arma 2: Private Military Company (Lite)",
            };

        // Hide mod items containing the string 'server'
        bool hideServerMods = true;

        /// <summary>
        /// Update the display panel objects
        /// </summary>
        public void UpdateDisplayPanel()
        {
            // Get the selected item
            ServerListInfo selectedItem = MainServerGrid.SelectedItem as ServerListInfo;
            // Get the index of the item
            int index = serverCollection.IndexOf(selectedItem);
            // Get the item from the list
            ServerListInfo serverInfo = serverCollection[index];

            // Set displayed server information
            ServerInfo_Name.Text = serverInfo.Name;
            ServerInfo_IP.Text = serverInfo.Host;
            ServerInfo_Port.Text = serverInfo.GamePort.ToString();
            ServerInfo_Map.Text = serverInfo.Map;
            ServerInfo_Version.Text = serverInfo.GameVer;
            ServerInfo_Passworded.Text = serverInfo.Passworded.ToString();
            ServerInfo_BattlEye.Text = "null"; // TODO Find BattleEye state
            ServerInfo_LastJoined.Text = "never"; // TODO History
            ServerInfo_Players.Text = serverInfo.Players;

            // Clear the player and mod lists
            ServerInfo_PlayerList.Items.Clear();
            ServerInfo_ModList.Items.Clear();

            // Check the server will have the lists
            if (serverInfo.Ping != 9999 && serverInfo.PlayerList != null)
            {
                // Update the player and mod lists
                foreach (string moditem in serverInfo.ModInfo.Split(';'))
                {
                    // Check if the moditem contains blacklisted strings
                    if (!modBlackList.Any(moditem.Contains) && (!moditem.ToLower().Contains("server") && hideServerMods) && !string.IsNullOrWhiteSpace(moditem))
                        ServerInfo_ModList.Items.Add(moditem);
                }

                foreach (PlayerInfo player in serverInfo.PlayerList)
                {
                    ServerInfo_PlayerList.Items.Add(player.Name);
                }
            }
        }

        /// <summary>
        /// Show the display panel
        /// </summary>
        public void ShowDisplayPanel()
        {
            UpdateDisplayPanel();

            // Check if the panel is hidden before animating
            if (ServerInfoPanelContainer.Visibility == Visibility.Hidden)
            {
                // Animate the display panel to slide into view
                ServerInfoPanelContainer.Margin = new Thickness(0, 0, -(ServerInfoPanelContainer.ActualWidth), 0);
                ServerInfoPanelContainer.Visibility = Visibility.Visible;

                QuarticEase easing = new QuarticEase();
                easing.EasingMode = EasingMode.EaseInOut;
                ThicknessAnimation panel_slide = new ThicknessAnimation() { From = new Thickness(0, 0, -(ServerInfoPanelContainer.ActualWidth), 0), To = new Thickness(0, 0, 0, 0), Duration = new Duration(TimeSpan.FromSeconds(.3)), EasingFunction = easing };

                ServerInfoPanelContainer.BeginAnimation(Grid.MarginProperty, panel_slide);
            }
        }

        /// <summary>
        /// Gets the selected item and displays it in the display panel
        /// </summary>
        private void MainServerGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check to make sure the selection is not -1 (null)
            if(MainServerGrid.SelectedIndex != -1)
            {
                // Get the selected item
                ServerListInfo serverInfo = MainServerGrid.SelectedItem as ServerListInfo;
                // Get the index in the server collection
                int index = serverCollection.IndexOf(serverInfo);
                // Update the selected item
                server.UpdateItem(serverCollection[index], index);
            }
        }

        /// <summary>
        /// Toggles the available mod filters
        /// </summary>
        private void modToggle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CubicEase easing = new CubicEase();
            easing.EasingMode = EasingMode.EaseInOut;

            if (Filter_ModsContainer.Height == 0)
            {
                DoubleAnimation anim = new DoubleAnimation() { From = 0, To = Filter_ModsContainer.ActualHeight, Duration = new Duration(TimeSpan.FromSeconds(.3)), EasingFunction = easing };
                Filter_ModsContainer.BeginAnimation(HeightProperty, anim);
                modToggle.Text = "Hide Mods";
            }
            else
            {
                DoubleAnimation anim = new DoubleAnimation() { From = Filter_ModsContainer.ActualHeight, To = 0, Duration = new Duration(TimeSpan.FromSeconds(.3)), EasingFunction = easing };
                Filter_ModsContainer.BeginAnimation(HeightProperty, anim);
                modToggle.Text = "Show Mods";
            }
        }

        /// <summary>
        /// Toggles the available map filters
        /// </summary>
        private void mapToggle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CubicEase easing = new CubicEase();
            easing.EasingMode = EasingMode.EaseInOut;

            if (Filter_MapsContainer.Height == 0)
            {
                DoubleAnimation anim = new DoubleAnimation() { From = 0, To = Filter_MapsContainer.ActualHeight, Duration = new Duration(TimeSpan.FromSeconds(.3)), EasingFunction = easing };
                Filter_MapsContainer.BeginAnimation(HeightProperty, anim);
                mapToggle.Text = "Hide Maps";
            }
            else
            {
                DoubleAnimation anim = new DoubleAnimation() { From = Filter_MapsContainer.ActualHeight, To = 0, Duration = new Duration(TimeSpan.FromSeconds(.3)), EasingFunction = easing };
                Filter_MapsContainer.BeginAnimation(HeightProperty, anim);
                mapToggle.Text = "Show Maps";
            }
        }

        /// <summary>
        /// Refreshes the current server view
        /// </summary>
        private void imageServerRefresh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}