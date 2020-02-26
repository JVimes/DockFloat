using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DockFloat
{
    static class Utils
    {
        /// <summary>
        ///   When parent window is restored from minimized, WPF does not
        ///   maximize previously maximized child windows. This makes that
        ///   happen.
        /// </summary>
        internal static void FixChildWindowRestoreToMaximize(Window parentWindow)
        {
            static void OnStateChanged(object? sender, EventArgs e)
            {
                var parent = sender as Window;
                var state = parent?.WindowState;
                if (parent == null || state == WindowState.Minimized)
                    return;

                var maximizedChildren =
                    from child in parent.OwnedWindows.OfType<Window>()
                    where child.WindowState == WindowState.Maximized
                    select child;

                foreach (var child in maximizedChildren)
                {
                    Action maximize =
                        () => child.WindowState = WindowState.Maximized;
                    child.Dispatcher.BeginInvoke(maximize);
                }
            }

            parentWindow.StateChanged -= OnStateChanged; // Don't subscribe more than once
            parentWindow.StateChanged += OnStateChanged;
        }
    }
}
