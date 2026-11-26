using System;
using System.Collections.Generic;
using System.Text;

namespace ClapStudio.Interface.Video
{
    public class File(string path, string name, TimeSpan duration, List<Chapter> chapters)
    {
        private readonly string m_Path = path;
        private readonly string m_Name = name;
        private readonly TimeSpan m_Duration = duration;
        private readonly List<Chapter> m_Chapters = chapters;

        public string Path => m_Path;
        public string Name => m_Name;
        public TimeSpan Duration => m_Duration;
        public Chapter[] Chapters => [..m_Chapters];
    }
}
