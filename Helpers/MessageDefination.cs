using DesktopTimer.Models.BackgroundWorkingModel.Definations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopTimer.Helpers
{

    public class TypedMessage<T>
    {
        private T? _value = default;

        public T? Value
        {
            get => _value;
            protected set=> _value = value;
        }
    }
    /// <summary>
    /// To request open setting
    /// </summary>
    public class RequestOpenSettingMessage: TypedMessage<bool>
    {

        public RequestOpenSettingMessage(bool openSetting)
        {
            Value = openSetting;
        }
    }
    /// <summary>
    /// To request fill current background message
    /// </summary>
    public class RequestFillBackgroundMessage:TypedMessage<int>
    {
        public RequestFillBackgroundMessage(int missingCount)
        {
            Value = missingCount;
        }
    }

    /// <summary>
    /// To request close current app
    /// </summary>
    public class RequestCloseProgramMessage : TypedMessage<object>
    { }


    /// <summary>
    /// To notify time update
    /// </summary>
    public class TimeUpdateMessage:TypedMessage<object>
    {
        public TimeUpdateMessage()
        {

        }
    }

    /// <summary>
    /// To notify background source has updated
    /// </summary>
    public class BackgroundSourceUpdateMessage : TypedMessage<string>
    {
        public BackgroundSourceUpdateMessage(string path)
        {
            Value = path;
        }
    }

    /// <summary>
    /// To notify abandon currnet background image cacahe
    /// </summary>
    public class RequestAbandonCurrentCacheMessage:TypedMessage<int>
    {
        public RequestAbandonCurrentCacheMessage(int step)
        {

        }
    }

    /// <summary>
    /// To notify current selected background request model change
    /// </summary>
    public class RequestModelChangedMessage:TypedMessage<RequestBaseUseage>
    {
        public RequestModelChangedMessage(RequestBaseUseage cur)
        {
            Value = cur;
        }
    }

    /// <summary>
    /// To notify that config file has readed
    /// </summary>
    public class ConfigReadComplecateMessage : TypedMessage<object>
    {

    }

    public enum ConfigType
    {
        User,
        Program
    }

    /// <summary>
    /// To notify to save config file
    /// (User: user related)
    /// (System:program related)
    /// </summary>
    public class RequestSaveConfigMessage : TypedMessage<ConfigType>
    {
        public RequestSaveConfigMessage(ConfigType var)
        {
            Value = var;
        }
    }

    public enum BrowserOperation
    {
        Invalid,
        StepBack,
        StepForward,
        Flush
    }
    public class WebBrowserOperationMessage:TypedMessage<BrowserOperation>
    {
        public WebBrowserOperationMessage(BrowserOperation var)
        {
            Value = var;
        }
    }

    public class ThemeChangedMessage : TypedMessage<Wpf.Ui.Appearance.ApplicationTheme>
    {
        public ThemeChangedMessage(Wpf.Ui.Appearance.ApplicationTheme curTheme)
        {
            Value = curTheme;
        }
    }
}
