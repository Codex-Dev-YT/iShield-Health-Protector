using System;
using System.Runtime.InteropServices;

namespace iShield.Classes
{
    public static unsafe class Win32Methods
    {
        private const string GDI32 = "gdi32.dll";

        [DllImport(GDI32, EntryPoint = "CreateDC")]
        public static extern IntPtr CreateDC(
            string lpszDriver,
            string lpszDevice,
            string lpszOutput,
            IntPtr lpInitData
        );

        [DllImport(GDI32, EntryPoint = "DeleteDC")]
        public static extern bool DeleteDC(IntPtr hDc);

        // This native method expects a byte array as its second argument
        // but to make things easier, I made a custom type that is easier 
        // to deal with while still has the same runtime representation as
        // the byte array required by the native method. I then set that 
        // type as the parameter type of the native method.
        [DllImport(GDI32, EntryPoint = "SetDeviceGammaRamp")]
        public static extern bool SetDeviceGammaRamp(IntPtr hDc, ref ScreenManager.GammaRampProfile lpRamp);

    }
}
