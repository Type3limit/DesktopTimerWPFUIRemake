using AllInAI.Sharp.API.Dto;
using AllInAI.Sharp.API.Req;
using AllInAI.Sharp.API.Res;
using AllInAI.Sharp.API.Service;
using CommunityToolkit.Mvvm.ComponentModel;
using DesktopTimer.Helpers;
using FFmpeg.AutoGen;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms.Design;
using static DesktopTimer.Models.BackgroundWorkingModel.Definations.SDAPI;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace DesktopTimer.Models.BackgroundWorkingModel.Definations
{
    public class SDResponse : IResponseBase
    {
        public ImgRes? imageData { set; get; }
    }
    public class SDQuery : Txt2ImgReq, IRequestQueryBase
    {

    }
    public partial class StableDiffusion : RequestBase
    {
        public override Type Type => typeof(StableDiffusion);

        public override RequestBaseUseage RequestUseage => RequestBaseUseage.PictureBackground;

        public new static string DisplayName => "Stable Diffusion";

        private new string requestUrl = "http://127.0.0.1:7860";
        public override string RequestUrl
        {
            get => requestUrl;
            set => SetProperty(ref requestUrl, value);
        }


        bool canDoNextRequest = true;
        public bool CanDoNextRequest
        {
            get => canDoNextRequest;
            set => SetProperty(ref canDoNextRequest, value);
        }

        #region Properties

        [ObservableProperty]
        int steps = 20;

        [ObservableProperty]
        int width = 1024;

        [ObservableProperty]
        int height = 1024;

        [ObservableProperty]
        int generateNumber = 1;

        [ObservableProperty]
        int generateTimes = 1;

        [ObservableProperty]
        int configScale = 7;

        [ObservableProperty]
        string? prompt = "";

        [ObservableProperty]
        string? negativePrompt = "";


        [ObservableProperty]
        List<ModelData> models = new List<ModelData>() { };

        
        ModelData? selectedModel = null;

        public ModelData? SelectedModel
        {
            get=>selectedModel;
            set
            {
                SetProperty(ref selectedModel,value);
                OnModelChangedCommand.Execute(null);
            }
        }

        [ObservableProperty]
        List<SamplerData> samplers = new List<SamplerData>();

        [ObservableProperty]
        SamplerData? selectedSampler = null;

        [ObservableProperty]
        List<SDPresets> sDPresets = new List<SDPresets>();

        
        SDPresets? selectedPreset = null;
        public SDPresets? SelectedPreset
        {
            get=>selectedPreset;
            set
            {
                SetProperty(ref selectedPreset,value);
                OnPresetsChangedCommand.Execute(null);
            }
        }
        #endregion

        #region constructor
        public StableDiffusion()
        {
            UpdateModelAndSamplers.Execute(null);
        }

        ICommand? updateModelAndSamplersCommand = null;
        ICommand UpdateModelAndSamplers
        {
            get => updateModelAndSamplersCommand ?? (updateModelAndSamplersCommand = new RelayCommand(() =>
            {
                Task.Run(async () =>
              {
                  try
                  {
                      Models = await SDAPI.GetModels(RequestUrl);
                      selectedModel = Models.FirstOrDefault();
                      OnPropertyChanged("SelectedModel");

                      Samplers = await SDAPI.GetSamplers(RequestUrl);
                      SelectedSampler = Samplers.FirstOrDefault();

                      SDPresets = JsonConvert.DeserializeObject<List<SDPresets>>(FileMapper.StableDiffusionPresetFile.ReadText());
                      SelectedPreset = null;
                  }
                  catch(Exception ex)
                  {
                      Trace.WriteLine(ex);
                  }
                 
              });
            }));
        }


        ICommand? onModelChangedCommand = null;
        public ICommand OnModelChangedCommand
        {
            get => onModelChangedCommand ?? (onModelChangedCommand = new RelayCommand(async() =>
            {
                await SDAPI.SetModel(RequestUrl, SelectedModel?.model_name ??(Models?.FirstOrDefault()?.model_name??""));
                await SDAPI.RefreshModels(RequestUrl);
            }));
        }

        ICommand? onPresetsChangedCommand = null;
        public ICommand OnPresetsChangedCommand
        {
            get=>onModelChangedCommand??(onPresetsChangedCommand = new RelayCommand(() => 
            {
                if(SelectedPreset != null)
                {
                    Prompt = SelectedPreset.tags;
                    NegativePrompt = SelectedPreset.ntags;
                    if(int.TryParse(SelectedPreset.param?.scale, out int curValue))
                    {

                        ConfigScale = curValue;
                    }
                }
            }
            ));
        }

        #endregion

        public override IRequestQueryBase? BuildQuery(bool AutoIncreasePage, params object[]? objs)
        {

            return null;
        }

        public override bool HasReachedEnd(IResponseBase? currentResponse)
        {
            return false;
        }

        public async override IAsyncEnumerable<object?> ParseResult(IResponseBase? currentResponse, [EnumeratorCancellation] CancellationToken canceller)
        {
            yield break;
        }

        public override Task<IResponseBase?> Request(IRequestQueryBase? query)
        {
            return null;
        }

        public override void ResetRequest()
        {
            return;
        }

        public override bool CanDoNext()
        {
            return CanDoNextRequest;
        }

        public override async Task<List<object?>?> RequestSourceCore()
        {
            try
            {
                CanDoNextRequest = false;
                List<object?> resultList = new List<object?>();
                Text2ImageRequestParam param = new Text2ImageRequestParam()
                {
                    Prompt = Prompt,
                    NegativePrompt = NegativePrompt,
                    Steps = Steps,
                    Width = Width/2,
                    Height = Height/2,
                    BatchSize = GenerateNumber,
                    NIter = GenerateTimes,
                    SamplerName = SelectedSampler?.name ?? "Euler",
                    EnableHr = true,
                    HrResizeX = Width,
                    HrResizeY = Height,
                    CfgScale = ConfigScale,
                };
                var res = await SDAPI.TextToImage(param, RequestUrl);
                if (res != null && res?.images?.Count > 0)
                {
                    foreach (var item in res.images)
                    {
                        var filePath = FileMapper.GeneratedPictureDir.PathCombine($"{Guid.NewGuid()}.png");
                        var imageData = Convert.FromBase64String(item);
                        await System.IO.File.WriteAllBytesAsync(filePath, imageData, CancellationToken.None);
                        resultList.Add(filePath);
                    }
                }
                return resultList;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return null;
            }
            finally
            {
                CanDoNextRequest = true;
            }
        }
    }

    public class SDPresetParam
    {
        public string scale { set;get;} = "7";
    }


    public class SDPresets
    {
        public List<string>? keywords;
        public string? pt;
        public SDPresetParam? param;
        public string? tags;
        public string? ntags;

        public string? DisplayName
        {
            get=>keywords?.SourcesToPrintString(',');
        }
    }

    public class SDAPI
    {

        public class Text2ImageRequestParam
        {

            /// <summary>
            /// 文本提示，用于生成图像的主要内容。
            /// </summary>
            public string? Prompt { get; set; } = string.Empty;

            /// <summary>
            /// 否定提示，模型将尽量避免生成带有该描述的元素。
            /// </summary>
            public string? NegativePrompt { get; set; } = string.Empty;

            /// <summary>
            /// 用于定义生成图像时应用的样式数组。
            /// </summary>
            public string[] Styles { get; set; } = new string[] { };

            /// <summary>
            /// 用于生成图像的随机种子，值为 -1 表示使用随机种子。
            /// </summary>
            public int Seed { get; set; } = -1;

            /// <summary>
            /// 可选的子种子，用于微调种子影响的细节。
            /// </summary>
            public int Subseed { get; set; } = -1;

            /// <summary>
            /// 子种子对最终生成图像影响的强度。
            /// </summary>
            public double SubseedStrength { get; set; } = 0;

            /// <summary>
            /// 通过种子调整图像高度的分辨率（从该高度调整种子）。
            /// </summary>
            public int SeedResizeFromH { get; set; } = -1;

            /// <summary>
            /// 通过种子调整图像宽度的分辨率（从该宽度调整种子）。
            /// </summary>
            public int SeedResizeFromW { get; set; } = -1;

            /// <summary>
            /// 使用的采样器名称，如 "Euler"、"LMS" 等。
            /// </summary>
            public string SamplerName { get; set; } = "Euler";

            /// <summary>
            /// 选择用于采样的调度器类型。
            /// </summary>
            public string Scheduler { get; set; } = "";

            /// <summary>
            /// 每批次生成图像的数量。
            /// </summary>
            public int BatchSize { get; set; } = 1;

            /// <summary>
            /// 迭代次数，表示图像生成的采样次数。
            /// </summary>
            public int NIter { get; set; } = 1;

            /// <summary>
            /// 生成图像的步骤数，较高的步数通常会生成更多细节。
            /// </summary>
            public int Steps { get; set; } = 50;

            /// <summary>
            /// 配置缩放比例，用于调整生成图像的整体准确性。
            /// </summary>
            public double CfgScale { get; set; } = 7;

            /// <summary>
            /// 生成图像的宽度，单位为像素。
            /// </summary>
            public int Width { get; set; } = 512;

            /// <summary>
            /// 生成图像的高度，单位为像素。
            /// </summary>
            public int Height { get; set; } = 512;

            /// <summary>
            /// 启用面部修复功能，确保生成的人脸更准确。
            /// </summary>
            public bool RestoreFaces { get; set; } = true;

            /// <summary>
            /// 是否启用平铺图像功能，使图像可以在二维平面上无缝平铺。
            /// </summary>
            public bool Tiling { get; set; } = true;

            /// <summary>
            /// 禁止保存图像样本。
            /// </summary>
            public bool DoNotSaveSamples { get; set; } = false;

            /// <summary>
            /// 禁止保存图像网格。
            /// </summary>
            public bool DoNotSaveGrid { get; set; } = false;

            /// <summary>
            /// 图像生成的额外随机性参数。
            /// </summary>
            public double Eta { get; set; } = 0;

            /// <summary>
            /// 用于控制去噪过程的强度。
            /// </summary>
            public double DenoisingStrength { get; set; } = 0;

            /// <summary>
            /// 影响未条件提示采样的最小值。
            /// </summary>
            public double SMinUncond { get; set; } = 0;

            /// <summary>
            /// 在采样时影响随机变化的参数。
            /// </summary>
            public double SChurn { get; set; } = 0;

            /// <summary>
            /// 用于设置采样时间的最大阈值。
            /// </summary>
            public double STmax { get; set; } = 0;

            /// <summary>
            /// 用于设置采样时间的最小阈值。
            /// </summary>
            public double STmin { get; set; } = 0;

            /// <summary>
            /// 控制生成图像时的噪声强度。
            /// </summary>
            public double SNoise { get; set; } = 0;

            /// <summary>
            /// 用于覆盖默认模型设置的自定义设置。
            /// </summary>
            public object OverrideSettings { get; set; } = new { };

            /// <summary>
            /// 是否在生成图像后恢复为原始的设置。
            /// </summary>
            public bool OverrideSettingsRestoreAfterwards { get; set; } = true;

            /// <summary>
            /// 自定义检查点文件，用于在图像生成过程中细化图像。
            /// </summary>
            public string RefinerCheckpoint { get; set; } = "";

            /// <summary>
            /// 设置图像生成过程中使用细化模型的开关时间。
            /// </summary>
            public int RefinerSwitchAt { get; set; } = 0;

            /// <summary>
            /// 禁用额外的神经网络模型。
            /// </summary>
            public bool DisableExtraNetworks { get; set; } = false;

            /// <summary>
            /// 图像生成过程中的第一阶段生成的图像。
            /// </summary>
            public string FirstpassImage { get; set; } = "";

            /// <summary>
            /// 自定义注释，可以用于传递额外信息。
            /// </summary>
            public object Comments { get; set; } = new { };

            /// <summary>
            /// 是否启用高分辨率生成。
            /// </summary>
            public bool EnableHr { get; set; } = false;

            /// <summary>
            /// 高分辨率生成时的第一阶段宽度。
            /// </summary>
            public int FirstphaseWidth { get; set; } = 0;

            /// <summary>
            /// 高分辨率生成时的第一阶段高度。
            /// </summary>
            public int FirstphaseHeight { get; set; } = 0;

            /// <summary>
            /// 高分辨率生成的缩放比例。
            /// </summary>
            public double HrScale { get; set; } = 2;

            /// <summary>
            /// 高分辨率图像生成时使用的上采样器。
            /// </summary>
            public string HrUpscaler { get; set; } = "";

            /// <summary>
            /// 高分辨率生成时的第二步图像生成的步数。
            /// </summary>
            public int HrSecondPassSteps { get; set; } = 0;

            /// <summary>
            /// 在高分辨率生成时调整图像的宽度。
            /// </summary>
            public int HrResizeX { get; set; } = 0;

            /// <summary>
            /// 在高分辨率生成时调整图像的高度。
            /// </summary>
            public int HrResizeY { get; set; } = 0;

            /// <summary>
            /// 高分辨率生成时使用的检查点名称。
            /// </summary>
            public string HrCheckpointName { get; set; } = "";

            /// <summary>
            /// 高分辨率图像生成时使用的采样器名称。
            /// </summary>
            public string HrSamplerName { get; set; } = "";

            /// <summary>
            /// 高分辨率图像生成时使用的调度器。
            /// </summary>
            public string HrScheduler { get; set; } = "";

            /// <summary>
            /// 高分辨率生成时使用的提示。
            /// </summary>
            public string HrPrompt { get; set; } = string.Empty;

            /// <summary>
            /// 高分辨率生成时使用的否定提示。
            /// </summary>
            public string HrNegativePrompt { get; set; } = string.Empty;

            /// <summary>
            /// 强制任务ID，用于特定任务的唯一标识。
            /// </summary>
            public string ForceTaskId { get; set; } = "";

            /// <summary>
            /// 自定义脚本名称，用于额外的脚本功能。
            /// </summary>
            public string ScriptName { get; set; } = "";

            /// <summary>
            /// 自定义脚本参数。
            /// </summary>
            public object ScriptParams { get; set; } = new { };
        }

        public class Text2ImageResonse
        {
            public List<string>? images { set; get; }
            public string? parameters { set; get; }
            public string? info { set; get; }
        }

        public static async Task<Text2ImageResonse?> TextToImage(Text2ImageRequestParam param, string apiUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                var jsonContent = System.Text.Json.JsonSerializer.Serialize(param);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(apiUrl + "/sdapi/v1/txt2img", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<Text2ImageResonse>(result);
                }
                else
                {
                    Trace.WriteLine($"Error: {response.StatusCode}:{response.Content.ToString()}");
                    return null;
                }
            }
        }


        public class SamplerData
        {
            public string? name { set; get; }
            public string[]? aliases { set; get; }
            public Dictionary<string, string?>? options { set; get; }
        }

        public static async Task<List<SamplerData>> GetSamplers(string IP)
        {
            var client = new HttpClient();
            var content = new System.Net.Http.StringContent("", Encoding.UTF8, "application/json");

            try
            {
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(IP + "/sdapi/v1/samplers"),
                    Method = HttpMethod.Get,
                    Content = content
                };

                var response = await client.SendAsync(request);
                var jsonRaw = await response.Content.ReadAsStringAsync();
                var jArray = JsonConvert.DeserializeObject<List<SamplerData>>(jsonRaw);
                return jArray;
            }
            catch { return null; }
        }

        public class ModelData
        {
            public string? title {set;get;}
            public string? model_name {set;get;}
            public string? hash { set; get; }
            public string? filename { set; get; }
            public string? config { set; get; }
        }

        public static async Task<List<ModelData>> GetModels(string IP)
        {
            var client = new HttpClient();
            var content = new System.Net.Http.StringContent("", Encoding.UTF8, "application/json");

            try
            {
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(IP + "/sdapi/v1/sd-models"),
                    Method = HttpMethod.Get,
                    Content = content
                };

                var response = await client.SendAsync(request);
                var jsonRaw = await response.Content.ReadAsStringAsync();
                var jArray = JsonConvert.DeserializeObject<List<ModelData>>(jsonRaw);
                return jArray;
            }
            catch { return null; }
        }


        class AutomaticJsonSetModels
        {
            public string sd_model_checkpoint = "";
        }

        public static async Task SetModel(string IP, string model)
        {
            AutomaticJsonSetModels payload = new AutomaticJsonSetModels();
            payload.sd_model_checkpoint = model;

            string json = JsonConvert.SerializeObject(payload);

            var client = new HttpClient();
            var content = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(IP + "/sdapi/v1/options"),
                    Method = HttpMethod.Post,
                    Content = content
                };
                var response = await client.SendAsync(request);
            }
            catch { }
        }


        public static byte[]? ImageToBytes(Image x)
        {
            ImageConverter _imageConverter = new ImageConverter();
            byte[]? xByte = (byte[]?)_imageConverter.ConvertTo(x, typeof(byte[]));
            return xByte;
        }


        public static async Task Interrupt(string URI)
        {

            var client = new HttpClient();
            var content = new System.Net.Http.StringContent("", Encoding.UTF8, "application/json");


            try
            {
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(URI + "/sdapi/v1/interrupt"),
                    Method = HttpMethod.Post,
                    Content = content
                };
                var response = await client.SendAsync(request);
            }
            catch { }

        }

        class AutomaticInterrogateStruct
        {
            public string image = "";
            public string model = "clip";
        }

        public static async Task<string> AutomaticInterrogate(string URI, Bitmap image)
        {
            string base64Image = "data:image/png;base64," + Convert.ToBase64String(ImageToBytes(image));
            var struc = new AutomaticInterrogateStruct
            {
                image = base64Image
            };

            try
            {
                string json = JsonConvert.SerializeObject(struc);
                var client = new HttpClient();
                var content = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(URI + "/sdapi/v1/interrogate"),
                    Method = HttpMethod.Post,
                    Content = content
                };
                var response = await client.SendAsync(request);
                dynamic responseD = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
                return responseD.caption.ToString();
            }
            catch { return null; }
        }

        public static async Task RefreshModels(string IP)
        {
            var client = new HttpClient();
            var content = new System.Net.Http.StringContent("", Encoding.UTF8, "application/json");


            try
            {
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(IP + "/sdapi/v1/refresh-checkpoints"),
                    Method = HttpMethod.Get,
                    Content = content
                };
                var response = await client.SendAsync(request);
            }
            catch { }
        }

        public static async Task<string> GetCurrentModel(string IP)
        {
            var client = new HttpClient();
            var content = new System.Net.Http.StringContent("", Encoding.UTF8, "application/json");


            try
            {
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(IP + "/sdapi/v1/options"),
                    Method = HttpMethod.Get,
                    Content = content
                };
                var response = await client.SendAsync(request);
                var jsonRaw = await response.Content.ReadAsStringAsync();

                JObject obj = JObject.Parse(jsonRaw);
                string value = (string)obj["sd_model_checkpoint"];
                return value;
            }
            catch { return null; }
        }




        public static async Task<string[]> Get_ControlNet_ModelList(string IP)
        {
            var client = new HttpClient();
            var content = new System.Net.Http.StringContent("", Encoding.UTF8, "application/json");


            try
            {
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(IP + "/controlnet/model_list"),
                    Method = HttpMethod.Get,
                    Content = content
                };
                var response = await client.SendAsync(request);

                var json = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
                List<string> modelList = responseObject["model_list"];

                return modelList.ToArray();
            }
            catch { return null; }
        }


    }
}