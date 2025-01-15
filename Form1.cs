using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using ImWorkin.Properties;

namespace ImWorkin
{
    public partial class Form1 : Form
    {
        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_SYSTEM_REQUIRED = 0x00000001,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000
        }
        public SYSTEMTIMEOUTS TimeOuts
        {
            get { return sysTimeouts; }
        }
        public struct SYSTEMTIMEOUTS
        {
            public int BATTERYIDLETIMEOUT;
            public int EXTERNALIDLETIMEOUT;
            public int WAKEUPIDLETIMEOUT;
        }

        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE flags);

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "SystemParametersInfo")]
        internal static extern int SystemParametersInfo(int uiAction, int uiParam, ref int pvParam, int fWinIni);

        private static System.Threading.Timer preventSleepTimer = null;
        public const int SPI_GETBATTERYIDLETIMEOUT = 252;
        public const int SPI_GETEXTERNALIDLETIMEOUT = 254;
        public const int SPI_GETWAKEUPIDLETIMEOUT = 256;
        public static int Counter = 0;
        public static int timeOutinMS = 0;
        public static int batteryIdleTimer;
        public static int externalIdleTimer;
        public static int wakeupIdleTimer;
        public static SYSTEMTIMEOUTS sysTimeouts;



        private NotifyIcon trayIcon;

        public Form1()
        {
            InitializeComponent();

            trayIcon = new NotifyIcon();
            trayIcon.Text = "ImWorking";
            // trayIcon.Icon = Resources.AppIcon; // Resouses new Icon(SystemIcons.Application.);
            trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);
            trayIcon.ContextMenu = new ContextMenu();
            trayIcon.ContextMenu.MenuItems.Add(new MenuItem("Exit", new EventHandler(exitToolStripMenuItem_Click))); 

            trayIcon.Visible = true;

            DisableDeviceSleep();


            WindowState = FormWindowState.Minimized;
            Hide();


        }

        private void button1_Click(object sender, EventArgs e)
        {
            GetSystemTimeOuts();

            if (timeOutinMS < sysTimeouts.BATTERYIDLETIMEOUT)
                timeOutinMS = sysTimeouts.BATTERYIDLETIMEOUT;
            if (timeOutinMS < sysTimeouts.EXTERNALIDLETIMEOUT)
                timeOutinMS = sysTimeouts.EXTERNALIDLETIMEOUT;
            if (timeOutinMS < sysTimeouts.WAKEUPIDLETIMEOUT)
                timeOutinMS = sysTimeouts.WAKEUPIDLETIMEOUT;

            if (timeOutinMS <= 0)
                timeOutinMS = 30;

            DisableDeviceSleep();



        }
        private static void GetSystemTimeOuts()
        {
            sysTimeouts.BATTERYIDLETIMEOUT = -2;
            sysTimeouts.EXTERNALIDLETIMEOUT = -2;
            sysTimeouts.WAKEUPIDLETIMEOUT = -2;


            if (SystemParametersInfo(SPI_GETBATTERYIDLETIMEOUT, 0, ref batteryIdleTimer, 0) == 1)
                sysTimeouts.BATTERYIDLETIMEOUT = batteryIdleTimer;
            else
                sysTimeouts.BATTERYIDLETIMEOUT = -1;

            if (SystemParametersInfo(SPI_GETEXTERNALIDLETIMEOUT, 0, ref externalIdleTimer, 0) == 1)
                sysTimeouts.EXTERNALIDLETIMEOUT = externalIdleTimer;
            else
                sysTimeouts.EXTERNALIDLETIMEOUT = -1;



            if (SystemParametersInfo(SPI_GETWAKEUPIDLETIMEOUT, 0, ref wakeupIdleTimer, 0) == 1)
                sysTimeouts.WAKEUPIDLETIMEOUT = wakeupIdleTimer;
            else
                sysTimeouts.WAKEUPIDLETIMEOUT = -1;


        }
        private static void DisableDeviceSleep()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_AWAYMODE_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
            preventSleepTimer = new System.Threading.Timer(new TimerCallback(PokeDeviceToKeepAwake), null, 0, timeOutinMS * 1000);
        }
        private static void PokeDeviceToKeepAwake(object extra)
        {

            Counter++;
            try
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
                IntPtr Handle = FindWindow("SysListView32", "FolderView");

                if (Handle == IntPtr.Zero)
                {
                    SetForegroundWindow(Handle);
                    SendKeys.SendWait("%1");
                }

                if (Counter > 1)
                    Console.Clear();
            }
            catch
            {

            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            trayIcon.Dispose();
            Application.Exit();
        }
    }
}
