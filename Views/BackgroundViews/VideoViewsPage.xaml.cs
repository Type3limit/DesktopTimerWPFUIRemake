using CommunityToolkit.Mvvm.Messaging;
using DesktopTimer.Helpers;
using DesktopTimer.Models;
using DesktopTimer.Models.BackgroundWorkingModel.Definations;
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
using System.Windows.Threading;

namespace DesktopTimer.Views.BackgroundViews
{
    /// <summary>
    /// VideoViewsPage.xaml 的交互逻辑
    /// </summary>
    public partial class VideoViewsPage : Page
    {

        public delegate void onMouseMove(MouseEventArgs e);
        public event onMouseMove? MouseMoveHandler;
        bool IsPlayVideoSuccess = false;
        bool IsBackgroundVideoChangedRaised = false;
        MainWorkModel? workModel = null;
        LocalVideo? videoModel = null;
        DispatcherTimer updatePositionTimer = new DispatcherTimer();
        public VideoViewsPage()
        {
            InitializeComponent();
            Loaded += VideoViewsPage_Loaded;
            Unloaded += VideoViewsPage_Unloaded;
            DataContextChanged += VideoViewsPage_DataContextChanged;

            updatePositionTimer.Interval = TimeSpan.FromMilliseconds(5000);
            updatePositionTimer.Tick += UpdatePositionTimer_Tick;
        }

        private void UpdatePositionTimer_Tick(object? sender, EventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new RequestSaveConfigMessage(ConfigType.User));
        }

        private void VideoViewsPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue is MainWorkModel main)
            {
                workModel = main;
                videoModel = (LocalVideo?)workModel?.BackgroundImageRequest?.FindInstance(typeof(LocalVideo));
            }
        }

        private void VideoViewsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            BackgroundVideoChanged(true,"");
            WeakReferenceMessenger.Default.Unregister<VideoPathChangedMessage>(this);
            WeakReferenceMessenger.Default.Unregister<VideoVolumeChangedMessage>(this);
            WeakReferenceMessenger.Default.Unregister<VideoSeekMessage>(this);
            WeakReferenceMessenger.Default.Unregister<VideoStatusChangedMessage>(this);
        }

        private async void VideoViewsPage_Loaded(object sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Register<VideoPathChangedMessage>(this, (e, t) =>
            {
                workModel.Config.UserConfigData.LastPlayedVideo = t.Value;
                BackgroundVideoChanged(false,t.Value);
            });
            WeakReferenceMessenger.Default.Register<VideoVolumeChangedMessage>(this, (e, t) => 
            {
                VideoVolumnChanged(t.Value);
            });
            WeakReferenceMessenger.Default.Register<VideoSeekMessage>(this, async(e, t) =>
            {
                if(await BackgroundVideo.Pause())
                {
                    await BackgroundVideo.Seek(t.Value);
                }
            });
            WeakReferenceMessenger.Default.Register<VideoStatusChangedMessage>(this, async (e, t) => 
            {
                switch (t.Value)
                {
                    case VideoStatus.Stop:
                        BackgroundVideoChanged(true,"");
                        break;
                    case VideoStatus.Play:
                        if(videoModel?.CurrentFile!=null)
                        {
                            if (await BackgroundVideo.Play())
                            {
                                if (videoModel != null)
                                {
                                    videoModel.MediaLoaded = true;
                                    videoModel.IsPlaying = true;
                                }
                            }
                           
                        }
                        break;
                    case VideoStatus.Pause:
                        {
                            if(await BackgroundVideo.Pause())
                            {
                                if (videoModel != null)
                                {
                                    videoModel.MediaLoaded = true;
                                    videoModel.IsPlaying = false;
                                }
                            }

                        }
                        break;
                }
            });
            BackgroundVideo.MediaStateChanged += MediaStateChanged;
            BackgroundVideo.MessageLogged += MessageLogged;


            if(videoModel.CurrentFile.IsFileExist())
            {
                _=await BackgroundVideoChanged(false,videoModel.CurrentFile,false);//no position reset
                var res = await BackgroundVideo.Seek(videoModel.Position);//seek to last position
                if(!res)
                {
                    Trace.WriteLine("seek failed!");
                }
            }
        }

        private async Task<bool> BackgroundVideoChanged(bool closeNow,string? VideoPath,bool shouldResetPosition = true)
        {
            if(!closeNow)
            {
                IsBackgroundVideoChangedRaised = true;
                if (IsPlayVideoSuccess)
                {
                    IsPlayVideoSuccess = await BackgroundVideo.Close();
                    if (!IsPlayVideoSuccess)
                        Trace.WriteLine("CloseFailed");
                    else
                        IsPlayVideoSuccess = false;
                }

                if (!IsPlayVideoSuccess)
                {
                    if (!string.IsNullOrEmpty(VideoPath))
                    {
                        if( await BackgroundVideo.Open(new Uri(VideoPath)))
                        {
                            
                            IsPlayVideoSuccess = await BackgroundVideo.Play();
                            if (videoModel != null)
                            {
                                videoModel.MediaLoaded = true;
                                videoModel.StartPosition = TimeSpan.FromSeconds(0);
                                videoModel.EndPostion = BackgroundVideo.NaturalDuration.HasValue ?
                                    BackgroundVideo.NaturalDuration.Value : BackgroundVideo.MediaInfo.Duration;
                                if(shouldResetPosition)
                                {
                                    videoModel.Position = TimeSpan.FromSeconds(0);
                                }
                                videoModel.PositionStep = BackgroundVideo.PositionStep;
                                videoModel.IsPlaying = IsPlayVideoSuccess;
                            }

                            updatePositionTimer.Start();
                        }
                        else
                        {
                            Trace.WriteLine("Open failed!");
                            if (videoModel != null)
                            {
                                videoModel.MediaLoaded = false;
                                videoModel.IsPlaying = false;
                            }
                        }
                    }
                    else
                    {
                        Trace.WriteLine("current video file closed");
                    }

                }
                else
                {
                    Trace.WriteLine($"fail to open video path:[{VideoPath}]");
                }
            }
            else
            {
                IsPlayVideoSuccess = await BackgroundVideo.Close();
                updatePositionTimer.Stop();
                if (IsPlayVideoSuccess)
                    Trace.WriteLine("Already close current video file");
                else
                    Trace.WriteLine("close video file failed!");
            }

            return true;
        }

        private void VideoVolumnChanged(double value)
        {
            BackgroundVideo.Volume = value;
        }

        private void MessageLogged(object? sender, Unosquare.FFME.Common.MediaLogMessageEventArgs e)
        {
            switch (e.MessageType)
            {
                case Unosquare.FFME.Common.MediaLogMessageType.None:
                    break;
                case Unosquare.FFME.Common.MediaLogMessageType.Info:
                    break;
                case Unosquare.FFME.Common.MediaLogMessageType.Debug:
                    Debug.WriteLine(e.Message);
                    break;
                case Unosquare.FFME.Common.MediaLogMessageType.Trace:
                    Trace.WriteLine(e.Message);
                    break;
                case Unosquare.FFME.Common.MediaLogMessageType.Error:
                    Trace.WriteLine(e.Message);
                    break;
                case Unosquare.FFME.Common.MediaLogMessageType.Warning:
                    Trace.WriteLine(e.Message);
                    break;
            }
        }

        private void MediaStateChanged(object? sender, Unosquare.FFME.Common.MediaStateChangedEventArgs e)
        {
            if (IsBackgroundVideoChangedRaised)
            {
                IsBackgroundVideoChangedRaised = false;
                return;
            }
            switch (e.MediaState)
            {
                case Unosquare.FFME.Common.MediaPlaybackState.Manual:
                    if (videoModel != null)
                    {
                        videoModel.IsPlaying = false;
                    }
                    break;
                case Unosquare.FFME.Common.MediaPlaybackState.Play:
                    {
                        if(videoModel!=null)
                        {
                            videoModel.IsPlaying = true;
                        }
                    }
                    break;
                case Unosquare.FFME.Common.MediaPlaybackState.Close:
                    {
                        if (videoModel != null)
                        {
                            videoModel.IsPlaying = false;
                        }
                    }
                    break;
                case Unosquare.FFME.Common.MediaPlaybackState.Pause:
                    if (videoModel != null)
                    {
                        videoModel.IsPlaying = false;
                    }
                    break;
                case Unosquare.FFME.Common.MediaPlaybackState.Stop:
                    if (videoModel != null)
                    {
                        videoModel.IsPlaying = false;
                    }
                    break;
            }


            if (e.OldMediaState == Unosquare.FFME.Common.MediaPlaybackState.Play 
                && e.MediaState == Unosquare.FFME.Common.MediaPlaybackState.Stop)
            {
               WeakReferenceMessenger.Default.Send(new VideoMoveNextMessage());
            }
        }

        private void BackgroundVideo_PositionChanged(object sender, Unosquare.FFME.Common.PositionChangedEventArgs e)
        {
            workModel.Config.UserConfigData.LastVideoPosition = e.Position;
            if ( videoModel != null&& !videoModel.DisablePositionUpdate)
            {
                videoModel.Position = e.Position;
            }
        }

        private void Page_MouseMove(object sender, MouseEventArgs e)
        {
            MouseMoveHandler?.Invoke(e);
        }
    }
}
