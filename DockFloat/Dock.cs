using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DockFloat
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:DockFloat"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:DockFloat;assembly=DockFloat"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    [ContentProperty("Content")]
    [TemplatePart(Name = "PART_PopOutButton", Type = typeof(ButtonBase))]
    public class Dock : Control
    {
        Window floatingWindow;

        public FrameworkElement Content
        {
            get { return (FrameworkElement)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(FrameworkElement), typeof(Dock), new PropertyMetadata((s, e) =>
            {
                var floatee = e.NewValue as FrameworkElement;
                floatee.RemoveFromParent();
                (s as Dock).AddLogicalChild(floatee);
            }));

        static Dock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Dock), new FrameworkPropertyMetadata(typeof(Dock)));

#if DEBUG
            // Avoid squiggles in the XAML designer
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;
#endif

            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            Application.Current.MainWindow.StateChanged += (s, e) =>
            {
                // Hide/show floating windows on minimize/restore
                var mainWindow = s as Window;
                var docks = mainWindow.FindLogicalChildren<Dock>();
                foreach (var dock in docks)
                    if (dock.floatingWindow != null)
                        dock.floatingWindow.Visibility = mainWindow.WindowState == WindowState.Minimized ? Visibility.Collapsed : Visibility.Visible;
            };
        }

        public override void OnApplyTemplate()
        {
            var button = GetTemplateChild("PART_PopOutButton") as Button;
            button.Click += (s, e) =>
            {
                var position = Content.PointToScreen(new Point(0, 0));
                PopOut(position.X, position.Y);
            };
        }

        private void PopOut(double x, double y)
        {
            if (Content == null) return;

            var width = Content.ActualWidth;
            var height = Content.ActualHeight;

            Content.RemoveFromParent();

            Content.Width = width;
            Content.Height = height;
            Content.HorizontalAlignment = HorizontalAlignment.Stretch;
            Content.VerticalAlignment = VerticalAlignment.Stretch;
            
            floatingWindow = new FloatWindow(Content, DockIn) { Left = x, Top = y };
            Content = null; // Triggers a binding
            floatingWindow.Show();
        }

        private void DockIn()
        {
            Content = floatingWindow.Content as FrameworkElement;
            floatingWindow.Close();
            floatingWindow = null;
        }
    }
}
