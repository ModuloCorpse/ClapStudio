namespace ClapStudio.Interface
{
    public class ResourcesFolder<TResource>(string name)
    {
        private readonly Dictionary<string, ResourcesFolder<TResource>> m_SubFolders = [];
        private readonly List<TResource> m_Resources = [];
        private string m_Name = name;

        public Dictionary<string, ResourcesFolder<TResource>> SubFolders => m_SubFolders;
        public List<TResource> Resources => m_Resources;
        public string Name => m_Name;

        public bool RenameFolder(string oldName, string newName)
        {
            newName = newName.Trim();
            if (newName.Length == 0 || !m_SubFolders.TryGetValue(oldName, out ResourcesFolder<TResource>? folder))
                return false;

            if (oldName == newName)
                return true;

            if (m_SubFolders.ContainsKey(newName))
                return false;

            m_SubFolders.Remove(oldName);
            folder.m_Name = newName;
            m_SubFolders[newName] = folder;
            return true;
        }

        public void AddFolder(ResourcesFolder<TResource> directory) => m_SubFolders[directory.Name] = directory;
        public void AddFolder(string[] path, ResourcesFolder<TResource> directory)
        {
            if (path.Length == 0)
            {
                AddFolder(directory);
                return;
            }

            string currentPathPart = path[0];
            string[] nextPath = path[1..];
            if (m_SubFolders.TryGetValue(currentPathPart, out ResourcesFolder<TResource>? subDirectory))
                subDirectory.AddFolder(nextPath, directory);
            else
            {
                ResourcesFolder<TResource> newDirectory = new(currentPathPart);
                newDirectory.AddFolder(nextPath, directory);
                m_SubFolders[currentPathPart] = newDirectory;
            }
        }

        public void AddResource(TResource resource) => m_Resources.Add(resource);
        public void AddResource(string[] path, TResource resource)
        {
            if (path.Length == 0)
            {
                AddResource(resource);
                return;
            }

            string currentPathPart = path[0];
            string[] nextPath = path[1..];
            if (m_SubFolders.TryGetValue(currentPathPart, out ResourcesFolder<TResource>? subDirectory))
                subDirectory.AddResource(nextPath, resource);
            else
            {
                ResourcesFolder<TResource> newDirectory = new(currentPathPart);
                newDirectory.AddResource(nextPath, resource);
                m_SubFolders[currentPathPart] = newDirectory;
            }
        }

        public bool RemoveFolder(string name) => m_SubFolders.Remove(name);
        public void RemoveFolder(string[] path, string name)
        {
            if (path.Length == 0)
            {
                RemoveFolder(name);
                return;
            }

            string currentPathPart = path[0];
            string[] nextPath = path[1..];
            if (m_SubFolders.TryGetValue(currentPathPart, out ResourcesFolder<TResource>? subDirectory))
                subDirectory.RemoveFolder(nextPath, name);
        }

        public bool RemoveResource(TResource resource) => m_Resources.Remove(resource);
        public void RemoveResource(string[] path, TResource resource)
        {
            if (path.Length == 0)
            {
                RemoveResource(resource);
                return;
            }

            string currentPathPart = path[0];
            string[] nextPath = path[1..];
            if (m_SubFolders.TryGetValue(currentPathPart, out ResourcesFolder<TResource>? subDirectory))
                subDirectory.RemoveResource(nextPath, resource);
        }

        public IEnumerable<TResource> EnumerateAllResources()
        {
            foreach (TResource resource in m_Resources)
                yield return resource;

            foreach (ResourcesFolder<TResource> subFolder in m_SubFolders.Values)
                foreach (TResource resource in subFolder.EnumerateAllResources())
                    yield return resource;
        }
    }
}