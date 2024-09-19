using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopTimer.Models.ChatRoom.Defination
{
    public enum MessageTypeEnum
    {
        TextType,
        ImageType,
        FileType
    }

    public class ChatMessageBase 
    {
        public static string DefaultProtocolType = "DesktopTimer_LANChat_Protocol";

        public string? ProtocolType = DefaultProtocolType;

        public MessageTypeEnum MessageType = MessageTypeEnum.TextType;

        public string? IpAdress;

        public MessagePayload? Payload;
    }

    public class MessagePayload
    {
        public virtual string? PayloadHeader
        {
            get;
            set;
        }
    }


    public class UserInfo : MessagePayload
    {

        public string? NickName;

        public string? AvatarUrl;
    }
}
