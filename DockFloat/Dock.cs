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
        Window floatWindow;

        static Dock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Dock), new FrameworkPropertyMetadata(typeof(Dock)));

            // Avoid squiggles in the XAML designer
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;

            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            Application.Current.MainWindow.StateChanged += (s, e) =>
            {
                // Hide/show floating windows on minimize/restore of main window
                var mainWindow = s as Window;
                var docks = mainWindow.FindLogicalChildren<Dock>();
                foreach (var dock in docks)
                    if (dock.floatWindow != null)
                        dock.floatWindow.Visibility = mainWindow.WindowState == WindowState.Minimized ? Visibility.Collapsed : Visibility.Visible;
            };
        }

        public FrameworkElement Content
        {
            get { return (FrameworkElement)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(FrameworkElement), typeof(Dock), new PropertyMetadata(
                (d, e) => { (d as Dock).AddLogicalChild(e.NewValue as FrameworkElement); }
                ));

        public bool FloatButtonOverContent
        {
            get { return (bool)GetValue(FloatButtonOverContentProperty); }
            set { SetValue(FloatButtonOverContentProperty, value); }
        }
        public static readonly DependencyProperty FloatButtonOverContentProperty =
            DependencyProperty.Register("FloatButtonOverContent", typeof(bool), typeof(Dock), new PropertyMetadata(true));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var button = GetTemplateChild("PART_PopOutButton") as Button;
            button.Click += (s, e) => PopOut();
        }

        private void PopOut()
        {
            if (Content == null) return;

            Visibility = Visibility.Collapsed;

            var floatContent = Content;
            var dataContext = floatContent.DataContext;

            var horizontalAlignment = floatContent.HorizontalAlignment;
            var verticalAlignment = floatContent.VerticalAlignment;
            var width = floatContent.Width;
            var height = floatContent.Height;
            var actualWidth = floatContent.ActualWidth;
            var actualHeight = floatContent.ActualHeight;

            Content = null;
            
            // Set things we want to carry over into the floating window
            floatContent.HorizontalAlignment = HorizontalAlignment.Stretch;
            floatContent.VerticalAlignment = VerticalAlignment.Stretch;
            floatContent.Width = actualWidth;
            floatContent.Height = actualHeight;

            Action dockIn = () =>
            {
                var content = floatWindow.Content as FrameworkElement;
                TerminateFloatWindow();
                content.HorizontalAlignment = horizontalAlignment;
                content.VerticalAlignment = verticalAlignment;
                content.Width = width;
                content.Height = height;
                Content = content;
                Visibility = Visibility.Visible;
            };

            var position = PointToScreen(new Point(0, 0));
            position.X -= 10; // Offset so user knows it popped out
            position.Y -= 10;
            position.X = Math.Max(position.X, 0); // Don't go off screen
            position.Y = Math.Max(position.Y, 0);

            floatWindow = new FloatWindow(floatContent, dockIn)
            {
                DataContext = dataContext,
                Left = position.X,
                Top = position.Y,
                Background = Background,
                SizeToContent = SizeToContent.WidthAndHeight,
                Owner = Application.Current.MainWindow
            };
            floatWindow.Show();
        }

        private void TerminateFloatWindow()
        {
            floatWindow.Content = null;
            floatWindow.Close();
            floatWindow = null;
        }
    }
}
