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


}
