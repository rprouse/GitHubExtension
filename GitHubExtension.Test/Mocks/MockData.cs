using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;

namespace Alteridem.GitHub.Extension.Test.Mocks
{
    internal static class MockData
    {
        internal static User CreateTestUser(string email, string username) =>
            new User("https://avatars.githubusercontent.com/u/493828?v=1", null, null, 1, null, DateTimeOffset.Now, DateTimeOffset.Now, 1, email, 1, 1, true, null, 1,
                1, null, username, username, 1, null, 0, 0, 0, null, null, false, null, null);

        internal static Issue CreateTestIssue(int id, ItemState state, string title, string body, User user, DateTimeOffset createdAt, Milestone milestone = null, IReadOnlyList<Label> labels = null) =>
            new Issue(null, null, null, null, id, state, title, body, null,
                user, labels, null, null, milestone, 1, null, null,
                createdAt, null, 2, false, null);

        internal static Milestone CreateTestMilestone(int id, ItemState state, string title, string description, DateTimeOffset created, DateTimeOffset? closed) =>
            new Milestone(null, null, id, state, title, description, null, 1, 0, created, null, closed, created.AddDays(1));

        internal static Repository CreateTestRepository(int id, string name) =>
            new Repository(null, null, null, null, null, null, null, id, null, name, "test\\test",
                "Test repository",
                null, null, false, false, 1, 1, "master", 1, DateTimeOffset.Now.AddDays(-1),
                DateTimeOffset.Now.AddDays(-2), DateTimeOffset.Now.AddDays(-1),
                null, null, null, null, true, true, true, false, 1, 1000, true, true, true);
    }
}
