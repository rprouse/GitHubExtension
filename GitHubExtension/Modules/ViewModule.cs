using Alteridem.GitHub.Extension.View;
using Ninject.Modules;

namespace Alteridem.GitHub.Extension.Modules
{
    public class ViewModule : NinjectModule
    {
        public override void Load()
        {
            Bind<LoginDialog>().To<LoginDialog>();
        }
    }
}
