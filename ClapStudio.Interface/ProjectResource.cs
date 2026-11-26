namespace ClapStudio.Interface
{
    public class ProjectResource(string path)
    {
        public enum EResourceType
        {
            Video,
            Audio,
            Image,
            Other
        }

        private static readonly string[] ms_VideoExtensions = [".mp4", ".mov", ".mkv", ".avi", ".webm", ".wmv", ".m4v"];
        private static readonly string[] ms_AudioExtensions = [".mp3", ".wav", ".flac", ".aac", ".ogg", ".m4a"];
        private static readonly string[] ms_ImageExtensions = [".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp", ".tif", ".tiff"];

        private static EResourceType GetResourceType(string path)
        {
            string extension = System.IO.Path.GetExtension(path).ToLowerInvariant();
            if (Array.Exists(ms_VideoExtensions, ext => ext == extension))
                return EResourceType.Video;
            else if (Array.Exists(ms_AudioExtensions, ext => ext == extension))
                return EResourceType.Audio;
            else if (Array.Exists(ms_ImageExtensions, ext => ext == extension))
                return EResourceType.Image;
            else
                return EResourceType.Other;
        }

        private readonly string m_Path = path;
        private string m_DisplayName = System.IO.Path.GetFileNameWithoutExtension(path);
        private readonly EResourceType m_ResourceType = GetResourceType(path);

        public string Path => m_Path;
        public string DisplayName => m_DisplayName;
        public EResourceType ResourceType => m_ResourceType;

        public void SetDisplayName(string name) => m_DisplayName = string.IsNullOrWhiteSpace(name) ? System.IO.Path.GetFileNameWithoutExtension(m_Path) : name.Trim();
    }
}
