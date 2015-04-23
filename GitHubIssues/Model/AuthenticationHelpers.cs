using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

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
