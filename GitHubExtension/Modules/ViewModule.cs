using Alteridem.GitHub.Extension.Interfaces;
using Alteridem.GitHub.Extension.View;
using Ninject.Modules;

namespace Alteridem.GitHub.Extension.Modules
{
    public class ViewModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ILoginView>().To<LoginDialog>();
            Bind<IAddComment>().To<AddComment>();
            Bind<IIssueEditor>().To<EditIssue>();
        }
    }
}
