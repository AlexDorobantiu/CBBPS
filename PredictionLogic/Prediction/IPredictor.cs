using System;
using System.Collections.Generic;
using System.Text;
using PredictionLogic.Prediction.BenchmarksAndReaders;

namespace PredictionLogic.Prediction
{
    /// <summary>
    /// Main interface to be implemented by predictors
    /// Important! All inheriting classes must have the filed "Properties" like this:
    ///     public static readonly List<PredictorPropertyBase> Properties = new List<PredictorPropertyBase>();
    /// This contains the parameters of the predictor and is used in presenting the options on the GUI.
    /// It is also used for creating instances. Properties must be in the same order as they apear in the constructor.
    /// This choice was considered for future implementations because the properties are defined inside the predictor.
    /// 
    /// </summary>
    public interface IPredictor
    {
        bool predictBranch(BranchInfo branch);
        void update(IBranch branch);
        void reset();
    }
}
