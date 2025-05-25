using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
namespace FishMessenger
{
    /// <summary>
    /// Interaction logic for MemberList.xaml
    /// </summary>
    public partial class MemberList : Window
    {
        public WebSocket webSocket;
        public Client client;
        public MemberList()
        {
            InitializeComponent();
            InsertGerder.Items.Clear();
        }

        public void Pass(JObject List, Client client, WebSocket ws)
        {
            InsertGerder.Items.Clear();
            try
            {
                webSocket = ws;
                this.client = client;
                Title = "Member List - " + client.Home_Username.Text + " - Fish Messenger";
                for (var i = 0; i < int.Parse(List["Count"].ToString()); i++)
                {
                    ListBoxItem item = new ListBoxItem();
                    item.Padding = new Thickness(0);
                    item.Tag = i;
                    item.Selected += (s, ee) => item.IsSelected = false;
                    Grid grid = new Grid();
                    Rectangle BackG = new Rectangle();
                    BackG.VerticalAlignment = VerticalAlignment.Top;
                    BackG.Height = 50;
                    BackG.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(List[i + "Col"].ToString()));
                    BackG.OpacityMask = new LinearGradientBrush
                    {
                        StartPoint = new Point(0.5, 0),
                        EndPoint = new Point(0.5, 1),
                        GradientStops = { new GradientStop(Color.FromArgb(190, 0, 0, 0), 0), new GradientStop(Color.FromArgb(0, 0, 0, 0), 1) }
                    };
                    grid.Children.Add(BackG);

                    Image PFP = new Image();
                    try
                    {
                        PFP.Source = new BitmapImage(new Uri(List[i + "PFP"].ToString()));
                    }
                    catch
                    {
                        PFP.Source = new BitmapImage(new Uri("pack://application:,,,/usertile0.png"));
                    }
                    PFP.HorizontalAlignment = HorizontalAlignment.Left;
                    PFP.VerticalAlignment = VerticalAlignment.Top;
                    PFP.Width = 40;
                    PFP.Height = 40;
                    PFP.Margin = new Thickness(10, 9, 0, 0);
                    grid.Children.Add(PFP);
                    Image PFPFrame = new Image();
                    PFPFrame.Source = new BitmapImage(new Uri("pack://application:,,,/FrameSmall.png"));
                    PFPFrame.HorizontalAlignment = HorizontalAlignment.Left;
                    PFPFrame.VerticalAlignment = VerticalAlignment.Top;
                    PFPFrame.Width = 64;
                    PFPFrame.Height = 64;
                    PFPFrame.Margin = new Thickness(-2, -2, 0, 0);
                    grid.Children.Add(PFPFrame);
                    TextBlock UserName = new TextBlock();
                    UserName.VerticalAlignment = VerticalAlignment.Top;
                    UserName.Margin = new Thickness(59, 5, 15, 0);
                    UserName.Text = List[i + "Name"].ToString();
                    UserName.FontSize = 16;
                    UserName.TextTrimming = TextTrimming.CharacterEllipsis;
                    grid.Children.Add(UserName);
                    TextBlock Realname = new TextBlock();
                    Realname.VerticalAlignment = VerticalAlignment.Top;
                    Realname.Margin = new Thickness(59, 25, 15, 0);
                    Realname.Text = List[i + "Name"].ToString() + "#" + List[i + "ID"].ToString();
                    Realname.TextTrimming = TextTrimming.CharacterEllipsis;
                    Realname.FontFamily = new FontFamily("Segoe UI Semilight");
                    grid.Children.Add(Realname);
                    WrapPanel wrapper = new WrapPanel();
                    wrapper.HorizontalAlignment = HorizontalAlignment.Right;
                    wrapper.Margin = new Thickness(0, 5, 8, 15);
                    wrapper.VerticalAlignment = VerticalAlignment.Top;
                    TextBlock Status = new TextBlock();
                    Status.VerticalAlignment = VerticalAlignment.Center;
                    Status.Text = "■ Online";
                    Status.TextTrimming = TextTrimming.CharacterEllipsis;
                    Status.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF24980C"));
                    Status.TextAlignment = TextAlignment.Right;
                    Status.HorizontalAlignment = HorizontalAlignment.Right;
                    Status.Margin = new Thickness(6, 0, 0, 0);
                    wrapper.Children.Add(Status);
                    Grid padding = new Grid();
                    padding.Width = 2;
                    padding.Height = 21.28;
                    wrapper.Children.Add(padding);
                    grid.Children.Add(wrapper);
                    item.Content = grid;
                    InsertGerder.Items.Add(item);
                }
            }
            catch
            {
                MessageBox.Show("Error");
                Close();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var bytes = Encoding.UTF8.GetBytes("{\"key\":\"CloseMemberRequest\"" + "}");
            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
            webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            client.MemList_Click(sender, e);
            Close();
        }
    }
}
