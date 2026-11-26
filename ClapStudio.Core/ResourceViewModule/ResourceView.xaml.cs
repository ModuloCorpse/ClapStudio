using ClapStudio.Interface;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace ClapStudio.Core.ResourceViewModule
{
    public sealed partial class ResourceView : UserControl
    {
        private readonly ProjectSession m_Session;
        private readonly Dictionary<object, ResourceItem> m_ItemCache = new(ReferenceEqualityComparer.Instance);
        private readonly List<ResourcesFolder<ProjectResource>> m_FolderPath = []; // racine -> dossier courant

        public ObservableCollection<ResourceItem> Items { get; } = [];

        private ResourcesFolder<ProjectResource>? CurrentFolder => m_FolderPath.Count > 0 ? m_FolderPath[^1] : null;

        public ResourceView(ProjectSession session)
        {
            m_Session = session;
            InitializeComponent();

            m_Session.CurrentChanged += OnProjectChanged;
            OnProjectChanged();
        }

        private void OnProjectChanged()
        {
            m_FolderPath.Clear();
            m_ItemCache.Clear();
            if (m_Session.Current is Project project)
                m_FolderPath.Add(project.Resources);
            Refresh();
        }

        private void Refresh()
        {
            Items.Clear();
            if (CurrentFolder is ResourcesFolder<ProjectResource> folder)
            {
                foreach (ResourcesFolder<ProjectResource> subFolder in
                         folder.SubFolders.Values.OrderBy(f => f.Name, StringComparer.CurrentCultureIgnoreCase))
                    Items.Add(GetItem(subFolder));

                foreach (ProjectResource resource in folder.Resources)
                    Items.Add(GetItem(resource));
            }

            EmptyHint.Visibility = Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            UpButton.IsEnabled = m_FolderPath.Count > 1;
            PathText.Text = string.Join(" / ", m_FolderPath.Select(f => f.Name));
        }

        private ResourceItem GetItem(ResourcesFolder<ProjectResource> folder)
        {
            if (!m_ItemCache.TryGetValue(folder, out ResourceItem? item))
                m_ItemCache[folder] = item = new ResourceItem(folder);
            return item;
        }

        private ResourceItem GetItem(ProjectResource resource)
        {
            if (!m_ItemCache.TryGetValue(resource, out ResourceItem? item))
                m_ItemCache[resource] = item = new ResourceItem(resource);
            return item;
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            if (CurrentFolder is not null && e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                e.DragUIOverride.Caption = "Ajouter aux ressources";
            }
        }

        private async void OnDrop(object sender, DragEventArgs e)
        {
            if (CurrentFolder is not ResourcesFolder<ProjectResource> target
                || m_Session.Current is not Project project
                || !e.DataView.Contains(StandardDataFormats.StorageItems))
                return;

            DragOperationDeferral deferral = e.GetDeferral();
            try
            {
                foreach (IStorageItem item in await e.DataView.GetStorageItemsAsync())
                    await ImportAsync(item, target, project);
            }
            finally
            {
                Refresh();
                deferral.Complete();
            }
        }

        private static async Task ImportAsync(IStorageItem item, ResourcesFolder<ProjectResource> target, Project project)
        {
            switch (item)
            {
                case StorageFile file when file.Path.Length > 0 && !project.ContainsResource(file.Path):
                    target.AddResource(new ProjectResource(file.Path));
                    break;

                case StorageFolder folder:
                    ResourcesFolder<ProjectResource> subFolder = GetOrCreateSubFolder(target, folder.DisplayName);
                    foreach (IStorageItem child in await folder.GetItemsAsync())
                        await ImportAsync(child, subFolder, project);
                    break;
            }
        }

        private static ResourcesFolder<ProjectResource> GetOrCreateSubFolder(ResourcesFolder<ProjectResource> parent, string name)
        {
            if (!parent.SubFolders.TryGetValue(name, out ResourcesFolder<ProjectResource>? subFolder))
            {
                subFolder = new ResourcesFolder<ProjectResource>(name);
                parent.AddFolder(subFolder);
            }
            return subFolder;
        }

        private void OnUpClick(object sender, RoutedEventArgs e)
        {
            if (m_FolderPath.Count <= 1) return;
            m_FolderPath.RemoveAt(m_FolderPath.Count - 1);
            Refresh();
        }

        private void OnItemDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if ((e.OriginalSource as FrameworkElement)?.DataContext is ResourceItem { Folder: ResourcesFolder<ProjectResource> folder })
            {
                m_FolderPath.Add(folder);
                Refresh();
            }
        }

        private void OnNewFolderClick(object sender, RoutedEventArgs e)
        {
            if (CurrentFolder is not ResourcesFolder<ProjectResource> current) return;

            string name = "Nouveau dossier";
            for (int index = 2; current.SubFolders.ContainsKey(name); index++)
                name = $"Nouveau dossier ({index})";

            current.AddFolder(new ResourcesFolder<ProjectResource>(name));
            Refresh();
        }

        private async void OnRenameClick(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is not ResourceItem item
                || CurrentFolder is not ResourcesFolder<ProjectResource> current)
                return;

            bool IsValid(string text)
            {
                string name = text.Trim();
                if (item.Folder is not ResourcesFolder<ProjectResource> folder)
                    return true;
                return name.Length > 0 && (name == folder.Name || !current.SubFolders.ContainsKey(name));
            }

            TextBox box = new() { Text = item.DisplayName };
            box.Loaded += (_, _) =>
            {
                box.Focus(FocusState.Programmatic);
                box.SelectAll();
            };

            ContentDialog dialog = new()
            {
                XamlRoot = XamlRoot,
                Title = "Renommer",
                Content = box,
                PrimaryButtonText = "OK",
                CloseButtonText = "Annuler",
                DefaultButton = ContentDialogButton.Primary
            };
            box.TextChanged += (_, _) => dialog.IsPrimaryButtonEnabled = IsValid(box.Text);

            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
                return;

            if (item.Folder is ResourcesFolder<ProjectResource> renamedFolder)
            {
                if (current.RenameFolder(renamedFolder.Name, box.Text))
                    Refresh();
            }
            else if (item.Resource is ProjectResource resource)
            {
                resource.SetDisplayName(box.Text);
                item.NotifyDisplayNameChanged();
            }
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is not ResourceItem item || CurrentFolder is not ResourcesFolder<ProjectResource> current)
                return;
            if (item.Folder is ResourcesFolder<ProjectResource> folder)
            {
                current.RemoveFolder(folder.Name);
                m_ItemCache.Remove(folder);
            }
            else if (item.Resource is ProjectResource resource)
            {
                current.RemoveResource(resource);
                m_ItemCache.Remove(resource);
            }

            Refresh();
        }
    }
}
