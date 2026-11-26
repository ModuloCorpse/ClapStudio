using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System.Runtime.InteropServices;
using WinUI.Dock;

namespace ClapStudio
{
    public partial class WindowHelper
    {
        private const uint WM_CLOSE = 0x0010;

        [LibraryImport("user32.dll", EntryPoint = "PostMessageW", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        public static void CloseDocument(Document document, DockManager mainDockManager)
        {
            DependencyObject? current = document.Parent ?? document.Owner;
            DocumentGroup? group = (current is DocumentGroup currentDocumentGroup) ? currentDocumentGroup : null;
            while (current != null)
            {
                if (current is DocumentGroup documentGroup)
                {
                    group = documentGroup;
                    break;
                }
                current = VisualTreeHelper.GetParent(current);
            }

            if (group == null)
                return;

            bool isDockedInMain = false;
            DependencyObject? ancestor = group;
            while (ancestor != null)
            {
                if (ReferenceEquals(ancestor, mainDockManager))
                {
                    isDockedInMain = true;
                    break;
                }
                ancestor = VisualTreeHelper.GetParent(ancestor);
            }
            if (!isDockedInMain && group.Children.Count == 1 &&
                group.XamlRoot?.ContentIslandEnvironment is { } env)
            {
                IntPtr hwnd = Win32Interop.GetWindowFromWindowId(env.AppWindowId);
                if (hwnd != IntPtr.Zero)
                {
                    PostMessage(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                    return;
                }
            }
            group.Children.Remove(document);
        }
    }
}
