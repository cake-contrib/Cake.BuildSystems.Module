using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Module.Shared;
using JetBrains.Annotations;

namespace Cake.TravisCI.Module
{
    /// <summary>
    /// <see cref="ICakeEngine"/> implementation for Travis CI.
    /// </summary>
    [UsedImplicitly]
    public sealed class TravisCILog : ServiceMessageLog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TravisCILog"/> class.
        /// </summary>
        /// <param name="console">Implementation of <see cref="IConsole"/>.</param>
        /// <param name="verbosity">Default <see cref="Verbosity"/>.</param>
        public TravisCILog(IConsole console, Verbosity verbosity = Verbosity.Normal)
            : base(console, s => s.StartsWith("travis_fold:"), verbosity)
        {
        }
    }
}
