using CommunityToolkit.Mvvm.Messaging;
using DesktopTimer.Helpers;
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
                        WeakReferenceMessenger.Default.Send(new RequestSendChatMessage(curMessage));
                    }
                    else if(payload.Direction<0)  //other client reply
                    {
                        WeakReferenceMessenger.Default.Send(new UpdateAliveStatusMessage(curMessage));
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
    }
}
