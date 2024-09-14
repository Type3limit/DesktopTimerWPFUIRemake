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
    /// To request show main window
    /// </summary>
    public class RequestShowMainWindowMessage : TypedMessage<object>
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
        Program,
        Translate,
        
        ChatRoomCurrentUser,
        ChatRoomOtherUser,
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
    public class WebBrowserOperationMessage:TypedMessage<(BrowserOperation,string?)>
    {
        public WebBrowserOperationMessage(BrowserOperation var,string? url)
        {
            Value = (var, url);
        }
    }

    public class ThemeChangedMessage : TypedMessage<Wpf.Ui.Appearance.ApplicationTheme>
    {
        public ThemeChangedMessage(Wpf.Ui.Appearance.ApplicationTheme curTheme)
        {
            Value = curTheme;
        }
    }

    public class VideoPathChangedMessage:TypedMessage<string?>
    {
        public VideoPathChangedMessage(string? VideoPath)
        {
            Value = VideoPath;
        }
    }
    public class VideoSeekMessage : TypedMessage<TimeSpan>
    {
        public VideoSeekMessage(TimeSpan curPosition)
        {
            Value = curPosition;
        }
    }

    public enum VideoStatus
    {
        Stop,
        Play,
        Pause,
    }
    public class VideoStatusChangedMessage:TypedMessage<VideoStatus>
    {
        public VideoStatusChangedMessage(VideoStatus cur)
        {
            Value = cur;
        }
    }

    public class VideoVolumeChangedMessage : TypedMessage<double>
    {
        public VideoVolumeChangedMessage(double VolumeLevel)
        {
            Value = VolumeLevel;
        }
    }

    public enum VolumeShortOption
    {
        Mute,
        Increase,
        Decrease
    }

    public class VideoVolumeShortCutMessage : TypedMessage<VolumeShortOption>
    {
        public VideoVolumeShortCutMessage(VolumeShortOption opt)
        {
            Value = opt;
        }
    }

    public class VideoMoveNextMessage:TypedMessage<object>
    {
    }

    public class RequestOpenEverythingWindow : TypedMessage<object>
    { }

}
