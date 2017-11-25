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
using System.Threading;

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
        private int logicTime;
        private string pseudo;

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
            Thread th = new Thread(new ThreadStart(synchronizeFromServer));
            th.Start();
        }
        // event handler for timer.Tick, been called every 1 second
        private void synchronizeFromServer()
        {
            while (true)
            {
                string response = LeRemot.getUpdateFromServer(logicTime);
                Debug.WriteLine($"--tick--response {response}");
                if (response != "")
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        ChatHistory.Text += $"{response}\n";
                        logicTime++;
                    });
                }
                //listMembers = LeRemot.getClientListFromServer();

                //membersViewList = convertToListView(listMembers);
                //Debug.WriteLine(listMembers);
            }
        }

        ObservableCollection<MemberViewItem> convertToListView(LinkedList<string> listMembers)
        {
            ObservableCollection<MemberViewItem> res = new ObservableCollection<MemberViewItem>();
            foreach (string name in listMembers)
            {
                res.Add(new MemberViewItem(name));
            }
            return res;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            int port = Int32.Parse(PortBox.Text);
            pseudo = PseudoBox.Text;
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
                logicTime = LeRemot.clientLogin(pseudo);
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

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"{pseudo}: {InputBox.Text}");
            if (InputBox.Text.Trim() != "")
            {
                LeRemot.sendMsgToServer($"{pseudo}: {InputBox.Text}");
            }
        }
    }
}
