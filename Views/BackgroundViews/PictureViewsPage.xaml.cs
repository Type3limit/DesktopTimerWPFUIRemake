using DesktopTimer.models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace DesktopTimer.Views.BackgroundViews
{
    /// <summary>
    /// PictureViewsPage.xaml 的交互逻辑
    /// </summary>
    public partial class PictureViewsPage : Page
    {
        public delegate void onMouseMove(MouseEventArgs e);
        public event onMouseMove? MouseMoveHandler;
        MainWorkModel? model = null;

        
        public PictureViewsPage()
        {
            InitializeComponent();
            DataContextChanged += PictureViewsPage_DataContextChanged;

        }

        private void PictureViewsPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            model = (this.DataContext as MainWorkModel);
            if(model!=null)
            {
                model.DisplaySetting.PropertyChanged += DisplaySetting_PropertyChanged;
            }
        }

        private void RootPage_MouseMove(object sender, MouseEventArgs e)
        {
            MouseMoveHandler?.Invoke(e);
        }

        private void DisplaySetting_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(model==null)
                return;
            if (e.PropertyName == nameof(model.DisplaySetting.BackgroundView))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var targetOpacity = model.DisplaySetting.BackgroundImageOpacity;
                    var curAnimiation = new DoubleAnimation(0.0, targetOpacity, TimeSpan.FromSeconds(0.5));
                    CurImage.BeginAnimation(Wpf.Ui.Controls.Image.OpacityProperty, curAnimiation);
                });
            }
        }
    }
}
