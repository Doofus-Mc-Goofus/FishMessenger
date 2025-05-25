using System;
using System.Net.NetworkInformation;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ChecksumGen;
using INI;
using Newtonsoft.Json.Linq;
namespace FishMessenger
{
    /// <summary>
    /// Interaction logic for Connect.xaml
    /// </summary>
    public partial class Connect : Window
    {
        MainWindow mainWindow;
        int ingus = 0;
        DispatcherTimer dispatcherTimer;
        public Connect()
        {
            InitializeComponent();
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += (s, ee) => UpdateThing();
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(33);
            dispatcherTimer.Start();
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

        public void Pass(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                EverythingElse.Visibility = Visibility.Hidden;
                Title.Content = "Connecting...";
                LoadingSirc.Visibility = Visibility.Visible;
                ConnectToServer(UsernameBox.Text);
            }
            else
            {
                UsernameError.Text = "Please connect to the internet.";
            }
        }
        private async Task ReceiveAsync(ClientWebSocket ws)
        {
            // Implement logic to receive and process messages
            while (ws.State == WebSocketState.Open)
            {
                try
                {
                    var buffer = new byte[1024];
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("Closing WebSocket");
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        break;
                    }

                    string message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received: {message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred during reception: {ex.Message}");
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }
            }
        }
        public void AutoConnect(string URL, string Password)
        {
            UsernameBox.Text = URL;
            PasswordBox.Text = Password;
            EverythingElse.Visibility = Visibility.Hidden;
            Title.Content = "Connecting...";
            LoadingSirc.Visibility = Visibility.Visible;
            ConnectToServer(URL);
        }
        async void ConnectToServer(string URL)
        {
            try
            {
                using (var ping = new Ping())
                {
                    PingReply reply = ping.Send("8.8.8.8", 2000);
                    if (reply.Status != IPStatus.Success)
                    {
                        if (reply.Status == IPStatus.TimedOut)
                        {
                            UsernameError.Text = "Request timed out (1x008).";
                        }
                        else if (reply.Status == IPStatus.DestinationHostUnreachable || reply.Status == IPStatus.DestinationPortUnreachable || reply.Status == IPStatus.DestinationNetworkUnreachable)
                        {
                            UsernameError.Text = "Client-side communication failure (1x004).";
                        }
                        else if (reply.Status == IPStatus.PacketTooBig)
                        {
                            UsernameError.Text = "Malformed packets, possibly due to a hardware failure (1x005).";
                        }
                        else if (reply.Status == IPStatus.NoResources)
                        {
                            UsernameError.Text = "Insufficient network resources (1x006).";
                        }
                        else if (reply.Status == IPStatus.HardwareError)
                        {
                            UsernameError.Text = "Unable to connect because of hardware failure (1x007).";
                        }
                        else
                        {
                            UsernameError.Text = "An unknown error has occurred (1x000).";
                        }
                        EverythingElse.Visibility = Visibility.Visible;
                        LoadingSirc.Visibility = Visibility.Hidden;
                        Title.Content = "Connect to a server";
                        return;
                    }
                }
            }
            catch (Exception)
            {
                UsernameError.Text = "An unknown error has occurred (1x000).";
                EverythingElse.Visibility = Visibility.Visible;
                LoadingSirc.Visibility = Visibility.Hidden;
                Title.Content = "Connect to a server";
                return;
            }
            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                IniFile iniFile = new IniFile("config.ini");
                string name = iniFile.Read("Name", "User");
                string ID = iniFile.Read("ID", "User");
                string pass = iniFile.Read("Password", "User");
                Checksum checksum = new Checksum();
                if (checksum.Generate(name + iniFile.Read("ID", "User")).ToString() != iniFile.Read("Checksum", "User"))
                {
                    UsernameError.Text = "Checksum failed. Please restart Fish Messenger and try again. (0x001)";
                    return;
                }
                var ws = new ClientWebSocket();
                await ws.ConnectAsync(new Uri(URL + "/ws"), CancellationToken.None);
                var buffer = new byte[1024 * 16];
                var bytes = Encoding.UTF8.GetBytes("{\"key\":\"Join\",\"ID\":\"" + ID + "\",\"user\":\"" + name + "\",\"pass\":\"" + PasswordBox.Text + "\",\"userpass\":\"" + pass + "\",\"pfp\":\"" + iniFile.Read("PFP", "User") + "\",\"col\":\"" + iniFile.Read("PageColor", "User") + "\"}");
                if (ws.State == WebSocketState.Open)
                {
                    var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
                    await ws.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                JObject obj = JObject.Parse(message);
                if (obj["pass"].ToString() == "200")
                {
                    // MessageBox.Show(message);
                    Client clientWindow = new Client();
                    clientWindow.Show();
                    clientWindow.Pass(this, ws, buffer, name, obj["servname"].ToString(), obj["desc"].ToString(), obj["timeout"].ToString(), obj["charlimit"].ToString(), mainWindow, ID);
                }
                else if (obj["pass"].ToString() == "403")
                {
                    UsernameError.Text = "Password is incorrect";
                    ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "403 for user", CancellationToken.None);
                }
                else if (obj["pass"].ToString() == "429")
                {
                    UsernameError.Text = "Too many people are in this server, please try again later";
                    ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "429 for user", CancellationToken.None);
                }
                else if (obj["pass"].ToString() == "401")
                {
                    UsernameError.Text = "Authentication failed (0x002)";
                    ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Impersonator kicked", CancellationToken.None);
                }
                else
                {
                    UsernameError.Text = "Server rejected request for unknown reason (2x009)";
                    ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "2x009 for user", CancellationToken.None);
                }


                // while (true)
                // {
                //     var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                //     if (result.MessageType == WebSocketMessageType.Close)
                //     {
                //         UsernameError.Text = "Server closed (3x000).";
                //         break;
                //     }
                //     var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                //     MessageBox.Show(message);
                // }
            }
            catch (WebSocketException e)
            {
                try
                {
                    UsernameError.Text = e.InnerException.Message;
                }
                catch (NullReferenceException)
                {
                    UsernameError.Text = "Unable to communicate to server (2x000)";
                }
            }
            catch (UriFormatException)
            {
                UsernameError.Text = "The URL could not be parsed (2x001)";
            }
            catch (InvalidOperationException)
            {
                UsernameError.Text = "Enter a valid URL including it's scheme / WebSocket type and port (1x002)";
            }
            catch (ArgumentException)
            {
                UsernameError.Text = "Only HTTPS is an allowed scheme (1x003)";
            }
            finally
            {
                EverythingElse.Visibility = Visibility.Visible;
                LoadingSirc.Visibility = Visibility.Hidden;
                Title.Content = "Connect to a server";
            }
        }
    }
}
