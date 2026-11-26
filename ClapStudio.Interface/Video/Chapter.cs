namespace ClapStudio.Interface.Video
{
    public class Chapter(TimeSpan timestamp, string title)
    {
        private readonly string m_Title = title;
        private readonly TimeSpan m_Timestamp = timestamp;

        public string Title => m_Title;
        public TimeSpan Timestamp => m_Timestamp;
    }
}
