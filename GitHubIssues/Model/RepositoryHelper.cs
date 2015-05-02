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

using System;
using System.IO;
using System.Linq;

namespace Alteridem.GitHub.Model
{
    /// <summary>
    /// Helper methods for working with Repositories
    /// </summary>
    public static class RepositoryHelper
    {
        /// <summary>
        /// Given a file or directory, finds the root of a Git repository
        /// by searching for the .git directory
        /// </summary>
        /// <param name="path">The path to a file or a directory in which to start the search</param>
        /// <returns></returns>
        public static DirectoryInfo FindRepositoryRoot(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Cannot be null or empty", "path");

            var directory = GetDirectory(path);
            while(directory != null)
            {
                if (IsGitRoot(directory))
                    return directory;
                directory = directory.Parent;
            }
            return null;
        }

        private static bool IsGitRoot(DirectoryInfo directory)
        {
            return directory.GetDirectories(".git", SearchOption.TopDirectoryOnly).Any();
        }

        private static DirectoryInfo GetDirectory(string path)
        {
            if(Directory.Exists(path))
            {
                return new DirectoryInfo(path);
            }
            else if(File.Exists(path))
            {
                return new FileInfo(path).Directory;
            }
            return null;
        }
    }
}
