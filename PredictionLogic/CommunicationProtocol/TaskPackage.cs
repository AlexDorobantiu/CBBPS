using System;
using PredictionLogic.Simulation;

namespace PredictionLogic.CommunicationProtocol
{
    [Serializable]
    public class TaskPackage
    {
        public uint taskID;
        public uint sessionID;
        public SimulationInfo simulationTask;
    }
}
