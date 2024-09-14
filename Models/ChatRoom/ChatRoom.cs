using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DesktopTimer.Helpers;
using DesktopTimer.Models.ChatRoom.Defination;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopTimer.Models.ChatRoom
{
    public partial class ChatRoom:ObservableObject
    {
        #region properties
        UdpClient udpClient = new UdpClient();

        [ObservableProperty]
        int port = 52530;


        UserInfo? currentUserInfo  = null;
        #endregion

        #region contructor

        public ChatRoom()
        {
            StartChatRoomUDPBoradCast();

            WeakReferenceMessenger.Default.Register<ConfigReadComplecateMessage>(this, (o, e) => 
            {
                
            });
        }

        #endregion

        #region method

        public void StartChatRoomUDPBoradCast()
        {
            udpClient.EnableBroadcast = true;  // 启用广播

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("255.255.255.255"), Port);  // 广播地址

            byte[] sendBuffer = Encoding.UTF8.GetBytes("DesktopTimer");
            udpClient.Send(sendBuffer, sendBuffer.Length, endPoint);  // 发送广播消息

            Trace.WriteLine("Broadcast message sent: " );
        }

        #endregion
    }
}
