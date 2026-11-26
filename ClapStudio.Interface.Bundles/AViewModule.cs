using Microsoft.UI.Xaml;

namespace ClapStudio.Interface.Bundles
{
    public abstract class AViewModule(string moduleID, string name, string description, string path = "") : AEntryModule(name, description, path)
    {
        private readonly string m_ModuleID = moduleID;
        public string ModuleID => m_ModuleID;

        public abstract FrameworkElement CreateView();
    }
}
