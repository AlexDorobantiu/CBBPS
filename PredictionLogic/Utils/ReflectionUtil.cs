using System.IO;
using System.Reflection;

namespace PredictionLogic.Utils
{
    public static class ReflectionUtil
    {
        public static string getCurrentExeDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }
    }
}
