using System.ComponentModel;

namespace BranchPredictionSimulator.SimulationResultStructures
{
    public class ResultListMessage : INotifyPropertyChanged
    {
        private string message;
        public string Message
        {
            get
            {
                return message;
            }
        }

        private bool isVisible;
        public bool IsVisible
        {
            get
            {
                return isVisible;
            }
            set
            {
                isVisible = value;
                NotifyPropertyChanged(IsVisiblePropertyChangedEventArgs);
            }
        }

        public ResultListMessage(string message)
        {
            this.message = message;
            isVisible = true;
        }

        public ResultListMessage(string message, bool isVisible)
        {
            this.message = message;
            this.isVisible = isVisible;
        }

        //  INotifyPropertyChanged stuff

        public event PropertyChangedEventHandler PropertyChanged;
        public static PropertyChangedEventArgs IsVisiblePropertyChangedEventArgs = new PropertyChangedEventArgs("IsVisible");

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void NotifyPropertyChanged(PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, propertyChangedEventArgs);
            }
        }
    }
}
