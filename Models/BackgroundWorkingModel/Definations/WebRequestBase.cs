using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DesktopTimer.Models.BackgroundWorkingModel.Definations
{
    public abstract class WebResponseBase : ObservableObject
    {
    }

    public abstract class WebRequestQueryBase : ObservableObject
    {

    }

    public abstract class WebRequestBase : ObservableObject
    {
        public abstract Type Type { get; }

        /// <summary>
        /// url of current request
        /// </summary>
        /// <returns></returns>
        public virtual string RequestUrl  =>  "";
        

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
        abstract public WebRequestQueryBase? BuildQuery(params object[]? objs);
        /// <summary>
        /// send request
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        abstract public Task<WebResponseBase?> Request(WebRequestQueryBase? query);

        /// <summary>
        /// parse result
        /// </summary>
        /// <param name="currentResponse"></param>
        /// <returns></returns>
        abstract public IAsyncEnumerable<object?> ParseResult(WebResponseBase? currentResponse);

    }
}
