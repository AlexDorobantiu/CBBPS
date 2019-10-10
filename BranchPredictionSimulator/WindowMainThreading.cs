using System;
using System.Threading;
using PredictionLogic.SimulationStatistics;
using PredictionLogic.Simulation;
using BranchPredictionSimulator.SimulationResultStructures;

namespace BranchPredictionSimulator
{
    public partial class WindowMain
    {
        private Thread[] threads;
        private int localThreadNumber = 1;
        private int threadSlots = Environment.ProcessorCount;

        private void startWork()
        {
            SimulationOptionsCurrentExecution = SimulationOptionsForBinding.GetCopy();
            session = new SimulationSession(SimulationOptionsCurrentExecution);

            MainChart.StartDrawData(simulationResultsDictionary);

            threads = null;
            if (!SimulationOptionsCurrentExecution.RemoteExecutionOnly)
            {
                threads = new Thread[threadSlots];
                for (int i = 0; i < threads.Length; i++)
                {
                    threads[i] = new Thread(performWork);
                    threads[i].Name = "Local Thread " + localThreadNumber++;
                    threads[i].Start();
                }
            }

            foreach (TCPSimulatorProxy proxy in TCPConnections)
            {
                if (proxy.Connected)
                {
                    proxy.startNewSession(session);
                }
            }

            btnSimulate.IsEnabled = false;
            btnAbort.IsEnabled = true;
        }

        public void proxyTaskRequestReceived(object sender, EventArgs e)
        {
            TCPSimulatorProxy tcpSimulatorProxy = (TCPSimulatorProxy)sender;
            SimulationInfo currentSimulation;
            lock (simulationQueue)
            {
                if (simulationQueue.Count > 0)
                {
                    currentSimulation = simulationQueue.Dequeue();
                }
                else
                {
                    return;
                }
            }
            if (currentSimulation != null)
            {
                tcpSimulatorProxy.sendSimulationTask(currentSimulation);
            }
        }

        public void proxyResultsReceived(object sender, BenchmarkStatisticsResultReceivedEventArgs e)
        {
            TCPSimulatorProxy proxy = (TCPSimulatorProxy)sender;

            lock (simulationResultsDictionary)
            {
                simulationResultsDictionary.setResultUsingDispatcher(Dispatcher, e.simulation.predictorInfo, e.simulation.benchmarkInfo, e.result);
            }
        }

        private void performWork()
        {
            try
            {
                SimulationInfo currentSimulation;
                while (true)
                {
                    // acquire data
                    lock (simulationQueue)
                    {
                        if (simulationQueue.Count > 0)
                        {
                            currentSimulation = simulationQueue.Dequeue();
                        }
                        else
                        {
                            return;
                        }
                    }

                    // big work
                    BenchmarkStatisticsResult result = currentSimulation.simulate(SimulationOptionsCurrentExecution, AppOptions);

                    // results
                    lock (simulationResultsDictionary)
                    {
                        simulationResultsDictionary.setResultUsingDispatcher(Dispatcher, currentSimulation.predictorInfo, currentSimulation.benchmarkInfo, result);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
                return;
            }
        }

        private void abortAllWork()
        {
            if (threads != null)
            {
                foreach (Thread t in threads)
                {
                    t.Abort();
                    t.Join();
                }
            }

            foreach (TCPSimulatorProxy proxy in TCPConnections)
            {
                if (proxy.Connected)
                {
                    proxy.sendAbortSessionRequest();
                }
            }

            displayedResults.Add(new ResultListMessage("Simulation aborted by user."));
            btnAbort.IsEnabled = false;
            btnSimulate.IsEnabled = true;
        }

        private void simulationResultsDictionaryFilled(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(workFinished));
        }

        private void workFinished()
        {
            displayedResults.Add(new ResultListMessage("Simulation completed."));
            btnAbort.IsEnabled = false;
            btnSimulate.IsEnabled = true;
        }

        private void beginInvokePrintResults(object toPrint)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(
                delegate()
                {
                    displayedResults.Add(toPrint);
                }
            ));
        }

        private void beginInvokeBenchmarkStatisticsCollectionAdd(BenchmarkStatisticsCollection benchmarkStatisticsCollection, BenchmarkStatisticsResult benchmarkStatisticsResult)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(
                delegate()
                {
                    benchmarkStatisticsCollection.addSorted(benchmarkStatisticsResult);
                }
            ));
        }
    }
}
