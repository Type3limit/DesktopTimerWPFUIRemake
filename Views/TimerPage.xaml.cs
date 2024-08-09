using DesktopTimer.models;
using DesktopTimer.Views.BackgroundViews;
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

namespace DesktopTimer.Views
{
    /// <summary>
    /// TimerPage.xaml 的交互逻辑
    /// </summary>
    public partial class TimerPage : Page
    {
        public delegate void onMouseMove(MouseEventArgs e);
        public event onMouseMove? MouseMoveHandler;
        MainWorkModel? modelInstance = null;

        object? lastContent = null;

        public TimerPage()
        {
            InitializeComponent();
            Loaded += TimerPage_Loaded;
            DataContextChanged += TimerPage_DataContextChanged;
        }

        private void TimerPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            modelInstance = e.NewValue as MainWorkModel;
        }

        private void TimerPage_Loaded(object sender, RoutedEventArgs e)
        {
            var picturesView = new PictureViewsPage();
            picturesView.MouseMoveHandler += PicturesView_MouseMoveHandler; ;
            picturesView.DataContext = this.DataContext;
            ContentFrame.Navigate(picturesView);
        }

        private void PicturesView_MouseMoveHandler(MouseEventArgs e)
        {
            MouseMoveHandler?.Invoke(e);
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            MouseMoveHandler?.Invoke(e);
        }



        private void CloseProgram_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

    }
}
