using System;
using System.Collections.Generic;
using System.Windows;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;
using System.ComponentModel;

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
        // continously send request to the server in order to be updated
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
            // use regular expression to check the validity of port number
            Regex rgxPort = new Regex(@"^[\d]+$");
            if (!rgxPort.IsMatch(PortBox.Text))
            {
                MessageBox.Show("Port number not validate, it should be a digit or digits", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int port = Int32.Parse(PortBox.Text);
            pseudo = PseudoBox.Text;
            // use regular expression to check the validity of pseudo name
            Regex rgxPseudo = new Regex(@"^[a-zA-Z][\w]*$");
            if (!rgxPseudo.IsMatch(pseudo))
            {
                MessageBox.Show("Pseudo name not validate, it should longer thant one and begin with a alphabet", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string ip = IPBox.Text;
            //Default address "tcp://localhost:12345/Serveur"
            LeRemot = (RemotingInterface.IRemotChaine)Activator.GetObject(
            typeof(RemotingInterface.IRemotChaine), $"tcp://{ip}:{port}/Serveur");

            // catch connection error
            try
            {
                logicTime = LeRemot.clientLogin(pseudo);
            }
            // when connection error occurs, ask the user to re-connect
            catch (System.Net.Sockets.SocketException err) 
            {
                MessageBox.Show("Error connecting the remote server, please choose another address", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine("{0} Exception caught.", err);
                return;
            }
            // handle username duplication error
            if (logicTime == -1)
            {
                //Debug.WriteLine("logicTime {0}", logicTime);
                MessageBox.Show("The current pseudo is not available, please choose another Pesedo", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                PortBox.IsEnabled = false;
                PseudoBox.IsEnabled = false;
                IPBox.IsEnabled = false;
                // start a thread to get updates from server
                Client_Logined();
            }
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
                // re-initialize the send text box to vide
                InputBox.Text = "";
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you really want quit?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                if (LeRemot != null)
                {
                    LeRemot.clientLogout(pseudo);
                    LeRemot = null;
                }
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}
