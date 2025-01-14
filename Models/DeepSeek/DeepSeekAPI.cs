using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DesktopTimer.Helpers;
using Flurl.Http;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DesktopTimer.Models.DeepSeek
{
    public partial class DeepSeek : ObservableObject
    {
        #region data
        public static string APIUrl = @"https://api.deepseek.com/v1/chat/completions";


        CancellationTokenSource? requestCanceller = null;

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);

        public LiteDatabase UserInfoDB = new LiteDatabase(FileMapper.DeepSeekInfoDBFile);

        #endregion

        #region properties

        [ObservableProperty]
        ObservableCollection<MessageContent> currentConversations = new ObservableCollection<MessageContent>();

        [ObservableProperty]
        ObservableCollection<string> historyConversations = new ObservableCollection<string>();


        [ObservableProperty]
        MessageContent currentInputingContent = new MessageContent()
        {
            Role = "user"
        };

        [ObservableProperty]
        MessageContent currentResponseContent = new MessageContent()
        {
            Role ="system"
        };

        [ObservableProperty]
        int maxToken = 2048;

        bool IsRequestStarted
        {
            get=>requestCanceller!=null;
        }
        #endregion

        MainWorkModel? currentModelInsatnce;

        public DeepSeek(MainWorkModel mainWorkModelInstance) 
        {
            currentModelInsatnce = mainWorkModelInstance;
            
        }

        #region command 

        ICommand? startCompletionsCommand = null;
        public ICommand StartCompletionsCommand
        {
            get => startCompletionsCommand ?? (startCompletionsCommand = new RelayCommand(() =>
            {
                try
                {
                    if(requestCanceller!=null)
                    {
                        CancelCurrentRequest();
                    }
                    else
                    {

                        SendRequest(CurrentInputingContent);
                    }
                }
                catch(Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }));
        }


        ICommand? startNewConversationCommand = null;
        public ICommand StartNewConversationCommand
        {
            get=>startNewConversationCommand??(startNewConversationCommand = new RelayCommand(() => 
            {
                SaveAndClearConversations();
            }));
        }
        #endregion


        #region methods

        public void SaveAndClearConversations()
        {
            var histories = UserInfoDB.GetCollection<Dictionary<string,List<MessageContent>>>
                ("CommunicateHistories").Query().ToList();

            var currentKey = DateTime.Now.ToString();
            foreach(var itr in histories)
            {
                itr.Add(currentKey,CurrentConversations.ToList());
            }

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                CurrentConversations.Clear();
            });

            CurrentInputingContent.Content = "";


        }


        public void BuildConversationHistories()
        {

            System.Windows.Application.Current.Dispatcher.Invoke(() => 
            {
                HistoryConversations.Clear();
            });

            var histories = UserInfoDB.GetCollection<Dictionary<string, List<MessageContent>>>
                ("CommunicateHistories").Query().ToList();

            foreach(var itr in histories)
            {
                foreach(var key in itr.Keys)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        HistoryConversations.Add(key);
                    });
                }
            }

        }


        DSRequestBody BuildRequest(MessageContent currentInputingContent)
        {
            DSRequestBody curRequest = new DSRequestBody();
            curRequest.max_tokens = MaxToken;
            curRequest.stream = true;
            var currentConversation = CurrentConversations.ToList();
            foreach(var itr in currentConversation) 
            {
                curRequest.messageContents.Add(itr);
            }
            return curRequest;
        }


        public async void SendRequest(MessageContent currentInputingContent)
        {
            if(requestCanceller!=null)
            {
                return;
            }
            try
            {

                requestCanceller = new CancellationTokenSource();
                OnPropertyChanged("IsRequestStarted");
                var currentRequestBody = BuildRequest(currentInputingContent);
                var token = currentModelInsatnce?.Config.UserConfigData.DeepSeekApiAuthKey;
                var responseStream = await  APIUrl
                    .WithHeader("Accept", "application/json")
                    .WithOAuthBearerToken(token)
                    .PostJsonAsync(currentRequestBody,cancellationToken: requestCanceller.Token)
                    .ReceiveStream();


                if (requestCanceller.IsCancellationRequested)
                {
                    return;
                }

                var buffer = new byte[4096];
                int bytesRead;
                StringBuilder stringBuilder = new StringBuilder();  
                while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length, requestCanceller.Token)) > 0)
                {
                    if(requestCanceller.IsCancellationRequested)
                        return;
                    string content = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    if (content == "data: [DONE]")
                    {
                        break;
                    }
                    var obj = JsonObject.Parse(content);
                    if (obj!=null&&obj["data"]!=null)
                    {
                        var response = JsonSerializer.Deserialize<DSResponseBody>(obj["data"]?.ToString()??"");
                        stringBuilder.Append(response?.choices?.Select(x=>x?.delta?.Content)?.Aggregate<string?>((e, o) => $"{e}{o}"));
                    }
                    CurrentResponseContent.Content = stringBuilder.ToString();
                }
                //record current conversation
                var userContent = new MessageContent(currentInputingContent);

                var systemContent = new MessageContent(CurrentResponseContent);

                System.Windows.Application.Current.Dispatcher.Invoke(() => 
                {
                    CurrentConversations.Add(userContent);
                    CurrentConversations.Add(systemContent);
                });

            }
            catch(OperationCanceledException)
            {
                Trace.WriteLine("request canceled");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                requestCanceller = null;
                OnPropertyChanged("IsRequestStarted");
                semaphore.Release();
            }
          
        }


        public async void CancelCurrentRequest()
        {
            requestCanceller?.Cancel();
            if (requestCanceller != null)
            {
                await semaphore.WaitAsync();
            }
        }

        #endregion

    }


    #region requestDefination
    public class DSRequestBody
    {
        /// <summary>
        /// 对话的消息列表,lenth >=1
        /// </summary>
        public List<MessageContent> messageContents = new List<MessageContent>();

        public string model { set;get;} = "deepseek-chat";

        /// <summary>
        /// 介于 -2.0 和 2.0 之间的数字。如果该值为正，那么新 token 会根据其在已有文本中的出现频率受到相应的惩罚，降低模型重复相同内容的可能性。
        /// </summary>
        public int? frequency_penalty { set;get;} = 0;
        /// <summary>
        /// 介于 -2.0 和 2.0 之间的数字。如果该值为正，那么新 token 会根据其是否已在已有文本中出现受到相应的惩罚，从而增加模型谈论新主题的可能性。
        /// </summary>
        public int? presence_penalty { set;get; } = 0;
        /// <summary>
        /// 限制一次请求中模型生成 completion 的最大 token 数。输入 token 和输出 token 的总长度受模型的上下文长度的限制。
        /// </summary>
        public int? max_tokens { set;get;} = 0;
        /// <summary>
        /// 一个 object，指定模型必须输出的格式。
        /// </summary>
        public ResponseFormat? response_format { set;get;} = new ResponseFormat();

        /// <summary>
        /// 一个 string 或最多包含 16 个 string 的 list，在遇到这些词时，API 将停止生成更多的 token
        /// </summary>
        public string? stop { set;get;} = "DesktopTimerForceStoped";
        /// <summary>
        /// 如果设置为 True，将会以 SSE（server-sent events）的形式以流式发送消息增量。消息流以 data: [DONE] 结尾。
        /// </summary>
        public bool? stream { set;get;} = false;
        /// <summary>
        /// 流式输出相关选项。只有在 stream 参数为 true 时，才可设置此参数。
        /// </summary>
        public StreamOption? stream_options { set;get;}

        /// <summary>
        /// 采样温度，介于 0 和 2 之间。更高的值，如 0.8，会使输出更随机，而更低的值，如 0.2，会使其更加集中和确定。 
        /// 我们通常建议可以更改这个值或者更改 top_p，但不建议同时对两者进行修改。
        /// </summary>
        public int? temperature { set; get;}  =1;

        /// <summary>
        /// 作为调节采样温度的替代方案，模型会考虑前 top_p 概率的 token 的结果。
        /// 所以 0.1 就意味着只有包括在最高 10% 概率中的 token 会被考虑。
        /// 我们通常建议修改这个值或者更改 temperature，但不建议同时对两者进行修改
        /// </summary>
        public int? top_p { set; get;} =1;
        /// <summary>
        /// 控制模型调用 tool 的行为。
        /// none 意味着模型不会调用任何 tool，而是生成一条消息。
        /// auto 意味着模型可以选择生成一条消息或调用一个或多个 tool。
        /// required 意味着模型必须调用一个或多个 tool。
        /// 通过 {"type": "function", "function": {"name": "my_function"}}
        /// 指定特定 tool，会强制模型调用该 tool。
        /// 当没有 tool 时，默认值为 none。如果有 tool 存在，默认值为 auto。
        /// </summary>
        public string? tool_choice { set;get;} = "none";
        /// <summary>
        /// 是否返回所输出 token 的对数概率。如果为 true，则在 message 的 content 中返回每个输出 token 的对数概率。
        /// </summary>
        public bool? logprobs { set;get;} =false ;
        /// <summary>
        /// 一个介于 0 到 20 之间的整数 N，指定每个输出位置返回输出概率 top N 的 token，且返回这些 token 的对数概率。
        /// 指定此参数时，logprobs 必须为 true。
        /// </summary>
        public int? top_logprobs { set; get;} = null;
    }

    public class StreamOption
    {
        /// <summary>
        /// 如果设置为 true，在流式消息最后的 data: [DONE] 之前将会传输一个额外的块。
        /// 此块上的 usage 字段显示整个请求的 token 使用统计信息，而 choices 字段将始终是一个空数组。
        /// 所有其他块也将包含一个 usage 字段，但其值为 null。
        /// </summary>
        public bool include_usage { set;get; } =false;
    }

    public class ResponseFormat
    {
        /// <summary>
        /// 设置为 json_object 以启用 JSON 模式，该模式保证模型生成的消息是有效的 JSON。
        /// 注意: 使用 JSON 模式时，你还必须通过系统或用户消息指示模型生成 JSON。
        /// 否则，模型可能会生成不断的空白字符，直到生成达到令牌限制，从而导致请求长时间运行并显得“卡住”。
        /// 此外，如果 finish_reason = "length"，这表示生成超过了 max_tokens 或对话超过了最大上下文长度，消息内容可能会被部分截断。
        /// </summary>
        public string type { set;get;} = "text";
    }

    public partial class MessageContent : ObservableObject
    {
        /// <summary>
        /// 消息的内容。
        /// </summary>
        [ObservableProperty]
        string? content;
        /// <summary>
        /// 该消息的发起角色，其值为 system。
        /// </summary>
        [ObservableProperty]
        string? role;
        /// <summary>
        /// 可以选填的参与者的名称，为模型提供信息以区分相同角色的参与者。
        /// </summary>
        [ObservableProperty]
        string? name;

        public MessageContent() { }

        public MessageContent(MessageContent other)
        {
            content = other.content;
            role = other.role;
            name = other.name;
        }

    }
    #endregion

    #region responseDefination

    public class DSResponseBody
    {
        public string? id { set;get;}

        public long created { set;get;}
        public string model { set;get; } = "deepseek-chat";

        [JsonPropertyName("object")]
        public string _object {set;get;} = "chat.completion.chunk";

        public string system_fingerprint { set;get;} = "";

        public List<ResponseChoice?>? choices { set;get; }

        public ResponseUsage? usage { set;get;}
    }

    public class ResponseChoice
    {
        public MessageContent? delta { set;get; }
        /// <summary>
        /// 模型停止生成 token 的原因。
        /// stop：模型自然停止生成，或遇到 stop 序列中列出的字符串。
        /// length ：输出长度达到了模型上下文长度限制，或达到了 max_tokens 的限制。
        /// content_filter：输出内容因触发过滤策略而被过滤。
        /// insufficient_system_resource: 由于后端推理资源受限，请求被打断。
        /// </summary>
        public string? finish_reason { set;get;} 

        public int index { set;get; }
    }

    public class ResponseUsage
    {
        public int completion_tokens { set; get;} = 0;
        public int prompt_tokens { set;get; } = 0;

        public int total_tokens { set;get; } = 0;
    }
    #endregion
}
