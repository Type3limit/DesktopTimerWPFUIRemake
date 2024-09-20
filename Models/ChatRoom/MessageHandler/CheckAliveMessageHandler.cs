using DesktopTimer.Models.ChatRoom.Defination;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopTimer.Models.ChatRoom.MessageHandler
{
    public class CheckAliveMessageHandler : IMessageHandler
    {

        public string AssignedMessageHeader => MessagePayload.MESSAGE_CHECK_ALIVE;

        public bool HandleMessage(ChatMessageBase curMessage)
        {
            try
            {
                var targetAddress = curMessage.IpAddress;
                var payload = curMessage.Payload as AliveCheck;
                if (payload != null)
                {
                    if(payload.Direction>0)//send to here
                    {
                        SendMessage();
                    }
                    else if(payload.Direction<0)  //other client reply
                    {

                    }
                }
                return false;
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex);
                return false;
            }
        
        }

        public bool SendMessage()
        {
            throw new NotImplementedException();
        }
    }
}
