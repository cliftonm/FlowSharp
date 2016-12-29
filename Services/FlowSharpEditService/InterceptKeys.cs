using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using Clifton.Core.ExtensionMethods;

namespace FlowSharpEditService
{
    public class KeyMessageEventArgs : EventArgs
    {
        public enum KeyState
        {
            KeyUp,
            KeyDown,
        }

        public KeyState State { get; set; }
        public int KeyCode { get; set; }
    }

    /// <summary>
    /// Low level keyboard hook in C#
    /// https://blogs.msdn.microsoft.com/toub/2006/05/03/low-level-keyboard-hook-in-c/
    /// </summary>
    public class InterceptKeys : IDisposable
    {
        public EventHandler<KeyMessageEventArgs> KeyboardEvent;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private LowLevelKeyboardProc proc;
        private IntPtr hookID = IntPtr.Zero;

        public void Initialize()
        {
            proc = HookCallback;
            hookID = SetHook(proc);
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(hookID);
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
                }
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                // Console.WriteLine((Keys)vkCode);
                KeyboardEvent.Fire(this, new KeyMessageEventArgs() { State = KeyMessageEventArgs.KeyState.KeyDown, KeyCode = vkCode });
            }
            else if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                // Console.WriteLine((Keys)vkCode);
                KeyboardEvent.Fire(this, new KeyMessageEventArgs() { State = KeyMessageEventArgs.KeyState.KeyUp, KeyCode = vkCode });
            }

            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }
    }
}