using Alteridem.GitHub.Model;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Alteridem.GitHub.Extension.Test.Model
{
    [TestFixture]
    public class RepositoryHelperTest
    {
        [Test]
        public void CanGetGitRemotes()
        {
            // Gets the Git remotes for the repository that this source is
            // maintained in. Assumes that one of the remotes is on GitHub
            var path = Assembly.GetExecutingAssembly().Location;
            var root = RepositoryHelper.GetRemotes(path).ToArray();

            Assert.That(root, Is.Not.Null);
            Assert.That(root.Length, Is.GreaterThan(0));
        }

        [Test]
        public void CanFindRootOfRepositoryFromFile()
        {
            var path = Assembly.GetExecutingAssembly().Location;
            var root = RepositoryHelper.FindRepositoryRoot(path);
            Assert.That(root, Is.Not.Null);
            Assert.That(root, Does.Exist);
        }

        [Test]
        public void CanFindRootOfRepositoryFromDirectory()
        {
            var path = Assembly.GetExecutingAssembly().Location;
            var file = new FileInfo(path);
            path = file.DirectoryName;

            var root = RepositoryHelper.FindRepositoryRoot(path);
            Assert.That(root, Is.Not.Null);
            Assert.That(root, Does.Exist);
        }

        [Test]
        public void NotRepositoryReturnsNull()
        {
            string windowDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            Assert.That(windowDir, Is.Not.Null.And.Not.Empty);

            var root = RepositoryHelper.FindRepositoryRoot(windowDir);
            Assert.That(root, Is.Null);
        }

        [Test]
        public void NotValidFileOrDirectoryReturnsNull()
        {
            var root = RepositoryHelper.FindRepositoryRoot("C:\\Garbage\\Does\\Not\\Exist");
            Assert.That(root, Is.Null);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void NullOrEmptyPathThrowsAnArgumentException(string path)
        {
            Assert.That(() => RepositoryHelper.FindRepositoryRoot(path), Throws.ArgumentException);
        }
    }
}
