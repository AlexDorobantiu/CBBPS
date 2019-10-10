﻿using System;

namespace PredictionLogic.Prediction.BenchmarksAndReaders
{
    [Serializable]
    public class BenchmarkInfo
    {
        public readonly string benchmarkName;
        public readonly BenchmarkType benchmarkType;

        public BenchmarkInfo(string benchmarkName, BenchmarkType benchmarkType)
        {
            this.benchmarkName = benchmarkName;
            this.benchmarkType = benchmarkType;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            BenchmarkInfo other = obj as BenchmarkInfo;
            return (benchmarkType == other.benchmarkType) && (benchmarkName.Equals(other.benchmarkName));
        }

        public override int GetHashCode()
        {
            return benchmarkName.GetHashCode();
        }
    }
}
