using CefSharp.Wpf;
using CefSharp;
using CommunityToolkit.Mvvm.Messaging;
using DesktopTimer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace DesktopTimer.Views.BackgroundViews
{
    /// <summary>
    /// WebViewsPage.xaml 的交互逻辑
    /// </summary>
    public partial class WebViewsPage : Page
    {

        private Dictionary<string, CookieVisitor> UrlCookies = new Dictionary<string, CookieVisitor>();


        public WebViewsPage()
        {
            InitializeComponent();
            BrowserInstance.LifeSpanHandler = new OpenPageSelf();
            BrowserSettings bset = new BrowserSettings();
            bset.WindowlessFrameRate = 60;
            bset.WebGl = CefState.Enabled;
            BrowserInstance.BrowserSettings = bset;
            BrowserInstance.FrameLoadStart += WebView_FrameLoadStart;
            BrowserInstance.FrameLoadEnd += WebView_FrameLoadEnd;
            Loaded += WebViewsPage_Loaded;
            Unloaded += WebViewsPage_Unloaded;
           
        }

        void WebViewsPage_Loaded(object sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Register<WebBrowserOperationMessage>(this, (e, t) =>
            {
                switch (t.Value)
                {
                    case BrowserOperation.Invalid:
                        break;
                    case BrowserOperation.StepBack:
                        BrowserInstance.Back();
                        break;
                    case BrowserOperation.StepForward:
                        BrowserInstance.Forward();
                        break;
                    case BrowserOperation.Flush:
                        BrowserInstance.Load(BrowserInstance.Address);
                        break;
                }

            });

        }

        void WebViewsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Unregister<WebBrowserOperationMessage>(this);
        }
        private void WebView_FrameLoadStart(object? sender, FrameLoadStartEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                try
                {
                    var currentUrl = BrowserInstance.GetFocusedFrame().Url;
                    if (UrlCookies.ContainsKey(currentUrl))
                    {
                        var currentVisitor = UrlCookies[currentUrl];
                        var currentCookies = new Cookie()
                        {
                            Domain = new Uri(currentUrl).Host,
                            Name = currentVisitor.name,
                            Value = currentVisitor.value,
                        };
                        var ok = await BrowserInstance.GetCookieManager().SetCookieAsync(currentUrl, currentCookies);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }

            });
        }

        private void WebView_FrameLoadEnd(object? sender, FrameLoadEndEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    var cookieManager = BrowserInstance.GetCookieManager();
                    var currentVisitor = new CookieVisitor();
                    if (cookieManager.VisitAllCookies(currentVisitor))
                    {
                        var currentUrl = BrowserInstance.GetFocusedFrame().Url;
                        if (UrlCookies.ContainsKey(currentUrl))
                        {
                            UrlCookies[currentUrl] = currentVisitor;
                        }
                        else
                        {
                            UrlCookies.Add(currentUrl, currentVisitor);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                }
            });
        }

    }

    class OpenPageSelf : ILifeSpanHandler
    {
        public bool DoClose(IWebBrowser browserControl, IBrowser browser)
        {
            return false;
        }

        public void OnAfterCreated(IWebBrowser browserControl, IBrowser browser)
        {

        }

        public void OnBeforeClose(IWebBrowser browserControl, IBrowser browser)
        {

        }

        public bool OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl,
string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures,
IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            newBrowser = null;
            var chromiumWebBrowser = (ChromiumWebBrowser)browserControl;
            chromiumWebBrowser.Load(targetUrl);
            return true;
        }
    }

    /// <summary>
    /// web Cookie
    /// </summary>
    class CookieVisitor : ICookieVisitor
    {
        public string? name { set; get; }
        public string? value { set; get; }
        public bool Visit(Cookie cookie, int count, int total, ref bool deleteCookie)
        {
            name = cookie.Name;
            value = cookie.Value;
            return true;
        }
        public void Dispose()
        {
        }
    }
}
