namespace ClapStudio.Interface.Bundles
{
    public class AEntryModule(string name, string description, string path = "") : AModule(name, description)
    {
        private readonly string m_Path = path;

        public string Path => m_Path;
    }
}
