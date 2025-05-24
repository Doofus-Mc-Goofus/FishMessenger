using System;
using System.Collections.Generic;
using System.IO;
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
using ChecksumGen;
using INI;

namespace FishMessenger
{
    /// <summary>
    /// Interaction logic for Setup.xaml
    /// </summary>
    public partial class Setup : Window
    {
        bool isClosing = false;
        MainWindow mainWindow;
        Random random = new Random();
        Checksum checksum = new Checksum();
        public Setup()
        {
            InitializeComponent();
            UsernameBox.Focus();
        }
        public void Pass(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!isClosing)
            {
                mainWindow.CloseMessenger();
            }
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            if (UsernameBox.Text.Length >= 3)
            {
                if (PasswordBox.Text.Length >= 3)
                {
                    Continue.Visibility = Visibility.Hidden;
                    UsernameBox.Visibility = Visibility.Hidden;
                    UsernameHeader.Visibility = Visibility.Hidden;
                    UsernameError.Visibility = Visibility.Hidden;
                    Title.Content = "Thank you!";
                    Desc.Text = "Fish Messenger is now creating your account. This may take a moment.";
                    using (FileStream fs = File.Create(@"config.ini"))
                    {
                        byte[] info = new UTF8Encoding(true).GetBytes("[User]\n[Servers]\n[Messages]");
                        fs.Write(info, 0, info.Length);
                    }
                    Task.Delay(1000);
                    IniFile iniFile = new IniFile("config.ini");
                    iniFile.Write("Name", UsernameBox.Text, "User");
                    iniFile.Write("PFP", "pack://application:,,,/usertile1.png", "User");
                    iniFile.Write("PageColor", "#FF5BBFFF", "User");
                    iniFile.Write("Ver", mainWindow.currentVer, "User");
                    iniFile.Write("Password", PasswordBox.Text, "User");
                    iniFile.Write("StaySign", "false", "User");
                    string ID = random.Next(10000, 99999).ToString();
                    iniFile.Write("ID", ID, "User");
                    iniFile.Write("Checksum", checksum.Generate(UsernameBox.Text + ID).ToString(), "User");
                    iniFile.Write("Count", "0", "Messages");
                    iniFile.Write("Count", "0", "Servers");
                    mainWindow.FirstRunAfter();
                    isClosing = true;
                    Close();
                }
                else
                {
                    UsernameError.Content = "Your password must be 3 characters or longer.";
                }
            }
            else
            {
                UsernameError.Content = "Your username must be 3 characters or longer.";
            }
        }
    }
}
