using System;
using Cake.Core;
using Cake.Core.Configuration;
using Cake.Core.Diagnostics;
using Cake.Module.Shared;
using CakeBuildLog = Cake.Core.Diagnostics.CakeBuildLog;

namespace Cake.MyGet.Module
{
    public class MyGetBuildLog : ICakeLog
    {
        private static bool _failOnFatal;
        public MyGetBuildLog(IConsole console, ICakeConfiguration configuration, Verbosity verbosity = Verbosity.Normal)
        {
            _failOnFatal = configuration.GetConfigFlag("BuildSystems_FailOnFatal");
            _cakeLogImplementation = new CakeBuildLog(console, verbosity);
        }
        private readonly ICakeLog _cakeLogImplementation;

        public void Write(Verbosity verbosity, LogLevel level, string format, params object[] args)
        {
            switch (level)
            {
                case LogLevel.Fatal:
                    _cakeLogImplementation.Write(Verbosity.Quiet, LogLevel.Information,
                        _failOnFatal
                            ? "##myget[buildProblem description='{0}']"
                            : "##myget[message text='{0}' status='FAILURE']",
                        string.Format(format, args));
                    break;
                case LogLevel.Error:
                    _cakeLogImplementation.Write(Verbosity.Quiet, LogLevel.Information,
                        "##myget[message text='{0}' status='ERROR']", string.Format(format, args));
                    break;
                case LogLevel.Warning:
                    _cakeLogImplementation.Write(Verbosity.Quiet, LogLevel.Information,
                        "##myget[message text='{0}' status='WARNING']", string.Format(format, args));
                    break;
                case LogLevel.Information:
                    _cakeLogImplementation.Write(verbosity, level, format, args);
                    break;
                case LogLevel.Verbose:
                case LogLevel.Debug:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        public Verbosity Verbosity
        {
            get { return _cakeLogImplementation.Verbosity; }
            set { _cakeLogImplementation.Verbosity = value; }
        }
    }
}