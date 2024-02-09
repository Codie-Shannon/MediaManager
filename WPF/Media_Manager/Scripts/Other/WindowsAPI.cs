using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Media_Manager
{
    public class WindowsAPI
    {
        #region DLL Import
        #region DLL
        // Set Foreground Window
        // =================================================
        // =================================================
        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hwnd);


        // Get Foreground Window
        // =================================================
        // =================================================
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();


        // Set Window Long
        // =================================================
        // =================================================
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);


        // Set Window Position
        // =================================================
        // =================================================
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);
        #endregion DLL



        #region Flags
        // Set Window Long
        // =================================================
        // =================================================
        public const int GWL_EX_STYLE = -20, WS_EX_APPWINDOW = 0x00040000, WS_EX_TOOLWINDOW = 0x00000080;


        // Set Window Position
        // =================================================
        // =================================================
        public const int HWND_TOPMOST = -1, HWND_NOTOPMOST = -2, SWP_NOMOVE = 0x0002, SWP_NOSIZE = 0x0001;
        #endregion Flags
        #endregion DLL Import




        // Variables
        // ==============================================================================
        // ==============================================================================
        private static IntPtr activeWindow;
        private static bool isMainWindowActive;




        #region Methods
        // Active Window
        // =================================================
        // =================================================
        // =================================================
        // Toggle Active Window
        // =================================================
        // =================================================
        public static void ToggleActiveWindow(bool istopmost)
        {
            //Run Task on UI Thread
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                //Validate Toggle
                if ((!istopmost && isMainWindowActive) || (istopmost && activeWindow == new WindowInteropHelper(Application.Current.MainWindow).Handle))
                {
                    //Toggle isMainWindowActive Boolean
                    isMainWindowActive = istopmost;

                    //Toggle Main Window Topmost
                    Application.Current.MainWindow.Topmost = istopmost;

                    //Get Active Window
                    if (istopmost) { GetActiveWindow(new WindowInteropHelper(Application.Current.MainWindow).Handle); }
                }
                else
                {
                    //Get Active Window
                    if (istopmost) { GetActiveWindow(); }

                    //Toggle Window Topmost
                    SetWindowPos(activeWindow, istopmost ? HWND_TOPMOST : HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
                }
            }));
        }


        // Get Active Window
        // =================================================
        // =================================================
        public static void GetActiveWindow(IntPtr window = new IntPtr()) { activeWindow = window == new IntPtr() ? GetForegroundWindow() : window; }



        // Foreground Window
        // =================================================
        // =================================================
        // =================================================
        public static void SetWindowFocus(IntPtr windowhandle = new IntPtr()) { Application.Current.Dispatcher.Invoke(new Action(() => { SetForegroundWindow(windowhandle == new IntPtr() ? activeWindow : windowhandle); })); }



        // Chrome Driver
        // =================================================
        // =================================================
        // =================================================
        public static void HideChromeDriverWindows()
        {
            //Get Chrome Driver Windows
            List<Process> processes = RetrieveProcesses("Welcome to Chrome");

            //Loop through Chrome Driver Processes
            foreach (Process process in processes)
            {
                //Change Chrome Driver Window Styles to Tool Window Style
                Application.Current.Dispatcher.Invoke(new Action(() => { SetWindowLong(process.MainWindowHandle, GWL_EX_STYLE, WS_EX_TOOLWINDOW); }));
            }
        }



        // Processes
        // =================================================
        // =================================================
        // =================================================
        public static List<Process> RetrieveProcesses(string title_contains) { return Process.GetProcesses().Where(x => x.MainWindowTitle.Contains(title_contains)).ToList(); }
        #endregion Methods
    }
}