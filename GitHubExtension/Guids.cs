// Guids.cs
// MUST match guids.h

using System;

namespace Alteridem.GitHub.Extension
{
    static class GuidList
    {
        public const string guidGitHubExtensionPkgString = "dae607b6-043f-400a-bd24-ffda143b13f4";
        public const string guidGitHubExtensionCmdSetString = "bf6a82ac-344c-427e-bafc-77b978131c65";
        public const string guidIssueListWindowPersistanceString = "04baf945-0cc3-4594-87ca-1656d83fdcfe";
        public const string guidIssueWindowPersistanceString = "05AF9426-E44E-404C-9653-0ADE833D1EAD";

        public static readonly Guid guidGitHubExtensionCmdSet = new Guid(guidGitHubExtensionCmdSetString);
    };
}