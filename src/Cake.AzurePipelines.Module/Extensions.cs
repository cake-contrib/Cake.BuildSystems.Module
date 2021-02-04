using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cake.Common.Build.AzurePipelines;
using Cake.Common.Build.AzurePipelines.Data;

namespace Cake.AzurePipelines.Module
{
    public static class Extensions
    {
        public static void UpdateProgress(this IAzurePipelinesProvider provider, Guid parent, int progress)
        {
            provider.Commands.UpdateRecord(parent, new AzurePipelinesRecordData {Progress = progress, Status = AzurePipelinesTaskStatus.InProgress});
        }

        internal static bool IsRunningOnPipelines(this Common.Build.BuildSystem b) => b.IsRunningOnAzurePipelines || b.IsRunningOnAzurePipelinesHosted;
    }
}
