using DockFloat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DemoApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        void OnFloatAllClick(object sender, RoutedEventArgs e)
        {
            var docks = FindLogicalChildren<DockFloat.Dock>(this);
            foreach (var dock in docks) dock.IsFloating = true;
        }

        static IEnumerable<T> FindLogicalChildren<T>(
            DependencyObject parent) where T : DependencyObject
        {
            var dependencyChildren =
                LogicalTreeHelper.GetChildren(parent)
                                 .OfType<DependencyObject>();

            foreach (var child in dependencyChildren)
            {
                if (child is T typedChild)
                    yield return typedChild;

                foreach (T childOfChild in FindLogicalChildren<T>(child))
                    yield return childOfChild;
            }
        }
    }
}
