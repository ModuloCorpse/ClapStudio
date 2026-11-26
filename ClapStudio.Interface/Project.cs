namespace ClapStudio.Interface
{
    public class Project(string name)
    {
        private readonly ResourcesFolder<ProjectResource> m_Resources = new(name);
        private string m_Name = name;

        public ResourcesFolder<ProjectResource> Resources => m_Resources;
        public string Name => m_Name;

        public void SetName(string name) => m_Name = name;
        public bool ContainsResource(string path) => m_Resources.EnumerateAllResources().Any(r => string.Equals(r.Path, path, StringComparison.OrdinalIgnoreCase));
    }
}
