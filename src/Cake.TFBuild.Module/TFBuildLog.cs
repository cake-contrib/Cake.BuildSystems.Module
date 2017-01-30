using System;
using System.Reflection;
using Cake.Common.Build;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Diagnostics;

namespace Cake.TFBuild.Module
{
    public class TFBuildLog : ICakeLog
    {
        public TFBuildLog(IConsole console, ICakeContext context, Verbosity verbosity = Verbosity.Normal)
        {
            _cakeLogImplementation = new CakeBuildLog(console, verbosity);
            _context = context;
        }
        private readonly ICakeLog _cakeLogImplementation;
        private readonly ICakeContext _context;

        public void Write(Verbosity verbosity, LogLevel level, string format, params object[] args)
        {
            var b = _context.BuildSystem();
            if (b.IsRunningOnVSTS || b.IsRunningOnTFS)
            {
                switch (level)
                {
                    case LogLevel.Fatal:
                    case LogLevel.Error:
                        _cakeLogImplementation.Write(Verbosity.Quiet, LogLevel.Information,
                            "##vso[task.logissue type=error;]{0}", string.Format(format, args));
                        break;
                    case LogLevel.Warning:
                        _cakeLogImplementation.Write(Verbosity.Quiet, LogLevel.Information,
                            "##vso[task.logissue type=warning;]{0}", string.Format(format, args));
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
    }
}