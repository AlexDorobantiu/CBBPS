using System;
using System.Collections.Generic;
using PredictionLogic.Prediction.PredictorPropertyTypes;

namespace PredictionLogic.Prediction.Predictors
{
    public class SIP3 : IPredictor
    {
        private const int minimumThreshold = 16;
        private const int maximumThreshold = 512;

        private const int maximumInertiaMinimum = 2;
        private const int maximumInertiaMaximum = 20;
        private const int maximumInertiaDefault = 4;

        private const int counterBitsMinimum = 2;
        private const int counterBitsMaximum = 12;
        private const int counterBitsDefault = 6;

        public static readonly uint numberOfBranchLocationsMinimum = 1;
        public static readonly uint numberOfBranchLocationsMaximum = 1048576;
        public static readonly uint numberOfBranchLocationsDefault = 16536;

        private const int initialThresholdMinimum = 0;
        private const int initialThresholdMaximum = 1024;
        private const int initialThresholdDefault = 16;

        private const int thresholdAdaptBitsMinimum = 0;
        private const int thresholdAdaptBitsMaximum = 16;
        private const int thresholdAdaptBitsDefault = 1;

        public static readonly string ToolTipInfo = "Simple Inertial Predictor 3.0";
        public static readonly List<PredictorPropertyBase> Properties = new List<PredictorPropertyBase>();

        static SIP3()
        {
            Properties.Add(new PredictorUInt32Property("branchLocations", "Number of branch locations", numberOfBranchLocationsDefault, numberOfBranchLocationsMinimum, numberOfBranchLocationsMaximum));
            Properties.Add(new PredictorInt32Property("counterBits", "Counter bits", counterBitsDefault, counterBitsMinimum, counterBitsMaximum));
            Properties.Add(new PredictorInt32Property("maximumInertia", "Maximum inertia", maximumInertiaDefault, maximumInertiaMinimum, maximumInertiaMaximum));
            Properties.Add(new PredictorInt32Property("initialThreshold", "Initial threshold", initialThresholdDefault, initialThresholdMinimum, initialThresholdMaximum));
            Properties.Add(new PredictorInt32Property("thresholdAdaptBits", "Threshold adapt bits", thresholdAdaptBitsDefault, thresholdAdaptBitsMinimum, thresholdAdaptBitsMaximum));
        }

        uint branchLocations;
        bool adaptThreshold;
        int thresholdAdaptCount;
        int thresholdAdaptCountMax;

        int counterBits;
        int counterMax;

        int[,] states;
        int[,] weights;

        int initialThreshold;
        int updateThreshold;

        int maximumInertia;
        int sum;
        uint branchIndex;
        bool prediction;

        public SIP3(uint branchLocations, int counterBits, int maximumInertia, int initialThreshold, int thresholdAdaptBits)
        {
            this.branchLocations = branchLocations;
            this.maximumInertia = maximumInertia;
            this.initialThreshold = initialThreshold;
            updateThreshold = initialThreshold;

            thresholdAdaptCount = 0;
            if (thresholdAdaptBits != 0)
            {
                thresholdAdaptCountMax = (1 << (thresholdAdaptBits - 1)) - 1;
                adaptThreshold = true;
            }
            else
            {
                thresholdAdaptCountMax = 0;
                adaptThreshold = false;
            }

            states = new int[branchLocations, maximumInertia];
            weights = new int[branchLocations, maximumInertia];

            this.counterBits = counterBits;
            counterMax = (1 << (counterBits - 1)) - 1;
            prediction = false;
        }

        #region IPredictor Members

        public bool predictBranch(BranchInfo branch)
        {
            branchIndex = branch.address % branchLocations;

            sum = 0;
            for (int inertia = 0; inertia < maximumInertia; inertia++)
            {
                if (states[branchIndex, inertia] < 0)
                {
                    sum -= weights[branchIndex, inertia];
                }
                else
                {
                    sum += weights[branchIndex, inertia];
                }
            }

            prediction = (sum >= 0);
            return prediction;
        }

        public void update(IBranch branch)
        {
            if ((Math.Abs(sum) < updateThreshold) || (prediction != branch.taken()))
            {
                for (int inertia = 0; inertia < maximumInertia; inertia++)
                {
                    if (branch.taken())
                    {
                        if (states[branchIndex, inertia] >= 0)
                        {
                            if (weights[branchIndex, inertia] < counterMax)
                            {
                                weights[branchIndex, inertia]++;
                            }
                        }
                        else
                        {
                            if (weights[branchIndex, inertia] > -counterMax - 1)
                            {
                                weights[branchIndex, inertia]--;
                            }
                        }

                        if (states[branchIndex, inertia] < inertia - 1)
                        {
                            states[branchIndex, inertia]++;
                        }
                    }
                    else
                    {
                        if (states[branchIndex, inertia] < 0)
                        {
                            if (weights[branchIndex, inertia] < counterMax)
                            {
                                weights[branchIndex, inertia]++;
                            }
                        }
                        else
                        {
                            if (weights[branchIndex, inertia] > -counterMax - 1)
                            {
                                weights[branchIndex, inertia]--;
                            }
                        }

                        if (states[branchIndex, inertia] > -inertia)
                        {
                            states[branchIndex, inertia]--;
                        }
                    }
                }
            }
            if (adaptThreshold)
            {
                if (branch.taken() != prediction)
                {
                    if (thresholdAdaptCount < thresholdAdaptCountMax)
                    {
                        thresholdAdaptCount++;
                    }
                    else
                    {
                        thresholdAdaptCount = 0;
                        if (updateThreshold < maximumThreshold)
                        {
                            updateThreshold++;
                        }
                    }
                }
                else
                {
                    if (Math.Abs(sum) < updateThreshold)
                    {
                        thresholdAdaptCount--;
                        if (thresholdAdaptCount > -thresholdAdaptCountMax - 1)
                        {
                            thresholdAdaptCount--;
                        }
                        else
                        {
                            thresholdAdaptCount = 0;
                            if (updateThreshold > minimumThreshold)
                            {
                                updateThreshold--;
                            }
                        }
                    }
                }
            }
        }

        public void reset()
        {
            thresholdAdaptCount = 0;
            updateThreshold = initialThreshold;

            for (int i = 0; i < branchLocations; i++)
            {
                for (int j = 0; j < maximumInertia; j++)
                {
                    states[i, j] = 0;
                    weights[i, j] = 0;
                }
            }
        }

        #endregion
    }
}
