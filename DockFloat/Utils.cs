using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DockFloat
{
    public static class Utils
    {
        public static IEnumerable<Dock> GetDocks(this Window window)
            => window.FindLogicalChildren<Dock>();

        static IEnumerable<T> FindLogicalChildren<T>(
            this DependencyObject parent) where T : DependencyObject
        {
            var dependencyChildren =
                LogicalTreeHelper.GetChildren(parent)
                                 .OfType<DependencyObject>();

            foreach (var child in dependencyChildren)
            {
                if (child is T typedChild)
                    yield return typedChild;

                foreach (T childOfChild in FindLogicalChildren<T>(child))
                    yield return childOfChild;
            }
        }
    }
}
