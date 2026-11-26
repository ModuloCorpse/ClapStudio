using ClapStudio.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClapStudio.Core
{
    public sealed class ProjectSession
    {
        private Project? m_Current;

        public event Action? CurrentChanged;

        public Project? Current => m_Current;

        public void SetCurrent(Project? project)
        {
            if (ReferenceEquals(m_Current, project))
                return;
            m_Current = project;
            CurrentChanged?.Invoke();
        }
    }
}
