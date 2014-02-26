using Alteridem.GitHub.Model;
using Alteridem.GitHub.View;
using Ninject.Modules;

namespace Alteridem.GitHub.Modules
{
    public class ViewModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ILogonView>().To<LoginDialog>();
        }
    }
}
