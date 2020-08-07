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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;

namespace DockFloat
{
    [TemplatePart(Name = "PART_DockButton", Type = typeof(ButtonBase))]
    public class FloatWindow : Window
    {
        static FloatWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FloatWindow), new FrameworkPropertyMetadata(typeof(FloatWindow)));
        }

        internal FloatWindow(object content,
                             Size contentAreaSize,
                             Window owner)
        {
            Owner = owner;

            Initialized += (s, e) =>
            {
                SetSize(contentAreaSize);
                Content = content;
            };

            FixRestoreToMaximize();
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var dockInButton = GetTemplateChild("PART_DockButton") as Button;
            if (dockInButton != null)
                dockInButton.Click += (s, e) => Close();
        }

        /// <summary>
        ///   When owner window is restored from minimized, maximize <see
        ///   cref="FloatWindow"/> if it was previously maximized.
        /// </summary>
        void FixRestoreToMaximize()
        {
            void OnOwnerStateChanged(object? sender, EventArgs e)
            {
                if (Owner.WindowState is WindowState.Minimized
                    || WindowState != WindowState.Maximized)
                    return;

                Action maximize = () => WindowState = WindowState.Maximized;
                Dispatcher.BeginInvoke(maximize);
            }

            Owner.StateChanged += OnOwnerStateChanged;
            Closed += (s, e) => Owner.StateChanged -= OnOwnerStateChanged;
        }

        void SetSize(Size contentAreaSize)
        {
            var activeWindowBorder = 2; // Accounts for active-window highlighting border from FloatWindow.xaml
            var windowChrome = WindowChrome.GetWindowChrome(this);
            var verticalFudge = -1; // Wish I knew where this was coming from

            var horizontalChrome = activeWindowBorder
                                   + Padding.Left
                                   + Padding.Right;
            var verticalChrome = windowChrome.CaptionHeight
                                 + windowChrome.ResizeBorderThickness.Top
                                 + activeWindowBorder
                                 + verticalFudge
                                 + Padding.Top
                                 + Padding.Bottom;

            Width = contentAreaSize.Width + horizontalChrome;
            Height = contentAreaSize.Height + verticalChrome;
        }
    }
}
