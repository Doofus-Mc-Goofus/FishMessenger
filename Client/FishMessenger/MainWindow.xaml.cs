using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ChecksumGen;
using INI;
using TaskDialogInterop;

namespace FishMessenger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Forms.NotifyIcon notifyIcon1 = new System.Windows.Forms.NotifyIcon();
        private bool isClosing = true;
        private bool loggingOff = false;
        private bool hasShown = false;
        private MediaPlayer Notif = new MediaPlayer();
        private MediaPlayer Startup = new MediaPlayer();
        private MediaPlayer StopUp = new MediaPlayer();
        private Random rng = new Random();
        private int newMessages;
        private List<string> Messages = new List<string>();
        private List<string> MessagesUser = new List<string>();
        private List<byte> MessagesStatus = new List<byte>();
        private List<bool> MessagesRead = new List<bool>();
        public List<string> Servers = new List<string>();
        private List<string> ServersTitle = new List<string>();
        private List<string> ServersPass = new List<string>();
        private List<bool> ServersAuto = new List<bool>();
        public List<int> ServersNull = new List<int>();
        private DispatcherTimer dispatcherTimer;
        private DispatcherTimer dispatcherTimer2;
        public readonly HttpClient client = new HttpClient();
        public string username;
        public readonly string currentVer = "0.2.1";
        int ingus = 0;
        public MainWindow()
        {
            InitializeComponent();
            HomePage.Visibility = Visibility.Hidden;
            LoginPage.Visibility = Visibility.Hidden;
            Notif.Open(new Uri(@"type.wav", UriKind.RelativeOrAbsolute));
            Startup.Open(new Uri(@"snd.wav", UriKind.RelativeOrAbsolute));
            StopUp.Open(new Uri(@"snd2.wav", UriKind.RelativeOrAbsolute));
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += (s, ee) => UpdateThing();
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(33);
            dispatcherTimer.Start();
            dispatcherTimer2 = new DispatcherTimer();
            dispatcherTimer2.Tick += (s, ee) => SendMessage("This is a phrase.", "FaceFace");
            dispatcherTimer2.Interval = TimeSpan.FromSeconds(10);
            // dispatcherTimer2.Start();
            if (File.Exists("config.ini"))
            {
                Startup.Play();
                IniFile iniFile = new IniFile("config.ini");
                if (iniFile.Read("Ver", "User") != currentVer)
                {
                    UserMigration(iniFile.Read("Ver", "User"), iniFile);
                }
                Home_Username.Text = iniFile.Read("Name", "User");
                Login_Username.Text = "Welcome " + iniFile.Read("Name", "User");
                if (bool.Parse(iniFile.Read("StaySign", "User")))
                {
                    HomePage.Visibility = Visibility.Visible;
                    isClosing = false;
                }
                else
                {
                    LoginPage.Visibility = Visibility.Visible;
                }
                Home_Realname.Text = iniFile.Read("Name", "User") + "#" + iniFile.Read("ID", "User");
                username = iniFile.Read("Name", "User");
                try
                {
                    Home_PFP.Source = new BitmapImage(new Uri(iniFile.Read("PFP", "User")));
                }
                catch
                {
                    Home_PFP.Source = new BitmapImage(new Uri("pack://application:,,,/usertile0.png"));
                }
                Home_GradRect.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(iniFile.Read("PageColor", "User")));
                var Checksummer = new Checksum();
                if (Checksummer.Generate(iniFile.Read("Name", "User") + iniFile.Read("ID", "User")).ToString() != iniFile.Read("Checksum", "User"))
                {
                    Startup.Stop();
                    Startup.Position = new TimeSpan(0, 0, 0);
                    TaskDialogOptions config = new TaskDialogOptions
                    {
                        Title = "Critical Error",
                        MainInstruction = "Your Fish Messenger profile has been corrupted.",
                        Content = "You will have to create a new one.",
                        CustomButtons = new string[] { "&OK" },
                        MainIcon = VistaTaskDialogIcon.Error
                    };
                    TaskDialog.Show(config);
                    File.Delete(@"config.ini");
                    this.Visibility = Visibility.Hidden;
                    Setup setupWindow = new Setup();
                    setupWindow.Show();
                    setupWindow.Pass(this);
                }
                else
                {
                    for (var i = 1; i <= int.Parse(iniFile.Read("Count", "Messages")); i++)
                    {
                        ListViewItem messageText = new ListViewItem
                        {
                            Content = iniFile.Read(i + "User", "Messages") + " - " + iniFile.Read(i + "Mess", "Messages"),
                            Tag = Messages.Count + 1
                        };
                        if (!bool.Parse(iniFile.Read(i + "Read", "Messages")))
                        {
                            messageText.FontWeight = FontWeights.Bold;
                            newMessages++;
                        }
                        messageText.MouseUp += (s, ee) => OpenMessage(Messages.Count - int.Parse(messageText.Tag.ToString()), messageText);
                        Home_MessageBox.Items.Insert(0, messageText);
                        Messages.Insert(0, iniFile.Read(i + "Mess", "Messages"));
                        MessagesUser.Insert(0, iniFile.Read(i + "User", "Messages"));
                        MessagesStatus.Insert(0, byte.Parse(iniFile.Read(i + "Stat", "Messages")));
                        MessagesRead.Insert(0, bool.Parse(iniFile.Read(i + "Read", "Messages")));
                    }
                    for (var i = 1; i <= int.Parse(iniFile.Read("Count", "Servers")); i++)
                    {
                        if (iniFile.Read(i + "URL", "Servers") != "")
                        {
                            ListViewItem messageText = new ListViewItem
                            {
                                Content = iniFile.Read(i + "Title", "Servers") + " - " + iniFile.Read(i + "URL", "Servers"),
                                Tag = Servers.Count + 1
                            };
                            messageText.MouseUp += (s, ee) => OpenServer(Servers.Count - int.Parse(messageText.Tag.ToString()), messageText);
                            Home_ServerBox.Items.Insert(0, messageText);
                            Servers.Insert(0, iniFile.Read(i + "URL", "Servers"));
                            ServersTitle.Insert(0, iniFile.Read(i + "Title", "Servers"));
                            ServersPass.Insert(0, iniFile.Read(i + "Pass", "Servers"));
                            ServersAuto.Insert(0, bool.Parse(iniFile.Read(i + "Auto", "Servers")));
                        }
                        else
                        {
                            ServersNull.Add(i);
                            Servers.Insert(0, "");
                            ServersTitle.Insert(0, "");
                            ServersPass.Insert(0, "");
                            ServersAuto.Insert(0, false);
                        }

                    }
                }
                Home_MessagesTab.Header = "Messages (" + newMessages + ")";
            }
            else
            {
                this.Visibility = Visibility.Hidden;
                Setup setupWindow = new Setup();
                setupWindow.Show();
                setupWindow.Pass(this);
            }
            notifyIcon1.Icon = new System.Drawing.Icon("fish.ico");
            notifyIcon1.Text = "(0) Fish Messenger";
            notifyIcon1.Visible = true;
            notifyIcon1.Click += (s, ee) => ShowWindow();
            notifyIcon1.BalloonTipClicked += (s, ee) => ShowWindow();

        }
        public void UpdateServList()
        {
            IniFile iniFile = new IniFile("config.ini");
            Home_ServerBox.Items.Clear();
            Servers = new List<string>();
            ServersTitle = new List<string>();
            ServersPass = new List<string>();
            ServersAuto = new List<bool>();
            ServersNull = new List<int>();
            for (var i = 1; i <= int.Parse(iniFile.Read("Count", "Servers")); i++)
            {
                if (iniFile.Read(i + "URL", "Servers") != "")
                {
                    ListViewItem messageText = new ListViewItem
                    {
                        Content = iniFile.Read(i + "Title", "Servers") + " - " + iniFile.Read(i + "URL", "Servers"),
                        Tag = Servers.Count + 1
                    };
                    messageText.MouseUp += (s, ee) => OpenServer(Servers.Count - int.Parse(messageText.Tag.ToString()), messageText);
                    Home_ServerBox.Items.Insert(0, messageText);
                    Servers.Insert(0, iniFile.Read(i + "URL", "Servers"));
                    ServersTitle.Insert(0, iniFile.Read(i + "Title", "Servers"));
                    ServersPass.Insert(0, iniFile.Read(i + "Pass", "Servers"));
                    ServersAuto.Insert(0, bool.Parse(iniFile.Read(i + "Auto", "Servers")));
                }
                else
                {
                    ServersNull.Add(i);
                    Servers.Insert(0, "");
                    ServersTitle.Insert(0, "");
                    ServersPass.Insert(0, "");
                    ServersAuto.Insert(0, false);
                }

            }
        }

        void UserMigration(string oldVer, IniFile ini)
        {
            if (oldVer == "0.1.6" || oldVer == "0.1.6.1")
            {
                ini.Write("Count", "0", "Servers");
                ini.Write("Ver", currentVer, "User");
            }
            else if (oldVer == "0.1.7" || oldVer == "0.1.8" || oldVer == "0.1.9" || oldVer == "0.2.0")
            {
                ini.Write("Ver", currentVer, "User");
            }
            else
            {
                TaskDialogOptions config = new TaskDialogOptions
                {
                    Title = "Profile Error",
                    MainInstruction = "Your Fish Messenger profile file format is too old for this version of the client.",
                    Content = "You will have to either create a new profile or use an older version of the client.",
                    CustomButtons = new string[] { "&OK" },
                    MainIcon = VistaTaskDialogIcon.Error
                };
                TaskDialog.Show(config);
                Close();
            }
        }

        void UpdateThing()
        {
            ingus++;
            if (ingus >= 18)
            {
                ingus = 0;
            }
            LoadingSirc.Source = new BitmapImage(new Uri("pack://application:,,,/loadingcircle/tile00" + ingus + ".png"));
        }
        public void Update()
        {
            IniFile iniFile = new IniFile("config.ini");
            Home_PFP.Source = new BitmapImage(new Uri(iniFile.Read("PFP", "User")));
            Home_GradRect.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(iniFile.Read("PageColor", "User")));
        }
        void ShowWindow()
        {
            if (File.Exists("config.ini"))
                this.Visibility = Visibility.Visible;
            this.Activate();
            this.Focus();
        }
        public void FirstRunAfter()
        {
            IniFile iniFile = new IniFile("config.ini");
            Home_Username.Text = iniFile.Read("Name", "User");
            username = iniFile.Read("Name", "User");
            Home_Realname.Text = iniFile.Read("Name", "User") + "#" + iniFile.Read("ID", "User");
            Home_PFP.Source = new BitmapImage(new Uri(iniFile.Read("PFP", "User")));
            Home_GradRect.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(iniFile.Read("PageColor", "User")));
            Startup.Play();
            ShowWindow();
            SendMessage("Hi. I hope you like my program.", "FishFace");
            Notif.Stop();
            LoginPage.Visibility = Visibility.Visible;
            Login_Username.Text = "Welcome " + iniFile.Read("Name", "User");
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!isClosing)
            {
                e.Cancel = true;
                this.Visibility = Visibility.Hidden;
                if (!hasShown)
                {
                    notifyIcon1.BalloonTipTitle = "Fish Messenger is still open.";
                    notifyIcon1.BalloonTipText = "Fish Messenger will remain open so that you can recieve incoming messages at a moments' notice.";
                    notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
                    notifyIcon1.ShowBalloonTip(10000);
                    hasShown = true;
                }
            }
            else if (loggingOff)
            {
                e.Cancel = true;
                HomePage.Visibility = Visibility.Hidden;
                Loading.Visibility = Visibility.Visible;
                LoginPage.Visibility = Visibility.Hidden;
                StopUp.Play();
                StopUp.MediaEnded += (s, ee) => loggingOff = false;
                StopUp.MediaEnded += (s, ee) => Close();
            }
        }

        private void MenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Out of fish! Please try again later.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            About aboutWindow = new About();
            aboutWindow.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            notifyIcon1.Dispose();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            isClosing = true;
            loggingOff = true;
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SendMessage("Hello!", "User" + rng.Next(1000, 9999));
        }

        void SendMessage(string message, string user)
        {
            newMessages++;
            Notif.Stop();
            Notif.Position = new TimeSpan(0, 0, 0);
            Notif.Play();
            ListViewItem messageText = new ListViewItem
            {
                Content = user + " - " + message,
                Tag = Messages.Count + 1,
                FontWeight = FontWeights.Bold
            };
            messageText.MouseUp += (s, ee) => OpenMessage(Messages.Count - int.Parse(messageText.Tag.ToString()), messageText);
            Home_MessageBox.Items.Insert(0, messageText);
            Messages.Insert(0, message);
            MessagesUser.Insert(0, user);
            MessagesStatus.Insert(0, 0);
            MessagesRead.Insert(0, false);
            if (Visibility == Visibility.Hidden)
            {
                notifyIcon1.BalloonTipTitle = "You have " + newMessages + " new direct messages.";
                notifyIcon1.BalloonTipText = "Make sure to answer your messages.";
                notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
                notifyIcon1.ShowBalloonTip(10000);
            }
            IniFile iniFile = new IniFile("config.ini");
            iniFile.Write(messageText.Tag + "Mess", message, "Messages");
            iniFile.Write(messageText.Tag + "User", user, "Messages");
            iniFile.Write(messageText.Tag + "Stat", "0", "Messages");
            iniFile.Write(messageText.Tag + "Read", "false", "Messages");
            iniFile.Write("Count", (int.Parse(iniFile.Read("Count", "Messages")) + 1).ToString(), "Messages");
            Home_MessagesTab.Header = "Messages (" + newMessages + ")";
        }
        public void PassBack(byte status, int ID)
        {
            MessagesStatus[ID] = status;
        }

        void OpenMessage(int ID, ListViewItem text)
        {
            if (text.FontWeight == FontWeights.Bold)
            {
                newMessages--;
            }
            Message message = new Message();
            message.Show();
            message.Pass(MessagesUser[ID], Messages[ID], ID, MessagesStatus[ID], this, Messages.Count - ID);
            text.FontWeight = FontWeights.Normal;
            Home_MessagesTab.Header = "Messages (" + newMessages + ")";
        }

        void OpenServer(int ID, ListViewItem text)
        {
            Connect connect = new Connect();
            connect.Show();
            connect.Pass(this);
            connect.AutoConnect(Servers[ID], ServersPass[ID]);
        }

        private void EditProf_Click(object sender, RoutedEventArgs e)
        {
            EditProfFunc();
        }
        void EditProfFunc()
        {
            Customize customWindow = new Customize();
            customWindow.Show();
            customWindow.Pass(this);
        }
        public void CloseMessenger()
        {
            isClosing = true;
            Close();
        }

        private void ConnectServer_Click(object sender, RoutedEventArgs e)
        {
            Connect connect = new Connect();
            connect.Show();
            connect.Pass(this);
        }

        private void ConnectSettings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            IniFile theIni = new IniFile("config.ini");
            if (PasswordBox.Text.Length == 0)
            {
                UsernameError.Content = "Please enter your password";
            }
            else if (PasswordBox.Text != theIni.Read("Password", "User"))
            {
                UsernameError.Content = "Password is incorrect";
            }
            else
            {
                if (LoginCheck.IsChecked == true)
                {
                    theIni.Write("StaySign", "true", "User");
                }
                LoginPage.Visibility = Visibility.Hidden;
                HomePage.Visibility = Visibility.Visible;
                isClosing = false;
            }
        }
    }
}
