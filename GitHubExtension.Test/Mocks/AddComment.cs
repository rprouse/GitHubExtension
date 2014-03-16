using Alteridem.GitHub.Extension.Interfaces;
using Alteridem.GitHub.Model;

namespace Alteridem.GitHub.Extension.Test.Mocks
{
    public class AddComment : IAddComment
    {
        public bool? ShowModal()
        {
            var api = Factory.Get<GitHubApiBase>();
            api.AddComment(api.Issue, "new comment");
            return true;
        }

        public void Close()
        {
        }
    }
}