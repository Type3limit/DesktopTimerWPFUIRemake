// 转换器实现
using DesktopTimer.Helpers;
using Markdig;
using System;
using System.Globalization;
using System.IO;
using System.Reflection.Metadata;
using System.Security.Cryptography.Xml;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace DesktopTimer.Models.DeepSeek
{
    // 对齐转换器
    public class RoleToAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() == "user" ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    // 半宽转换器
    public class DoubleHalfConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value * 0.45;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
    public class MarkdownToFlowDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string markdown)
            {
                var flowDocument = Markdig.Wpf.Markdown.ToFlowDocument(markdown);
                return flowDocument;
            }
            return new FlowDocument();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    class MarkdownToHtmlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var content = value as MessageContent;
            if (content == null) return null;

            var htmlContent = $@"
<!DOCTYPE html>
<html style='margin:0;padding:0;height:auto;'>
<head>
    <meta charset='UTF-8'>
    <!-- 引用外部 highlight.js 代码高亮库 -->
    <link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.7.0/styles/vs.min.css'>
    <script src='https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.7.0/highlight.min.js'></script>
    <style>
        /* Fluent Design Base */
        body {{margin: 0;
            padding: 0;
            font-family: 'Segoe UI', sans-serif;
            background-color: #00F3F4F6; /* Soft light background for contrast */
            color: #333;
            overflow-wrap: break-word;
            line-height: 1.6;
        }}

        /* Container for markdown */
        .markdown-container {{max - width: 800px;  /* Wider container for better readability */
            min-width: 320px;  /* Ensures the content is not too narrow */
            margin: 20px auto;
            padding: 16px;
            background-color: #00FFFFFF;
            border-radius: 12px;  /* More rounded corners */
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);  /* Subtle shadow for floating effect */
            box-sizing: border-box;
            transition: all 0.3s ease;  /* Smooth transition effect */
        }}

        /* Add a hover effect to the container */
        .markdown-container:hover {{transform: scale(1.01);  /* Slightly enlarge on hover */
            box-shadow: 0 8px 16px rgba(0, 0, 0, 0.2);  /* Increased shadow on hover */
        }}

        /* Preformatted text block (code highlighting) */
        pre {{background - color: #1e1e1e;  /* Dark background for code */
            color: #dcdcdc;  /* Lighter text color for contrast */
            padding: 16px;
            border-radius: 8px;  /* Rounded corners for the code block */
            margin: 0;
            overflow-x: auto;
            font-size: 14px;
            font-family: 'Courier New', monospace;
        }}

        /* Highlight code */
        code {{font - size: 1em;
            padding: 2px 4px;
            border-radius: 4px;
            background-color: rgba(0, 123, 255, 0.1);  /* Subtle background for inline code */
        
            }}

        /* Links */
        a {{color: #0078d4; /* Fluent blue */
            text-decoration: none;
            transition: color 0.2s ease;
        }}

        a:hover {{color: #005a9e;  /* Darker blue on hover */
        }}
    </style>
</head>
<body>
    <div class='markdown-container'>
        {Markdown.ToHtml(content.Content)}
    </div>
    <!-- Check for errors and apply syntax highlighting -->
    <script>
        hljs.highlightAll();
        if (!document.querySelector('.markdown-container')) {{document.body.innerHTML = '<div class='markdown-container'>ERROR: 容器丢失</div>';
        }}
    </script>
</body>
</html>";

            var tempFilePath = Path.Combine(FileMapper.ConversationTempDir, $"{content.UniqueID}.html");
            File.WriteAllText(tempFilePath, htmlContent);
            return new Uri(tempFilePath);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class BoolToReverseVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var cur = value as bool?;
                if (cur == null)
                    return Binding.DoNothing;
                return cur.Value ? Visibility.Collapsed : Visibility.Visible;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return Binding.DoNothing;
            }
        }
        public class RoleToBrushConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return (value as string) switch
                {
                    "user" => new SolidColorBrush(Color.FromRgb(65, 65, 88)),
                    "assistant" => new SolidColorBrush(Color.FromRgb(48, 48, 48)),
                    _ => Brushes.White
                };
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class BusyStateConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return (bool)value ? "停止生成" : "发送";
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }