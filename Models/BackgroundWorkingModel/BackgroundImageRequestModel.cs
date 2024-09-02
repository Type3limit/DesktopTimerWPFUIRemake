using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DesktopTimer.Helpers;
using DesktopTimer.models;
using DesktopTimer.Models.BackgroundWorkingModel.Definations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DesktopTimer.Models.BackgroundWorkingModel
{
    public class BackgroundImageRequestModel : ObservableObject
    {

        #region data
        private Dictionary<string, RequestBase> requestCollections = new Dictionary<string, RequestBase>();
        public Dictionary<string, RequestBase> RequestCollections
        {
            get => requestCollections;
            set => SetProperty(ref requestCollections, value);
        }


        private Dictionary<string, Type> requestTypes = new Dictionary<string, Type>()
        {
            { "WallHaven", typeof(WallHavenRequest) },
            { "Local",typeof(LocalBackground)},
        };
        #endregion

        #region properties


        private List<string> useableRequest = new List<string>();
        public List<string> UseableRequest
        {
            get => useableRequest;
            set => SetProperty(ref useableRequest, value);
        }

        private string? selectedRequest = "";
        public string? SelectedRequest
        {
            get => selectedRequest;
            set
            {
                SetProperty(ref selectedRequest, value);
                OnPropertyChanged("SelectedRequestInstance");
                WeakReferenceMessenger.Default.Send(new RequestModelChangedMessage(SelectedRequestInstance?.RequestUseage??RequestBaseUseage.NormalRequest));
            }
        }

        /// <summary>
        /// selected webType
        /// </summary>
        public RequestBase? SelectedRequestInstance => SelectedRequest.IsNullOrEmpty() ? null : RequestCollections[SelectedRequest];

        /// <summary>
        /// response of current request 
        /// </summary>
        ResponseBase? currentResponse = null;

        /// <summary>
        /// parsed results
        /// </summary>
        IAsyncEnumerable<object?>? results = null;


        CancellationTokenSource? RequestCanceller = null;


        bool DisableAutoPageIncrease = false;
        #endregion


        #region constructor
        MainWorkModel? currentModelInstance = null;
        public BackgroundImageRequestModel(MainWorkModel modelInstance)
        {
            currentModelInstance = modelInstance;

            ReadWebTypes();

            WriteWebTypes();


            WeakReferenceMessenger.Default.Register<RequestAbandonCurrentCacheMessage>(this, (e, t) => 
            {
                if(t.Value==0)
                {
                    if (RequestCanceller != null)
                    {
                        RequestCanceller.Cancel();
                    }
                    currentResponse = null;
                    results = null;
                    
                }
                DisableAutoPageIncrease = true;
            });
        }


        #endregion


        #region command

        private ICommand? startRequestCommand = null;
        public ICommand? StartRequestCommand
        {
            get => startRequestCommand ?? (startRequestCommand = new RelayCommand(() =>
            {
                QueryWebRequestCache();
            }));
        }

        #endregion

        #region methods


        #region init
        private RequestBase? CreateInstance(Type type)
        {

            var constructor = type?.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                return null;
            }

            var newExpression = Expression.New(constructor);
            var lambda = Expression.Lambda<Func<RequestBase>>(newExpression).Compile();
            return lambda();
        }

        private void ReadWebTypes()
        {
            var webConfig = FileMapper.BackTypesJson.ReadText();
            if (webConfig.IsNullOrEmpty())
                return;
            JsonSerializerOptions opt = new JsonSerializerOptions()
            {
                Converters = { new TypeJsonConverter() }
            };
            var res = JsonSerializer.Deserialize<Dictionary<string, Type>>(webConfig, opt);
            if (res == null || res.Count < 0)
                return;
            requestTypes = res;
        }

        private void WriteWebTypes()
        {
            JsonSerializerOptions opt = new JsonSerializerOptions()
            {
                Converters = { new TypeJsonConverter() }
            };
            using (var stream = System.IO.File.Open(FileMapper.BackTypesJson, System.IO.FileMode.OpenOrCreate))
            {
                stream.Flush();
                JsonSerializer.Serialize(stream, requestTypes, opt);
            }
        }

        public void Initialize()
        {
            try
            {
                foreach (var requestType in requestTypes)
                {
                    var instance = CreateInstance(requestType.Value);
                    if (instance != null)
                    {
                        instance.Initiliaze();
                        if (!RequestCollections.ContainsKey(requestType.Key))
                        {
                            RequestCollections.Add(requestType.Key, instance);
                            UseableRequest.Add(requestType.Key);
                        }
                    }
                }
                SelectedRequest = UseableRequest?.FirstOrDefault();

                WeakReferenceMessenger.Default.Register<RequestFillBackgroundMessage>(this, (t, mes) => 
                {
                    DisplaySetting_RequestFillBackgroundHandler(mes.Value);
                });

            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Initialize with exception {ex}");
            }
        }

        private void DisplaySetting_RequestFillBackgroundHandler(int missingCount)
        {
            for (int i = 0; i < RequestCollections.Count; i++)
            {
                QueryWebRequestCache();
            }
        }
        #endregion

        #region cache

        async void QueryWebRequestCache()
        {
            Trace.WriteLine("Start request cache");
            try
            {
                if(RequestCanceller!=null)
                {
                    RequestCanceller.Cancel();
                }
                RequestCanceller= new CancellationTokenSource();
                if (SelectedRequestInstance == null )
                    return;
                if (currentResponse == null)
                {
                    var query = SelectedRequestInstance.BuildQuery(!DisableAutoPageIncrease);
                    currentResponse = await SelectedRequestInstance.Request(query);
                    if (currentResponse == null)//request failed!
                        return;
                    results = SelectedRequestInstance.ParseResult(currentResponse,RequestCanceller.Token);
                    if(DisableAutoPageIncrease)
                        DisableAutoPageIncrease = false;
                }

                if (results != null)
                {
                    await foreach (var itr in results.WithCancellation(RequestCanceller.Token))
                    {
                        if(itr == null&& SelectedRequestInstance.HasReachedEnd(currentResponse))//when reaching end ,reset request to the first
                        {
                            SelectedRequestInstance.ResetRequest();
                            break;
                        }
                        else
                        {
                            if (itr is string str)
                               WeakReferenceMessenger.Default.Send(new BackgroundSourceUpdateMessage(str));
                        }
                    }
                }
            }
            catch(OperationCanceledException )
            {
                Trace.WriteLine("requst cancelled");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"query cache with exception :{ex}");
            }
            finally
            {
                currentResponse = null;
                Trace.WriteLine("Request cache finished");
            }
        }


        #endregion

        #endregion
    }
}
