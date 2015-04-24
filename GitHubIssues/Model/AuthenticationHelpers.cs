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

using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace Alteridem.GitHub.Model
{
    public static class AuthenticationHelpers
    {
        public static string GetFingerprint()
        {
            return Sha256Hash(GetNetworkInterface());
        }

        public static string GetMachineName()
        {
            try
            {
                return Dns.GetHostName();
            }
            catch (Exception)
            {
                try
                {
                    return Environment.MachineName;
                }
                catch (Exception)
                {
                    return "--unknown--";
                }
            }
        }

        public static string Sha256Hash(string input)
        {
            using (SHA256 shA256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                var hash = from b in shA256.ComputeHash(bytes) select b.ToString("x2", CultureInfo.InvariantCulture);
                return string.Join("", hash);
            }
        }

        public static string GetNetworkInterface()
        {
            var nics = from nic in NetworkInterface.GetAllNetworkInterfaces()
                      where nic.OperationalStatus == OperationalStatus.Up
                      orderby nic.Speed
                      select nic.GetPhysicalAddress().ToString();
            return nics.FirstOrDefault(addr => addr.Length > 12) ?? GetMachineName();
        }
    }
}
