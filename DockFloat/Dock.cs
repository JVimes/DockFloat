﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
    public class Dock : Control
    {
        static Window floatingWindow;

        public FrameworkElement Content
        {
            get { return (FrameworkElement)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(FrameworkElement), typeof(Dock), new PropertyMetadata((s, e) =>
            {
                (s as Dock).AddLogicalChild(e.NewValue);
            }));

        static Dock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Dock), new FrameworkPropertyMetadata(typeof(Dock)));

#if DEBUG
            // Avoid squiggels in the XAML designer
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;
#endif

            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            Application.Current.MainWindow.StateChanged += (s, e) =>
            {
                if (floatingWindow == null) return;
                var mainWindow = s as Window;
                floatingWindow.Visibility = mainWindow.WindowState == WindowState.Minimized ? Visibility.Collapsed : Visibility.Visible;
            };
        }

        public override void OnApplyTemplate()
        {
            var button = GetTemplateChild("popOutButton") as Button;
            button.Click += (s, e) =>
            {
                PopOut(Content);
                Content = null; // Triggers binding
            };
        }

        private static void PopOut(FrameworkElement floatee)
        {
            if (floatee == null) return;

            var width = floatee.ActualWidth;
            var height = floatee.ActualHeight;
            var parent = floatee.Parent;

            floatee.DetachFromParent();

            floatee.Width = width;
            floatee.Height = height;
            floatee.HorizontalAlignment = HorizontalAlignment.Stretch;
            floatee.VerticalAlignment = VerticalAlignment.Stretch;

            var window = new Window()
            {
                MinWidth = 200,
                WindowStyle = WindowStyle.ToolWindow,
                ShowInTaskbar = false,
                SizeToContent = SizeToContent.WidthAndHeight,
                Content = floatee,
            };
            window.Loaded += (s2, e2) =>
            {
                window.SizeToContent = SizeToContent.Manual;
                floatee.Width = double.NaN;
                floatee.Height = double.NaN;
                floatingWindow?.Close();
                floatingWindow = window;
            };
            window.Closed += (s, e) => PopIn(s as Window);
            window.Show();
        }

        private static void PopIn(Window window)
        {
            var floatee = window.Content as FrameworkElement;
        }
    }
}
