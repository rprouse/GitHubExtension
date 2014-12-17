// **********************************************************************************
// The MIT License (MIT)
// 
// Copyright (c) 2014 Rob Prouse
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

using System.Collections.Generic;
using System.Linq;
using Octokit;

namespace Alteridem.GitHub.Filters
{
    /// <summary>
    /// Filters a list of issues based on the given user
    /// </summary>
    public static class UserFilter
    {
        public static IEnumerable<Issue> Filter(this IEnumerable<Issue> issues, User user, UserFilterType filter)
        {
            if (user == null) return issues;

            switch (filter)
            {
            case UserFilterType.AssignedToMe:
                    return issues.Where(i => i.Assignee != null && i.Assignee.Id == user.Id);
            case UserFilterType.ReportedByMe:
                    return issues.Where(i => i.User != null && i.User.Id == user.Id);
            case UserFilterType.Unassigned:
                    return issues.Where(i => i.Assignee == null);
            default:
                    return issues;
            }       
        }
    }
}