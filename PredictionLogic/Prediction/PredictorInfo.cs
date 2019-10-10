using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace PredictionLogic.Prediction
{
    [Serializable]
    public class PredictorInfo : ISerializable
    {
        public readonly string predictorTypeFullName;
        public readonly object[] arguments;
        public readonly string description;
        public readonly ApplicationOptions applicationOptions;

        private List<IPredictor> predictorInstances = new List<IPredictor>();
        private List<bool> predictorLocked = new List<bool>();

        public PredictorInfo(Type predictorType, string description, ApplicationOptions options, params object[] constructorArgs)
        {
            predictorTypeFullName = predictorType.FullName;
            this.description = description;
            arguments = constructorArgs;
            applicationOptions = options;
        }

        public IPredictor createPredictorInstance()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                object instance = assembly.CreateInstance(predictorTypeFullName, false, BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.Public, null, arguments, null, null);
                if (!(instance is IPredictor))
                {
                    throw new Exception("Invalid PropertyInfo attributes. Instantiation is not possible.");
                }
                return instance as IPredictor;
            }
            catch (OutOfMemoryException outOfMemoryException)
            {
                ErrorNotifier.showError("The Predictor " + predictorTypeFullName + " requires too much memory to be instatiated.\n" + outOfMemoryException.Message);
                return null;
            }
            catch (Exception e)
            {
                ErrorNotifier.showError(e.Message);
                return null;
            }
        }

        public IPredictor getPredictor()
        {
            IPredictor predictor = null;

            // Prepare the predictor
            // Create new instance for each thread for concurrent simulation
            // In the same time reuse the predictors after threads released them
            lock (this)
            {
                bool foundFree = false;
                int i = 0;

                for (; i < predictorLocked.Count; i++)
                {
                    if (!predictorLocked[i])
                    {
                        foundFree = true; break;
                    }
                }

                if (foundFree)
                {
                    if (predictorLocked[i] == true)
                    {
                        throw new Exception("Lock mechanism fail!");
                    }
                    predictorLocked[i] = true;
                    predictor = predictorInstances[i];
                }
                else
                {
                    predictor = createPredictorInstance();
                    if (predictor == null)
                    {
                        return null;
                    }

                    predictorInstances.Add(predictor);
                    predictorLocked.Add(true);
                }
            }

            predictor.reset();
            return predictor;
        }

        public void freePredictor(IPredictor predictor)
        {
            // free the predictor
            lock (this)
            {
                predictorLocked[predictorInstances.IndexOf(predictor)] = false;
            }
        }

        public override int GetHashCode()
        {
            int h = predictorTypeFullName.GetHashCode() ^ arguments.Length;
            return h;
        }

        public override bool Equals(object obj)
        {
            PredictorInfo other = obj as PredictorInfo;
            if (other == null)
            {
                return false;
            }
            if (other.predictorTypeFullName != predictorTypeFullName)
            {
                return false;
            }
            if (other.arguments.Length != arguments.Length)
            {
                return false;
            }
            for (int i = 0; i < arguments.Length; i++)
            {
                if (!other.arguments[i].Equals(arguments[i]))
                {
                    return false;
                }
            }
            return true;
        }

        #region Serialization

        protected PredictorInfo(SerializationInfo info, StreamingContext context)
        {
            predictorTypeFullName = info.GetString("PredictorTypeFullName");
            description = info.GetString("Description");
            arguments = (object[])info.GetValue("Arguments", typeof(object[]));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("PredictorTypeFullName", predictorTypeFullName);
            info.AddValue("Description", description);
            info.AddValue("Arguments", arguments);
        }

        #endregion
    }
}
