using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DesktopTimer.Helpers;
using DesktopTimer.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DesktopTimer.Models.BackgroundWorkingModel.Definations
{
    public enum RequestBaseUseage
    {
        NormalRequest,
        PictureBackground,
        VideoBackground,
        WebsiteBackground
    }

    public  class ResponseBase : ObservableObject
    {
    }

    public  class RequestQueryBase : ObservableObject
    {
    }

    public abstract class RequestBase : ObservableObject
    {
        public abstract Type Type { get; }

        public virtual RequestBaseUseage RequestUseage  => RequestBaseUseage.NormalRequest;

        public static string DisplayName => "（无）";

        public MainWorkModel? ModelInstance = null;
        /// <summary>
        /// url of current request
        /// </summary>
        /// <returns></returns>
        protected string requestUrl = "";
        public virtual string RequestUrl
        {
            get=> requestUrl;
            set=>SetProperty(ref requestUrl,value);
        }
        

        /// <summary>
        /// Initiliazer for current request base type
        /// </summary>
        public virtual void Initiliaze()
        {

        }

        /// <summary>
        /// reset request
        /// </summary>
        abstract public void ResetRequest();

        /// <summary>
        /// build query
        /// </summary>
        /// <returns></returns>
        abstract public RequestQueryBase? BuildQuery(bool AutoIncreasePage ,params object[]? objs);
        /// <summary>
        /// send request
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        abstract public Task<ResponseBase?> Request(RequestQueryBase? query);

        /// <summary>
        /// parse result
        /// </summary>
        /// <param name="currentResponse"></param>
        /// <returns></returns>
        abstract public IAsyncEnumerable<object?> ParseResult(ResponseBase? currentResponse, CancellationToken canceller);

        /// <summary>
        /// mark if current response is last page
        /// </summary>
        /// <param name="currentResponse"></param>
        /// <returns></returns>
        abstract public bool HasReachedEnd(ResponseBase? currentResponse);


        protected ICommand? requestNewQueryCommand = null;
        /// <summary>
        /// update current query
        /// </summary>
        public virtual ICommand RequestNewQueryCommand
        {
            get => requestNewQueryCommand ?? (requestNewQueryCommand = new RelayCommand(() =>
            {
                WeakReferenceMessenger.Default.Send(new RequestAbandonCurrentCacheMessage(0));
                WeakReferenceMessenger.Default.Send(new RequestAbandonCurrentCacheMessage(1));
            }));
        }
    }
}
