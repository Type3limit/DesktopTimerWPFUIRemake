using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DesktopTimer.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace DesktopTimer.Models.BackgroundWorkingModel.Definations
{
    public partial class LocalVideo : RequestBase
    {
        #region datas
        public override Type Type => typeof(LocalVideo);

        public override RequestBaseUseage RequestUseage => RequestBaseUseage.VideoBackground;

        public static new string DisplayName =>"本地视频";

        [JsonIgnore]
        [ObservableProperty]
        int localFileCount = 0;


        [JsonIgnore]
        [ObservableProperty]
        int curPage = -1;

        [JsonIgnore]
        int PageSize = 20;

        [JsonIgnore]
        int TotalPage => (LocalFileCount / PageSize) + 1;


        [JsonPropertyName("LocalPath")]
        [ObservableProperty]
        string? localFileLoadPath = "";

        [JsonIgnore]
        List<string> localFiles = new List<string>();
        [JsonIgnore]
        public List<string> LocalFiles
        {
            get=>localFiles;
            set=>SetProperty(ref localFiles,value);
        }

        [JsonIgnore]
        string? currentFile = "";
        [JsonIgnore]
        public string? CurrentFile
        {
            get=>currentFile;
            set
            {
                SetProperty(ref currentFile,value);
                WeakReferenceMessenger.Default.Send(new VideoPathChangedMessage(value));
            }
        }

        [JsonPropertyName("KeyWords")]
        [ObservableProperty]
        string keyWords = "";
        #endregion

        #region play control
        [JsonIgnore]
        [ObservableProperty]
        bool loopPlay = false;

        [JsonIgnore]
        [ObservableProperty]
        TimeSpan startPosition = TimeSpan.FromSeconds(0);

        [JsonIgnore]
        [ObservableProperty]
        TimeSpan endPostion = TimeSpan.FromSeconds(0);

        public bool DisablePositionUpdate {get;private set;}

        [JsonIgnore]
        [ObservableProperty]
        TimeSpan position = TimeSpan.FromSeconds(0);

        [JsonIgnore]
        [ObservableProperty]
        TimeSpan positionStep = TimeSpan.FromSeconds(0);

        [JsonIgnore]
        [ObservableProperty]
        bool isPlaying = false;

        [JsonIgnore]
        [ObservableProperty]
        bool pausing = false;

        double originVolume = 0d;
        private bool isVideoMute = false;
        public bool IsVideoMute
        {
            get => isVideoMute;
            set
            {
                if (value)
                {
                    originVolume = videoVolume;
                    VideoVolume = 0;
                }
                else
                {
                    VideoVolume = originVolume;
                }
                SetProperty(ref isVideoMute, value);
            }
        }

        private double videoVolume = 1d;
        public double VideoVolume
        {
            get => videoVolume;
            set
            {
                if (videoVolume != value)
                {
                    SetProperty(ref videoVolume, value);
                    WeakReferenceMessenger.Default.Send(new VideoVolumeChangedMessage(value));
                }

            }
        }

        [JsonIgnore]
        [ObservableProperty]
        bool mediaLoaded = false;
        #endregion


        #region constructor

        public LocalVideo()
        {
            WeakReferenceMessenger.Default.Register<ConfigReadComplecateMessage>(this, async(e, t) =>
            {
                LocalFileLoadPath = ModelInstance?.Config.UserConfigData.LocalVideoLoadPath;
                if(!LocalFileLoadPath.IsNullOrEmpty())
                {
                    ResetRequest();
                    await Request(BuildQuery(true));

                    if(!ModelInstance.Config.UserConfigData.LastPlayedVideo.IsNullOrEmpty())
                    {
                        CurrentFile = ModelInstance.Config.UserConfigData.LastPlayedVideo;
                        Position = ModelInstance.Config.UserConfigData.LastVideoPosition??TimeSpan.FromMicroseconds(0);
                    }
                    
                }
            });
            WeakReferenceMessenger.Default.Register<VideoMoveNextMessage>(this, (e, t) =>
            {
                if(loopPlay)
                {
                    WeakReferenceMessenger.Default.Send(new VideoPathChangedMessage(CurrentFile));
                }
                else
                {
                    if (CurrentFile == null)
                    {
                        CurrentFile = LocalFiles?.FirstOrDefault();
                    }
                    else
                    {
                        var index = LocalFiles.IndexOf(CurrentFile);
                        if (index < 0)
                            index = 0;
                        CurrentFile = LocalFiles[(index + 1) % LocalFiles.Count];
                    }

                }

            });
            WeakReferenceMessenger.Default.Register<VideoVolumeShortCutMessage>(this, (e, t) =>
            {
                switch (t.Value)
                {
                    case VolumeShortOption.Mute:
                        MuteCommand.Execute(null);
                        break;
                    case VolumeShortOption.Increase:
                        VideoVolume  = (VideoVolume + 0.1)>1?1:VideoVolume+0.1;
                        break;
                    case VolumeShortOption.Decrease:
                        VideoVolume = (VideoVolume -0.1)<0?0:VideoVolume-0.1;
                        break;
                }
            });
        }

        #endregion


        #region Command

        ICommand? seekCommand =  null;
        public ICommand SeekCommand
        {
            get=>seekCommand ?? (seekCommand = new RelayCommand<string?>((var) => 
            {
                var seekpos = Position;
                if(var =="0")
                {
                    seekpos = (Position - TimeSpan.FromSeconds(1));
                    if(seekpos<StartPosition)
                        seekpos = StartPosition;
                }
                else if(var =="1")
                {
                    seekpos = (Position + TimeSpan.FromSeconds(1));
                    if(seekpos>EndPostion)
                    {
                        seekpos = EndPostion;
                    }
                }
                WeakReferenceMessenger.Default.Send(new VideoSeekMessage(seekpos));
            }));
        }

        ICommand? changeUpdatePositionStatusCommand = null;
        public ICommand? ChangeUpdatePositionStatusCommand
        {
            get=> changeUpdatePositionStatusCommand??(changeUpdatePositionStatusCommand= new RelayCommand(() => 
            {
                DisablePositionUpdate = !DisablePositionUpdate;
            }));
        }


        ICommand? playCommand = null;
        public ICommand PlayCommand
        {
            get=>playCommand??(playCommand = new RelayCommand(() => 
            {
                if(MediaLoaded)
                {
                    WeakReferenceMessenger.Default.Send(new VideoStatusChangedMessage(IsPlaying ? VideoStatus.Pause:VideoStatus.Play));
                }
                
            }));
        }

        ICommand ? stopCommand = null;
        public ICommand StopCommand
        {
            get=>stopCommand ?? (stopCommand = new RelayCommand(() =>
            {
                WeakReferenceMessenger.Default.Send(new VideoStatusChangedMessage(VideoStatus.Stop));
            }));
        }

        ICommand? muteCommand = null;
        public ICommand MuteCommand
        {
            get=>muteCommand??(muteCommand = new RelayCommand(() => 
            {
                IsVideoMute = !IsVideoMute;
            }));
        }


        ICommand? browseDirectoryCommand = null;
        public ICommand BrowseDirectoryCommand
        {
            get => browseDirectoryCommand ?? (browseDirectoryCommand = new RelayCommand(() =>
            {
                System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
                var res = folderBrowserDialog.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    LocalFileLoadPath = folderBrowserDialog.SelectedPath;
                    ResetRequest();
                    Request(BuildQuery(true));
                }

            }));
        }
        #endregion

        public override RequestQueryBase? BuildQuery(bool AutoIncreasePage, params object[]? objs)
        {
            return new LocalFileQuery()
            {
                Page = AutoIncreasePage ? CurPage + 1 : CurPage,
                KeyWords = this.KeyWords
            };
        }

        public override bool HasReachedEnd(ResponseBase? currentResponse)
        {
            return TotalPage == CurPage;
        }

        public override async IAsyncEnumerable<object?> ParseResult(ResponseBase? currentResponse, [EnumeratorCancellation]CancellationToken canceller)
        {
            if (!(currentResponse is LocalFileResponse))
                yield break;
            var response = currentResponse as LocalFileResponse;
            if (response == null || response.Files == null)
                yield break;
            if (response.Files.Count > 0)
            {
                foreach (var x in response.Files)
                {
                    if (canceller.IsCancellationRequested)
                    {
                        yield break;
                    }
                    yield return await Task.FromResult<string>(x);
                }
            }
        }

        public override Task<ResponseBase?> Request(RequestQueryBase? query)
        {
            if (query is not LocalFileQuery localQuery)
            {
                return Task.FromResult<ResponseBase?>(null);
            }

            var response = new LocalFileResponse();


            if (LocalFiles.Count <= 0 && Path.Exists(LocalFileLoadPath))
            {
                if(ModelInstance!=null)
                {
                    ModelInstance.Config.UserConfigData.LocalVideoLoadPath = LocalFileLoadPath;
                    WeakReferenceMessenger.Default.Send(new RequestSaveConfigMessage(ConfigType.User));
                }

                LocalFiles = RegexDirectoryEnumrator.GetFiles(LocalFileLoadPath,
                    @"\.mp4$|\.avi$|\.mpg$|\.flv$|\.mkv$|\.rmvb$|\.mov$", SearchOption.AllDirectories).ToList();
            }

            var resultList = LocalFiles;
            if (!localQuery.KeyWords.IsNullOrEmpty())
            {
                resultList = LocalFiles.Where(x => x.Contains(KeyWords) || KeyWords.Contains(x)).ToList();
            }

            response.Files = resultList.Skip(localQuery.Page * PageSize).Take(PageSize).ToList();

            return Task.FromResult<ResponseBase?>(response);
        }

        public override void ResetRequest()
        {
            CurPage = 0;
            LocalFileCount = 0;
        }
    }
}
