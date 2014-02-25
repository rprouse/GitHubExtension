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
using System.Threading.Tasks;
using Akavache;
using Alteridem.GitHub.Model;
using NLog;

namespace Alteridem.GitHub
{
    public static class Cache
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private const string CredentialCache = "CredentialCache";

        public static void ClearCredentials()
        {
            BlobCache.Secure.Invalidate(CredentialCache);
        }

        public static void SaveCredentials(string logon, string password)
        {
            BlobCache.Secure.InsertObject(CredentialCache, new CredentialCache { Logon = logon, Password = password });
        }

        public static async Task<CredentialCache> GetCredentials()
        {
            try
            {
                return await BlobCache.Secure.GetObjectAsync<CredentialCache>(CredentialCache);
            }
            catch (Exception ex)
            {
                log.InfoException("Credentials are not in the cache", ex);
            }
            return null;
        }
    }
}