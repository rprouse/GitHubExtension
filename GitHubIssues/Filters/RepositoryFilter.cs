// **********************************************************************************
// The MIT License (MIT)
// 
// Copyright (c) 2015 Rob Prouse
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// **********************************************************************************

#region Using Directives

using System.Collections.Generic;
using System.Linq;
using Alteridem.GitHub.Interfaces;
using Octokit;

#endregion

namespace Alteridem.GitHub.Filters
{
    /// <summary>
    /// Filters a list of repositories based on the options
    /// </summary>
    public static class RepositoryFilter
    {
        public static IEnumerable<Repository> Filter(this IEnumerable<Repository> repositories)
        {
            var ret = from r in repositories where r.HasIssues select r;
            var options = Factory.Get<IOptionsProvider>();
            if (options != null && options.Options != null && options.Options.HideRepositoriesWithNoIssues)
            {
                ret = ret.Where(r => r.OpenIssuesCount > 0);
            }
            return ret;
        }
    }
}