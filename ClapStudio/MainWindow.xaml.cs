using ClapStudio.Interface;
using ClapStudio.Interface.Bundles;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.Dock;

namespace ClapStudio
{
    public class ModuleView(AViewModule module)
    {
        private readonly ToggleMenuFlyoutItem m_MenuItem = new()
        {
            Text = module.Name,
            Tag = module,
            IsChecked = false
        };
        private Document? m_View;

        public AViewModule? Module => m_MenuItem.Tag as AViewModule;
        public ToggleMenuFlyoutItem MenuItem => m_MenuItem;
        public Document? View => m_View;

        public void LoadView(Document view)
        {
            m_View = view;
            m_MenuItem.IsChecked = true;
        }

        public void UnloadView()
        {
            m_View = null;
            m_MenuItem.IsChecked = false;
        }
    }

    public sealed partial class MainWindow : Window, IDockAdapter
    {
        private static readonly string ms_ApplicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ClapStudio");
        private static readonly string ms_LayoutPath = Path.Combine(ms_ApplicationDataPath, "layout.json");
        private static readonly string ms_TitleSeparator = "##";

        private readonly ResourcesFolder<AViewModule> m_Modules = new("Modules");
        private readonly Dictionary<string, ModuleView> m_ModulesView = [];

        public MainWindow()
        {
            InitializeComponent();
            MainDockManager.Adapter = this;
            AppWindow.Closing += (_, __) => SaveLayout();
        }

        public void Initialize()
        {
            ModulesMenuBarItem.Items.Clear();
            ConvertDirectory(m_Modules, ModulesMenuBarItem.Items);
            RestoreLayout();
        }

        public void RegisterViewModule(AViewModule module) => m_Modules.AddResource(module.Path.Split('/', StringSplitOptions.RemoveEmptyEntries), module);

        public void RegisterBundle(ABundle bundle)
        {
            ResourcesFolder<AViewModule> bundleDirectory = new(bundle.Name);
            foreach (AViewModule module in bundle.GetViewModules())
                bundleDirectory.AddResource(module.Path.Split('/', StringSplitOptions.RemoveEmptyEntries), module);
            m_Modules.AddFolder(bundleDirectory);
        }

        private void ConvertDirectory(ResourcesFolder<AViewModule> directory, IList<MenuFlyoutItemBase> items)
        {
            foreach (ResourcesFolder<AViewModule> subDirectory in directory.SubFolders.Values)
            {
                MenuFlyoutSubItem subDirectoryMenuItem = new() { Text = subDirectory.Name };
                ConvertDirectory(subDirectory, subDirectoryMenuItem.Items);
                items.Add(subDirectoryMenuItem);
            }
            foreach (AViewModule module in directory.Resources)
            {
                ModuleView moduleView = new(module);
                ToolTipService.SetToolTip(moduleView.MenuItem, module.Description);
                moduleView.MenuItem.Click += OnModuleMenuItemClick;
                items.Add(moduleView.MenuItem);
                m_ModulesView[module.ModuleID] = moduleView;
            }
        }

        private void OpenModule(string moduleID, Document document)
        {
            if (m_ModulesView.TryGetValue(moduleID, out ModuleView? moduleView) && moduleView.Module is not null)
            {
                document.Title = $"{moduleID}{ms_TitleSeparator}{moduleView.Module.Name}";
                document.Content = moduleView.Module.CreateView();
                document.CanClose = true;
                document.Loaded += (_, __) => DispatcherQueue.TryEnqueue(() => { moduleView.LoadView(document); });
                document.Unloaded += (_, __) => DispatcherQueue.TryEnqueue(() =>
                {
                    if (!document.IsLoaded && document.Owner == null)
                        moduleView.UnloadView();
                });
            }
        }

        private void CloseModule(string moduleID)
        {
            if (m_ModulesView.TryGetValue(moduleID, out ModuleView? moduleView) && moduleView.View is not null)
            {
                WindowHelper.CloseDocument(moduleView.View, MainDockManager);
            }
        }

        private static DocumentGroup? FindDocumentGroup(object? node)
        {
            switch (node)
            {
                case DocumentGroup group:
                    return group;

                case LayoutPanel panel:
                    foreach (object? child in panel.Children)
                    {
                        if (FindDocumentGroup(child) is { } found)
                            return found;
                    }
                    break;
            }

            return null;
        }

        private DocumentGroup GetOrCreateRootGroup()
        {
            DocumentGroup? group = FindDocumentGroup(MainDockManager.Panel);

            if (group is null)
            {
                group = new DocumentGroup();
                LayoutPanel panel = new() { Orientation = Orientation.Horizontal };
                panel.Children.Add(group);
                MainDockManager.Panel = panel;
            }

            group.ShowWhenEmpty = true;
            return group;
        }

        private void OnModuleMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleMenuFlyoutItem { Tag: AViewModule module } menuItem)
                return;

            if (menuItem.IsChecked)
            {
                Document document = new();
                OpenModule(module.ModuleID, document);

                if (MainDockManager.ActiveDocument is not null)
                    document.DockTo(MainDockManager.ActiveDocument, DockTarget.Center);
                else
                    GetOrCreateRootGroup().Children.Add(document);
                MainDockManager.ActiveDocument = document;
            }
            else
                CloseModule(module.ModuleID);
        }

        public void SaveLayout()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ms_LayoutPath)!);
            File.WriteAllText(ms_LayoutPath, MainDockManager.SaveLayout());
        }

        private void RestoreLayout()
        {
            if (!File.Exists(ms_LayoutPath))
                return;
            try
            {
                MainDockManager.LoadLayout(File.ReadAllText(ms_LayoutPath));
            } catch {}
        }

        public void OnCreated(Document document)
        {
            int index = document.Title.IndexOf(ms_TitleSeparator, StringComparison.Ordinal);
            string moduleID = index > 0 ? document.Title[..index] : string.Empty;
            if (string.IsNullOrEmpty(moduleID))
            {
                document.Content = new TextBlock { Text = "Module introuvable" };
                document.CanClose = true;
            }
            else
                OpenModule(moduleID, document);
        }

        public void OnCreated(DocumentGroup group, Document? draggedDocument) { }

        public object? GetFloatingWindowTitleBar(Document? draggedDocument) => null;
    }
}
