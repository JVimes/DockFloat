using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DockFloat
{
    public class DockingManager
    {
        public static bool GetFloatable(DependencyObject obj)
        {
            return (bool)obj.GetValue(FloatableProperty);
        }
        public static void SetFloatable(DependencyObject obj, bool value)
        {
            obj.SetValue(FloatableProperty, value);
        }
        public static readonly DependencyProperty FloatableProperty =
            DependencyProperty.RegisterAttached("Floatable", typeof(bool), typeof(DockingManager),
                new PropertyMetadata((s, e) =>
                {

                }));
    }
}
