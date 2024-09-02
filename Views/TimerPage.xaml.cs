using CommunityToolkit.Mvvm.Messaging;
using DesktopTimer.Helpers;
using DesktopTimer.models;
using DesktopTimer.Models.BackgroundWorkingModel.Definations;
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

        PictureViewsPage? picturePage = null;

        UpperLayerPage? upperLayerPage = null;

        public TimerPage()
        {
            InitializeComponent();
            Loaded += TimerPage_Loaded;
            Unloaded += TimerPage_Unloaded;
            DataContextChanged += TimerPage_DataContextChanged;
        }
        private void TimerPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            modelInstance = e.NewValue as MainWorkModel;
        }

        private void TimerPage_Unloaded(object sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Unregister<RequestModelChangedMessage>(this);
        }


        private void TimerPage_Loaded(object sender, RoutedEventArgs e)
        {
            //default with picture page
            CheckPicturePage();
            ContentFrame.Navigate(picturePage);
            CheckUpperLayerPage();
            UpperLayerFrame.Navigate(upperLayerPage);
            WeakReferenceMessenger.Default.Register<RequestModelChangedMessage>(this, (e, t) => 
            {
                OnSelectedRequestModelTypeChanged(t.Value);
            });
        }

        void CheckUpperLayerPage()
        {
            if(upperLayerPage == null)
            {
                upperLayerPage = new UpperLayerPage();
                upperLayerPage.DataContext = this.DataContext;
            }
        }

        void CheckPicturePage()
        {
            if (picturePage == null)
            {
                picturePage = new PictureViewsPage();
                picturePage.MouseMoveHandler += PicturesView_MouseMoveHandler; ;
                picturePage.DataContext = this.DataContext;
            }
        }

        void OnSelectedRequestModelTypeChanged(RequestBaseUseage curUseage)
        {
            switch (curUseage)
            {
                case RequestBaseUseage.NormalRequest:
                    break;
                case RequestBaseUseage.PictureBackground:
                    {
                        CheckPicturePage();
                        ContentFrame.Navigate(picturePage);
                        break;
                    }

                case RequestBaseUseage.VideoBackground:
                    {
                        //TODO:add video background page
                        break;
                    }

                case RequestBaseUseage.WebsiteBackground:
                    {
                        //TODO: add website background page
                        break;
                    }

            }
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
