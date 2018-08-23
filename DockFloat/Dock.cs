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
            Application.Current.MainWindow.StateChanged += SyncMainWindowStateToPopUp;
        }

        private static void SyncMainWindowStateToPopUp(object sender, EventArgs e)
        {
            var mainWindow = sender as Window;
            var docksWithFloatWindows = from dock in mainWindow.FindLogicalChildren<Dock>()
                                        where dock.floatWindow != null
                                        select dock;
            foreach (var dock in docksWithFloatWindows)
                dock.floatWindow.Visibility =
                    mainWindow.WindowState == WindowState.Minimized ?
                    Visibility.Collapsed :
                    Visibility.Visible;
        }

        public FrameworkElement Content
        {
            get { return (FrameworkElement)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(FrameworkElement), typeof(Dock),
                new PropertyMetadata(MakeContentLogicalChild));

        private static void MakeContentLogicalChild(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dock = d as Dock;
            var newContent = e.NewValue as FrameworkElement;
            dock.AddLogicalChild(newContent);
        }

        public bool ButtonOverlapsContent
        {
            get { return (bool)GetValue(ButtonOverlapsContentProperty); }
            set { SetValue(ButtonOverlapsContentProperty, value); }
        }
        public static readonly DependencyProperty ButtonOverlapsContentProperty =
            DependencyProperty.Register("ButtonOverlapsContent", typeof(bool), typeof(Dock), new PropertyMetadata(true));

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

            // Set properties we want to persist in the floating window
            floatContent.HorizontalAlignment = HorizontalAlignment.Stretch;
            floatContent.VerticalAlignment = VerticalAlignment.Stretch;
            floatContent.Width = actualWidth;
            floatContent.Height = actualHeight;

            void dockIn()
            {
                var content = floatWindow.Content as FrameworkElement;
                TerminateFloatWindow();
                content.HorizontalAlignment = horizontalAlignment;
                content.VerticalAlignment = verticalAlignment;
                content.Width = width;
                content.Height = height;
                Content = content;
                Visibility = Visibility.Visible;
            }

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
