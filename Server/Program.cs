using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace remotServeur
{
    /// <summary>
    /// Description résumée de demarreServeur.
    /// </summary>
    public class Serveur : MarshalByRefObject, RemotingInterface.IRemotChaine
    {
        private LinkedList<string> listMembers = new LinkedList<string>();
        private List<string> historyMessage = new List<string>();
        static void Main()
        {
            // Création d'un nouveau canal pour le transfert des données via un port 
            TcpChannel canal = new TcpChannel(12345);

            // Le canal ainsi défini doit être Enregistré dans l'annuaire
            ChannelServices.RegisterChannel(canal);

            // Démarrage du serveur en écoute sur objet en mode Singleton
            // Publication du type avec l'URI et son mode 
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(Serveur), "Serveur", WellKnownObjectMode.Singleton);

            Console.WriteLine("Le serveur est bien démarré");
            // pour garder la main sur la console
            Console.ReadLine();

        }

        // Pour laisser le serveur fonctionner sans time out
        public override object InitializeLifetimeService()
        {
            return null;
        }


        #region Membres de IRemotChaine

        public void sendMsgToServer(string msg)
        {
            Console.WriteLine($"{msg}");
            historyMessage.Add(msg);
        }

        public int clientLogin(string pseudo)
        {
            if (listMembers.Contains(pseudo))
                return -1;
            listMembers.AddLast(pseudo);
            sendMsgToServer($"{pseudo} has joined the chat");
            return historyMessage.Count;
        }

        public void clientLogout(string pseudo)
        {
            listMembers.Remove(pseudo);
            sendMsgToServer($"{pseudo} has left the chat");
        }

        public string getUpdateFromServer(int logicTime)
        {
            if (logicTime > 0 && logicTime < historyMessage.Count)
            {
                return historyMessage[logicTime];
            }
            return "";
        }

        public LinkedList<string> getClientListFromServer()
        {
            return listMembers;
        }

        #endregion
    }
}
