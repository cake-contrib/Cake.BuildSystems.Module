using System;
using Cake.Core;
using Cake.Core.Diagnostics;
using JetBrains.Annotations;
using CakeBuildLog = Cake.Core.Diagnostics.CakeBuildLog;

namespace Cake.TeamCity.Module
{
    /// <summary>
    /// <see cref="ICakeEngine"/> implementation for TeamCity.
    /// </summary>
    [UsedImplicitly]
    public class TeamCityLog : ICakeLog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamCityLog"/> class.
        /// </summary>
        /// <param name="console">Implementation of <see cref="IConsole"/>.</param>
        /// <param name="verbosity">Default <see cref="Verbosity"/>.</param>
        public TeamCityLog(IConsole console, Verbosity verbosity = Verbosity.Normal)
        {
            _cakeLogImplementation = new CakeBuildLog(console, verbosity);
        }

        private readonly ICakeLog _cakeLogImplementation;

        /// <inheritdoc />
        public void Write(Verbosity verbosity, LogLevel level, string format, params object[] args)
        {
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEAMCITY_VERSION")))
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

        /// <inheritdoc />
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
