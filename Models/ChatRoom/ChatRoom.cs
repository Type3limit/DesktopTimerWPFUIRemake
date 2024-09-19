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
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopTimer.Models.ChatRoom
{
    public partial class ChatRoom : ObservableObject
    {
        #region Defination
        public static string MESSAGE_REG_AND_LOGIN ="[Reg&Login]";
        #endregion

        #region properties
        UdpClient udpClient = new UdpClient();

        [ObservableProperty]
        int port = 52530;

        [ObservableProperty]
        string curIpAddress = "";
        #endregion


        #region contructor
        MainWorkModel? curModelInstance = null;
        public ChatRoom(MainWorkModel modelInstance)
        {
            curModelInstance = modelInstance;

            WeakReferenceMessenger.Default.Register<ConfigReadComplecateMessage>(this, (o, e) =>
            {
                CurIpAddress = GetIpAddress();

                RegisterForAllUser();

            });
        }

        #endregion

        #region method

        #region base

        public string GetIpAddress(bool ipV6 = false)
        {
            string ipAddress = "";
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                AddressFamily comprare = AddressFamily.InterNetwork;
                if (ipV6)
                {
                    comprare = AddressFamily.InterNetworkV6;
                }
                if (ip.AddressFamily == comprare)
                {
                    ipAddress = ip.ToString();
                    break;
                }
            }
            return ipAddress;
        }


        ChatMessageBase GetMessageContent(MessageTypeEnum curMessageType= MessageTypeEnum.TextType)
        {
            return new ChatMessageBase()
            {
                IpAdress = CurIpAddress,
                MessageType = curMessageType
            };
        }


        async Task<int> SendMessage(ChatMessageBase messageToSend, IPEndPoint endPoint)
        {
            try
            {
                var sendStr = JsonSerializer.Serialize(messageToSend);

                byte[] sendBuffer = Encoding.UTF8.GetBytes(sendStr);

                var byteSend = await udpClient.SendAsync(sendBuffer, sendBuffer.Length, endPoint);

                Trace.WriteLine($"Broadcast message sent: [\n{sendStr}\n]");

                return byteSend;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return 0;
            }
        }
        #endregion


        #region server


        public async Task StartMessageListenning()
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, Port);

                Trace.WriteLine("Start Listening LAN Message...");

                while (true)
                {
                    UdpReceiveResult result = await udpClient.ReceiveAsync();  

                    string receivedMessage = Encoding.UTF8.GetString(result.Buffer);

                    
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

        }


        #endregion

        #region register

        public async void RegisterForAllUser()
        {
            udpClient.EnableBroadcast = true;

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, Port);

            var messageToSend = GetMessageContent();

            messageToSend.Payload= new UserInfo()
            {
                PayloadHeader = MESSAGE_REG_AND_LOGIN,
                NickName = curModelInstance.Config.UserConfigData.ChatNickName,
                AvatarUrl = curModelInstance.Config.UserConfigData.UserAvatarPath,
            };
            await SendMessage(messageToSend,endPoint);
        }
        #endregion



        #endregion
    }
}
