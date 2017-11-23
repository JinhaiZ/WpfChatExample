using System;

namespace RemotingInterface
{
    /// <summary>
    /// cette interface contiendra la déclaration de toutes les 
    /// méthodes de l'objet distribué
    /// </summary>
    public interface IRemotChaine
    {
        string Hello();
        void sendMsgToServer(string msg);
        int clientLogin(string pseudo);
        void clientLogout();
        string getUpdateFromServer(int logicTime);
    }
}
