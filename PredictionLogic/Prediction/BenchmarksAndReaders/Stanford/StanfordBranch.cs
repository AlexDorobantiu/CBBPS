namespace PredictionLogic.Prediction.BenchmarksAndReaders.Stanford
{
    public class StanfordBranch : IBranch
    {
        public BranchInfo branchInfo;
        public bool branchTaken;
        public uint targetAddress;

        public StanfordBranch()
        {
        }

        #region IBranch Members

        public BranchInfo getBranchInfo()
        {
            return branchInfo;
        }

        public bool taken()
        {
            return branchTaken;
        }

        public uint getTargetAddress()
        {
            return targetAddress;
        }

        #endregion
    }
}
