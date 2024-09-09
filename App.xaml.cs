using CefSharp;
using CefSharp.Wpf;
using DesktopTimer.Helpers;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace DesktopTimer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void AddTraceListener()
        {
            string logfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "/logs", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff") + ".log");
            if (!Directory.Exists(Path.GetDirectoryName(logfile)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logfile));
            }
            StreamWriter writer = new StreamWriter(logfile, false);
            TextWriterTraceListener listener = new TextWriterTraceListener(writer, "log");
            listener.TraceOutputOptions = TraceOptions.Timestamp;
            Trace.Listeners.Add(listener);
            Trace.AutoFlush = true;
        }

        private static Semaphore? semaphore = null;

        private static readonly bool DebuggingSubProcess = Debugger.IsAttached;

        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        void InitSubProcess()
        {
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;
            string strMenu = AppDomain.CurrentDomain.BaseDirectory;
            Unosquare.FFME.Library.FFmpegDirectory = strMenu+@"Assets";

            //pepflashplayerDLL 地址
            string flashPath = strMenu + @"Assets\pepflashplayer64_34_0_0_211.dll";

            DeleteCefBrowserCache();
            var setting = new CefSettings();
            setting.RootCachePath = FileMapper.CefBrowserDataDir;
            setting.PersistSessionCookies = true;
            setting.LogFile = FileMapper.CefBrowserLogPath;
            setting.CachePath = FileMapper.CefBrowserCacheDir;
            setting.PersistUserPreferences = true;
            setting.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.93 Safari/537.36 Edg/96.0.1054.53";
            setting.CefCommandLineArgs.Add("--ignore-urlfetcher-cert-requests", "1");
            setting.CefCommandLineArgs.Add("--ignore-certificate-errors", "1");
            setting.CefCommandLineArgs.Add("use-fake-ui-for-media-stream");
            setting.CefCommandLineArgs.Add("enable-usermedia-screen-capturing");
            setting.CefCommandLineArgs.Add("enable-npapi", "1");
            setting.CefCommandLineArgs.Add("ppapi-flash-path", flashPath); 
            setting.CefCommandLineArgs.Add("ppapi-flash-version", "34.0.0.211"); 
            setting.CefCommandLineArgs.Add("enable-media-stream", "enable-media-stream");
            setting.UncaughtExceptionStackSize = 10;
            bool performDependencyCheck = !DebuggingSubProcess;

            if (!Cef.Initialize(setting, performDependencyCheck, browserProcessHandler: null))
            {
                throw new Exception("Unable to Initialize Cef");
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;

            var currentProgramName = Assembly.GetExecutingAssembly().GetName().Name;
            semaphore = new Semaphore(0, 1, currentProgramName, out createdNew);
            if (createdNew)
            {
                //UI线程未捕获异常处理事件
                DispatcherUnhandledException += App_DispatcherUnhandledException;

                //Task线程内未捕获异常处理事件
                TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

                //非UI线程未捕获异常处理事件
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                InitSubProcess();

                AddTraceListener();

                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                base.OnStartup(e);

            }
            else
            {

                Process[] temp = Process.GetProcessesByName(currentProgramName);//在所有已启动的进程中查找需要的进程；  
                if (temp.Length > 0)//如果查找到  
                {
                    IntPtr? handle = temp.LastOrDefault()?.MainWindowHandle;
                    if(handle!=null&&handle!=IntPtr.Zero)
                    {
                        SwitchToThisWindow(handle.Value, true);    // 激活，显示在最前  
                    }
                }
                Application.Current.Shutdown();
            }
        }

        private static void DeleteCefBrowserCache()
        {
            try
            {
                if (Directory.Exists(FileMapper.CefBrowserDataDir))
                {
                    //清除文件夹
                    var dirs = Directory.GetDirectories(FileMapper.CefBrowserDataDir);
                    if (dirs != null && dirs.Length > 0)
                    {
                        foreach (var dir in dirs)
                        {
                            Directory.Delete(dir, true);
                        }
                    }

                    //清除文件
                    var files = Directory.GetFiles(FileMapper.CefBrowserDataDir);
                    if (files != null && files.Length > 0)
                    {
                        foreach (var file in files)
                        {
                            //不清除日志文件
                            if (file != FileMapper.CefBrowserLogPath)
                            {
                                File.Delete(file);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine($"[{DateTime.Now.ToLocalTime()}]{Environment.NewLine}" +
                    $"{e}");
            }
        }

        private void App_DispatcherUnhandledException(object? sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Trace.WriteLine($"[{DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss:fff")}]DispatcherUnhandledException:{Environment.NewLine}{e.Exception}");
            e.Handled = true;
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            Trace.WriteLine($"[{DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss:fff")}]TaskUnhandledException:{Environment.NewLine}{e.Exception}");
        }

        private void CurrentDomain_UnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {


            if (e.ExceptionObject is Exception exception)
            {
                Trace.WriteLine($"[{DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss:fff")}]ThreadUnhandledException:{Environment.NewLine}{exception}");
            }
            else
            {
                Trace.WriteLine($"[{DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss:fff")}]ThreadUnhandledException:{Environment.NewLine}{e.ExceptionObject}");
            }

        }

        protected override void OnExit(ExitEventArgs e)
        {
            Trace.WriteLine($"[{DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss:fff")}]Application->Exit Start");
            TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
            base.OnExit(e);
            Trace.WriteLine($"[{DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss:fff")}]Application->Exit End");
            Trace.WriteLine($"[{DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss:fff")}]Application->Exit Code:{e.ApplicationExitCode}");
            Environment.Exit(e.ApplicationExitCode);
        }
    }

}
