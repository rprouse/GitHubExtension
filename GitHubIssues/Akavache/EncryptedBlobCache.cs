using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography;

namespace Alteridem.GitHub.Akavache
{
    public abstract class EncryptedBlobCache : PersistentBlobCache, ISecureBlobCache
    {
        protected EncryptedBlobCache(
            string cacheDirectory = null, 
            IFilesystemProvider filesystemProvider = null, 
            IScheduler scheduler = null,
            Action<AsyncSubject<byte[]>> invalidatedCallback = null) 
            : base(cacheDirectory, filesystemProvider, scheduler, invalidatedCallback)
        {
        }

        protected override IObservable<byte[]> BeforeWriteToDiskFilter(byte[] data, IScheduler scheduler)
        {
            try
            {
                return Observable.Return(ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser));
            } 
            catch(Exception ex)
            {
                return Observable.Throw<byte[]>(ex);
            }
            
        }

        protected override IObservable<byte[]> AfterReadFromDiskFilter(byte[] data, IScheduler scheduler)
        {
            try
            {
                return Observable.Return(ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser));
            } 
            catch(Exception ex)
            {
                return Observable.Throw<byte[]>(ex);
            }
        }

    }

    class CEncryptedBlobCache : EncryptedBlobCache {
        public CEncryptedBlobCache(string cacheDirectory = "LocalMachine", IFilesystemProvider fsProvider = null) : base(cacheDirectory, fsProvider, System.Reactive.Concurrency.Scheduler.Default) { }
    }
}
