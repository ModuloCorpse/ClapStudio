using System;
using System.Collections.Generic;
using System.Text;

namespace ClapStudio.Interface.Bundles
{
    public class AModule(string name, string description)
    {
        private readonly string m_Name = name;
        private readonly string m_Description = description;

        public string Name => m_Name;
        public string Description => m_Description;
    }
}
