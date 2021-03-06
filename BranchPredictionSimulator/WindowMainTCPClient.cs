﻿using System;
using System.Windows;

using System.Collections.ObjectModel;
using System.Threading;

namespace BranchPredictionSimulator
{
    public partial class WindowMain : Window
    {
        public ObservableCollection<TCPSimulatorProxy> TCPConnections = new ObservableCollection<TCPSimulatorProxy>();
        public ObservableCollection<string> ConnectionMessages = new ObservableCollection<string>();
        public ConnectionsWindow ConnectionsWindow = null;
                
        private void MenuItemTCPConnections_Click(object sender, RoutedEventArgs e)
        {
            //if (ConnectionsWindow != null) ConnectionsWindow.Close();
            ConnectionsWindow = new ConnectionsWindow();
            ConnectionsWindow.InitWindow(this);
            ConnectionsWindow.Show();
        }       

        public void TCPProxy_MessagePosted(object sender, StringEventArgs e)
        {
            if (Dispatcher.Thread.Equals(Thread.CurrentThread))
                ConnectionMessages.Add(e.message);
            else
            {
                Dispatcher.BeginInvoke(
                  System.Windows.Threading.DispatcherPriority.Normal,
                  new Action(
                    delegate()
                    {
                        ConnectionMessages.Add(e.message);
                    }
                ));
            }
        }

        
    }

    
}