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
    [TemplatePart(Name = "PART_PopOutButton", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_Presenter", Type = typeof(ContentPresenter))]
    public class Dock : ContentControl
    {
        Window? floatWindow;
        ContentPresenter? presenter;


        static Dock()
            => DefaultStyleKeyProperty.OverrideMetadata(
                                   typeof(Dock),
                                   new FrameworkPropertyMetadata(typeof(Dock)));


        public bool IsButtonVisible
        {
            get => (bool)GetValue(IsButtonVisibleProperty);
            set => SetValue(IsButtonVisibleProperty, value);
        }
        public static readonly DependencyProperty IsButtonVisibleProperty =
            DependencyProperty.Register("IsButtonVisible", typeof(bool), typeof(Dock),
                new PropertyMetadata(true));

        public object Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(object), typeof(Dock),
                new PropertyMetadata());

        public bool ButtonOverlapsContent
        {
            get => (bool)GetValue(ButtonOverlapsContentProperty);
            set => SetValue(ButtonOverlapsContentProperty, value);
        }
        public static readonly DependencyProperty ButtonOverlapsContentProperty =
            DependencyProperty.Register("ButtonOverlapsContent", typeof(bool), typeof(Dock),
                new PropertyMetadata(true));

        public string WindowTitle
        {
            get => (string)GetValue(WindowTitleProperty);
            set => SetValue(WindowTitleProperty, value);
        }
        public static readonly DependencyProperty WindowTitleProperty =
            DependencyProperty.Register("WindowTitle", typeof(string), typeof(Dock),
                new PropertyMetadata(""));

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


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            presenter = (ContentPresenter)GetTemplateChild("PART_Presenter");

            var popOutButton = (Button)GetTemplateChild("PART_PopOutButton");
            popOutButton.Click += (s, e) => IsFloating = true;
        }


        void OnIsFloatingChanged()
        {
            if (IsFloating) PopOut();
            else DockIn();
        }

        void PopOut()
        {
            var content = Content;
            Content = null;
            ShowNewFloatWindow(content);
            HideTheDock();
        }

        void DockIn()
        {
            if (floatWindow is null) return;
            var content = floatWindow.Content;
            floatWindow = null;
            Content = content;
            ShowTheDock();
        }

        void ShowNewFloatWindow(object content)
        {
            floatWindow = new FloatWindow(owner: Window.GetWindow(this),
                                          content,
                                          contentAreaSize: presenter!.RenderSize,
                                          position: GetFloatWindowPosition(),
                                          Padding);
            floatWindow.Closed += (s, e) => IsFloating = false;


            void Bind(string path, DependencyProperty targetProperty)
            {
                var binding = new Binding(path) { Source = this };
                floatWindow.SetBinding(targetProperty, binding);
            }

            Bind(nameof(DataContext), DataContextProperty);
            Bind(nameof(Background), BackgroundProperty);
            Bind(nameof(Foreground), ForegroundProperty);
            Bind(nameof(Padding), PaddingProperty);
            Bind(nameof(WindowTitle), Window.TitleProperty);

            floatWindow.Show();
        }

        void HideTheDock() => Visibility = Visibility.Collapsed;
        void ShowTheDock() => Visibility = Visibility.Visible;

        Point GetFloatWindowPosition()
        {
            var positionInDock = new Point(10, 10);
            var positionInScreen = PointToScreen(positionInDock);
            positionInScreen = AccountForOSDpiScaling(positionInScreen);
            return positionInScreen;
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
