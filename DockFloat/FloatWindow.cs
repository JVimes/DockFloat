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

        internal FloatWindow(ContentState contentState)
        {
            Initialized += (s, e) =>
            {
                SetSize(contentState);
                Content = contentState.FloatContent;
            };
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var dockInButton = GetTemplateChild("PART_DockButton") as Button;
            if (dockInButton != null)
                dockInButton.Click += (s, e) => Close();
        }


        void SetSize(ContentState contentState)
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

            Width = contentState.Width + horizontalChrome;
            Height = contentState.Height + verticalChrome;
        }
    }
}
