using Alteridem.GitHub.Extension.Interfaces;
using Alteridem.GitHub.Extension.View;
using Alteridem.GitHub.Logging;
using Ninject.Modules;

namespace Alteridem.GitHub.Extension.Modules
{
    public class ViewModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ILoginView>().To<Login>();
            Bind<IAddComment>().To<AddComment>();
            Bind<IIssueEditor>().To<IssueEditor>();
            Bind<ILabelPicker>().To<LabelPicker>();
            Bind<IOutputWriter>().To<OutputWriter>().InSingletonScope();
        }
    }
}
