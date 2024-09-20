using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DesktopTimer.Helpers;
using DesktopTimer.Models.ChatRoom.Defination;
using DesktopTimer.Models.ChatRoom.MessageHandler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopTimer.Models.ChatRoom
{
    public partial class ChatRoom : ObservableObject
    {
        #region Defination

        #endregion

        #region properties
        UdpClient udpClient = new UdpClient();

        [ObservableProperty]
        int port = 52530;

        [ObservableProperty]
        string curIpAddress = "";

        CancellationTokenSource ListenningCanceller = new CancellationTokenSource();

        #endregion


        #region contructor
        MainWorkModel? curModelInstance = null;
        public ChatRoom(MainWorkModel modelInstance)
        {
            curModelInstance = modelInstance;

            MessageHandlerFactory.LoadAllHandlers();

            WeakReferenceMessenger.Default.Register<ConfigReadComplecateMessage>(this, async(o, e) =>
            {
                CurIpAddress = GetIpAddress();

                await StartMessageListenning();

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


        ChatMessageBase GetMessageContent() => new ChatMessageBase(){IpAddress = CurIpAddress};

        async Task<int> SendMessage(ChatMessageBase messageToSend, IPEndPoint endPoint)
        {
            try
            {
                var sendStr = JsonSerializer.Serialize(messageToSend);

                byte[] sendBuffer = Encoding.UTF8.GetBytes(sendStr);

                var byteSend = await udpClient.SendAsync(sendBuffer, sendBuffer.Length, endPoint);

                Trace.WriteLine($"Message sent: [\n{sendStr}\n]");

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

                while (!ListenningCanceller.IsCancellationRequested)
                {
                    UdpReceiveResult result = await udpClient.ReceiveAsync(ListenningCanceller.Token);  

                    string receivedMessage = Encoding.UTF8.GetString(result.Buffer);

                    var curContent = JsonSerializer.Deserialize<ChatMessageBase>(receivedMessage);

                    if(curContent==null)
                    {
                        Trace.WriteLine($"unreadable message recived : {receivedMessage}");
                        continue;
                    }
                    else
                    {
                        try
                        {
                            MessageHandlerFactory.GetHandler(curContent?.Payload?.PayloadHeader)?.HandleMessage(curContent);
                        }
                        catch (Exception nex)
                        {
                            Trace.WriteLine(nex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

        }

        public void StopMessageListenning()
        {
            ListenningCanceller.Cancel();
        }


        #endregion

        #region register

        public async void RegisterForAllUser()
        {
            udpClient.EnableBroadcast = true;

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, Port);

            var messageToSend = GetMessageContent();

            var curInfo = new UserInfo()
            {
                PayloadHeader = MessagePayload.MESSAGE_REG_AND_LOGIN,
                NickName = curModelInstance.Config.UserConfigData.ChatNickName,
            };
            if(curModelInstance.Config.UserConfigData.UserAvatarPath.IsFileExist())
            {
                var curBase64Str = await curModelInstance.Config.UserConfigData.UserAvatarPath.ConvertFileToBase64();
                curInfo.Avater = curBase64Str.CompressBrotli();
            }
            messageToSend.Payload = curInfo;
            await SendMessage(messageToSend,endPoint);
        }
        #endregion



        #endregion
    }
}
