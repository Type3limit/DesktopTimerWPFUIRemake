using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DesktopTimer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DesktopTimer.Models.BackgroundWorkingModel.Definations
{
    public class WebBrowser : RequestBase
    {
        public override Type Type => typeof(WebBrowser);


        public override RequestBaseUseage RequestUseage => RequestBaseUseage.WebsiteBackground;

        public static new string DisplayName => "网页背景";

        public string? requstUrl = "";
        public new string? RequestUrl
        {
            get=>requstUrl;
            set=>SetProperty(ref requstUrl,value);
        }


        private ICommand? browserOptionCommand = null;
        public ICommand? BrowserOptionCommand
        {
            get=> browserOptionCommand = new RelayCommand<string?>((opt) => 
            {
                BrowserOperation operation = opt switch 
                {
                    "StepForward"=> BrowserOperation.StepForward,
                    "StepBack" => BrowserOperation.StepBack,
                    "Flush" => BrowserOperation.Flush,
                    _=>BrowserOperation.Invalid,
                };

                WeakReferenceMessenger.Default.Send(new WebBrowserOperationMessage(operation,RequestUrl));
            });
        }


        public override IRequestQueryBase? BuildQuery(bool AutoIncreasePage, params object[]? objs)
        {
            return null;
        }

        public override bool HasReachedEnd(IResponseBase? currentResponse)
        {
            return false;
        }

        public override IAsyncEnumerable<object?> ParseResult(IResponseBase? currentResponse, CancellationToken canceller)
        {
            return null;
        }

        public override Task<IResponseBase?> Request(IRequestQueryBase? query)
        {
            return null;
        }

        public override void ResetRequest()
        {
        }
    }
}
