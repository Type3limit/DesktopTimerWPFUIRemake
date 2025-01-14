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
    public class RegLoginMessageHandler : IMessageHandler
    {
        public string AssignedMessageHeader { get => MessagePayload.MESSAGE_REG_AND_LOGIN; }

        public bool HandleMessage(ChatMessageBase curMessage)
        {
            try
            {
                var targetAddress = curMessage.IpAddress;
                var userInfoPayload = curMessage.Payload as UserInfo;
                if (userInfoPayload != null)
                {
                    WeakReferenceMessenger.Default.Send(new UpdateContactListMessage(curMessage));
                    return true;
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
