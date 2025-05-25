using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using INI;
using Newtonsoft.Json.Linq;
using TaskDialogInterop;

namespace FishMessenger
{
    /// <summary>
    /// Interaction logic for Client.xaml
    /// </summary>
    public partial class Client : Window
    {
        bool error = false;
        Connect connectWindow;
        ClientWebSocket webSocket;
        int timeOut = 1;
        DispatcherTimer timeOutTimer = new DispatcherTimer();
        MediaPlayer ServMess = new MediaPlayer();
        MediaPlayer ServErr = new MediaPlayer();
        MediaPlayer Recieve = new MediaPlayer();
        MediaPlayer Send = new MediaPlayer();
        string lastSentMessage = string.Empty;
        MainWindow mainWindow;
        MemberList GlobalMemberList;
        int timeOutTicker;
        int charlimit = 500;
        string name;
        string ID;
        public Client()
        {
            InitializeComponent();
            ChatBox.Items.Clear();
            timeOutTimer.Tick += (s, ee) => timeOutTicker--;
            timeOutTimer.Interval = TimeSpan.FromSeconds(1);
            timeOutTimer.Start();
            ServMess.Open(new Uri(@"ServMess.wav", UriKind.RelativeOrAbsolute));
            ServErr.Open(new Uri(@"ServErr.wav", UriKind.RelativeOrAbsolute));
            Recieve.Open(new Uri(@"boop2.wav", UriKind.RelativeOrAbsolute));
            Send.Open(new Uri(@"0004.wav", UriKind.RelativeOrAbsolute));
        }
        public async Task Pass(Connect connect, ClientWebSocket ws, byte[] buffer, string name, string servname, string desc, string timeout, string charlimit, MainWindow mainWindow, string ID)
        {
            this.name = name;
            Title = servname + " - Fish Messenger";
            this.ID = ID;
            this.mainWindow = mainWindow;
            connectWindow = connect;
            connect.Visibility = Visibility.Hidden;
            webSocket = ws;
            Home_Username.Text = servname;
            Home_Username.ToolTip = desc;
            try
            {
                timeOut = int.Parse(timeout);
                this.charlimit = int.Parse(charlimit);
            }
            catch
            {

            }
            TheChatBoxSender.MaxLength = this.charlimit;
            UpdateBookmarkButton(connect.UsernameBox.Text);
            while (true)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    connect.Visibility = Visibility.Visible;
                    connect.UsernameError.Text = "Server closed (2x010).";
                    error = true;
                    break;
                }
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                try
                {
                    JObject obj = JObject.Parse(message);
                    if (obj["key"].ToString() == "ServMess")
                    {
                        Grid FUCKKKKKKKK = new Grid();
                        WrapPanel servMessWrap = new WrapPanel();
                        servMessWrap.HorizontalAlignment = HorizontalAlignment.Left;
                        servMessWrap.Margin = new Thickness(0, 0, 140, 0);
                        TextBlock textBlock = new TextBlock();
                        textBlock.Text = obj["mess"].ToString();
                        textBlock.TextWrapping = TextWrapping.Wrap;
                        textBlock.Foreground = new SolidColorBrush(Colors.Gray);
                        textBlock.FontStyle = FontStyles.Italic;
                        TextBlock textBlock2 = new TextBlock();
                        textBlock2.Text = obj["date"].ToString();
                        textBlock2.Foreground = new SolidColorBrush(Colors.Gray);
                        textBlock2.HorizontalAlignment = HorizontalAlignment.Right;
                        textBlock2.Margin = new Thickness(0, 0, 2, 0);
                        servMessWrap.Children.Add(textBlock);
                        FUCKKKKKKKK.Children.Add(servMessWrap);
                        FUCKKKKKKKK.Children.Add(textBlock2);
                        ChatBox.Items.Add(FUCKKKKKKKK);
                        ServMess.Stop();
                        ServMess.Position = new TimeSpan(0, 0, 0);
                        ServMess.Play();
                    }
                    else if (obj["key"].ToString() == "UserMess")
                    {
                        Grid FUCKKKKKKKK = new Grid();
                        WrapPanel servMessWrap = new WrapPanel();
                        servMessWrap.HorizontalAlignment = HorizontalAlignment.Left;
                        servMessWrap.Margin = new Thickness(0, 0, 180, 0);
                        TextBlock textBlockName = new TextBlock();
                        textBlockName.Text = obj["user"].ToString() + ": ";
                        textBlockName.ToolTip = obj["user"].ToString() + "#" + obj["ID"].ToString();
                        textBlockName.FontWeight = FontWeights.Bold;
                        servMessWrap.Children.Add(textBlockName);
                        TextBlock textBlock = new TextBlock();
                        textBlock.Text = obj["mess"].ToString();
                        textBlock.TextWrapping = TextWrapping.Wrap;
                        TextBlock textBlock2 = new TextBlock();
                        textBlock2.Text = obj["date"].ToString();
                        textBlock2.Foreground = new SolidColorBrush(Colors.Gray);
                        textBlock2.HorizontalAlignment = HorizontalAlignment.Right;
                        textBlock2.Margin = new Thickness(0, 0, 2, 0);
                        servMessWrap.Children.Add(textBlock);
                        FUCKKKKKKKK.Children.Add(servMessWrap);
                        FUCKKKKKKKK.Children.Add(textBlock2);
                        ChatBox.Items.Add(FUCKKKKKKKK);
                        if (lastSentMessage == obj["mess"].ToString())
                        {
                            Send.Stop();
                            Send.Position = new TimeSpan(0, 0, 0);
                            Send.Play();
                        }
                        else
                        {
                            Recieve.Stop();
                            Recieve.Position = new TimeSpan(0, 0, 0);
                            Recieve.Play();
                        }
                    }
                    else if (obj["key"].ToString() == "MemberResponse")
                    {
                        MemberList memberList = new MemberList();
                        memberList.Show();
                        memberList.Pass(obj, this, ws);
                        GlobalMemberList = memberList;
                    }
                    else if (obj["key"].ToString() == "MemberUpdate")
                    {
                        GlobalMemberList.Pass(obj, this, ws);
                    }
                    else
                    {
                        MessageBox.Show(message);
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        SendLocalServError("Server sent malformed message: " + ex.InnerException.Message);
                    }
                    catch (NullReferenceException)
                    {
                        SendLocalServError("Server sent malformed message: " + ex.Message);
                    }
                }
            }
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (error)
            {
                connectWindow.Visibility = Visibility.Visible;
            }
            else
            {
                connectWindow.Close();
                webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Goodbye!", CancellationToken.None);
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendFujc();
        }

        async void SendFujc()
        {
            long unixTimestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            var bytes = Encoding.UTF8.GetBytes("{\"key\":\"UserMess\",\"date\":\"" + DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).ToString() + "\",\"user\":\"" + name + "\",\"mess\":\"" + TheChatBoxSender.Text.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\",\"ID\":\"" + ID + "\"}");
            if (webSocket.State == WebSocketState.Open)
            {
                if (timeOutTicker > 0)
                {
                    switch (timeOutTicker)
                    {
                        case 1:
                            SendLocalServMessage("Please wait 1 second before you send a message");
                            break;
                        default:
                            SendLocalServMessage("Please wait " + timeOutTicker + " seconds before you send a message");
                            break;
                    }

                }
                else
                {
                    if (TheChatBoxSender.Text.Length > 0)
                    {
                        var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
                        await webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                        lastSentMessage = TheChatBoxSender.Text;
                        TheChatBoxSender.Text = "";
                        timeOutTicker = timeOut;
                    }
                    else
                    {
                        SendLocalServMessage("Please enter a message");
                    }
                }
            }
            else
            {
                connectWindow.Visibility = Visibility.Visible;
                connectWindow.UsernameError.Text = "Server closed (2x010).";
                error = true;
                Close();
            }
        }

        private void SendLocalServMessage(string message)
        {
            WrapPanel servMessWrap = new WrapPanel();
            servMessWrap.HorizontalAlignment = HorizontalAlignment.Left;
            TextBlock textBlock = new TextBlock();
            textBlock.Text = message;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Foreground = new SolidColorBrush(Colors.Gray);
            textBlock.FontStyle = FontStyles.Italic;
            servMessWrap.Children.Add(textBlock);
            ChatBox.Items.Add(servMessWrap);
            ServMess.Stop();
            ServMess.Position = new TimeSpan(0, 0, 0);
            ServMess.Play();
        }

        private void SendLocalServError(string message)
        {
            WrapPanel servMessWrap = new WrapPanel();
            servMessWrap.HorizontalAlignment = HorizontalAlignment.Left;
            TextBlock textBlock = new TextBlock();
            textBlock.Text = message;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Foreground = new SolidColorBrush(Colors.IndianRed);
            textBlock.FontStyle = FontStyles.Italic;
            servMessWrap.Children.Add(textBlock);
            ChatBox.Items.Add(servMessWrap);
            ServErr.Stop();
            ServErr.Position = new TimeSpan(0, 0, 0);
            ServErr.Play();
        }

        private void TheChatBoxSender_TextChanged(object sender, TextChangedEventArgs e)
        {
            CharLimit.Text = TheChatBoxSender.Text.Length == 0 ? "" : (charlimit - TheChatBoxSender.Text.Length).ToString() + " characters left";
        }

        private void Pin_Click(object sender, RoutedEventArgs e)
        {
            FlipBookmark(connectWindow.UsernameBox.Text);
        }
        void FlipBookmark(string URL)
        {
            IniFile myIni = new IniFile("config.ini");
            if (mainWindow.Servers.Contains(URL))
            {
                int location = mainWindow.Servers.Count - mainWindow.Servers.IndexOf(URL);
                myIni.Write(location + "URL", "", "Servers");
                myIni.Write(location + "Title", "", "Servers");
                myIni.Write(location + "Pass", "", "Servers");
                myIni.Write(location + "Auto", "false", "Servers");
            }
            else
            {
                int location;
                if (mainWindow.ServersNull.Count == 0)
                {
                    location = mainWindow.Servers.Count + 1;
                    myIni.Write("Count", (int.Parse(myIni.Read("Count", "Servers")) + 1).ToString(), "Servers");
                }
                else
                {
                    location = mainWindow.ServersNull[0];
                }
                myIni.Write(location + "URL", URL, "Servers");
                myIni.Write(location + "Title", Home_Username.Text, "Servers");
                myIni.Write(location + "Pass", connectWindow.PasswordBox.Text, "Servers");
                myIni.Write(location + "Auto", "false", "Servers");
            }
            mainWindow.UpdateServList();
            UpdateBookmarkButton(URL);
        }

        void UpdateBookmarkButton(string URL)
        {
            Pin.Content = mainWindow.Servers.Contains(URL) ? "Unbookmark" : "Bookmark";
            BookmarkMenuItem.Header = mainWindow.Servers.Contains(URL) ? "Unbookmark" : "Bookmark";
        }

        private async void TheChatBoxSender_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SendFujc();
            }
        }

        public async void MemList_Click(object sender, RoutedEventArgs e)
        {
            var bytes = Encoding.UTF8.GetBytes("{\"key\":\"MemberRequest\"" + "}");
            if (webSocket.State == WebSocketState.Open)
            {
                if (timeOutTicker > 0)
                {
                    switch (timeOutTicker)
                    {
                        case 1:
                            SendLocalServMessage("Please wait 1 second before you send a request");
                            break;
                        default:
                            SendLocalServMessage("Please wait " + timeOutTicker + " seconds before you send a request");
                            break;
                    }

                }
                else
                {
                    var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
                    await webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                    timeOutTicker = timeOut;
                }
            }
            else
            {
                connectWindow.Visibility = Visibility.Visible;
                connectWindow.UsernameError.Text = "Server closed (2x010).";
                error = true;
                Close();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ChatBox.Items.Clear();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            FlipBookmark(connectWindow.UsernameBox.Text);
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
