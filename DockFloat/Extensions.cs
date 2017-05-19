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
        public static void DetachFromParent(this FrameworkElement element)
        {
            var parent = element.Parent as UIElement;
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
    }
}
