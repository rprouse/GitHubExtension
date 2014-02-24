using System;
using System.Runtime.CompilerServices;
using Alteridem.GitHub.Annotations;
using Microsoft.Win32;
using NLog;

namespace Alteridem.GitHub
{
    public static class Settings
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private const string REGISTRY_ROOT = "Alteridem\\GitHubExtension";

        /// <summary>
        /// Gets or sets the authentication token.
        /// </summary>
        [CanBeNull]
        public static string Token
        {
            get { return ReadString(); }
            set { WriteString(value); }
        }

        /// <summary>
        /// Gets or sets the selected repository.
        /// </summary>
        [CanBeNull]
        public static string Repository
        {
            get { return ReadString(); }
            set { WriteString( value ); }
        }

        [CanBeNull]
        private static string ReadString( [NotNull,CallerMemberName]string keyName = null)
        {
            try
            {
                using ( var key = Registry.CurrentUser.CreateSubKey( REGISTRY_ROOT, RegistryKeyPermissionCheck.ReadSubTree ) )
                {
                    if ( key != null )
                        return key.GetValue( keyName ) as string;
                }
            }
            catch(Exception ex)
            {
                log.ErrorException( "Failed to read registry key " + keyName, ex );
            }
            return string.Empty;
        }

        private static void WriteString( [NotNull] string value, [NotNull,CallerMemberName]string keyName = null)
        {
            try
            {
                using ( var key = Registry.CurrentUser.CreateSubKey( REGISTRY_ROOT, RegistryKeyPermissionCheck.ReadWriteSubTree ) )
                {
                    if ( key != null )
                        key.SetValue( keyName, value, RegistryValueKind.String );
                }
            }
            catch(Exception ex)
            {
                log.ErrorException("Failed to write registry key " + keyName, ex);
            }
        }
    }
}
