using CommunityToolkit.Mvvm.Messaging;
using DesktopTimer.Models;
using DesktopTimer.Views.Window;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DesktopTimer.Helpers
{
    public class DoubleCtrlHook:IDisposable
    {

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int VK_CONTROL = 0x11;

        public DoubleCtrlHook()
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        public void DisableHook()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private  LowLevelKeyboardProc? _proc;
        
        private  IntPtr _hookID = IntPtr.Zero;

        private  DateTime _lastCtrlPress = DateTime.MinValue;

        private const int DoubleClickTime = 500; 
       
        private  IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                if (vkCode == VK_CONTROL)
                {
                    DateTime now = DateTime.Now;

                    if ((now - _lastCtrlPress).TotalMilliseconds <= DoubleClickTime)
                    {
                        _lastCtrlPress = DateTime.MinValue;
                        WeakReferenceMessenger.Default.Send(new RequestOpenEverythingWindow());
                    }
                    else
                    {
                        _lastCtrlPress = now;
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

       

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public void Dispose()
        {
            UnhookWindowsHookEx(_hookID);
        }
    }
}
