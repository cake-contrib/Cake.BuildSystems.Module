using System;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Module.Shared;
using CakeBuildLog = Cake.Core.Diagnostics.CakeBuildLog;

namespace Cake.TeamCity.Module
{
    public class TeamCityLog : ICakeLog
    {
        public TeamCityLog(IConsole console, Verbosity verbosity = Verbosity.Normal)
        {
            _cakeLogImplementation = new CakeBuildLog(console, verbosity);
        }
        private readonly ICakeLog _cakeLogImplementation;

        public void Write(Verbosity verbosity, LogLevel level, string format, params object[] args)
        {
            if (!string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable("TF_BUILD")))
            {
                switch (level)
                {
                    case LogLevel.Fatal:
                    case LogLevel.Error:
                        _cakeLogImplementation.Write(Verbosity.Quiet, LogLevel.Information,
                            "##teamcity[buildProblem description='{0}']", Escape(string.Format(format, args)));
                        break;
                    case LogLevel.Warning:
                        _cakeLogImplementation.Write(Verbosity.Quiet, LogLevel.Information,
                            "##teamcity[message text='{0}' status='WARNING']", Escape(string.Format(format, args)));
                        break;
                    case LogLevel.Information:
                    case LogLevel.Verbose:
                    case LogLevel.Debug:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(level), level, null);
                }
            }
            _cakeLogImplementation.Write(verbosity, level, format, args);
        }

        public Verbosity Verbosity
        {
            get { return _cakeLogImplementation.Verbosity; }
            set { _cakeLogImplementation.Verbosity = value; }
        }

        private string Escape(string text)
        {
            return text.Replace("|", "||")
                .Replace("'", "|'")
                .Replace("\n", "|n")
                .Replace("\r", "|r")
                .Replace("[", "|[")
                .Replace("]", "|]");
        }
    }
}