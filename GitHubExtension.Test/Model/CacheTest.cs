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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Alteridem.GitHub.Model;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using NUnit.Framework;
using IVsSettingsManager = Microsoft.VisualStudio.Shell.Interop.IVsSettingsManager;
using IVsSettingsStore = Microsoft.VisualStudio.Shell.Interop.IVsSettingsStore;
using IVsWritableSettingsStore = Microsoft.VisualStudio.Shell.Interop.IVsWritableSettingsStore;
using SVsSettingsManager = Microsoft.VisualStudio.Shell.Interop.SVsSettingsManager;

#endregion

namespace Alteridem.GitHub.Extension.Test.Model
{
    [TestFixture]
    public class CacheTest
    {
        private ExportProvider _exportProvider;

        [SetUp]
        public void SetUp()
        {
            var issuesAssemblyCatalog = new AssemblyCatalog(typeof(Cache).Assembly);
            var mockServicesCatalog = new TypeCatalog(typeof(MockServiceProvider));
            var catalog = new AggregateCatalog(issuesAssemblyCatalog, mockServicesCatalog);
            _exportProvider = new CompositionContainer(catalog);
        }

        private Cache SettingsCache
        {
            get
            {
                return _exportProvider.GetExportedValue<Cache>();
            }
        }

        [Test]
        public void TestRepository()
        {
            SettingsCache.Repository = 0;
            Assert.That(SettingsCache.Repository, Is.EqualTo(0));

            SettingsCache.Repository = 2;
            Assert.That(SettingsCache.Repository, Is.EqualTo(2));
        }

        [Test]
        public void TestCredentials()
        {
            SettingsCache.Credentials = new CredentialCache { Logon = "user", Password = "password", AccessToken = string.Empty };
            var cached = SettingsCache.Credentials;
            Assert.That(cached, Is.Not.Null);
            Assert.That(cached.Logon, Is.EqualTo("user"));
            Assert.That(cached.Password, Is.EqualTo("password"));
            Assert.That(cached.AccessToken, Is.EqualTo(string.Empty));

            SettingsCache.Credentials = null;
            Assert.That(SettingsCache.Credentials, Is.Null);
        }

        [Export(typeof(SVsServiceProvider))]
        internal class MockServiceProvider : SVsServiceProvider
        {
            private readonly ConcurrentDictionary<Type, object> _services =
                new ConcurrentDictionary<Type, object>();

            public object GetService(Type serviceType)
            {
                return _services.GetOrAdd(serviceType, ServiceFactory);
            }

            private object ServiceFactory(Type serviceType)
            {
                if (IsSameType(serviceType, typeof(SVsSettingsManager)))
                    return new MockSettingsManager();

                throw new NotSupportedException(string.Format("The '{0}' service is not yet supported.", serviceType));
            }

            private static bool IsSameType(Type x, Type y)
            {
                if (x == y)
                    return true;
                if (x == null || y == null)
                    return false;

                if (x.IsImport && y.IsImport)
                {
                    return x.GUID == y.GUID;
                }

                return false;
            }
        }

        private class MockSettingsManager : SVsSettingsManager, IVsSettingsManager
        {
            private readonly SettingsCollection _rootCollection = new SettingsCollection(string.Empty);

            #region IVsSettingsManager Members

            public int GetApplicationDataFolder(uint folder, out string folderPath)
            {
                throw new NotImplementedException();
            }

            public int GetCollectionScopes(string collectionPath, out uint scopes)
            {
                throw new NotImplementedException();
            }

            public int GetCommonExtensionsSearchPaths(uint paths, string[] commonExtensionsPaths, out uint actualPaths)
            {
                throw new NotImplementedException();
            }

            public int GetPropertyScopes(string collectionPath, string propertyName, out uint scopes)
            {
                throw new NotImplementedException();
            }

            public int GetReadOnlySettingsStore(uint scope, out IVsSettingsStore store)
            {
                store = new MockSettingsStore((SettingsScope)scope, _rootCollection);
                return VSConstants.S_OK;
            }

            public int GetWritableSettingsStore(uint scope, out IVsWritableSettingsStore writableStore)
            {
                writableStore = new MockWritableSettingsStore((SettingsScope)scope, _rootCollection);
                return VSConstants.S_OK;
            }

            #endregion
        }

        private class SettingsCollection
        {
            private readonly string _name;
            private readonly Dictionary<string, SettingsCollection> _collections =
                new Dictionary<string, SettingsCollection>();
            private readonly Dictionary<string, SettingsProperty> _properties =
                new Dictionary<string, SettingsProperty>();

            public SettingsCollection(string name)
            {
                if (name == null)
                    throw new ArgumentNullException("name");

                _name = name;
            }

            internal SettingsCollection GetOrCreateSettingsCollection(string name)
            {
                SettingsCollection collection;
                if (!_collections.TryGetValue(name, out collection))
                {
                    collection = new SettingsCollection(name);
                    _collections[name] = collection;
                }

                return collection;
            }

            internal bool TryGetSettingsCollection(string name, out SettingsCollection collection)
            {
                return _collections.TryGetValue(name, out collection);
            }

            internal bool TryGetProperty(string propertyName, out SettingsProperty property)
            {
                return _properties.TryGetValue(propertyName, out property);
            }

            internal void SetProperty(string propertyName, object value, Type type)
            {
                _properties[propertyName] = new SettingsProperty(propertyName, value, type);
            }
        }

        private class SettingsProperty
        {
            private readonly string _name;
            private readonly object _value;
            private readonly Type _type;

            public SettingsProperty(string name, object value, Type type)
            {
                _name = name;
                _value = value;
                _type = type;
            }

            public string Name
            {
                get
                {
                    return _name;
                }
            }

            public object Value
            {
                get
                {
                    return _value;
                }
            }

            public Type Type
            {
                get
                {
                    return _type;
                }
            }
        }

        private class MockSettingsStore : IVsSettingsStore
        {
            private readonly SettingsScope _scope;
            private readonly SettingsCollection _rootCollection;

            public MockSettingsStore(SettingsScope scope, SettingsCollection rootCollection)
            {
                _scope = scope;
                _rootCollection = rootCollection;
            }

            #region IVsSettingsStore Members

            public int CollectionExists(string collectionPath, out int pfExists)
            {
                throw new NotImplementedException();
            }

            public int GetBinary(string collectionPath, string propertyName, uint byteLength, byte[] pBytes = null, uint[] actualByteLength = null)
            {
                throw new NotImplementedException();
            }

            public int GetBool(string collectionPath, string propertyName, out int value)
            {
                throw new NotImplementedException();
            }

            public int GetBoolOrDefault(string collectionPath, string propertyName, int defaultValue, out int value)
            {
                throw new NotImplementedException();
            }

            public int GetInt(string collectionPath, string propertyName, out int value)
            {
                throw new NotImplementedException();
            }

            public int GetInt64(string collectionPath, string propertyName, out long value)
            {
                throw new NotImplementedException();
            }

            public int GetInt64OrDefault(string collectionPath, string propertyName, long defaultValue, out long value)
            {
                return GetValueOrDefault(collectionPath, propertyName, defaultValue, out value);
            }

            public int GetIntOrDefault(string collectionPath, string propertyName, int defaultValue, out int value)
            {
                return GetValueOrDefault(collectionPath, propertyName, defaultValue, out value);
            }

            public int GetLastWriteTime(string collectionPath, Microsoft.VisualStudio.Shell.Interop.SYSTEMTIME[] lastWriteTime)
            {
                throw new NotImplementedException();
            }

            public int GetPropertyCount(string collectionPath, out uint propertyCount)
            {
                throw new NotImplementedException();
            }

            public int GetPropertyName(string collectionPath, uint index, out string propertyName)
            {
                throw new NotImplementedException();
            }

            public int GetPropertyType(string collectionPath, string propertyName, out uint type)
            {
                throw new NotImplementedException();
            }

            public int GetString(string collectionPath, string propertyName, out string value)
            {
                throw new NotImplementedException();
            }

            public int GetStringOrDefault(string collectionPath, string propertyName, string defaultValue, out string value)
            {
                return GetValueOrDefault(collectionPath, propertyName, defaultValue, out value);
            }

            public int GetSubCollectionCount(string collectionPath, out uint subCollectionCount)
            {
                throw new NotImplementedException();
            }

            public int GetSubCollectionName(string collectionPath, uint index, out string subCollectionName)
            {
                throw new NotImplementedException();
            }

            public int GetUnsignedInt(string collectionPath, string propertyName, out uint value)
            {
                throw new NotImplementedException();
            }

            public int GetUnsignedInt64(string collectionPath, string propertyName, out ulong value)
            {
                throw new NotImplementedException();
            }

            public int GetUnsignedInt64OrDefault(string collectionPath, string propertyName, ulong defaultValue, out ulong value)
            {
                return GetValueOrDefault(collectionPath, propertyName, defaultValue, out value);
            }

            public int GetUnsignedIntOrDefault(string collectionPath, string propertyName, uint defaultValue, out uint value)
            {
                return GetValueOrDefault(collectionPath, propertyName, defaultValue, out value);
            }

            public int PropertyExists(string collectionPath, string propertyName, out int pfExists)
            {
                throw new NotImplementedException();
            }

            #endregion

            protected int SetProperty(string collectionPath, string propertyName, object value, Type type)
            {
                SettingsCollection collection = GetOrCreateSettingsCollection(collectionPath);
                collection.SetProperty(propertyName, value, type);
                return VSConstants.S_OK;
            }

            protected SettingsCollection GetOrCreateSettingsCollection(string collectionPath)
            {
                if (collectionPath == string.Empty)
                    return _rootCollection;

                string[] path = collectionPath.Split('\\');
                SettingsCollection collection = _rootCollection;
                foreach (var segment in path)
                    collection = collection.GetOrCreateSettingsCollection(segment);

                return collection;
            }

            protected bool TryGetSettingsCollection(string collectionPath, out SettingsCollection collection)
            {
                collection = _rootCollection;
                if (collectionPath == string.Empty)
                    return true;

                string[] path = collectionPath.Split('\\');
                foreach (var segment in path)
                {
                    if (!collection.TryGetSettingsCollection(segment, out collection))
                        return false;
                }

                return true;
            }

            protected bool TryGetProperty(string collectionPath, string propertyName, out SettingsProperty property)
            {
                SettingsCollection collection;
                if (!TryGetSettingsCollection(collectionPath, out collection))
                {
                    property = null;
                    return false;
                }

                return collection.TryGetProperty(propertyName, out property);
            }

            protected int GetValueOrDefault<T>(string collectionPath, string propertyName, T defaultValue, out T value)
            {
                SettingsProperty property;
                if (!TryGetProperty(collectionPath, propertyName, out property))
                {
                    value = defaultValue;
                    return VSConstants.S_FALSE;
                }

                if (!typeof(T).IsAssignableFrom(property.Type))
                {
                    value = default(T);
                    return VSConstants.E_INVALIDARG;
                }

                value = (T)property.Value;
                return VSConstants.S_OK;
            }
        }

        private class MockWritableSettingsStore : MockSettingsStore, IVsWritableSettingsStore
        {
            public MockWritableSettingsStore(SettingsScope scope, SettingsCollection rootCollection)
                : base(scope, rootCollection)
            {
            }

            #region IVsWritableSettingsStore Members


            public int CreateCollection(string collectionPath)
            {
                GetOrCreateSettingsCollection(collectionPath);
                return VSConstants.S_OK;
            }

            public int DeleteCollection(string collectionPath)
            {
                throw new NotImplementedException();
            }

            public int DeleteProperty(string collectionPath, string propertyName)
            {
                throw new NotImplementedException();
            }

            public int SetBinary(string collectionPath, string propertyName, uint byteLength, byte[] pBytes)
            {
                byte[] value = pBytes;
                if (value != null)
                {
                    Array.Resize(ref value, (int)byteLength);
                    if (value == pBytes)
                        value = (byte[])value.Clone();
                }

                return SetProperty(collectionPath, propertyName, value, typeof(byte[]));
            }

            public int SetBool(string collectionPath, string propertyName, int value)
            {
                return SetProperty(collectionPath, propertyName, value, typeof(bool));
            }

            public int SetInt(string collectionPath, string propertyName, int value)
            {
                return SetProperty(collectionPath, propertyName, value, typeof(int));
            }

            public int SetInt64(string collectionPath, string propertyName, long value)
            {
                return SetProperty(collectionPath, propertyName, value, typeof(long));
            }

            public int SetString(string collectionPath, string propertyName, string value)
            {
                return SetProperty(collectionPath, propertyName, value, typeof(string));
            }

            public int SetUnsignedInt(string collectionPath, string propertyName, uint value)
            {
                return SetProperty(collectionPath, propertyName, value, typeof(uint));
            }

            public int SetUnsignedInt64(string collectionPath, string propertyName, ulong value)
            {
                return SetProperty(collectionPath, propertyName, value, typeof(ulong));
            }

            #endregion
        }
    }
}