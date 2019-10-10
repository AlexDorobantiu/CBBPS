using System;
using System.Windows;

namespace BranchPredictionSimulator
{
    /// <summary>
    /// Interaction logic for ConnectionsAddWindow.xaml
    /// </summary>
    public partial class ConnectionsAddWindow : Window
    {
        public ConnectionsWindow caller = null;

        public ConnectionsAddWindow()
        {
            InitializeComponent();
        }

        private void btn_Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TCPSimulatorProxy tcpSimulatorProxy = new TCPSimulatorProxy(tb_host.Text, int.Parse(tb_port.Text));
                tcpSimulatorProxy.messagePosted += new EventHandler<StringEventArgs>(caller.TCPProxy_MessagePosted);
                tcpSimulatorProxy.taskRequestReceived += new EventHandler(caller.ParentWindow.proxyTaskRequestReceived);
                tcpSimulatorProxy.resultsReceived += new statisticsResultReceivedEventHandler(caller.ParentWindow.proxyResultsReceived);
                caller.ConnectionProxys.Add(tcpSimulatorProxy);
            }
            catch (Exception)
            {
            }
            finally
            {
                Hide();
            }
        }
    }
}
