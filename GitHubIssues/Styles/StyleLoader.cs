using Alteridem.GitHub.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Alteridem.GitHub.Styles
{
    public static class StyleLoader
    {
        public static string LoadResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static string LoadCss()
        {
            var options = Factory.Get<IOptionsProvider>();
            if (options == null || options.Options == null)
                return string.Empty;

            string resource = options.Options.IssueTheme == IssueTheme.Light ? "light_theme.css" : "dark_theme.css";
            return LoadResource(string.Format("Alteridem.GitHub.Styles.{0}", resource));
        }
    }
}
