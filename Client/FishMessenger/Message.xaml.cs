using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using INI;

namespace FishMessenger
{
    /// <summary>
    /// Interaction logic for Message.xaml
    /// </summary>
    public partial class Message : Window
    {
        private MediaPlayer LikeSnd = new MediaPlayer();
        private MediaPlayer DislikeSnd = new MediaPlayer();
        private MediaPlayer Revoke = new MediaPlayer();
        private byte status = 0;
        private int ID = 0;
        private int realID = 0;
        private MainWindow mainWindow = null;
        IniFile file = new IniFile("config.ini");
        public Message()
        {
            InitializeComponent();
            LikeSnd.Open(new Uri(@"like.wav", UriKind.RelativeOrAbsolute));
            DislikeSnd.Open(new Uri(@"dislike.wav", UriKind.RelativeOrAbsolute));
            Revoke.Open(new Uri(@"0004.wav", UriKind.RelativeOrAbsolute));
        }
        public void Pass(string username, string message, int ID, byte status, MainWindow dingus, int trueID)
        {
            Username.Content = username;
            MessageBlock.Text = message;
            this.Title = username + " - " + message;
            this.ID = ID;
            realID = trueID;
            mainWindow = dingus;
            if (status == 1)
            {
                Icon.Content = "👍";
            }
            else if (status == 2)
            {
                Icon.Content = "👎";
            }
            else
            {
                Icon.Content = "";
            }
            this.status = status;
            file.Write(realID.ToString() + "Read", "true", "Messages");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (status == 1)
            {
                status = 0;
                Revoke.Stop();
                Revoke.Position = new TimeSpan(0, 0, 0);
                Revoke.Play();
                Icon.Content = "";
            }
            else
            {
                status = 1;
                LikeSnd.Stop();
                LikeSnd.Position = new TimeSpan(0, 0, 0);
                LikeSnd.Play();
                Icon.Content = "👍";
            }
            file.Write(realID.ToString() + "Stat", status.ToString(), "Messages");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (status == 2)
            {
                status = 0;
                Revoke.Stop();
                Revoke.Position = new TimeSpan(0, 0, 0);
                Revoke.Play();
                Icon.Content = "";
            }
            else
            {
                status = 2;
                DislikeSnd.Stop();
                DislikeSnd.Position = new TimeSpan(0, 0, 0);
                DislikeSnd.Play();
                Icon.Content = "👎";
            }
            file.Write(realID.ToString() + "Stat", status.ToString(), "Messages");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mainWindow.PassBack(status, this.ID);
        }
    }
}
