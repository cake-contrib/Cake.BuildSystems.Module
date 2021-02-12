using System;

using Cake.Common.Build.AzurePipelines;
using Cake.Common.Build.AzurePipelines.Data;

namespace Cake.AzurePipelines.Module
{
    /// <summary>
    /// Extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Update the progress.
        /// </summary>
        /// <param name="provider">The <see cref="IAzurePipelinesProvider"/>.</param>
        /// <param name="parent">The parent <see cref="Guid"/>.</param>
        /// <param name="progress">The progress.</param>
        public static void UpdateProgress(this IAzurePipelinesProvider provider, Guid parent, int progress)
        {
            provider.Commands.UpdateRecord(parent, new AzurePipelinesRecordData { Progress = progress, Status = AzurePipelinesTaskStatus.InProgress });
        }

        internal static bool IsRunningOnPipelines(this Common.Build.BuildSystem b) => b.IsRunningOnAzurePipelines || b.IsRunningOnAzurePipelinesHosted;
    }
}
