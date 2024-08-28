using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

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
        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            semaphore = new Semaphore(0, 1, Assembly.GetExecutingAssembly().GetName().Name, out createdNew);
            if (createdNew)
            {
                //UI线程未捕获异常处理事件
                DispatcherUnhandledException += App_DispatcherUnhandledException;

                //Task线程内未捕获异常处理事件
                TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

                //非UI线程未捕获异常处理事件
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                AddTraceListener();

                base.OnStartup(e);

            }
            else
            {
                Wpf.Ui.Controls.MessageBox w = new Wpf.Ui.Controls.MessageBox();
                w.Content = "程序已启动";
                w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                w.Topmost = true;
                _ = w.ShowDialogAsync();

                Environment.Exit(0);
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
