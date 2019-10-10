using System;
using System.IO;

namespace PredictionLogic.Prediction.BenchmarksAndReaders.Stanford
{
    class StanfordReader : ITraceReader, IDisposable
    {
        StreamReader streamReader;
        int numberOfBranches;

        private StanfordBranch branch;
        
        #region ITraceReader Members

        public bool openTrace(string folderPath, string filename)
        {
            branch = new StanfordBranch();
            try
            {
                streamReader = new StreamReader(folderPath + filename);
            }
            catch
            {
                if (streamReader != null)
                {
                    streamReader.Close();
                }
                return false;
            }
            numberOfBranches = 0;
            return true;
        }

        public IBranch getNextBranch()
        {
            try
            {
                String line = streamReader.ReadLine();
                if (line == null)
                {
                    return null;
                }
                string[] splitLine = line.Split(new char[] { ' ' });
                if (splitLine.Length < 3)
                {
                    return null;
                }
                numberOfBranches++;
                branch.branchInfo.address = uint.Parse(splitLine[1]);

                branch.branchInfo.branchFlags = 0;
                if ((splitLine[0][1] == 'T') || (splitLine[0][1] == 'F'))
                {
                    branch.branchInfo.branchFlags |= BranchInfo.BR_CONDITIONAL;
                }
                if ((splitLine[0][1] == 'S'))
                {
                    branch.branchInfo.branchFlags |= BranchInfo.BR_CALL;
                }
                if ((splitLine[0][1] == 'M'))
                {
                    branch.branchInfo.branchFlags |= BranchInfo.BR_RETURN;
                }
                branch.branchTaken = (splitLine[0][0] == 'B');
                branch.targetAddress = uint.Parse(splitLine[2]);
                return branch;
            }
            catch
            {
                return null;
            }
        }

        public void closeTrace()
        {
            if (streamReader != null)
            {
                streamReader.Close();
            }
        }

        public int getNumberOfBranches()
        {
            return numberOfBranches;
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    closeTrace();
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
