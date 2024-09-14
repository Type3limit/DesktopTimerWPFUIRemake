using DesktopTimer.Models.Everything;
using DesktopTimer.Views.Models;
using MahApps.Metro.Controls;
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
using System.Windows.Shapes;

namespace DesktopTimer.Views.Window
{
    /// <summary>
    /// EverythingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EverythingWindow : MetroWindow
    {
        private bool isClosed = false;
        public bool IsClosed
        {
            get => isClosed;
            private set => isClosed = value;
        }

        EverythingWrapper? viewModel;
        public EverythingWindow()
        {
            InitializeComponent();
            DataContextChanged += EverythingWindow_DataContextChanged; ;
            Closed += EverythingWindow_Closed; ;
            InPutText.Focus();
            Loaded += EverythingWindow_Loaded;

        }

        private ScrollViewer GetScrollViewerFromListBox(ListBox listBox)
        {
            if (listBox == null)
                return null;
            // 确保模板已应用
            listBox.ApplyTemplate();
            // 获取 ListBox 的模板并查找 ScrollViewer
            var border = VisualTreeHelper.GetChild(listBox, 0) as Border;
            if (border != null)
            {
                return border.Child as ScrollViewer;
            }

            return null;
        }

        private void EverythingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取 ListBox 中的 ScrollViewer
            var scrollViewer = GetScrollViewerFromListBox(ResultBox);

            if (scrollViewer != null)
            {
                // 监听 ScrollViewer 的滚动事件
                scrollViewer.ScrollChanged += ResultBox_ScrollChanged;
            }
            InPutText.Focus();
        }


        private void ResultBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;

          
        }

        private void EverythingWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            viewModel = e.NewValue as EverythingWrapper;
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            WindowClose();
        }

        private void EverythingWindow_Closed(object? sender, EventArgs e)
        {
            IsClosed = true;
            if (viewModel != null)
            {
                viewModel.ShouldOpenFlyouts = false;
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key==Key.Enter)
            {
                if(viewModel.SelectedResult!=null)
                {
                    viewModel.OpenFileCommand.Execute(null);
                }
            }
            else if (e.Key == Key.Up)
            {
                var index = viewModel?.CurrentResults.IndexOf(viewModel?.SelectedResult);
                if (index >= 0)
                {
                    viewModel.SelectedResult = viewModel.CurrentResults.ElementAt((int)((index - 1 < 0 ? 0 : index - 1) % viewModel.CurrentResults.Count));
                }
            }
            else if (e.Key == Key.Down)
            {
                var index = viewModel?.CurrentResults.IndexOf(viewModel?.SelectedResult);
                if (index >= 0)
                {
                    viewModel.SelectedResult = viewModel.CurrentResults.ElementAt((int)((index + 1) % viewModel.CurrentResults.Count));
                }
            }
        }

        bool IsInClose = false;

        public void WindowClose()
        {
            viewModel.CancelAll();
            viewModel.SearchKey = "";
            if (IsInClose)
                return;
            IsInClose = true;
            Close();
        }

        private void ResultBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (viewModel.SelectedResult != null)
            {
                viewModel.OpenFileCommand.Execute(null);
            }
        }

        private void ResultBox_ScrollChanged_1(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange > 0)
            {
                if (e.VerticalOffset + e.ViewportHeight >= (e.ExtentHeight - 10))
                {
                    Task.Run(()=>viewModel.LoadMoreResults());
                }
            }
          
        }
    }
}
