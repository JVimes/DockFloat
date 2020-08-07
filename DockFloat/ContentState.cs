using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DockFloat
{
    class ContentState
    {
        ContentState(object content, double width, double height)
        {
            FloatContent = content;

            Width = width;
            Height = height;
        }

        internal object FloatContent { get; }
        internal double Width { get; }
        internal double Height { get; }

        internal static ContentState Save(object content, double width, double height)
            => new ContentState(content, width, height);

        internal object Restore() => FloatContent;
    }
}
