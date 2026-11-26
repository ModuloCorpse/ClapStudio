using ClapStudio.Interface.Bundles;

namespace ClapStudio.Core.TestBundle
{
    public class TestBundle() : ABundle("Test", "A test bundle for ClapStudio.", "ClapStudio Team", "1.0.0")
    {
        public override AViewModule[] GetViewModules() => [
            new TestViewModule("1268c8f2-65d1-41c4-bbca-0f500bbacabb", "Test1", 255, 0, 0),
            new TestViewModule("e250fe27-c70b-4eb5-b399-db8c1a0dee74", "Test2", 0, 255, 0),
            new SubTestViewModule("7e363075-5cfa-4d08-a6a8-82ce30436a19", "Test3", 0, 0, 255)
        ];
    }
}
