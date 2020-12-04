using System.Windows;

namespace SaneWpf.ExtensionMethods
{
    public static class WindowExtensions
    {
        public static T ViewModel<T>(this Window window) => (T) window.DataContext;
    }
}
