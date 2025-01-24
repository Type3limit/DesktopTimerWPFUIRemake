using CefSharp;
using CefSharp.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace DesktopTimer.Views
{
    public class MarkdownViewer : UserControl
    {
        private readonly WebBrowser _webBrowser;

        public static readonly DependencyProperty MarkdownProperty =
            DependencyProperty.Register(nameof(Markdown), typeof(string), typeof(MarkdownViewer),
                new PropertyMetadata(string.Empty, OnMarkdownChanged));

        public string Markdown
        {
            get => (string)GetValue(MarkdownProperty);
            set => SetValue(MarkdownProperty, value);
        }

        public MarkdownViewer()
        {
            _webBrowser = new WebBrowser();
            Content = _webBrowser;
        }

        private static void OnMarkdownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MarkdownViewer viewer)
            {
                viewer.RenderMarkdown((string)e.NewValue);
            }
        }

        private void RenderMarkdown(string markdown)
        {
            var html = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <script src='https://cdn.jsdelivr.net/npm/marked/marked.min.js'></script>
                <link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/github-markdown-css/github-markdown.min.css'>
                <style>
                    .markdown-body {{
                        box-sizing: border-box;
                        min-width: 200px;
                        max-width: 980px;
                        margin: 0 auto;
                        padding: 45px;
                    }}
                    @media (max-width: 767px) {{
                        .markdown-body {{
                            padding: 15px;
                        }}
                    }}
                    body {{
                        background-color: transparent;
                    }}
                    /* 代码高亮样式 */
                    pre {{
                        background-color: #f6f8fa;
                        border-radius: 3px;
                        padding: 16px;
                        overflow: auto;
                    }}
                    code {{
                        font-family: Consolas, 'Courier New', monospace;
                        font-size: 14px;
                    }}
                </style>
            </head>
            <body class='markdown-body'>
                <div id='content'></div>
                <script>
                    marked.setOptions({{
                        breaks: true,
                        gfm: true,
                        tables: true,
                        sanitize: false,
                        smartLists: true,
                        smartypants: true,
                        xhtml: true
                    }});
                    document.getElementById('content').innerHTML = marked.parse(`{markdown.Replace("`", "\\`")}`);
                </script>
            </body>
            </html>";

            _webBrowser.NavigateToString(html);
        }
    }
}
