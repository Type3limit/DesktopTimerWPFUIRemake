using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DesktopTimer.Models.BackgroundWorkingModel.Definations
{

    public partial class LocalFileQuery: RequestQueryBase
    {
        [ObservableProperty]
        int page = 0;
    }
    public partial class LocalFileResponse:ResponseBase
    {
        [ObservableProperty]
        public List<string> files = new List<string>();
    }

    public partial class LocalBackground : RequestBase
    {

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
        public string localFileLoadPath = "";

        [JsonIgnore]
        List<string> LocalFiles = new List<string>();

        #endregion

        public override Type Type => typeof(LocalBackground);


        public override void ResetRequest()
        {
            CurPage = -1;
            LocalFileCount = 0;
        }

        public override RequestQueryBase? BuildQuery(params object[]? objs)
        {
            return new LocalFileQuery(){ Page = CurPage + 1};
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
                LocalFiles = Directory.EnumerateFiles(LocalFileLoadPath, @"\.png$|\.jpg$|\.jpeg$|\.bmp$", SearchOption.AllDirectories).ToList();
            }

            response.Files = LocalFiles.Skip(localQuery.Page * PageSize).Take(PageSize).ToList();

            return Task.FromResult<ResponseBase?>(response);
        }

        public override async IAsyncEnumerable<object?> ParseResult(ResponseBase? currentResponse)
        {
            if (!(currentResponse is LocalFileResponse))
                yield return null;
            var response = currentResponse as LocalFileResponse;
            if (response == null || response.Files == null)
                yield return null;
            if(response.Files.Count>0)
            {
                foreach (var x in response.Files)
                {
                    yield return await Task.FromResult<string>(x);
                }
            }
            else
            {
                yield return null;
            }
        }

        public override bool HasReachedEnd(ResponseBase? currentResponse)
        {
            return TotalPage==CurPage;
        }
    }
}
