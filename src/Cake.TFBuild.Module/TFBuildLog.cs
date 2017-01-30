using System;
using System.Reflection;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Diagnostics;

namespace Cake.TFBuild.Module
{
    public class TFBuildLog : ICakeLog
    {
        public TFBuildLog(IConsole console, Verbosity verbosity = Verbosity.Normal)
        {
            _cakeLogImplementation = new CakeBuildLog(console, verbosity);
        }
        private readonly ICakeLog _cakeLogImplementation;
        public void Write(Verbosity verbosity, LogLevel level, string format, params object[] args)
        {
            if (level == LogLevel.Warning)
            {
                _cakeLogImplementation.Write(Verbosity.Quiet, LogLevel.Information, "##vso[task.logissue type=warning;]{0}", string.Format(format, args));
            }
            _cakeLogImplementation.Write(verbosity, level, format, args);
        }

        public Verbosity Verbosity
        {
            get { return _cakeLogImplementation.Verbosity; }
            set { _cakeLogImplementation.Verbosity = value; }
        }
    }
}