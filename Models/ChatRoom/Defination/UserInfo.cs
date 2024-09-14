using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopTimer.Models.ChatRoom.Defination
{
    public partial class UserInfo:ObservableObject
    {
        [ObservableProperty]
        public string? protocolType = "DesktopTimerLANChatProtocol";

        [ObservableProperty]
        public string? nickName;

        [ObservableProperty]
        public string? ip;

        [ObservableProperty]
        public string? avatarUrl;
    }
}
