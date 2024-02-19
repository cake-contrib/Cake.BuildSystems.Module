using System;

using Cake.Core.Composition;

namespace Cake.AzurePipelines.Module
{
    /// <summary>
    /// Legacy <see cref="ICakeModule"/> implementation for GitLab CI.
    /// </summary>
    /// <remarks>
    /// When <see cref="GitLabCIModule"/> was introduced initially, it was placed in the <c>Cake.AzurePipelines.Module</c> namespace by accident.
    /// <para>
    /// The namespace has since been adjusted, but this class is still provided in the <c>Cake.AzurePipelines.Module</c> namespace for backwards compatibility in Cake.Frosting projects.
    /// When possible, use <see cref="GitLabCI.Module.GitLabCIModule"/> instead.
    /// </para>
    /// </remarks>
    [Obsolete($"Use {nameof(GitLabCIModule)} from namespace Cake.GitLabCI.Module instead")]
    public class GitLabCIModule : GitLabCI.Module.GitLabCIModule
    { }
}
