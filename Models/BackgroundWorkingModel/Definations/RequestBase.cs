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

    public interface IResponseBase
    {

    }

    public interface IRequestQueryBase
    {

    }

    public abstract class RequestBase : ObservableObject
    {
        public abstract Type Type { get; }

        public virtual RequestBaseUseage RequestUseage => RequestBaseUseage.NormalRequest;

        public static string DisplayName => "（无）";

        protected bool withAutomaticOption = false;
        /// <summary>
        /// mark if current request type can load sources automaticly
        /// </summary>
        public virtual bool WithAutomaticOption
        {
            get => withAutomaticOption;
            set => SetProperty(ref withAutomaticOption, value);
        }

        public MainWorkModel? ModelInstance = null;
        /// <summary>
        /// url of current request
        /// </summary>
        /// <returns></returns>
        protected string requestUrl = "";
        public virtual string RequestUrl
        {
            get => requestUrl;
            set => SetProperty(ref requestUrl, value);
        }

        /// <summary>
        /// if current type is not an automatic type, call this when every things done.
        /// </summary>
        /// <param name="afterWhenReady"></param>
        public virtual Action<object?>? AfterWhenOneRequestReadyAct { set; get; } = null;

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
        abstract public IRequestQueryBase? BuildQuery(bool AutoIncreasePage, params object[]? objs);
        /// <summary>
        /// send request
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        abstract public Task<IResponseBase?> Request(IRequestQueryBase? query);

        /// <summary>
        /// parse result
        /// </summary>
        /// <param name="currentResponse"></param>
        /// <returns></returns>
        abstract public IAsyncEnumerable<object?> ParseResult(IResponseBase? currentResponse, CancellationToken canceller);

        /// <summary>
        /// mark if current response is last page
        /// </summary>
        /// <param name="currentResponse"></param>
        /// <returns></returns>
        abstract public bool HasReachedEnd(IResponseBase? currentResponse);


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
                if (!WithAutomaticOption)
                {
                    RequestSourceOnce();
                }
            }));
        }

        /// <summary>
        /// request a fully request
        /// </summary>
        /// <returns></returns>
        public virtual Task<List<object?>?> RequestSourceCore()
        {
            return null;
        }

        /// <summary>
        /// request current source once
        /// </summary>
        public virtual async void RequestSourceOnce()
        {
            var itrs = await RequestSourceCore();
            if(itrs!=null)
            {
                foreach (var itr in itrs)
                {
                    AfterWhenOneRequestReadyAct?.Invoke(itr);
                }
            }

        }
        /// <summary>
        /// mark if current request can send next request
        /// </summary>
        /// <returns></returns>
        public virtual bool CanDoNext()
        {
            return true;
        }
    }
}
