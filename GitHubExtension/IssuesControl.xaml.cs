using System.Windows;
using System.Windows.Controls;

namespace Alteridem.GitHub.Extension
{
    /// <summary>
    /// Interaction logic for MyControl.xaml
    /// </summary>
    public partial class IssuesControl : UserControl
    {
        public IssuesControl()
        {
            InitializeComponent();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(string.Format(System.Globalization.CultureInfo.CurrentUICulture, "We are inside {0}.button1_Click()", this.ToString()),
                            "GitHub Issues");

        }
    }
}