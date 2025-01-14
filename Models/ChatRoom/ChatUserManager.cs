using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DesktopTimer.Helpers;
using DesktopTimer.Models.ChatRoom.Defination;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LiteDB;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.Json;
using System.Net;

namespace DesktopTimer.Models.ChatRoom
{

    public partial class ChatUserManager : ObservableObject
    {
        #region properties


        ChatRoom? chatRoomInstance = null;

        ObservableCollection<UserAppearance> users = new ObservableCollection<UserAppearance>();
        /// <summary>
        /// restore all avaliable user
        /// </summary>
        public ObservableCollection<UserAppearance> Users
        {
            get => users;
            set => SetProperty(ref users, value);
        }


        public LiteDatabase UserInfoDB = new LiteDatabase(FileMapper.ChatRoomUserInfoDBFile);


        [ObservableProperty]
        int updateContractListCount = 5;//every 5 seconds check alive

        int CurrentContractUpdateCount = 0;
        #endregion


        #region constructor


        public ChatUserManager(ChatRoom roomInstance)
        {

            chatRoomInstance = roomInstance;

            WeakReferenceMessenger.Default.Register<UpdateContactListMessage>(this, (e, t) => UpdateContactList(t.Value));

            WeakReferenceMessenger.Default.Register<ConfigReadComplecateMessage>(this, (e, t) =>
            {
                LoadContactListFromDB();
            });

            BsonMapper.Global.RegisterType<UserAppearance>(
              serialize: obj =>
              {
                  var doc = new BsonDocument();
                  doc["Avator"] = obj.AvatarOriginData;
                  doc["NickName"] = obj.NickName;
                  doc["IpAddress"] = obj.IpAddress;
                  return doc;
              },
              deserialize: doc => new UserAppearance()
              {
                  AvatarOriginData = doc["Avator"].AsString,
                  NickName = doc["NickName"].AsString,
                  IpAddress = doc["IPAddress"].AsString
              });


            WeakReferenceMessenger.Default.Register<TimeUpdateMessage>(this, (e, t) =>
            {
                ++CurrentContractUpdateCount;
                if (CurrentContractUpdateCount >= UpdateContractListCount)
                {
                    CurrentContractUpdateCount = 0;
                    SendAliveCheck();
                }
            });

            WeakReferenceMessenger.Default.Register<RequestSendChatMessage>(this, (e, t) => 
            {
                if(t?.Value?.Payload?.PayloadHeader == MessagePayload.MESSAGE_CHECK_ALIVE)
                {
                    ReplyAliveCheck(t.Value);
                }
            });

            WeakReferenceMessenger.Default.Register<UpdateAliveStatusMessage>(this, (e, t) =>
            {
                if(t.Value!=null)
                {
                    var target = Users.FirstOrDefault(x => x.IpAddress == t.Value.IpAddress);
                    if(target!=null)
                    {
                        target.Online = true;
                    }
                }
            });
        }


        ~ChatUserManager()
        {
            UserInfoDB?.Dispose();
        }

        #endregion

        #region methods

        #region userUpdate
        /// <summary>
        /// update info in database
        /// </summary>
        /// <param name="curInfo"></param>
        public void UpdateDBInfo(UserAppearance curInfo)
        {
            var collections = UserInfoDB.GetCollection<UserAppearance>("ChatUsers");

            collections.EnsureIndex(x => x.IpAddress);

            var existOne = collections.FindOne(x => x.IpAddress == curInfo.IpAddress);
            if (existOne != null)
            {

                existOne.IpAddress = curInfo.IpAddress;
                existOne.NickName = curInfo.NickName;
                existOne.AvatarOriginData = curInfo.AvatarOriginData;
                collections.Update(existOne);
            }
            else
            {
                collections.Insert(curInfo);
            }
        }

        /// <summary>
        /// update user list when a chat message incoming.
        /// </summary>
        /// <param name="curMessage"></param>
        public void UpdateContactList(ChatMessageBase? curMessage)
        {
            if (curMessage == null ||
                curMessage?.Payload?.PayloadHeader != MessagePayload.MESSAGE_REG_AND_LOGIN)
            {
                return;
            }

            var curInfo = (curMessage.Payload as UserInfo);
            if (curInfo == null)
                return;


            var appearance = BuildUserAppearanceFromMessage(curMessage);
            if (appearance == null)
                return;
            UpdateDBInfo(appearance);
            var existOne = Users.FirstOrDefault(x => x.IpAddress == curMessage.IpAddress);
            var existIndex = -1;
            if (existOne != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    existIndex = Users.IndexOf(existOne);
                    Users.Remove(existOne);
                });
            }

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                appearance.LoadAvatar();
                if (existIndex >= 0)
                {
                    Users.Insert(existIndex, appearance);
                }
                else
                {
                    Users.Add(appearance);
                }
            });
        }

        /// <summary>
        /// load users from data base
        /// </summary>
        public void LoadContactListFromDB()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Users.Clear();
            });

            var collections = UserInfoDB.GetCollection<UserAppearance>("ChatUsers");
            foreach (var itr in collections.Query().ToList())
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    itr.LoadAvatar();
                    Users.Add(itr);
                });
            }
        }
        #endregion

        #region common

        /// <summary>
        /// build user appearance info from chat message
        /// </summary>
        /// <param name="curMessage"></param>
        /// <returns></returns>
        public UserAppearance? BuildUserAppearanceFromMessage(ChatMessageBase? curMessage)
        {
            if (curMessage == null || !(curMessage.Payload is UserInfo curInfo))
                return null;

            return new UserAppearance()
            {
                AvatarOriginData = curInfo.Avater,
                NickName = curInfo.NickName,
                IpAddress = curMessage.IpAddress
            };
        }

        private void SendMessage(string ipAddress, ChatMessageBase replyMessage)
        {
            if (IPAddress.TryParse(ipAddress, out var curAddress))
            {
                IPEndPoint endPoint = new IPEndPoint(curAddress, chatRoomInstance?.Port ?? 52530);
                _ = chatRoomInstance?.SendMessage(replyMessage, endPoint);
            }
        }

        #endregion

        #region Alive check
        /// <summary>
        /// check user list alive status
        /// </summary>
        public void SendAliveCheck()
        {
            foreach (var itr in Users)
            {
                SendAliveCheck(itr);
                ++ itr.OfflineCount;
                itr?.OnlineStatusChanged();
            }
        }
        /// <summary>
        /// check target user alive status
        /// </summary>
        /// <param name="target"></param>
        public void SendAliveCheck(UserAppearance target)
        {
            if(target.IpAddress?.IsNullOrEmpty()!=false)
                return;
            var curMessage = new ChatMessageBase();
            curMessage.Payload = new AliveCheck()
            {
                Direction = 1 // send to other
            };
            SendMessage (target.IpAddress,curMessage);
        }

        /// <summary>
        /// reply alive check message
        /// </summary>
        /// <param name="curMessage"></param>
        public void ReplyAliveCheck(ChatMessageBase curMessage)
        {
            if (curMessage?.IpAddress?.IsNullOrEmpty() != false)
                return;
            var replyMessage = new ChatMessageBase();
            replyMessage.Payload = new AliveCheck()
            {
                Direction = -1 // reply
            };
            SendMessage(curMessage.IpAddress, replyMessage);
        }

        #endregion

        #endregion
    }

    public partial class UserAppearance : ObservableObject
    {
        [ObservableProperty]
        string? nickName;

        [ObservableProperty]
        ImageSource? userAvatarImage;

        [ObservableProperty]
        string? ipAddress;

        [ObservableProperty]
        string? avatarOriginData;

        [ObservableProperty]
        int offlineCount = 0;
        public bool Online
        {
            get => OfflineCount >= 2;
            set
            {
                OfflineCount = value ? 2 : 0;
            }
        }

        public void LoadAvatar()
        {
            try
            {

                UserAvatarImage = AvatarOriginData?.DecompressBrotli()?.ImageFromBase64()?.ToImageSource();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public void OnlineStatusChanged()
        {
            OnPropertyChanged("Online");
        }
    }
    public static class ImageExtensions
    {
        public static ImageSource ToImageSource(this System.Drawing.Image image)
        {
            using (var memoryStream = new MemoryStream())
            {
                image.Save(memoryStream, ImageFormat.Png);
                memoryStream.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}
