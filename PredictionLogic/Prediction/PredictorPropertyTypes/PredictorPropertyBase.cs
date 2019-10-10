﻿using System.Collections.Generic;
using System.ComponentModel;

namespace PredictionLogic.Prediction.PredictorPropertyTypes
{
    public abstract class PredictorPropertyBase : INotifyPropertyChanged
    {
        public string PropertyName { get; set; }
        public string DisplayName { get; set; }
        public abstract List<object> loadValuesFromUI();

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, args);
            }
        }

        #endregion
    }
}
