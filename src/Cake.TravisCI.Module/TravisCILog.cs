using System.Text.RegularExpressions;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Module.Shared;

namespace Cake.TravisCI.Module
{


    public sealed class TravisCILog : ServiceMessageLog
    {
        public TravisCILog(IConsole console, Verbosity verbosity = Verbosity.Normal) : base(console, s => s.StartsWith("travis_fold:"), verbosity)
        {
        }
    }

}
