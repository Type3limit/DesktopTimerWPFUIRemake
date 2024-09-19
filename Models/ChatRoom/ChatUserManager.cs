using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DesktopTimer.Models.ChatRoom
{

    public partial class ChatUserManager:ObservableObject
    {
        #region properties

        ObservableCollection<UserAppearance> users = new ObservableCollection<UserAppearance>();
        /// <summary>
        /// restore all avaliable user
        /// </summary>
        public ObservableCollection<UserAppearance> Users
        {
            get=>users;
            set=>SetProperty(ref users, value);
        }

        #endregion


        #region constructor

        public ChatUserManager()
        {

        }

        #endregion
    }

    public partial class UserAppearance:ObservableObject
    {
        [ObservableProperty]
        string nickName ="";

        [ObservableProperty]
        ImageSource? userAvatarImage;

        [ObservableProperty]
        string? ipAddress;
    }
}
