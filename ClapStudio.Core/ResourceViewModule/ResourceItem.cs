using ClapStudio.Interface;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System.ComponentModel;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace ClapStudio.Core.ResourceViewModule
{
    public sealed partial class ResourceItem : INotifyPropertyChanged
    {
        private BitmapImage? m_Thumbnail;
        private bool m_IsMissing;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ResourceItem(ResourcesFolder<ProjectResource> folder) => Folder = folder;

        public ResourceItem(ProjectResource resource)
        {
            Resource = resource;
            _ = LoadThumbnailAsync(resource); // appelé depuis le thread UI
        }

        public ProjectResource? Resource { get; }
        public ResourcesFolder<ProjectResource>? Folder { get; }

        public string DisplayName => Resource?.DisplayName ?? Folder!.Name;
        public string ToolTip => Resource?.Path ?? Folder!.Name;
        public BitmapImage? Thumbnail => m_Thumbnail;
        public Visibility IconVisibility => m_Thumbnail is null ? Visibility.Visible : Visibility.Collapsed;

        // Segoe Fluent Icons
        public string Glyph
        {
            get
            {
                if (Folder is not null) return "\uE8B7";
                if (m_IsMissing) return "\uE7BA";
                return Resource!.ResourceType switch
                {
                    ProjectResource.EResourceType.Video => "\uE714",
                    ProjectResource.EResourceType.Audio => "\uE8D6",
                    ProjectResource.EResourceType.Image => "\uEB9F",
                    _ => "\uE7C3"
                };
            }
        }

        public void NotifyDisplayNameChanged() => Notify(nameof(DisplayName));

        private async Task LoadThumbnailAsync(ProjectResource resource)
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(resource.Path);
                ThumbnailMode mode = resource.ResourceType switch
                {
                    ProjectResource.EResourceType.Video => ThumbnailMode.VideosView,
                    ProjectResource.EResourceType.Audio => ThumbnailMode.MusicView,
                    ProjectResource.EResourceType.Image => ThumbnailMode.PicturesView,
                    _ => ThumbnailMode.DocumentsView
                };

                using StorageItemThumbnail? thumbnail = await file.GetThumbnailAsync(mode, 256);
                if (thumbnail is null) return;

                BitmapImage image = new();
                await image.SetSourceAsync(thumbnail);
                m_Thumbnail = image;
                Notify(nameof(Thumbnail));
                Notify(nameof(IconVisibility));
            }
            catch (FileNotFoundException)
            {
                m_IsMissing = true;
                Notify(nameof(Glyph));
            }
            catch (Exception)
            {
                // pas de miniature : on garde l'icône par défaut
            }
        }

        private void Notify(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
