/* Note: the majority of this tool's code is taken from:
 * https://stackoverflow.com/a/6313576/9785450
 * Although some of it's issues was fixed, the fixes were 
 * suggested by different answers on the same question:
 * https://stackoverflow.com/questions/3372303/dropshadow-for-wpf-borderless-window
 * Some of those issues were mentioned in the comments of 
 * the answer itself.
 * 
 * Also, note that for this tool to work properly, the 
 * window's AllowsTransparency property need to be set to
 * "False".
 * 
 * Finally, using this tool will lead to a white border 
 * showing arround the window's client area for a moment
 * before the window itself is actually shown. The call
 * to this tool is possible after the InitiallizeComponets
 * call in the constructor of the window, but I found that
 * putting it at the end of the "Load" event of the window
 * makes that border appear for a shorter time.
 */

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace iShield.Utilities
{
    public static class DwmDropShadow
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Margins
        {
            public int Left;
            public int Right;
            public int Top;
            public int Bottom;
        }

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMarInset);

        /// <summary>
        /// Drops a standard shadow to a WPF Window, even if the window is borderless. Only works with DWM (Windows Vista or newer).
        /// This method is much more efficient than setting AllowsTransparency to true and using the DropShadow effect,
        /// as AllowsTransparency involves a huge performance issue (hardware acceleration is turned off for all the window).
        /// </summary>
        /// <param name="window">Window to which the shadow will be applied</param>
        public static void DropShadowToWindow(Window window)
        {
            if (!DropShadow(window))
            {
                window.SourceInitialized += new EventHandler(window_SourceInitialized);
            }
        }

        private static void window_SourceInitialized(object sender, EventArgs e)
        {
            Window window = (Window)sender;

            DropShadow(window);

            window.SourceInitialized -= new EventHandler(window_SourceInitialized);
        }

        /// <summary>
        /// The actual method that makes API calls to drop the shadow to the window
        /// </summary>
        /// <param name="window">Window to which the shadow will be applied</param>
        /// <returns>True if the method succeeded, false if not</returns>
        private static bool DropShadow(Window window)
        {
            try
            {
                WindowInteropHelper helper = new WindowInteropHelper(window);
                int val = 2;
                int ret1 = DwmSetWindowAttribute(helper.Handle, 2, ref val, 4);

                if (ret1 == 0)
                {
                    const int amount = 1;
                    Margins m = new Margins { Bottom = amount, Left = amount, Right = amount, Top = amount};
                    int ret2 = DwmExtendFrameIntoClientArea(helper.Handle, ref m);
                    return ret2 == 0;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Probably dwmapi.dll not found (incompatible OS)
                return false;
            }
        }
    }
}
