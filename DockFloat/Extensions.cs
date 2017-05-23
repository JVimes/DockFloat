using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DockFloat
{
    static class Extensions
    {
        public static void RemoveFromParent(this FrameworkElement element)
        {
            var parent = element?.Parent as UIElement;

            if (parent == null) return;

            switch (parent)
            {
                case Panel panel:
                    panel.Children.Remove(element);
                    break;
                case Decorator decorator:
                    decorator.Child = null;
                    break;
                case ContentPresenter presenter:
                    presenter.Content = null;
                    break;
                case ContentControl control:
                    control.Content = null;
                    break;
            }
            parent?.InvalidateArrange();
        }

        public static IEnumerable<T> FindLogicalChildren<T>(this DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) yield return null;

            var dependencyChildren = LogicalTreeHelper.GetChildren(parent).OfType<DependencyObject>();
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
