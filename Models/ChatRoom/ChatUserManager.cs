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

namespace DesktopTimer.Models.ChatRoom
{

    public partial class ChatUserManager : ObservableObject
    {
        #region properties

        ObservableCollection<UserAppearance> users = new ObservableCollection<UserAppearance>();
        /// <summary>
        /// restore all avaliable user
        /// </summary>
        public ObservableCollection<UserAppearance> Users
        {
            get => users;
            set => SetProperty(ref users, value);
        }


        LiteDatabase UserInfoDB = new LiteDatabase(FileMapper.ChatRoomUserInfoDBFile);
        #endregion


        #region constructor

        public ChatUserManager()
        {
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
        }


        ~ChatUserManager()
        {
            UserInfoDB?.Dispose();
        }

        #endregion

        #region methods
        public UserAppearance? BuildFromMessage(ChatMessageBase? curMessage)
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


            var appearance = BuildFromMessage(curMessage);
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
        bool online = true;
        public void LoadAvatar()
        {

            UserAvatarImage = AvatarOriginData?.DecompressBrotli()?.ImageFromBase64()?.ToImageSource();
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
