using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    ///   Use this as a container for the UI elements that will be docked.
    ///   Note, this class has nothing to do with <see
    ///   cref="System.Windows.Controls.Dock"/> or <see cref="DockPanel"/>.
    /// </summary>
    [ContentProperty("Content")]
    [TemplatePart(Name = "PART_PopOutButton", Type = typeof(ButtonBase))]
    public class Dock : Control
    {
        Window floatWindow;
        ContentState savedContentState;


        static Dock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Dock), new FrameworkPropertyMetadata(typeof(Dock)));

            var RunninginXamlDesigner = DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (RunninginXamlDesigner) return;

            Application.Current.MainWindow.StateChanged += MinimizeOrRestoreWithMainWindow;
        }


        public FrameworkElement Content
        {
            get => (FrameworkElement)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(FrameworkElement), typeof(Dock),
                new PropertyMetadata(null));

        public bool ButtonOverlapsContent
        {
            get => (bool)GetValue(ButtonOverlapsContentProperty);
            set => SetValue(ButtonOverlapsContentProperty, value);
        }
        public static readonly DependencyProperty ButtonOverlapsContentProperty =
            DependencyProperty.Register("ButtonOverlapsContent", typeof(bool), typeof(Dock),
                new PropertyMetadata(true));


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var popOutButton = GetTemplateChild("PART_PopOutButton") as Button;
            popOutButton.Click += (s, e) => PopOut();
        }


        static void MinimizeOrRestoreWithMainWindow(object sender, EventArgs e)
        {
            var mainWindow = sender as Window;
            var floatWindows = GetAllFloatWindows(mainWindow);
            foreach (var floatWindow in floatWindows)
                floatWindow.Visibility =
                    mainWindow.WindowState == WindowState.Minimized ?
                    Visibility.Collapsed :
                    Visibility.Visible;
        }

        static IEnumerable<Window> GetAllFloatWindows(Window mainWindow) =>
            from dock in mainWindow.FindLogicalChildren<Dock>()
            where dock.floatWindow != null
            select dock.floatWindow;

        void PopOut()
        {
            SaveContentFromDock();
            AddContentToNewFloatingWindow();
            HideTheDock();
        }

        void DockIn()
        {
            floatWindow = null;
            RestoreContentToDock();
            ShowTheDock();
        }

        void SaveContentFromDock()
        {
            savedContentState = ContentState.Save(Content);
            Content = null;
        }

        void RestoreContentToDock()
        {
            Content = savedContentState.Restore();
            savedContentState = null;
        }

        void AddContentToNewFloatingWindow()
        {
            var parentWindow = Window.GetWindow(this);
            var position = GetFloatWindowPosition(parentWindow);

            floatWindow = new FloatWindow(savedContentState.FloatContent)
            {
                DataContext = DataContext,
                Left = position.X,
                Top = position.Y,
                Background = Background,
                Owner = parentWindow,
            };

            floatWindow.Closed += (s, e) => DockIn();
            floatWindow.Show();
        }

        void HideTheDock() => Visibility = Visibility.Collapsed;
        void ShowTheDock() => Visibility = Visibility.Visible;

        Point GetFloatWindowPosition(Window parentWindow)
        {
            var position = new Point(10, 10);
            position = TranslatePoint(position, parentWindow);
            position = parentWindow.PointToScreen(position);
            position = AccountForWindowDpiScaling(position);
            return position;
        }

        Point AccountForWindowDpiScaling(Point position)
        {
            var dpiScale = VisualTreeHelper.GetDpi(this);
            position.X = position.X * 96.0 / dpiScale.PixelsPerInchX;
            position.Y = position.Y * 96.0 / dpiScale.PixelsPerInchY;
            return position;
        }
    }
}
