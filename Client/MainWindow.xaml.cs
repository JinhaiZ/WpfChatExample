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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Windows.Threading;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private RemotingInterface.IRemotChaine LeRemot;
        private DispatcherTimer timer;
        private LinkedList<string> listMembers;

        public ObservableCollection<MemberViewItem> membersViewList = new ObservableCollection<MemberViewItem>();
        public MainWindow()
        {
            InitializeComponent();
            // for binding listView to the Linkedlist listMembers
            this.DataContext = this;
            listMembers = new LinkedList<string>();
        }
        // for binding listView to the Linkedlist listMembers
        public ObservableCollection<MemberViewItem> MembersViewList
        {
            get { return membersViewList; }
        }

        // add a Timer Control thread in WPF 
        void Client_Logined()
        {
            //timer.Elapsed += timer1_Tick
            //timer.AutoReset = false;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer1_Tick;

            timer.Start();
        }
        // event handler for timer.Tick, been called every 1 second
        private void timer1_Tick(object sender, EventArgs e)
        {
            //Debug.WriteLine("haha");
            
            //timer.Stop();
            //this.Close();
        }

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OutputBox.Text = LeRemot.Hello();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            int port = Int32.Parse(PortBox.Text);
            string pseudo = PseudoBox.Text;
            string ip = IPBox.Text;
            PortBox.IsEnabled = false;
            PseudoBox.IsEnabled = false;
            IPBox.IsEnabled = false;
            //Default address "tcp://localhost:12345/Serveur"
            LeRemot = (RemotingInterface.IRemotChaine)Activator.GetObject(
                typeof(RemotingInterface.IRemotChaine), $"tcp://{ip}:{port}/Serveur");
            if (LeRemot != null)
            {
                // start a thread to get updates from server
                Client_Logined();
            }
            //TODO: manage error input
        }

        public class MemberViewItem
        {
            public MemberViewItem(string MemberName)
            {
                this.MemberName = MemberName;
            }
            public string MemberName { get; set; }
        }

    }
}
