using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DesktopTimer.Helpers;
using Flurl.Http;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace DesktopTimer.Models.BackgroundWorkingModel.Definations
{

    #region query definations
    //request definations
    public class WallHavenCategories
    {
        public bool none = true;
        public bool general = false;
        public bool anime = false;
        public bool people = false;
    }
    public class WallHavenPurity
    {
        public bool none = true;
        public bool sfw = false;
        public bool sketchy = false;
        public bool nsfw = false;
    }
    public enum WallHavenSorting
    {
        date_added,
        relevance,
        random,
        views,
        favorites,
        toplist
    }
    public static class EnumExtension
    {
        public static string ToDescriptionString(this WallHavenColors val)
        {

            DescriptionAttribute[]? attributes = (DescriptionAttribute[]?)(val
               .GetType()
               ?.GetField(val.ToString())
               ?.GetCustomAttributes(typeof(DescriptionAttribute), false));
            return attributes?.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }
    public enum WallHavenColors
    {

        [Description("660000")]
        lonestar,
        [Description("990000")]
        red_berry,
        [Description("cc000")]
        guardsman_red,
        [Description("cc3333")]
        persian_red,
        [Description("ea4c88")]
        french_rose,
        [Description("993399")]
        plum,
        [Description("663399")]
        royal_purple,
        [Description("333399")]
        sapphire,
        [Description("0066cc")]
        science_blue,
        [Description("0099cc")]
        pacific_blue,
        [Description("66cccc")]
        downy,
        [Description("77cc33")]
        atlantis,
        [Description("669900")]
        limeade,
        [Description("336600")]
        verdun_green,
        [Description("666600")]
        verdun_green_2,
        [Description("999900")]
        olive,
        [Description("cccc33")]
        earls_green,
        [Description("ffff00")]
        yellow,
        [Description("ffcc33")]
        sunglow,
        [Description("ff9900")]
        orange_peel,
        [Description("ff6600")]
        blaze_orange,
        [Description("cc6633")]
        tuscany,
        [Description("996633")]
        potters_clay,
        [Description("663300")]
        nutmeg_wood_finish,
        [Description("000000")]
        black,
        [Description("999999")]
        dusty_gray,
        [Description("cccccc")]
        silver,
        [Description("ffffff")]
        white,
        [Description("424153")]
        gun_powder,
    }

    public class WallhavenRequestQueryCore
    {
        public string? tagname { set; get; }

        public string? excludeTagName { set; get; }

        public List<string>? addTags { set; get; }

        public List<string>? excludeTags { set; get; }

        public string? userName { set; get; }

        public string? id { set; get; }

        public string? type { set; get; }

        public string? likeID { set; get; }
    }

    public static class ToQueryExtension
    {
        public static string ToQuery(this WallhavenRequestQuery curQuery)
        {
            StringBuilder sber = new StringBuilder();
            sber.Append("?");
            if (!curQuery.catagories.none)
            {
                sber.Append($"categories={(curQuery.catagories.general ? "1" : "0")}{(curQuery.catagories.anime ? "1" : "0")}{(curQuery.catagories.people ? "1" : "0")}");
            }
            if (!curQuery.purity.none)
            {
                sber.Append($"&purity={(curQuery.purity.sfw ? "1" : "0")}{(curQuery.purity.sketchy ? "1" : "0")}{(curQuery.purity.nsfw ? "1" : "0")}");
            }

            sber.Append($"&sorting={Enum.GetName(curQuery.sorting)?.ToLower()}");
            sber.Append($"&order={curQuery.order}");
            if (curQuery.sorting == WallHavenSorting.toplist)
            {
                sber.Append($"&topRange={curQuery.topRange}");
            }
            if (!string.IsNullOrEmpty(curQuery.atleast))
            {
                sber.Append($"&atleast={curQuery.atleast}");
            }
            if (!string.IsNullOrEmpty(curQuery.resolutions))
            {
                sber.Append($"&resolutions={curQuery.resolutions}");
            }
            if (!string.IsNullOrEmpty(curQuery.ratios))
            {
                sber.Append($"&ratios={curQuery.ratios}");
            }


            if (curQuery?.colors != null && curQuery.colors.Count > 0)
            {
                var str = string.Join(" ", curQuery.colors.ToArray());
                sber.Append($"&colors={str}");
            }

            sber.Append($"&page={curQuery?.page}");

            if (!string.IsNullOrEmpty(curQuery?.seed))
                sber.Append($"&seed={curQuery.seed}");

            if (curQuery?.queryCore != null)
            {
                sber.Append("&q=");
                var core = curQuery.queryCore;
                if (!string.IsNullOrEmpty(core.tagname))
                    sber.Append($"{core.tagname} ");
                if (!string.IsNullOrEmpty(core.excludeTagName))
                    sber.Append($"-{core.excludeTagName} ");
                if (core.addTags != null && core.addTags.Count > 0)
                {
                    core.addTags.ForEach(x =>
                    {
                        sber.Append($"+{x} ");
                    });
                }
                if (core.excludeTags != null && core.excludeTags.Count > 0)
                {
                    core.excludeTags.ForEach(x =>
                    {
                        sber.Append($"-{x} ");
                    });
                }
                if (!string.IsNullOrEmpty(core.userName))
                {
                    sber.Append($"@{core.userName} ");
                }
                if (!string.IsNullOrEmpty(core.id))
                {
                    sber.Append($"id:{core.id} ");
                }
                if (!string.IsNullOrEmpty(core.type))
                    sber.Append($"type:{core.type} ");
                if (!string.IsNullOrEmpty(core.likeID))
                    sber.Append($"like:{core.likeID}");
            }

            return sber.ToString();
        }
    }

    public class WallhavenRequestQuery : RequestQueryBase
    {
        public WallhavenRequestQueryCore? queryCore { set; get; }
        public WallHavenCategories catagories { set; get; } = new WallHavenCategories();
        public WallHavenPurity purity { set; get; } = new WallHavenPurity();
        public WallHavenSorting sorting { set; get; } = WallHavenSorting.date_added;
        public string order { set; get; } = "desc";//asc
        public string topRange { set; get; } = "1d";//1d, 3d, 1w,1M*, 3M, 6M, 1y//sorting需要在toplist下生效
        public string atleast { set; get; } = "1920x1080";//miniumresolution
        public string resolutions { set; get; } = "1920x1080,1920x1200";
        public string ratios { set; get; } = "16x9,16x10";
        public List<WallHavenColors>? colors { set; get; }
        public long page { set; get; } = 1;
        public string? seed { set; get; } //[a-zA-Z0-9]{6}


    }
    #endregion



    #region response definations
    public class WallhavenQuery
    {
        public long id { set; get; }
        public string? tag { set; get; }
    }
    public class WallhavenMeta
    {
        public long current_page { set; get; }
        public long last_page { set; get; }
        public long per_page { set; get; }
        public long total { set; get; }
        public string? query { set; get; }
    }
    public class WallhavenData
    {
        public string? id { set; get; }
        public string? url { set; get; }
        public string? short_url { set; get; }
        public long? views { set; get; }
        public long? favorites { set; get; }
        public string? purity { set; get; }
        public string? category { set; get; }
        public long? dimension_x { set; get; }
        public long? dimension_y { set; get; }
        public string? resolution { set; get; }
        public string? ratio { set; get; }
        public long file_size { set; get; }
        public string? file_type { set; get; }
        public string? created_at { set; get; }
        public List<string?>? colors { set; get; }
        public string? path { set; get; }
    }

    public class WallhavenResponse : ResponseBase
    {
        public List<WallhavenData?>? data { set; get; }
        public WallhavenMeta? meta { set; get; }
        public string? seed { set; get; }
    }
    #endregion


    #region request defainations

    public class WallHavenRequest : RequestBase
    {
        #region properties

        public override Type Type => typeof(WallHavenRequest);

        private bool isGeneralEnable = false;
        public bool IsGeneralEnable
        {
            get => isGeneralEnable;
            set => SetProperty(ref isGeneralEnable, value);
        }

        private bool isAnimationEnable = true;
        public bool IsAnimationEnable
        {
            get => isAnimationEnable;
            set => SetProperty(ref isAnimationEnable, value);
        }

        private bool isPeopleEnable = false;
        public bool IsPeopleEnable
        {
            get => isPeopleEnable;
            set => SetProperty(ref isPeopleEnable, value);
        }

        [JsonIgnore]
        private Dictionary<string, string> orderText = new Dictionary<string, string>()
        {
            {"一天","1d" },
            {"三天","3d" },
            {"一周","1w" },
            {"一月","1M" },
            {"三月","3M" },
            {"六月","6M" },
            {"一年","1y" }
        };
        private List<string> orderLists = new List<string>() { "一天", "三天", "一周", "一月", "三月", "六月", "一年" };
        public List<string> OrderLists
        {
            get => orderLists;
        }

        private string searchOrderString = "1y";

        private string selectedOrder = "一年";

        public string SelectedOrder
        {
            get => selectedOrder;
            set
            {
                SetProperty(ref selectedOrder, value);
                searchOrderString = orderText[value];
            }
        }

        private long curPage = 0;
        [JsonIgnore]
        public long CurPage
        {
            get => curPage;
            set => SetProperty(ref curPage, value);
        }

        private long totalPage = 1;
        [JsonIgnore]
        public long TotalPage
        {
            get => totalPage;
            set => SetProperty(ref totalPage, value);
        }

        private string wallHavenSearchKeyWords = "";
        /// <summary>
        /// wallHaven search keyword
        /// </summary>
        public string WallHavenSearchKeyWords
        {
            get => wallHavenSearchKeyWords;
            set => SetProperty(ref wallHavenSearchKeyWords, value);
        }
        #endregion
        [JsonIgnore]
        private List<string> WallHavenCache = new List<string>();
        [JsonIgnore]
        public override string RequestUrl => @"https://wallhaven.cc/api/v1/search";

        public override void ResetRequest()
        {
            CurPage = 0;
            TotalPage = 1;
            IsGeneralEnable = false;
            IsAnimationEnable = true;
            IsPeopleEnable= false;
        }

        public override RequestQueryBase? BuildQuery(params object[]? objs)
        {
            var query = new WallhavenRequestQuery();
            query.atleast = "";
            query.queryCore = new WallhavenRequestQueryCore();
            query.sorting = WallHavenSorting.toplist;
            query.topRange = searchOrderString;
            query.catagories.none = false;
            query.catagories.people = IsPeopleEnable;
            query.catagories.anime = IsAnimationEnable;
            query.catagories.general = IsGeneralEnable;
            query.resolutions = "";
            query.ratios = "";
            query.purity.none = false;
            query.purity.sfw = true;
            query.purity.sketchy = true;
            query.page = CurPage + 1;
            if (!string.IsNullOrEmpty(WallHavenSearchKeyWords))
                query.queryCore.addTags = new List<string>() { WallHavenSearchKeyWords };

            return query;
        }

        public override async IAsyncEnumerable<object?> ParseResult(ResponseBase? currentResponse)
        {

            if (!(currentResponse is WallhavenResponse))
                yield return null;
            var response = currentResponse as WallhavenResponse;
            if (response == null || response.meta == null)
                yield return null;
#pragma warning disable CS8602 // 解引用可能出现空引用。
            TotalPage = response.meta.last_page;
            CurPage = response.meta.current_page;
            if (response.data != null)
            {
                foreach (var x in response.data)
                {

                    if (string.IsNullOrEmpty(x?.path))
                        continue;
                    Guid guid = Guid.NewGuid();
                    var curFileName = x.id;//DateTime.Now.ToString($"yyyy_MM_dd_HH_mm_ss_FFFF_{guid}");
                    var exten = "." + x?.file_type?.Split('/').LastOrDefault();
                    string res = "";
                    var totalPath = FileMapper.NormalPictureDir.PathCombine(curFileName + exten);
                    try
                    {
                        
                        if(!totalPath.IsFileExist())
                        {
                            if (x != null)
                                res = await x.path.DownloadFileAsync(FileMapper.NormalPictureDir, curFileName + exten,
                                    4096);
                        }

                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                    }

                    if (!totalPath.IsFileExist())
                        continue;
#pragma warning restore CS8602 // 解引用可能出现空引用。
                    yield return totalPath;
                }
            }
            else
            {

                yield return null;
            }

        }

        public override async Task<ResponseBase?> Request(RequestQueryBase? query)
        {
            if (!(query is WallhavenRequestQuery wallHavenQuery))
                return null;
            var curQuery = wallHavenQuery.ToQuery();
            var requestUrl = RequestUrl + curQuery;
            var res = await requestUrl.GetAsync();
            return await res.GetJsonAsync<WallhavenResponse?>();
        }

        public override bool HasReachedEnd(ResponseBase? currentResponse)
        {
            if (!(currentResponse is WallhavenResponse))
                return false;
            var response = currentResponse as WallhavenResponse;
            if (response == null || response.meta == null)
                return false;
            return response.meta.current_page == response.meta.last_page;
        }
    }

    #endregion
}
