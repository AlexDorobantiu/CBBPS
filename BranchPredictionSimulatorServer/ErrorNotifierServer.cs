using System;
using PredictionLogic;

namespace BranchPredictionSimulatorServer
{
    public class ErrorNotifierActionServer : ErrorNotifierAction
    {
        public override void showError(string message)
        {
            Console.WriteLine("Error: " + message);
        }
    }
}
