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

namespace DesktopTimer.Models.BackgroundWorkingModel.Definations
{

    public partial class LocalFileQuery: RequestQueryBase
    {
        [ObservableProperty]
        int page = 0;

        [ObservableProperty]
        string keyWords = "";
    }
    public partial class LocalFileResponse:ResponseBase
    {
        [ObservableProperty]
        public List<string> files = new List<string>();
    }

    public partial class LocalBackground : RequestBase
    {
        public override Type Type => typeof(LocalBackground);

        public override RequestBaseUseage RequestUseage => RequestBaseUseage.PictureBackground;

        public static new string DisplayName => "本地图片";

        #region properties
        [JsonIgnore]
        [ObservableProperty]
        int localFileCount = 0;


        [JsonIgnore]
        [ObservableProperty]
        int curPage = -1;

        [JsonIgnore]
        int PageSize = 20;

        [JsonIgnore]
        int TotalPage => ( LocalFileCount / PageSize ) + 1;


        [JsonPropertyName("LocalPath")]
        [ObservableProperty]
        string? localFileLoadPath = "";

        [JsonIgnore]
        List<string> LocalFiles = new List<string>();

        [JsonPropertyName("KeyWords")]
        [ObservableProperty]
        string keyWords = "";

        #endregion

        #region constructor

        public LocalBackground()
        {
            WeakReferenceMessenger.Default.Register<ConfigReadComplecateMessage>(this, (e, t) => 
            { 
               
                LocalFileLoadPath = ModelInstance?.Config.UserConfigData.LocalPictureLoadPath;
            });
        }

        #endregion

        ICommand? browseDirectoryCommand = null;
        public ICommand BrowseDirectoryCommand
        {
            get=>browseDirectoryCommand??(browseDirectoryCommand = new RelayCommand(() => 
            {
                System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
                var res = folderBrowserDialog.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    LocalFileLoadPath = folderBrowserDialog.SelectedPath;
                }
            }));
        }


        public override void ResetRequest()
        {
            CurPage = 0;
            LocalFileCount = 0;
        }

        public override RequestQueryBase? BuildQuery(bool AutoIncreasePage, params object[]? objs)
        {
            return new LocalFileQuery()
            {
                Page = AutoIncreasePage?CurPage + 1:CurPage,
                KeyWords = this.KeyWords
            };
        }


        public override Task<ResponseBase?> Request(RequestQueryBase? query)
        {

            if(query is not LocalFileQuery localQuery)
            {
                return Task.FromResult<ResponseBase?>(null);
            }

            var response = new LocalFileResponse();
            if (LocalFiles.Count<=0&& Path.Exists(LocalFileLoadPath))
            {
                if (ModelInstance != null)
                {
                    ModelInstance.Config.UserConfigData.LocalPictureLoadPath = LocalFileLoadPath;
                    WeakReferenceMessenger.Default.Send(new RequestSaveConfigMessage(ConfigType.User));
                }
                LocalFiles = RegexDirectoryEnumrator.GetFiles(LocalFileLoadPath, @"\.png$|\.jpg$|\.jpeg$|\.bmp$", SearchOption.AllDirectories).ToList();
            }

            var resultList = LocalFiles;
            if(!localQuery.KeyWords.IsNullOrEmpty())
            {
                resultList = LocalFiles.Where(x=>x.Contains(KeyWords)||KeyWords.Contains(x)).ToList();
            }

            response.Files = resultList.Skip(localQuery.Page * PageSize).Take(PageSize).ToList();

            return Task.FromResult<ResponseBase?>(response);
        }

        public override async IAsyncEnumerable<object?> ParseResult(ResponseBase? currentResponse, [EnumeratorCancellation]CancellationToken canceller)
        {
            if (!(currentResponse is LocalFileResponse))
                yield break;
            var response = currentResponse as LocalFileResponse;
            if (response == null || response.Files == null)
                yield break;
            if(response.Files.Count>0)
            {
                foreach (var x in response.Files)
                {
                    if(canceller.IsCancellationRequested)
                    {
                        yield break;
                    }
                    yield return await Task.FromResult<string>(x);
                }
            }

        }

        public override bool HasReachedEnd(ResponseBase? currentResponse)
        {
            return TotalPage==CurPage;
        }
    }
}
