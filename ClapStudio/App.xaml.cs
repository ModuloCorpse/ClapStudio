using ClapStudio.Core;
using ClapStudio.Core.ResourceViewModule;
using ClapStudio.Core.TestBundle;
using ClapStudio.Interface;
using Microsoft.UI.Xaml;

namespace ClapStudio
{
    public partial class App : Application
    {
        private ProjectSession m_Session = new();
        private MainWindow? m_Window;

        public App()
        {
            InitializeComponent();
            m_Session.SetCurrent(new("New Project"));
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_Window = new MainWindow();

            TestBundle testBundle = new();
            m_Window.RegisterViewModule(new ResourceViewModule(m_Session));


            m_Window.RegisterBundle(testBundle);
            //TODO Instanciate and register other bundles here

            m_Window.Initialize();
            m_Window.Activate();
        }
    }
}
