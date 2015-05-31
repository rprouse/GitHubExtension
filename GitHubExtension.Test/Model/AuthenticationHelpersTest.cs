using Alteridem.GitHub.Model;
using NUnit.Framework;

namespace Alteridem.GitHub.Extension.Test.Model
{
    [TestFixture]
    public class AuthenticationHelpersTest
    {
        [Test]
        public void CanGetFingerprint()
        {
            string fingerprint = AuthenticationHelpers.GetFingerprint();
            Assert.That(fingerprint, Is.Not.Null);
            Assert.That(fingerprint, Is.Not.Empty);
        }

        [Test]
        public void CanGetMachineName()
        {
            string name = AuthenticationHelpers.GetMachineName();
            Assert.That(name, Is.Not.Null);
            Assert.That(name, Is.Not.Empty);
        }

        [Test]
        public void CanCreateSha256Hash()
        {
            const string hello = "Hello World";
            var hash = AuthenticationHelpers.Sha256Hash(hello);
            Assert.That(hash, Is.Not.Null);
            Assert.That(hash, Is.Not.Empty);
            Assert.That(hash, Is.Not.EqualTo(hello));
            Assert.That(hash, Is.EqualTo("a591a6d40bf420404a011733cfb7b190d62c65bf0bcda32b57b277d9ad9f146e"));
        }

        [Test]
        public void CanResolveNetworkInterface()
        {
            var net = AuthenticationHelpers.GetNetworkInterface();
            Assert.That(net, Is.Not.Null);
            Assert.That(net, Is.Not.Empty);
        }
    }
}
