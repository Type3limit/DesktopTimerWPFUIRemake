using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopTimer.Models.ChatRoom.Defination
{


    public class ChatMessageBase 
    {
        public static string DefaultProtocolType = "DesktopTimer_LANChat_Protocol";

        public string? ProtocolType = DefaultProtocolType;

        public string? IpAddress;

        public MessagePayload? Payload;
    }

    public class MessagePayload
    {
        /// <summary>
        /// check if target machine still alive
        /// </summary>
        public static string MESSAGE_CHECK_ALIVE = @"[CheckAlive]";
        /// <summary>
        /// regist and login 
        /// </summary>
        public static string MESSAGE_REG_AND_LOGIN = @"[RegLogin]";

        public static List<string> MESSAGE_TYPES= new List<string>()
        {
            MESSAGE_CHECK_ALIVE,
            MESSAGE_REG_AND_LOGIN,
        };

        public virtual string? PayloadHeader
        {
            get;
            set;
        }
    }

    /// <summary>
    /// mark user 
    /// </summary>
    public class UserInfo : MessagePayload
    {
        /// <summary>
        /// user nick name
        /// </summary>
        public string? NickName;

        /// <summary>
        /// base 64 image
        /// </summary>
        public string? Avater;
    }

    public class AliveCheck:MessagePayload
    {
        public int Direction = 0;
    }
}
