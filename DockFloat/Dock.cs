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
    ///   Use this as a container for a UI element that will be docked/floated.
    ///   <br/> This class is unrelated to <see
    ///   cref="System.Windows.Controls.Dock"/> or <see cref="DockPanel"/>.
    /// </summary>
    [ContentProperty("Content")]
    [TemplatePart(Name = "PART_PopOutButton", Type = typeof(ButtonBase))]
    public class Dock : Control
    {
        static readonly bool runninginXamlDesigner =
            DesignerProperties.GetIsInDesignMode(new DependencyObject());

        Window? floatWindow;
        ContentState? savedContentState;


        static Dock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Dock), new FrameworkPropertyMetadata(typeof(Dock)));
        }

        public Dock()
        {
            if (!runninginXamlDesigner)
                FixChildWindowRestoreToMaximize();
        }


        public object Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(object), typeof(Dock),
                new PropertyMetadata());

        public FrameworkElement? Content
        {
            get => (FrameworkElement?)GetValue(ContentProperty);
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

        public bool IsFloating
        {
            get => (bool)GetValue(IsFloatingProperty);
            set => SetValue(IsFloatingProperty, value);
        }
        public static readonly DependencyProperty IsFloatingProperty =
            DependencyProperty.Register("IsFloating", typeof(bool), typeof(Dock),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => (d as Dock)?.OnIsFloatingChanged()));

        public string WindowTitle
        {
            get => (string)GetValue(WindowTitleProperty);
            set => SetValue(WindowTitleProperty, value);
        }
        public static readonly DependencyProperty WindowTitleProperty =
            DependencyProperty.Register("WindowTitle", typeof(string), typeof(Dock),
                new PropertyMetadata((d, e) => (d as Dock)?.OnWindowTitleChanged()));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var popOutButton = GetTemplateChild("PART_PopOutButton") as Button;
            if (popOutButton != null)
                popOutButton.Click += (s, e) => IsFloating = true;
        }


        void FixChildWindowRestoreToMaximize()
        {
            void OnLoaded(object sender, RoutedEventArgs e)
            {
                Loaded -= OnLoaded;
                var parentWindow = Window.GetWindow(this);
                Utils.FixChildWindowRestoreToMaximize(parentWindow);
            }
            Loaded += OnLoaded; // Using Loaded. Can't use Initialized because parent window was null in one case.
        }

        void OnIsFloatingChanged()
        {
            if (IsFloating) PopOut();
            else DockIn();
        }

        void OnWindowTitleChanged()
        {
            if (floatWindow != null)
                floatWindow.Title = WindowTitle;
        }

        void PopOut()
        {
            SaveContentFromDock();
            AddContentToNewFloatingWindow();
            HideTheDock();
            IsFloating = true;
        }

        void DockIn()
        {
            if (floatWindow != null)
            {
                floatWindow.Close();
                floatWindow = null;
            }

            RestoreContentToDock();
            ShowTheDock();
            IsFloating = false;
        }

        void SaveContentFromDock()
        {
            if (Content == null) return;

            savedContentState = ContentState.Save(Content);
            Content = null;
        }

        void RestoreContentToDock()
        {
            Content = savedContentState?.Restore();
            savedContentState = null;
        }

        void AddContentToNewFloatingWindow()
        {
            if (savedContentState == null) return;

            var parentWindow = Window.GetWindow(this);
            var position = GetFloatWindowPosition(parentWindow);

            floatWindow = new FloatWindow(savedContentState.FloatContent)
            {
                Title = WindowTitle ?? "",
                DataContext = DataContext,
                Left = position.X,
                Top = position.Y,
                Background = Background,
                Foreground = Foreground,
                Padding = Padding,
                Owner = parentWindow,
            };

            floatWindow.Closed += (s, e) => IsFloating = false;
            floatWindow.Show();
        }

        void HideTheDock() => Visibility = Visibility.Collapsed;
        void ShowTheDock() => Visibility = Visibility.Visible;

        Point GetFloatWindowPosition(Window parentWindow)
        {
            var position = new Point(10, 10);
            position = TranslatePoint(position, parentWindow);
            position = parentWindow.PointToScreen(position);
            position = AccountForOSDpiScaling(position);
            return position;
        }

        Point AccountForOSDpiScaling(Point position)
        {
            var dpiScale = VisualTreeHelper.GetDpi(this);
            position.X = position.X * 96.0 / dpiScale.PixelsPerInchX;
            position.Y = position.Y * 96.0 / dpiScale.PixelsPerInchY;
            return position;
        }
    }
}
