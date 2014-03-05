using Alteridem.GitHub.Extension.View;
using Alteridem.GitHub.Model;
using Ninject.Modules;

namespace Alteridem.GitHub.Extension.Modules
{
    public class ViewModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ILogonView>().To<LoginDialog>();
        }
    }
}
