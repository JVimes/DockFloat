﻿using System;
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
        Window floatingWindow;
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


        Window ParentWindow { get => Window.GetWindow(this); }


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
            where dock.floatingWindow != null
            select dock.floatingWindow;

        void PopOut()
        {
            SaveContentFromDock();
            AddContentToNewFloatingWindow();
            HideTheDock();
        }

        void DockIn()
        {
            floatingWindow = null;
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
            var position = GetPopupPosition();

            floatingWindow = new FloatWindow(savedContentState.FloatContent)
            {
                DataContext = DataContext,
                Left = position.X,
                Top = position.Y,
                Background = Background,
                Owner = ParentWindow,
            };
            floatingWindow.Loaded += (s, e) =>
            {
                floatingWindow.Width = savedContentState.ActualWidth;
                floatingWindow.Height = savedContentState.ActualHeight;
            };
            floatingWindow.Closed += (s, e) => DockIn();
            floatingWindow.Show();
        }

        void HideTheDock() => Visibility = Visibility.Collapsed;
        void ShowTheDock() => Visibility = Visibility.Visible;

        Point GetPopupPosition()
        {
            var dockPosition = PointToScreen(new Point(0, 0));
            var position = new Point(dockPosition.X - 20, dockPosition.Y - 20);
            position.X = Math.Max(position.X, 0);
            position.Y = Math.Max(position.Y, 0);
            return position;
        }
    }
}
