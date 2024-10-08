﻿using CommunityToolkit.Mvvm.ComponentModel;
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
            { WallHavenRequest.DisplayName, typeof(WallHavenRequest) },
            { LocalBackground.DisplayName,typeof(LocalBackground)},
            { LocalVideo.DisplayName,typeof(LocalVideo) },
            { WebBrowser.DisplayName,typeof(WebBrowser) },
            { StableDiffusion.DisplayName,typeof(StableDiffusion) }
        };
        #endregion

        #region properties


        private List<string> useableRequest = new List<string>();
        public List<string> UseableRequest
        {
            get => useableRequest;
            set => SetProperty(ref useableRequest, value);
        }

        bool DisableSelectionNotice = false;

        private string? selectedRequest = "";
        public string? SelectedRequest
        {
            get => selectedRequest;
            set
            {
                SetProperty(ref selectedRequest, value);
                if (!DisableSelectionNotice)
                {
                    WeakReferenceMessenger.Default.Send(new RequestModelChangedMessage(SelectedRequestInstance?.RequestUseage ?? RequestBaseUseage.NormalRequest));
                    if (currentModelInstance != null && !(value.IsNullOrEmpty()))
                    {
                        currentModelInstance.Config.UserConfigData.LastRequestBaseType = SelectedRequestInstance?.Type;
                        WeakReferenceMessenger.Default.Send(new RequestSaveConfigMessage(ConfigType.User));
                    }
                }
                OnPropertyChanged("SelectedRequestInstance");
                OnPropertyChanged("IsWebBackground");
                OnPropertyChanged("IsVideoBackground");
                OnPropertyChanged("IsPictureBackground");
            }
        }

        public bool IsWebBackground
        {
            get => SelectedRequestInstance?.RequestUseage == RequestBaseUseage.WebsiteBackground;
        }

        public bool IsVideoBackground
        {
            get => SelectedRequestInstance?.RequestUseage == RequestBaseUseage.VideoBackground;
        }
        public bool IsPictureBackground
        {
            get => SelectedRequestInstance?.RequestUseage == RequestBaseUseage.PictureBackground;
        }

        /// <summary>
        /// selected webType
        /// </summary>
        public RequestBase? SelectedRequestInstance => SelectedRequest.IsNullOrEmpty() ? null : RequestCollections[SelectedRequest];

        /// <summary>
        /// response of current request 
        /// </summary>
        IResponseBase? currentResponse = null;

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
                if (t.Value == 0)
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

            WeakReferenceMessenger.Default.Register<ConfigReadComplecateMessage>(this, (e, t) =>
            {
                SelectedRequest = RequestCollections.FirstOrDefault(x =>
                    x.Value.Type == currentModelInstance.Config.UserConfigData.LastRequestBaseType).Key;
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


        private ICommand? nextVideoCommand = null;
        public ICommand? NextVideoCommand
        {
            get => nextVideoCommand ?? (nextVideoCommand = new RelayCommand(() =>
            {
                if (IsVideoBackground)
                {

                    WeakReferenceMessenger.Default.Send(new VideoMoveNextMessage());
                }
            }));
        }
        #endregion

        #region methods


        #region init

        public RequestBase? FindInstance(Type curType)
        {
            return RequestCollections.FirstOrDefault(x => x.Value.Type == curType).Value;
        }

        private RequestBase? CreateInstance(Type type)
        {
            var constructor = type?.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                return null;
            }

            var newExpression = Expression.New(constructor);
            var lambda = Expression.Lambda<Func<RequestBase>>(newExpression).Compile();
            var target = lambda();
            target.ModelInstance = currentModelInstance;
            return target;
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
            foreach (var itr in res)
            {
                if (!requestTypes.ContainsKey(itr.Key))
                {
                    requestTypes.Add(itr.Key, itr.Value);
                }
            }
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
                DisableSelectionNotice = true;
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
            finally
            {
                DisableSelectionNotice = false;
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

        void QueryWebRequestCache()
        {

            try
            {
                if (SelectedRequestInstance == null)
                    return;
                Action? resAct = SelectedRequestInstance.RequestUseage switch
                {
                    RequestBaseUseage.PictureBackground => OnPictureType,
                    _ => null
                };
                resAct?.Invoke();
            }
            catch (OperationCanceledException)
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
            }
        }

        async void OnPictureType()
        {
            try
            {
                if (SelectedRequestInstance == null)
                    return;

                if (RequestCanceller != null)
                {
                    RequestCanceller.Cancel();
                }
                RequestCanceller = new CancellationTokenSource();

                if (SelectedRequestInstance.WithAutomaticOption)
                {
                    if (currentResponse == null)
                    {
                        var query = SelectedRequestInstance.BuildQuery(!DisableAutoPageIncrease);
                        currentResponse = await SelectedRequestInstance.Request(query);
                        if (currentResponse == null)
                            return;
                        results = SelectedRequestInstance.ParseResult(currentResponse, RequestCanceller?.Token??CancellationToken.None);
                        if (DisableAutoPageIncrease)
                            DisableAutoPageIncrease = false;
                    }

                    if (results != null)
                    {
                        await foreach (var itr in results.WithCancellation(RequestCanceller?.Token?? CancellationToken.None))
                        {
                            if (itr == null && SelectedRequestInstance.HasReachedEnd(currentResponse))//when reaching end ,reset request to the first
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
                else
                {
                    if (SelectedRequestInstance.AfterWhenOneRequestReadyAct == null)
                    {
                        SelectedRequestInstance.AfterWhenOneRequestReadyAct = (itr) =>
                        {
                            if (itr is string str)
                                WeakReferenceMessenger.Default.Send(new BackgroundSourceUpdateMessage(str));
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                RequestCanceller = null;
            }
        }



        #endregion

        #endregion
    }
}
