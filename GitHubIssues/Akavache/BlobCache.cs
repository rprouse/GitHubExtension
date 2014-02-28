using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Ninject.Parameters;
using NLog;

namespace Alteridem.GitHub.Akavache
{
    public static class BlobCache
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        static string applicationName;

        /// <summary>
        /// Your application's name. Set this at startup, this defines where
        /// your data will be stored (usually at %AppData%\[ApplicationName])
        /// </summary>
        public static string ApplicationName
        {
            get
            {
                if (applicationName == null)
                    throw new Exception("Make sure to set BlobCache.ApplicationName on startup");

                return applicationName;
            }
            set { applicationName = value; }
        }

        static IBlobCache localMachine;
        static IBlobCache userAccount;
        static ISecureBlobCache secure;

        /// <summary>
        /// The local machine cache. Store data here that is unrelated to the
        /// user account or shouldn't be uploaded to other machines (i.e.
        /// image cache data)
        /// </summary>
        public static IBlobCache LocalMachine
        {
            get { return localMachine ?? Factory.Get<IBlobCache>(new ConstructorArgument("cacheDirectory", "LocalMachine")); }
            set { localMachine = value; }
        }

        /// <summary>
        /// The user account cache. Store data here that is associated with
        /// the user; in large organizations, this data will be synced to all
        /// machines via NT Roaming Profiles.
        /// </summary>
        public static IBlobCache UserAccount
        {
            get { return userAccount ?? Factory.Get<IBlobCache>(new ConstructorArgument("cacheDirectory", "UserAccount")); }
            set { userAccount = value; }
        }

        /// <summary>
        /// An IBlobCache that is encrypted - store sensitive data in this
        /// cache such as login information.
        /// </summary>
        public static ISecureBlobCache Secure
        {
            get { return secure ?? Factory.Get<ISecureBlobCache>(); }
            set { secure = value; }
        }

        /// <summary>
        /// This method shuts down all of the blob caches. Make sure call it
        /// on app exit and await / Wait() on it!
        /// </summary>
        /// <returns>A Task representing when all caches have finished shutting
        /// down.</returns>
        public static Task Shutdown()
        {
            var toDispose = new[] { LocalMachine, UserAccount, Secure };

            var ret = toDispose.Select(x =>
            {
                x.Dispose();
                return x.Shutdown;
            }).Merge().ToList().Select(_ => Unit.Default);

            return ret.ToTask();
        }
    }
}
