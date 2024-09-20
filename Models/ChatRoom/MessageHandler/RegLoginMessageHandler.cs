using DesktopTimer.Models.ChatRoom.Defination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopTimer.Models.ChatRoom.MessageHandler
{
    public class RegLoginMessageHandler : IMessageHandler
    {
        public string AssignedMessageHeader { get => MessagePayload.MESSAGE_REG_AND_LOGIN; }

        public bool HandleMessage(ChatMessageBase curMessage)
        {
            throw new NotImplementedException();
        }
    }
}
