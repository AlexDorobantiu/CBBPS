using System;
using System.Collections.Generic;
using System.Threading;
using PredictionLogic.CommunicationProtocol;
using PredictionLogic.SimulationStatistics;

namespace BranchPredictionSimulatorServer
{
    public class SimulationWorkerThread : IDisposable
    {
        public readonly Queue<TaskPackage> taskSource;
        public readonly ConnectionThread ParentConnectionThread;
        private EventWaitHandle waitHandleBetweenTasks = new AutoResetEvent(false);
        private Thread thread;
        private bool isPendingClose = false;

        public SimulationWorkerThread(ConnectionThread parent, string threadName)
        {
            ParentConnectionThread = parent;
            taskSource = parent.TaskBuffer;
            waitHandleBetweenTasks = new AutoResetEvent(false);
            thread = new Thread(doWork);
            thread.Name = threadName;
            thread.Start();
        }

        public void awake()
        {
            waitHandleBetweenTasks.Set();
        }

        public void abortCurrentTask()
        {
            thread.Abort();
        }

        public void kill()
        {
            isPendingClose = true;
            waitHandleBetweenTasks.Set();
            thread.Join();
            waitHandleBetweenTasks.Close();
        }

        private void doWork()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        if (isPendingClose)
                        {
                            return;
                        }
                        TaskPackage taskPackage = null;
                        lock (taskSource)
                        {
                            if (taskSource.Count > 0)
                            {
                                taskPackage = taskSource.Dequeue();
                            }
                        }

                        if (taskPackage == null)
                        {
                            waitHandleBetweenTasks.WaitOne();
                        }
                        else
                        {
                            Console.WriteLine("Simulating " + taskPackage.simulationTask.predictorInfo.predictorTypeFullName + " on " + taskPackage.simulationTask.benchmarkInfo.benchmarkName);
                            BenchmarkStatisticsResult benchmarkStatisticsResult = taskPackage.simulationTask.simulate(ParentConnectionThread.SimulationSession.sessionOptions, Program.applicationOptions);
                            ParentConnectionThread.sendResult(taskPackage.taskID, benchmarkStatisticsResult);
                            ParentConnectionThread.sendTaskRequest();
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        Thread.ResetAbort();
                    }
                }
            }
            catch (ThreadAbortException)
            {
                Console.WriteLine("The thread abortion process occured at a time when it could not be properly handled.");
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    kill();
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

}
