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

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private RemotingInterface.IRemotChaine LeRemot;
        public MainWindow()
        {
            InitializeComponent();
            LeRemot = (RemotingInterface.IRemotChaine)Activator.GetObject(
                typeof(RemotingInterface.IRemotChaine), "tcp://localhost:12345/Serveur");

        }

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OutputBox.Text = LeRemot.Hello();
        }
    }
}
