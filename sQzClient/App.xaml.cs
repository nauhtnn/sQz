using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace sQzClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Members

        /// <summary>
        /// Event hook types
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/ms644959%28v=vs.85%29.aspx
        /// </summary>
        public static class EventHookTypes
        {
            public static readonly int WH_MSGFILTER = -1;
            public static readonly int WH_JOURNALRECORD = 0;
            public static readonly int WH_JOURNALPLAYBACK = 1;
            public static readonly int WH_KEYBOARD = 2;
            public static readonly int WH_GETMESSAGE = 3;
            public static readonly int WH_CALLWNDPROC = 4;
            public static readonly int WH_CBT = 5;
            public static readonly int WH_SYSMSGFILTER = 6;
            public static readonly int WH_MOUSE = 7;
            public static readonly int WH_HARDWARE = 8;
            public static readonly int WH_DEBUG = 9;
            public static readonly int WH_SHELL = 10;
            public static readonly int WH_FOREGROUNDIDLE = 11;
            public static readonly int WH_CALLWNDPROCRET = 12;
            public static readonly int WH_KEYBOARD_LL = 13;
            public static readonly int WH_MOUSE_LL = 14;
        }

        // Key states
        const int KEYSTATE_NONE = 0;
        const int KEYSTATE_NOTPRESSED_TOGGLED = 1;
        const int KEYSTATE_PRESSED_TOGGLED = -128;
        const int KEYSTATE_PRESSED_NOT_TOGGLED = -127;

        /// <summary>
        /// Contains information about a low-level keyboard input event.
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/ms644967%28v=vs.85%29.aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct KBLLHOOKSTRUCT
        {
            public int KeyCode;
            public int ScanCode;
            public int Flags;
            public int Time;
            public int ExtraInfo;
        }

        private static int _hookHandle;
        private static HookProc _altTabCallBack;

        #endregion

        #region Methods

        /// <summary>
                /// Sets a keyboard Alt+Tab hook.
                /// </summary>
        private static void SetAltTabHook()
        {
            // Set reference to callback
            _altTabCallBack = AltTabProcessor;

            // Set system-wide hook.
            _hookHandle = SetWindowsHookEx(EventHookTypes.WH_KEYBOARD_LL,
                _altTabCallBack, IntPtr.Zero, 0);
        }

        /// <summary>
                /// The processor for Alt+Tab key presses.
                /// </summary>
                /// <param name="code">The code.</param>
                /// <param name="wParam">The w parameter.</param>
                /// <param name="lParam">The l parameter.</param>
                /// <returns></returns>
        private static int AltTabProcessor(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code >= 0)
            {
                var hookStruct = (KBLLHOOKSTRUCT)Marshal.PtrToStructure(
                lParam, typeof(KBLLHOOKSTRUCT));

                bool bAlt = GetKeyState(0x12) == KEYSTATE_PRESSED_NOT_TOGGLED ||
                    GetKeyState(0x12) == KEYSTATE_PRESSED_TOGGLED;
                if (bAlt && (hookStruct.KeyCode == 0x09 || hookStruct.KeyCode == 0x73))
                    //pressed
                    return 1;
                //else //released
                //    bool bCtrl = GetKeyState(0x11) == KEYSTATE_PRESSED_NOT_TOGGLED ||
                //        GetKeyState(0x11) == KEYSTATE_PRESSED_TOGGLED;
                if (hookStruct.KeyCode == 0x1b)
                    return 1;
                if (hookStruct.KeyCode == 0x5b || hookStruct.KeyCode == 0x5c)
                    return 1;
                bool bWind = GetKeyState(0x5b) == KEYSTATE_PRESSED_NOT_TOGGLED ||
                    GetKeyState(0x5b) == KEYSTATE_PRESSED_TOGGLED ||
                    GetKeyState(0x5c) == KEYSTATE_PRESSED_NOT_TOGGLED ||
                    GetKeyState(0x5c) == KEYSTATE_PRESSED_TOGGLED;
                if (bWind && hookStruct.KeyCode == 0x4c)
                    return 1;
            }

            // Pass to other keyboard handlers. This allows other applications with hooks to 
            // get the notification.
            return CallNextHookEx(_hookHandle, code, wParam, lParam);
        }

        #endregion

        #region Events

        /// <summary>
                /// Raises the <see cref="E:System.Windows.Application.Startup" /> event.
                /// </summary>
                /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs" /> that contains the 
                /// event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // Startup
            SetAltTabHook();

            // Base
            base.OnStartup(e);
        }

        /// <summary>
                /// Raises the <see cref="E:System.Windows.Application.Exit" /> event.
                /// </summary>
                /// <param name="e">An <see cref="T:System.Windows.ExitEventArgs" /> that contains the 
                /// event data.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            // Unhook
            UnhookWindowsHookEx(_hookHandle);

            // Base
            base.OnExit(e);
        }

        #endregion

        #region Native Methods

        /// <summary>
                /// Hook Processor.
                /// </summary>
                /// <param name="code">The code.</param>
                /// <param name="wParam">The w parameter.</param>
                /// <param name="lParam">The l parameter.</param>
                /// <returns></returns>
        public delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);

        /// <summary>
                /// Sets a windows hook.
                /// </summary>
                /// <param name="hookId">The hook identifier.</param>
                /// <param name="processor">The processor.</param>
                /// <param name="moduleInstance">The module instance.</param>
                /// <param name="threadId">The thread identifier.</param>
                /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int hookId, HookProc processor, IntPtr moduleInstance, int threadId);

        /// <summary>
                /// Unsets a windows hook.
                /// </summary>
                /// <param name="hookId">The hook identifier.</param>
                /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int hookId);

        /// <summary>
                /// Calls the next hook.
                /// </summary>
                /// <param name="hookId">The hook identifier.</param>
                /// <param name="code">The code.</param>
                /// <param name="wParam">The w parameter.</param>
                /// <param name="lParam">The l parameter.</param>
                /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int hookId, int code, IntPtr wParam, IntPtr lParam);

        /// <summary>
                /// Gets the state of a virtual key.
                /// </summary>
                /// <param name="virtualKey">The virtual key.</param>
                /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern short GetKeyState(int virtualKey);

        #endregion

        protected override void OnDeactivated(EventArgs e)
        {
            //MainWindow.Topmost = true;
            base.OnDeactivated(e);
        }
    }
}
