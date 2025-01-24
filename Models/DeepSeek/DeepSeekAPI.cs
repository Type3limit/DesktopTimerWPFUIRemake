using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ControlzEx.Standard;
using DesktopTimer.Helpers;
using DesktopTimer.Models.ChatRoom.Defination;
using FFmpeg.AutoGen;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DesktopTimer.Models.DeepSeek
{
    public class DeepSeekException : Exception
    {
        public int StatusCode { get; }
        public string ErrorType { get; }

        public DeepSeekException(int code, string type, string message)
            : base(message)
        {
            StatusCode = code;
            ErrorType = type;
        }
    }

    public partial class DeepSeek : ObservableObject, IDisposable
    {
        #region Constants
        private const int MaxContextLength = 32768;
        #endregion

        #region Data
        public static string APIUrl = @"https://api.deepseek.com/v1/chat/completions";

        private CancellationTokenSource? _requestCanceller;
        private readonly LiteDatabase _userInfoDb = new(FileMapper.DeepSeekInfoDBFile);


        private DeepSeekConfig _config;
        public DeepSeekConfig Config
        {
            get => _config;
            set
            {
                OnPropertyChanged("Config");
            }
        }
        #endregion

        #region Properties
        [ObservableProperty]
        private ObservableCollection<MessageContent> _currentConversations = new();

        [ObservableProperty]
        private List<Dictionary<string, List<MessageContent>>> _historyConversations = new();


        private Dictionary<string, List<MessageContent>>? _selectedHistory = null;
        public Dictionary<string, List<MessageContent>>? SelectedHistory
        {
            get => _selectedHistory;
            set
            {
                if (value != _selectedHistory)
                {
                    SetProperty(ref _selectedHistory, value);
                    OnHistorySelected();
                }

            }
        }


        private MessageContent _currentInputingContent = null;
        public MessageContent CurrentInputingContent
        {
            get => _currentInputingContent ?? (_currentInputingContent = new MessageContent(this) { Role = "user" });
            set => SetProperty(ref _currentInputingContent, value);
        }


        private MessageContent _currentResponseContent = null;
        public MessageContent CurrentResponseContent
        {
            get => _currentResponseContent ?? (_currentResponseContent = new MessageContent(this) { Role = "assistant" });
            set => SetProperty(ref _currentResponseContent, value);
        }

        [ObservableProperty]
        private int _maxToken = 2048;



        public bool IsRequestStarted => _requestCanceller != null;


        MainWorkModel? CurrentModelInstance = null;

        #endregion

        #region Constructor
        public DeepSeek(MainWorkModel mainWorkModel, DeepSeekConfig config)
        {
            _config = config ?? new DeepSeekConfig();
            CurrentModelInstance = mainWorkModel;
        }
        #endregion

        #region Commands
        [RelayCommand]
        public async void StartCompletions()
        {
            try
            {
                if (IsRequestStarted)
                {
                    CancelCurrentRequest();
                }
                else
                {
                    _ = Task.Run(() =>
                    {
                        _ = SendRequestAsync(CurrentInputingContent);
                    });
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"请求启动失败: {ex}");
            }
        }

        [RelayCommand]
        public void StartNewConversation()
        {
            SaveAndClearConversations();
        }
        #endregion

        #region Core Methods
        private DSRequestBody BuildRequest(MessageContent input)
        {
            var messages = TrimContext(CurrentConversations.ToList());
            messages.Add(input);

            return new DSRequestBody
            {
                Messages = messages,
                Model = _config.Model,
                MaxTokens = MaxToken,
                Temperature = _config.Temperature,
                Stream = true,
                Stop = "DesktopTimerForceStoped"
            };
        }
        private const int ReadBufferSize = 4096;
        private readonly StringBuilder _dataBuffer = new();
        private readonly JsonDocumentOptions _jsonOptions = new()
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        };
        public async Task SendRequestAsync(MessageContent input)
        {
            if (IsRequestStarted) return;

            try
            {
                _requestCanceller = new CancellationTokenSource();
                OnPropertyChanged(nameof(IsRequestStarted));
                CurrentResponseContent = null;
                // 先添加用户消息
                await System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    SaveConversation(new MessageContent(input));

                    // 初始化助手的响应消息
                    CurrentResponseContent = new MessageContent(this)
                    {
                        Role = "assistant",
                        Content = ""
                    };
                    CurrentConversations.Add(CurrentResponseContent);
                });
                input.Content = "";
                // 构建请求
                var request = BuildRequest(input);
                var token = CurrentModelInstance?.Config.UserConfigData.DeepSeekApiAuthKey;

                // 发送请求
                using var response = await APIUrl
                    .WithOAuthBearerToken(token)
                    .WithHeader("Content-Type", "application/json")
                    .PostJsonAsync(request, cancellationToken: _requestCanceller.Token)
                    .ReceiveStream();

                // 处理响应流
                await ProcessResponseStream(
                    response,
                    _requestCanceller.Token);


            }
            catch (OperationCanceledException)
            {

                Trace.WriteLine("请求已取消");
            }
            catch (Exception ex)
            {
                _requestCanceller?.Cancel();
                _requestCanceller = null;
                OnPropertyChanged(nameof(IsRequestStarted));
                if (CurrentResponseContent != null)
                {
                    _ = System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        var finalMessage = new MessageContent(this)
                        {
                            Role = CurrentResponseContent.Role,
                            Content = CurrentResponseContent.Content
                        };

                        var lastMessage = CurrentConversations.LastOrDefault();
                        lastMessage.Content = ex.Message;
                        if (lastMessage?.Role == "assistant")
                        {
                            CurrentConversations.Remove(lastMessage);
                        }
                        CurrentConversations.Add(finalMessage);
                    });
                }

                Trace.WriteLine(ex);
            }
            finally
            {
                _requestCanceller?.Dispose();
                _requestCanceller = null;
                OnPropertyChanged(nameof(IsRequestStarted));
            }
        }

        private async Task ProcessResponseStream(Stream stream, CancellationToken ct)
        {

            using var reader = new StreamReader(stream);
            var buffer = new char[ReadBufferSize];

            while (!reader.EndOfStream && !ct.IsCancellationRequested)
            {
                var readCount = await reader.ReadAsync(buffer, 0, buffer.Length);
                if (readCount == 0) continue;

                _dataBuffer.Append(buffer, 0, readCount);
                ProcessBufferChunks();
            }
            // 确保最后一条消息被正确保存
            if (CurrentResponseContent != null)
            {
                _ = System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    var finalMessage = new MessageContent(this)
                    {
                        Role = CurrentResponseContent.Role,
                        Content = CurrentResponseContent.Content
                    };

                    var lastMessage = CurrentConversations.LastOrDefault();
                    if (lastMessage?.Role == "assistant")
                    {
                        CurrentConversations.Remove(lastMessage);
                    }
                    CurrentConversations.Add(finalMessage);
                });
            }
        }

        private void ProcessBufferChunks()
        {
            //var buffer = _dataBuffer.ToString();
            //var processedLength = 0;

            //while (true)
            //{
            //    var dataStart = buffer.IndexOf("data: ", processedLength);
            //    if (dataStart == -1) break;

            //    var dataEnd = buffer.IndexOf("\n\n", dataStart, StringComparison.Ordinal);
            //    if (dataEnd == -1) break;

            //    var eventData = buffer.Substring(
            //        dataStart + "data: ".Length,
            //        dataEnd - dataStart - "data: ".Length
            //    );

            //    ProcessSingleEvent(eventData);
            //    processedLength = dataEnd + 2;
            //}

            //_dataBuffer.Remove(0, processedLength);
            var buffer = _dataBuffer.ToString();
            var processedLength = 0;

            while (true)
            {
                var dataStart = buffer.IndexOf("data: ", processedLength);
                if (dataStart == -1) break;

                var dataEnd = buffer.IndexOf("\n\n", dataStart, StringComparison.Ordinal);
                if (dataEnd == -1) break;

                var eventData = buffer.Substring(
                    dataStart + "data: ".Length,
                    dataEnd - dataStart - "data: ".Length
                );

                ProcessSingleEvent(eventData);
                processedLength = dataEnd + 2;

                // 立即处理而不是累积所有事件
                _dataBuffer.Remove(0, processedLength);
                buffer = _dataBuffer.ToString();
                processedLength = 0;
            }

            _dataBuffer.Remove(0, processedLength);
        }

        private void ProcessSingleEvent(string eventData)
        {
            if (string.IsNullOrWhiteSpace(eventData) || eventData == "[DONE]")
                return;

            try
            {
                using var doc = JsonDocument.Parse(eventData, _jsonOptions);
                var delta = GetDeltaContent(doc);
                UpdateResponseContent(delta);
            }
            catch (JsonException ex)
            {
                Trace.WriteLine($"JSON解析失败: {ex.Message}");
                Trace.WriteLine($"原始数据: {eventData}");
            }
        }

        private string? GetDeltaContent(JsonDocument doc)
        {
            try
            {
                return doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("delta")
                    .GetProperty("content")
                    .GetString();
            }
            catch
            {
                return null;
            }
        }

        private void UpdateResponseContent(string? delta)
        {
            if (string.IsNullOrEmpty(delta)) return;
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (CurrentResponseContent == null)
                {
                    CurrentResponseContent = new MessageContent(this)
                    {
                        Role = "assistant",
                        Content = delta
                    };
                    CurrentConversations.Add(CurrentResponseContent);
                }
                else
                {
                    CurrentResponseContent.AppendContent(delta);
                }

                // 强制UI立即刷新
                CommandManager.InvalidateRequerySuggested();
            }, System.Windows.Threading.DispatcherPriority.Render);
        }


        #endregion

        #region Context Management
        private List<MessageContent> TrimContext(List<MessageContent> messages)
        {
            var totalLength = messages.Sum(m => m.Content?.Length ?? 0);
            while (totalLength > MaxContextLength && messages.Count > 1)
            {
                messages.RemoveAt(0);
                totalLength = messages.Sum(m => m.Content?.Length ?? 0);
            }
            return messages;
        }

        private void SaveConversation(MessageContent input)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                CurrentConversations.Add(input);
            });
        }
        #endregion

        #region Helper Methods

        public List<Dictionary<string, List<MessageContent>>> GetConversationHistory()
        {
            return _userInfoDb.GetCollection<Dictionary<string, List<MessageContent>>>
                ("CommunicateHistories").Query().ToList();
        }

        public void SaveAndClearConversations()
        {
            var histories = GetConversationHistory();

            var currentKey = DateTime.Now.ToString();
            foreach (var itr in histories)
            {
                itr.Add(currentKey, CurrentConversations.ToList());
            }

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                CurrentConversations.Clear();
            });

            CurrentInputingContent.Content = "";

            _userInfoDb.GetCollection<Dictionary<string, List<MessageContent>>>
                ("CommunicateHistories").Update(histories);

            HistoryConversations = GetConversationHistory();
        }

        public void LoadHistory()
        {
            var collection = GetConversationHistory();
            HistoryConversations = collection ?? new List<Dictionary<string, List<MessageContent>>>();
        }


        public void OnHistorySelected()
        {
            if (SelectedHistory != null)
            {
                SaveAndClearConversations();
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    CurrentConversations = new ObservableCollection<MessageContent>(SelectedHistory.FirstOrDefault().Value);
                });
            }
        }


        public void CancelCurrentRequest()
        {
            _requestCanceller?.Cancel();
        }

        public void Dispose()
        {
            _requestCanceller?.Dispose();
            _userInfoDb.Dispose();
        }
        #endregion
    }

    #region Configuration Classes
    public class DeepSeekConfig : ObservableObject
    {
        private List<string> models = new List<string>() { "deepseek-chat", "deepseek-reasoner" };

        public List<string> Models
        {
            get => models;
        }

        private string model = "deepseek-reasoner";
        public string Model
        {
            get => model;
            set => SetProperty(ref model, value);
        }
        public float Temperature { get; set; } = 1.0f;
        public int MaxHistoryMessages { get; set; } = 10;
    }

    public class DSRequestBody
    {
        [JsonPropertyName("messages")]
        public List<MessageContent> Messages { get; set; } = new();

        [JsonPropertyName("model")]
        public string Model { get; set; } = "deepseek-chat";

        [JsonPropertyName("frequency_penalty")]
        public float? FrequencyPenalty { get; set; }

        [JsonPropertyName("presence_penalty")]
        public float? PresencePenalty { get; set; }

        [JsonPropertyName("max_tokens")]
        public int? MaxTokens { get; set; }

        [JsonPropertyName("temperature")]
        public float? Temperature { get; set; } = 1f;

        [JsonPropertyName("top_p")]
        public float? TopP { get; set; } = 1f;

        [JsonPropertyName("stream")]
        public bool? Stream { get; set; }

        [JsonPropertyName("stop")]
        public string? Stop { get; set; }
    }

    public class MessageContent : ObservableObject
    {

        private string uniqueId = "";
        public string UniqueID
        {
            get=> uniqueId;
            private set=>SetProperty(ref uniqueId,value);
        }

        DeepSeek? _parent = null;
        [JsonIgnore]
        public DeepSeek? Parent
        {
            get => _parent;
        }
        [JsonIgnore]
        public bool IsContentEmpty
        {
            get => Content?.IsNullOrEmpty() ?? true;
        }

        private string? _content;
        [JsonPropertyName("content")] // 明确指定JSON字段名
        public string? Content
        {
            get => _content;
            set
            {
                SetProperty(ref _content, value);

                OnPropertyChanged("IsContentEmpty");
            }
        }

        public void NotifyContentChanged()
        {
            OnPropertyChanged("Content");
            OnPropertyChanged("IsContentEmpty");
        }

        private string? _name;
        [JsonPropertyName("name")]
        public string? Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }


        private string? _role;
        [JsonPropertyName("role")]
        public string? Role
        {
            get => _role;
            set => SetProperty(ref _role, value);
        }

        public MessageContent(DeepSeek parent)
        {
            UniqueID = Guid.NewGuid().ToString();
            _parent = parent;
        }

        public MessageContent(MessageContent other)
        {
            UniqueID = Guid.NewGuid().ToString();
            _parent = other.Parent;
            _content = other.Content;
            _role = other.Role;
            _name = other.Name;
        }
        public void AppendContent(string delta)
        {
            _content += delta;
            OnPropertyChanged(nameof(Content));
            OnPropertyChanged(nameof(IsContentEmpty));
        }

    }

    public class DSResponseBody
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("choices")]
        public List<ResponseChoice>? Choices { get; set; }

        [JsonPropertyName("usage")]
        public ResponseUsage? Usage { get; set; }
    }

    public class ResponseChoice
    {
        [JsonPropertyName("delta")]
        public MessageContent? Delta { get; set; }
    }

    public class ResponseUsage
    {
        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
    #endregion
}