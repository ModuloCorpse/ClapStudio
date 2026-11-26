using ClapStudio.Interface.Bundles;
using Microsoft.UI.Xaml;

namespace ClapStudio.Core.ResourceViewModule
{
    public sealed class ResourceViewModule(ProjectSession session) : AViewModule("18974e65-49d5-4ee2-81d5-2c99b64b0a14", "Resource View", "Display and manage project resources")
    {
        public override FrameworkElement CreateView() => new ResourceView(session);
    }
}
