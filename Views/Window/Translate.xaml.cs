using DesktopTimer.Models;
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
using System.Windows.Threading;
using Wpf.Ui.Controls;

namespace DeskTopTimer
{
    /// <summary>
    /// FontSelectionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OptionsWindow : MahApps.Metro.Controls.MetroWindow 
    {
        private DispatcherTimer _searchDelayTimer;
        TranslateModel? viewModel = null;
        private bool isClosed  = false;
        public bool IsClosed
        {
            get=>isClosed;
            private set=>isClosed = value;
        }
        public OptionsWindow()
        {
            InitializeComponent();
            DataContextChanged += OptionsWindow_DataContextChanged;
            Closed += OptionsWindow_Closed;
            InPutText.Focus();

            _searchDelayTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _searchDelayTimer.Tick += SearchDelayTimer_Tick;
        }
        private void SearchDelayTimer_Tick(object? sender, EventArgs e)
        {
            // 当定时器触发时停止计时，并执行搜索
            _searchDelayTimer.Stop();
            viewModel?.RunTranslateCommand?.Execute(viewModel?.TranslateSource);
        }
        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            WindowClose();
        }
        
        private void OptionsWindow_Closed(object? sender, EventArgs e)
        {
            IsClosed = true;
            if (viewModel != null)
            {
                viewModel.ShouldOpenTranslateResult = false;
            }

        }

        private void OptionsWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            viewModel = e.NewValue as TranslateModel;
        }


        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 每次输入时重置定时器
            if (_searchDelayTimer.IsEnabled)
            {
                _searchDelayTimer.Stop();
            }

            _searchDelayTimer.Start();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key== Key.Enter)
            {
                if(viewModel?.SelectedTranslateResult!=null)
                {
                    DataObject dataObject = new DataObject();
                    dataObject.SetData(DataFormats.StringFormat, viewModel?.SelectedTranslateResult);
                    Clipboard.SetDataObject(dataObject);
                }
                WindowClose();
            }
            else if(e.Key==Key.Up)
            {
                var index = viewModel?.TranslateResult.IndexOf(viewModel?.SelectedTranslateResult);
                if(index>=0)
                {
                    viewModel.SelectedTranslateResult = viewModel.TranslateResult.ElementAt((int)((index-1<0?0:index-1) % viewModel.TranslateResult.Count));
                }
            }
            else if(e.Key==Key.Down)
            {
                var index = viewModel?.TranslateResult.IndexOf(viewModel?.SelectedTranslateResult);
                if (index >= 0)
                {
                    viewModel.SelectedTranslateResult = viewModel.TranslateResult.ElementAt((int)((index + 1 ) % viewModel.TranslateResult.Count));
                }
            }
        }

        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (viewModel?.SelectedTranslateResult != null)
            {
                DataObject dataObject = new DataObject();
                dataObject.SetData(DataFormats.StringFormat, viewModel?.SelectedTranslateResult);
                Clipboard.SetDataObject(dataObject);
            }
            WindowClose();
        }
        bool IsInClose = false;
        public void WindowClose()
        {
            if(IsInClose)
                return;
            IsInClose = true;
            Close();
        }
    }
}
