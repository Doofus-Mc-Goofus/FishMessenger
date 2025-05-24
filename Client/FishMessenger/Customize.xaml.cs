using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using INI;

namespace FishMessenger
{
    /// <summary>
    /// Interaction logic for Customize.xaml
    /// </summary>
    public partial class Customize : Window
    {
        bool ready = false;
        public byte selected = 1;
        public byte selectedcol = 6;
        string url;
        public MainWindow mainWindow;
        private MediaPlayer FocusSnd = new MediaPlayer();
        private MediaPlayer ClickSnd = new MediaPlayer();
        Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
        public void Pass(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }
        public Customize()
        {
            InitializeComponent();
            IniFile myIni = new IniFile("config.ini");
            FocusSnd.Open(new Uri(@"FOCUS_A.wav", UriKind.RelativeOrAbsolute));
            ClickSnd.Open(new Uri(@"Click.wav", UriKind.RelativeOrAbsolute));
            url = myIni.Read("PFP", "User");
            switch (url)
            {
                case "pack://application:,,,/usertile1.png":
                    selected = 1;
                    break;
                case "pack://application:,,,/usertile2.png":
                    selected = 2;
                    break;
                case "pack://application:,,,/usertile3.png":
                    selected = 3;
                    break;
                case "pack://application:,,,/usertile4.png":
                    selected = 4;
                    break;
                case "pack://application:,,,/usertile5.png":
                    selected = 5;
                    break;
                case "pack://application:,,,/usertile6.png":
                    selected = 6;
                    break;
                case "pack://application:,,,/usertile7.png":
                    selected = 7;
                    break;
                case "pack://application:,,,/usertile8.png":
                    selected = 8;
                    break;
                case "pack://application:,,,/usertile9.png":
                    selected = 9;
                    break;
                case "pack://application:,,,/usertile10.png":
                    selected = 10;
                    break;
                case "pack://application:,,,/usertile11.png":
                    selected = 11;
                    break;
                default:
                    selected = 0;
                    break;
            }
            Choose.IsChecked = (selected != 0);
            Upload.IsChecked = (selected == 0);
            dialog.Filter = "Image Files|*.png;*.jpg;*.gif;*.bmp;*.jpeg"; // Filter files by extension
            ready = true;
            switch (selected)
            {
                case 0:
                    ChoosePFP.Visibility = Visibility.Collapsed;
                    UploadButton.Visibility = Visibility.Visible;
                    break;
                default:
                    ChoosePFP.Visibility = Visibility.Visible;
                    UploadButton.Visibility = Visibility.Hidden;
                    break;
            }
            Arr.Margin = new Thickness(-32 + selected * 52, 80, 0, 0);
            FUCK(myIni.Read("PageColor", "User"));
            Home_GradRect.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(myIni.Read("PageColor", "User")));
            Arr2.Margin = new Thickness(-32 + selectedcol * 52, 80, 0, 0);
            try
            {
                PFP.Source = new BitmapImage(new Uri(url));
            }
            catch
            {
                PFP.Source = new BitmapImage(new Uri("pack://application:,,,/usertile0.png"));
            }
        }

        private void Choose_Checked(object sender, RoutedEventArgs e)
        {
            if (ready)
            {
                selected = 1;
                Arr.Margin = new Thickness(-32 + selected * 52, 80, 0, 0);
                ChoosePFP.Visibility = Visibility.Visible;
                UploadButton.Visibility = Visibility.Hidden;
            }
        }

        private void Upload_Checked(object sender, RoutedEventArgs e)
        {
            if (ready)
            {
                selected = 0;
                ChoosePFP.Visibility = Visibility.Collapsed;
                UploadButton.Visibility = Visibility.Visible;
            }
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                PFP.Source = new BitmapImage(new Uri(dialog.FileName));
                url = dialog.FileName;
            }
        }

        private void PFP_1_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangePFP(1);
        }

        private void PFP_2_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangePFP(2);
        }

        private void PFP_3_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangePFP(3);
        }

        private void PFP_4_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangePFP(4);
        }

        private void PFP_5_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangePFP(5);
        }

        private void PFP_6_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangePFP(6);
        }

        private void PFP_7_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangePFP(7);
        }

        private void PFP_8_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangePFP(8);
        }

        private void PFP_9_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangePFP(9);
        }
        private void PFP_10_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangePFP(10);
        }

        private void PFP_11_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangePFP(11);
        }

        void ChangePFP (byte ptr)
        {
            PFP.Source = new BitmapImage(new Uri("pack://application:,,,/usertile" + ptr + ".png"));
            url = "pack://application:,,,/usertile" + ptr + ".png";
            selected = ptr;
            Arr.Margin = new Thickness(-32 + selected * 52, 80, 0, 0);
            ClickSnd.Stop();
            ClickSnd.Play();
        }

        void ChangeColor(string ptr)
        {
            Home_GradRect.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ptr));
            ClickSnd.Stop();
            ClickSnd.Play();
            FUCK(ptr);
            Arr2.Margin = new Thickness(-32 + selectedcol * 52, 80, 0, 0);
        }

        private void PFP_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            FocusSnd.Stop();
            FocusSnd.Play();
        }


        private void Color_1_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangeColor("#FFFF5B88");
        }

        private void Color_2_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangeColor("#FFFF905B");
        }

        private void Color_3_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangeColor("#FFFFE25B");
        }

        private void Color_4_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangeColor("#FF8DFF5B");
        }

        private void Color_5_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangeColor("#FF4BE83E");
        }
        private void Color_6_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangeColor("#FF5BBFFF");
        }
        private void Color_7_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangeColor("#FF5B74FF");
        }
        private void Color_8_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangeColor("#FF8D5BFF");
        }
        private void Color_9_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangeColor("#FFE05BFF");
        }
        private void Color_10_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangeColor("#FFFF9FEA");
        }

        private void Color_11_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangeColor("#FFA6C3D4");
        }

        void FUCK(string ptr)
        {
            switch (ptr)
            {
                case "#FFFF5B88":
                    selectedcol = 1;
                    break;
                case "#FFFF905B":
                    selectedcol = 2;
                    break;
                case "#FFFFE25B":
                    selectedcol = 3;
                    break;
                case "#FF8DFF5B":
                    selectedcol = 4;
                    break;
                case "#FF4BE83E":
                    selectedcol = 5;
                    break;
                case "#FF5BBFFF":
                    selectedcol = 6;
                    break;
                case "#FF5B74FF":
                    selectedcol = 7;
                    break;
                case "#FF8D5BFF":
                    selectedcol = 8;
                    break;
                case "#FFE05BFF":
                    selectedcol = 9;
                    break;
                case "#FFFF9FEA":
                    selectedcol = 10;
                    break;
                case "#FFA6C3D4":
                    selectedcol = 11;
                    break;
                default:
                    selectedcol = 0;
                    break;
            }
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            IniFile myIni = new IniFile("config.ini");
            myIni.Write("PFP", url, "User");
            myIni.Write("PageColor", Home_GradRect.Fill.ToString(), "User");
            mainWindow.Update();
        }


    }
}
