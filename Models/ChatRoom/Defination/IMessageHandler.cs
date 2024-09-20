using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopTimer.Models.ChatRoom.Defination
{
    public interface IMessageHandler
    {
        public abstract string AssignedMessageHeader { get; }
        public abstract bool HandleMessage(ChatMessageBase curMessage);
    }
}
