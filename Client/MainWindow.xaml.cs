using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Threading;
using System.Text.RegularExpressions;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private RemotingInterface.IRemotChaine LeRemot;
        private LinkedList<string> listMembers;
        private int logicTime;
        private string pseudo;

        public MainWindow()
        {
            InitializeComponent();
            // for binding listView to the Linkedlist listMembers
            this.DataContext = this;
            listMembers = new LinkedList<string>();
            // initialize UI component status
            Send.IsEnabled = false;
            Logout.IsEnabled = false;
        }
        // wrapper of membersViewList, the wrapper is used to binding the
        // ListView to MembersViewList
        /*
        public ObservableCollection<MemberViewItem> MembersViewList
        {
            get { return convertToListView(listMembers); }
        }*/

        // add a Timer Control thread in WPF 
        void Client_Logined()
        {
            Thread th = new Thread(new ThreadStart(synchronizeFromServer));
            th.Start();

            listMembers = LeRemot.getClientListFromServer();
            convertToListView(listMembers);
            // change UI component status
            Send.IsEnabled = true;
            Logout.IsEnabled = true;
            Login.IsEnabled = false;
        }
        // event handler for timer.Tick, been called every 1 second
        private void synchronizeFromServer()
        {
            while (true)
            {
                string response = LeRemot.getUpdateFromServer(logicTime);
                //Debug.WriteLine($"--tick--response {response}");
                if (response != "")
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        ChatHistory.Text += $"{response}\n";
                        logicTime++;
                        checkLoginMessage(response);
                        checkLogoutMessage(response);
                    });
                }
                //Debug.WriteLine(listMembers);
            }
        }
        public void checkLoginMessage(string response)
        {
            string pattern = @"\A([\w]+)\ has\ joined\ the\ chat\Z";
            MatchCollection matches = Regex.Matches(response, pattern);
            Debug.WriteLine("$matches count {matches.Count}\n");
            if (matches.Count > 0)
            {
                foreach (Match match in Regex.Matches(response, pattern))
                {
                    //Debug.WriteLine($"add {match.Groups[1].ToString()} to listMembers\n");
                    listMembers.AddLast(match.Groups[1].ToString());
                }
                convertToListView(listMembers);
            }
            //Debug.WriteLine($"listMembers count {listMembers.Count.ToString()}");
            //foreach (string name in listMembers)
                //Debug.WriteLine($"listMembers {name}");
        }
        public void checkLogoutMessage(string response)
        {
            string pattern = @"\A([\w]+)\ has\ left\ the\ chat\Z";
            MatchCollection matches = Regex.Matches(response, pattern);
            Debug.WriteLine("$matches count {matches.Count}\n");
            if (matches.Count > 0)
            {
                foreach (Match match in Regex.Matches(response, pattern))
                {
                    //Debug.WriteLine($"add {match.Groups[1].ToString()} to listMembers\n");
                    listMembers.Remove(match.Groups[1].ToString());
                }
                convertToListView(listMembers);
            }
            //Debug.WriteLine($"listMembers count {listMembers.Count.ToString()}");
            //foreach (string name in listMembers)
            //Debug.WriteLine($"listMembers {name}");
        }
        public void convertToListView(LinkedList<string> listMembers)
        {
            lvMembers.Items.Clear();
            foreach (string name in listMembers)
                lvMembers.Items.Add(new MemberViewItem(name));
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
            if (LeRemot != null && InputBox.Text.Trim() != "")
            {
                LeRemot.sendMsgToServer($"{pseudo}: {InputBox.Text}");
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            LeRemot.clientLogout(pseudo);
            Close();
        }
    }
}
