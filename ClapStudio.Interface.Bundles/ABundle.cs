namespace ClapStudio.Interface.Bundles
{
    public abstract class ABundle(string name, string description, string author, string version)
    {
        private readonly string m_Name = name;
        private readonly string m_Description = description;
        private readonly string m_Author = author;
        private readonly string m_Version = version;

        public string Name => m_Name;
        public string Description => m_Description;
        public string Author => m_Author;
        public string Version => m_Version;

        public abstract AViewModule[] GetViewModules();
    }
}
