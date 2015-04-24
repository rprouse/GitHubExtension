using Alteridem.GitHub.Interfaces;
using Alteridem.GitHub.Styles;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alteridem.GitHub.Extension.Test.Styles
{
    [TestFixture]
    public class StyleLoaderTests
    {
        [TestCase("Alteridem.GitHub.Styles.light_theme.css")]
        public void CanLoadResource(string resourceName)
        {
            string css = StyleLoader.LoadResource(resourceName);
            Assert.That(css, Is.Not.Null);
            Assert.That(css, Is.Not.Empty);
            Assert.That(css, Does.Contain("body").IgnoreCase);
        }

        [Test]
        public void CanLoadCss([Values]IssueTheme theme)
        {
            var options = Factory.Get<IOptionsProvider>();
            Assert.That(options, Is.Not.Null);
            Assert.That(options.Options, Is.Not.Null);

            options.Options.IssueTheme = theme;

            string css = StyleLoader.LoadCss();
            Assert.That(css, Is.Not.Null);
            Assert.That(css, Is.Not.Empty);
            Assert.That(css, Does.Contain("body").IgnoreCase);
        }
    }
}
