using DesktopTimer.Models;
using DesktopTimer.Models.DeepSeek;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace DesktopTimer.Views
{
    public static class BrowserBehavior
    {
        public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached(
            "Html",
            typeof(string),
            typeof(BrowserBehavior),
            new FrameworkPropertyMetadata(OnHtmlChanged));

        [AttachedPropertyBrowsableForType(typeof(WebBrowser))]
        public static string GetHtml(WebBrowser d)
        {
            return (string)d.GetValue(HtmlProperty);
        }

        public static void SetHtml(WebBrowser d, string value)
        {
            d.SetValue(HtmlProperty, value);
        }

        static void OnHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WebBrowser? wb = d as WebBrowser;
            if (wb != null)
                wb.NavigateToString(e.NewValue as string);
        }
    }

    public static class CollectionExtensions
    {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }
    /// <summary>
    /// DeepSeekConversation.xaml 的交互逻辑
    /// </summary>
    public partial class DeepSeekChatWindow : FluentWindow
    {
        public DeepSeekChatWindow()
        {
            InitializeComponent();
            DataContextChanged += DeepSeekChatWindow_DataContextChanged;
        }

        private void DeepSeekChatWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                var vm = DataContext as DeepSeek;
                vm?.StartCompletionsCommand.Execute(null);
                e.Handled = true;
            }
        }
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange > 0)
            {
                ((ScrollViewer)sender).ScrollToEnd();
            }
        }

        private void MarkdownViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            
            e.Handled= false;
        }


        private async Task TryRecoverWebView(Microsoft.Web.WebView2.Wpf.WebView2 webView)
        {
            // 方案 1: 重新初始化核心组件
            try
            {
                await webView.EnsureCoreWebView2Async();
                Debug.WriteLine("WebView2 核心组件重新初始化成功");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"核心组件恢复失败: {ex.Message}");
            }

            // 方案 2: 重载内容
            var tempFile = ((dynamic)webView.DataContext).CurrentHtmlPath;
            webView.Source = new Uri(tempFile);
        }

        public class DimensionResult
        {
            public double height { get; set; }
            public double width { get; set; }
            public string? error { get; set; }
        }
        private async void WebView2_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            var webView = sender as Microsoft.Web.WebView2.Wpf.WebView2;

            if (webView == null ||
                webView.CoreWebView2 == null) // 检查底层核心组件
            {
                Debug.WriteLine("WebView2 未正确初始化");
                return;
            }

            // 确保导航成功（处理失败重定向）
            if (!e.IsSuccess)
            {
                Debug.WriteLine($"导航失败: {e.WebErrorStatus}");
                return;
            }
            var jsScript = @"
(() => {
    try {
        // 调试输出当前文档状态
        console.log('[DEBUG] 文档状态:', document.readyState);
        console.log('[DEBUG] 容器元素:', document.querySelector('.markdown-container'));

        const container = document.querySelector('.markdown-container');
        if (!container) {
            console.error('错误: 未找到容器元素');
            return JSON.stringify({ error: '容器未找到', height: 0, width: 0 });
        }

        // 强制同步布局计算
        container.style.overflow = 'hidden'; // 禁用滚动条影响
        const height = Math.ceil(container.scrollHeight);
        const width = Math.ceil(container.clientWidth);
        
        return JSON.stringify({ 
            error: null, 
            height: height,
            width: width,
            computedStyle: window.getComputedStyle(container) // 调试样式
        });
    } catch (e) {
        console.error('脚本执行异常:', e);
        return JSON.stringify({ error: e.message, height: 0, width: 0 });
    }
})()";
            // 等待渲染稳定（关键！）
            await Task.Delay(400);

            try
            {
                var jsonResult = await webView.ExecuteScriptAsync(jsScript);

                // 空值保护
                if (string.IsNullOrEmpty(jsonResult))
                {
                    Debug.WriteLine("脚本返回空值，可能未注入成功");
                    await TryRecoverWebView(webView); // 跳转到恢复逻辑
                    return;
                }

                // 处理转义字符
                jsonResult = JsonSerializer.Deserialize<string>(jsonResult);

                var result  = JsonSerializer.Deserialize<DimensionResult>(jsonResult);
                // 错误处理
                if (result?.error != null)
                {
                    Debug.WriteLine($"JavaScript 错误: {result.error}");
                    return;
                }

                // 空值检查
                if (result == null)
                {
                    Debug.WriteLine("返回的 JSON 数据为空");
                    return;
                }

                // 类型验证
                if (result.height <= 0 || result.width <= 0)
                {
                    if(result.width <= 0)
                    {
                        result.width = 100;
                    }
                    if(result.height <=0)
                    {
                        result.height = 100;
                    }
                }

                // DPI 转换
                var dpi = VisualTreeHelper.GetDpi(webView);
                webView.Height = result.height + 2;
                webView.Width = result.width / dpi.DpiScaleX +2;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Dimension Error: {ex.Message}");
                webView.Height = 20; // 最小安全高度
            }
        }

        private void ChromiumWebBrowser_FrameLoadEnd(object sender, CefSharp.FrameLoadEndEventArgs e)
        {

        }
    }
}
