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

using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akavache;
using Alteridem.GitHub.Model;
using NLog;
using Octokit;

namespace Alteridem.GitHub
{
    public static class Cache
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private const string CredentialCache = "CredentialCache";
        private const string Repository = "Repository";

        public static void DeleteCredentials()
        {
            DeleteObject(CredentialCache);
        }

        public static void SaveCredentials(string logon, string password)
        {
            SaveObject(CredentialCache, new CredentialCache { Logon = logon, Password = password });
        }

        public static IObservable<CredentialCache> GetCredentials()
        {
            return GetObjectAsync<CredentialCache>(CredentialCache);
        }

        public static void SaveRepository(Repository repository)
        {
            SaveObject(Repository, repository);
        }

        public static IObservable<Repository> GetRepository()
        {
            return GetObjectAsync<Repository>(Repository);
        }

        public static void DeleteRepository()
        {
            DeleteObject(Repository);
        }

        #region Private Members

        private static IObservable<T> GetObjectAsync<T>(string key)
        {
            return BlobCache.Secure.GetObjectAsync<T>(key)
                .ObserveOn(SynchronizationContext.Current);
        }

        private static void SaveObject<T>(string key, T obj)
        {
            BlobCache.Secure.InsertObject(key, obj);
        }

        private static void DeleteObject(string key)
        {
            BlobCache.Secure.Invalidate(key);
        }

        #endregion

    }
}