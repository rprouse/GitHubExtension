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

#region Using Directives

using System;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace Alteridem.GitHub.Model
{
    public class CredentialCache
    {
        private static readonly byte[] s_entropy =
        {
             0xC, 0x1, 0xB, 0xE, 0xF, 0x5, 0x9, 0xF, 0xF, 0x8, 0x9, 0x6, 0xA, 0xC, 0x1, 0xC,
             0x4, 0x2, 0x9, 0xA
        };

        public string Logon { get; set; }
        public string Password { get; set; }
        public string AccessToken { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}\t{1}\t{2}", Logon, Encrypt(Password), Encrypt(AccessToken));
        }

        public static CredentialCache FromString(string cached)
        {
            if (String.IsNullOrWhiteSpace(cached))
                return null;

            var split = cached.Split(new[] { '\t' }, 3);
            if (split.Length < 2 || split.Length > 3)
                return null;

            try
            {
                string encryptedAccessToken = split.Length > 2 ? split[2] : null;
                return new CredentialCache { Logon = split[0], Password = Decrypt(split[1]), AccessToken = Decrypt(encryptedAccessToken) };
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string Encrypt(string text)
        {
            if (text == null)
                return null;

            byte[] strBytes = Encoding.Unicode.GetBytes(text);
            byte[] encryptedBytes = ProtectedData.Protect(strBytes, s_entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedBytes);
        }

        private static string Decrypt(string text)
        {
            if (text == null)
                return null;

            try
            {
                byte[] strBytes = Convert.FromBase64String(text);
                byte[] decryptedBytes = ProtectedData.Unprotect(strBytes, s_entropy, DataProtectionScope.CurrentUser);
                return Encoding.Unicode.GetString(decryptedBytes);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}