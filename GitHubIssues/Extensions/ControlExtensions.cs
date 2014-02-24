using System.Windows;
using System.Windows.Media;
using Alteridem.GitHub.Annotations;

namespace Alteridem.GitHub.Extensions
{
    public static class ControlExtensions
    {
        [CanBeNull]
        public static Window GetParentWindow( [NotNull] this DependencyObject child )
        {
            DependencyObject parent = VisualTreeHelper.GetParent( child );
            if ( parent == null )
                return null;

            var window = parent as Window;
            if ( window != null )
                return window;

            return parent.GetParentWindow();
        }
    }
}