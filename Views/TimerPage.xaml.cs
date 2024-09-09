using CommunityToolkit.Mvvm.Messaging;
using DesktopTimer.Helpers;
using DesktopTimer.models;
using DesktopTimer.Models;
using DesktopTimer.Models.BackgroundWorkingModel.Definations;
using DesktopTimer.Views.BackgroundViews;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        bool HasLoaded = false;

        PictureViewsPage? picturePage = null;

        WebViewsPage? webViewsPage = null;

        UpperLayerPage? upperLayerPage = null;

        VideoViewsPage ? videoPage = null;

        public TimerPage()
        {
            InitializeComponent();
            Loaded += TimerPage_Loaded;
            Unloaded += TimerPage_Unloaded;
            DataContextChanged += TimerPage_DataContextChanged;

            WeakReferenceMessenger.Default.Register<RequestModelChangedMessage>(this, (e, t) =>
            {
                if(HasLoaded)
                {
                    OnSelectedRequestModelTypeChanged(t.Value);
                }
                
            });
        }
        private void TimerPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            modelInstance = e.NewValue as MainWorkModel;
        }

        private void TimerPage_Unloaded(object sender, RoutedEventArgs e)
        {
            HasLoaded =false;
            WeakReferenceMessenger.Default.Unregister<RequestModelChangedMessage>(this);
        }


        private void TimerPage_Loaded(object sender, RoutedEventArgs e)
        {
            HasLoaded = true;

            CheckUpperLayerPage();
            UpperLayerFrame.Navigate(upperLayerPage);
            
            OnSelectedRequestModelTypeChanged(modelInstance?.BackgroundImageRequest?.SelectedRequestInstance?.RequestUseage??
                RequestBaseUseage.PictureBackground);
            
        }

        void CheckUpperLayerPage()
        {
            if(upperLayerPage == null)
            {
                upperLayerPage = new UpperLayerPage();
                upperLayerPage.DataContext = this.DataContext;
            }
        }

        void CheckWebViewsPage()
        {
            if(webViewsPage==null)
            {
                webViewsPage = new WebViewsPage();
                webViewsPage.DataContext = this.DataContext;
            }
        }

        void CheckPicturePage()
        {
            if (picturePage == null)
            {
                picturePage = new PictureViewsPage();
                picturePage.MouseMoveHandler += PicturesView_MouseMoveHandler; 
                picturePage.DataContext = this.DataContext;
            }
        }

        void CheckVideoPage()
        {
            if(videoPage == null)
            {
                videoPage = new VideoViewsPage();
                videoPage.MouseMoveHandler += PicturesView_MouseMoveHandler;
                videoPage.DataContext = this.DataContext;
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
                        CheckVideoPage();
                        ContentFrame.Navigate(videoPage);
                        break;
                    }

                case RequestBaseUseage.WebsiteBackground:
                    {
                        CheckWebViewsPage();
                        ContentFrame.Navigate(webViewsPage);
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

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if(sender is ContextMenu  con)
            {
                con.DataContext = modelInstance;
            }
        }

        private void ContentFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            Trace.WriteLine($"load to {e?.Uri}");
        }
    }
}
