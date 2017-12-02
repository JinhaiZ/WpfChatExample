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
        // déclarer le RemotingInterface
        private RemotingInterface.IRemotChaine LeRemot;
        // déclarer une liste pour enregister les membres
        private LinkedList<string> listMembers;
        // déclarer une varible pour determiner quels messages devraient demander depuis le serveur
        private int logicTime;
        // le pseudonyme d'utilisateur
        private string pseudo;
        // le thread qui s'occupe la communication entre le client et le serveur
        Thread th;
        // fonction qui va être appelé lorsque l'utilisateur clique login
        public MainWindow()
        {
            InitializeComponent();
            // pour la data binding entre la listView et le Linkedlist de listMembers
            this.DataContext = this;
            listMembers = new LinkedList<string>();
            // initialiser l'état des composants UI
            Send.IsEnabled = false;
            Logout.IsEnabled = false;
            ChatHistory.IsReadOnly = true;
        }
        // fonction qui va être appelé lorsque l'utilisateur login avec succès
        // créer un thread qui s'occupe la communication entre le client et le serveur
        void Client_Logined()
        {
            th = new Thread(new ThreadStart(synchronizeFromServer));
            th.Start();
            // récuperer la liste de membres en ligne
            listMembers = LeRemot.getClientListFromServer();
            // mise à jour le listView egalement
            convertToListView(listMembers);
            // mise à jour l'état des composants UI
            Send.IsEnabled = true;
            Logout.IsEnabled = true;
            Login.IsEnabled = false;
        }
        // envoyer en continu la requête au serveur pour être mis à jour
        private void synchronizeFromServer()
        {
            while (true)
            {
                string response = LeRemot.getUpdateFromServer(logicTime);
                //Debug.WriteLine($"--tick--response {response}");
                if (response != "")
                {
                    // pour que ce thread pussie modifier les états du thread pricipal
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        ChatHistory.Text += $"{response}\n";
                        logicTime++;
                        // vérifier les messages spéciaux, message de login ou message de logout
                        checkLoginMessage(response);
                        checkLogoutMessage(response);
                    });
                }
                //Debug.WriteLine(listMembers);
            }
        }
        // si un message reçu est de type login, ajouter ce membre à la liste des membres
        public void checkLoginMessage(string response)
        {
            // identifier ce qui a login en utilisant les expressions régulières
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
        // si un message reçu est de type logout, supprimer ce membre de la liste des membres
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
        // mise à jour la listView de membres en ligne
        public void convertToListView(LinkedList<string> listMembers)
        {
            lvMembers.Items.Clear();
            foreach (string name in listMembers)
                lvMembers.Items.Add(new MemberViewItem(name));
        }
        // fonction qui va être appelé lorsque l'utilisateur clique le bouton login
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            // utiliser l'expression régulière pour vérifier la validité du numéro de port
            Regex rgxPort = new Regex(@"^[\d]+$");
            if (!rgxPort.IsMatch(PortBox.Text))
            {
                MessageBox.Show("Port number not validate, it should be a digit or digits", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int port = Int32.Parse(PortBox.Text);
            pseudo = PseudoBox.Text;
            // utiliser une expression régulière pour vérifier la validité du pseudonyme
            Regex rgxPseudo = new Regex(@"^[a-zA-Z][\w]*$");
            if (!rgxPseudo.IsMatch(pseudo))
            {
                MessageBox.Show("Pseudo name not validate, it should longer thant one and begin with a alphabet", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string ip = IPBox.Text;
            // Adresse par défaut "tcp://localhost:12345/Serveur"
            LeRemot = (RemotingInterface.IRemotChaine)Activator.GetObject(
                typeof(RemotingInterface.IRemotChaine), $"tcp://{ip}:{port}/Serveur");

            // attraper une erreur de connexion
            try
            {
                // récuperer le horlge logique dépuis le serveur, s'il y une erreur de connexion,
                // une erreur de type System.Net.Sockets.SocketException va se poser 
                logicTime = LeRemot.clientLogin(pseudo);
            }
            // lorsque l'erreur de connexion se produit, demandez à l'utilisateur de se reconnecter
            catch (System.Net.Sockets.SocketException err) 
            {
                MessageBox.Show("Error connecting the remote server, please choose another combination of port and address", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine("{0} Exception caught.", err);
                return;
            }
            // gérer l'erreur de duplication du nom d'utilisateur
            if (logicTime == -1)
            {
                // Debug.WriteLine("logicTime {0}", logicTime);
                MessageBox.Show("The current pseudo is not available, please choose another Pesedo", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                PortBox.IsEnabled = false;
                PseudoBox.IsEnabled = false;
                IPBox.IsEnabled = false;
                // démarrer le thread pour obtenir des mises à jour du serveur
                Client_Logined();
            }
        }
        // la classe qui définit l'élément dans la listView de membres en ligne
        public class MemberViewItem
        {
            public MemberViewItem(string MemberName)
            {
                this.MemberName = MemberName;
            }
            public string MemberName { get; set; }
        }
        // fonction qui va être appelé lorsque l'utilisateur clique le bouton send
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
        // fonction qui va être appelé lorsque l'utilisateur clique le bouton logout
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // arrêter le thread
            th.Abort();
            // envoyer un message de type logout au serveur
            LeRemot.clientLogout(pseudo);
            // annuler la connections entre le client et le serveur
            LeRemot = null;
            // re-initialiser la liste listMembers
            listMembers = new LinkedList<string>();
            // mise à jour la listView également
            convertToListView(listMembers);
            // initialize UI component status
            Send.IsEnabled = false;
            Logout.IsEnabled = false;
            Login.IsEnabled = true;
            PortBox.IsEnabled = true;
            PseudoBox.IsEnabled = true;
            IPBox.IsEnabled = true;
            ChatHistory.Text = "";
        }
        // fonction qui va être appelé lorsque l'utilisateur clique le croix rouge
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
