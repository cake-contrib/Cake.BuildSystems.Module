List<NuSpecContent> GetContent(IEnumerable<string> frameworks, ProjectCollection projects, Func<SolutionProject, bool> projectFilter = null) {
    projectFilter = projectFilter ?? (p => true);
    var content = new List<NuSpecContent>();
    foreach (var framework in frameworks) {
        foreach (var project in projects.SourceProjects.Where(projectFilter)) {
            Verbose("Loading package files for " + project.Name);
            var match = GetFiles(project.Path.GetDirectory() + "/bin/" + configuration + "/" + framework + "/" + project.Name +".*");
            var libFiles = match
                .Where(f => f.GetExtension() != ".pdb")
                .Where(f => !f.FullPath.Contains("Cake.Core"))
                .Where(f => !f.FullPath.Contains("Cake.Common"))
                .Select(f => new NuSpecContent { Source = f.FullPath, Target = "lib/" + framework});
            content.AddRange(libFiles);
        }
    }
    return content;
}

public class ProjectCollection {
    public IEnumerable<SolutionProject> SourceProjects {get;set;}
    public IEnumerable<DirectoryPath> SourceProjectPaths {get { return SourceProjects.Select(p => p.Path.GetDirectory()); } } 
    public IEnumerable<SolutionProject> TestProjects {get;set;}
    public IEnumerable<DirectoryPath> TestProjectPaths { get { return TestProjects.Select(p => p.Path.GetDirectory()); } }
    public IEnumerable<SolutionProject> AllProjects { get { return SourceProjects.Concat(TestProjects); } }
    public IEnumerable<DirectoryPath> AllProjectPaths { get { return AllProjects.Select(p => p.Path.GetDirectory()); } }
}

ProjectCollection GetProjects(FilePath slnPath) {
    var solution = ParseSolution(slnPath);
    var projects = solution.Projects.Where(p => p.Type != "{2150E333-8FDC-42A3-9474-1A3956D46DE8}");
    var testAssemblies = projects.Where(p => p.Name.Contains(".Tests")).Select(p => p.Path.GetDirectory() + "/bin/" + configuration + "/" + p.Name + ".dll");
    return new ProjectCollection {
        SourceProjects = projects.Where(p => !p.Name.Contains(".Tests")),
        TestProjects = projects.Where(p => p.Name.Contains(".Tests"))
    };
    
}