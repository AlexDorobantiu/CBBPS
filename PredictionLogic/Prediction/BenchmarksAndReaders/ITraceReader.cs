namespace PredictionLogic.Prediction.BenchmarksAndReaders
{
    public interface ITraceReader
    {
        bool openTrace(string folderPath, string filename);
        void closeTrace();
        IBranch getNextBranch();
        int getNumberOfBranches();
    }
}
