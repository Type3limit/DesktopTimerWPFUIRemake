using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Appearance;
namespace DesktopTimer.Helpers
{
    public partial class ThemeHelper : ObservableObject
    {


        long TimeCount = 0;


        #region const datas

        static Dictionary<ApplicationTheme, SolidColorBrush> BackgroundBrushes = new Dictionary<ApplicationTheme, SolidColorBrush>()
        {
            { ApplicationTheme.Light, new SolidColorBrush(Color.FromRgb(248, 250, 251))},
            { ApplicationTheme.Dark, new SolidColorBrush(Color.FromRgb(2, 9, 41))}
        };

        static Dictionary<ApplicationTheme, SolidColorBrush> BorderBackgroundBrushes = new Dictionary<ApplicationTheme, SolidColorBrush>()
        {
            { ApplicationTheme.Light,new SolidColorBrush(Color.FromRgb(255,255,255))},
            { ApplicationTheme.Dark,new SolidColorBrush(Color.FromRgb(23, 24, 33))}
        };

        static Dictionary<ApplicationTheme, string> BorderImageUris = new Dictionary<ApplicationTheme, string>()
        {
            { ApplicationTheme.Light, @"pack://application:,,,/Assets/LightBackground.png"},
            { ApplicationTheme.Dark, @"pack://application:,,,/Assets/DarkBackground.png"}
        };

        static Dictionary<ApplicationTheme, SolidColorBrush> PartcleBackgroundPointBrushes = new Dictionary<ApplicationTheme, SolidColorBrush>()
        {
            {ApplicationTheme.Light,new SolidColorBrush(Colors.Gray) },
            {ApplicationTheme.Dark,new SolidColorBrush(Colors.LightGray) }
        };

        static Dictionary<ApplicationTheme, SolidColorBrush> TrayForegroundBrushes = new Dictionary<ApplicationTheme, SolidColorBrush>()
        {
            {ApplicationTheme.Light,new SolidColorBrush(Colors.Black) },
            {ApplicationTheme.Dark,new SolidColorBrush(Colors.White) }
        };

        #endregion

        #region properties
        [ObservableProperty]
        ImageSource? backgroundImage = null;

        [ObservableProperty]
        SolidColorBrush? backgroundBrush = BackgroundBrushes[ApplicationTheme.Light];

        [ObservableProperty]
        SolidColorBrush? borderBackgroundBrush = BorderBackgroundBrushes[ApplicationTheme.Light];

        [ObservableProperty]
        SolidColorBrush? particleBackgroundBrush = PartcleBackgroundPointBrushes[ApplicationTheme.Light];

        [ObservableProperty]
        SolidColorBrush? trayForegroundBrush = TrayForegroundBrushes[ApplicationTheme.Light];
        #endregion


        public ThemeHelper()
        {

            UpdateThemeBasedOnTime();

            WeakReferenceMessenger.Default.Register<TimeUpdateMessage>(this, (e, t) =>
            {

                if (TimeCount >= 3600)
                {
                    UpdateThemeBasedOnTime();//update Theme by 1 hour
                    TimeCount = 0;
                }
                TimeCount++;
            });
        }

        ~ThemeHelper()
        {
            WeakReferenceMessenger.Default.Unregister<TimeUpdateMessage>(this);
        }




        #region command

        private ICommand? changeThemeCommand = null;
        /// <summary>
        /// 更改主题
        /// </summary>
        public ICommand ChangeThemeCommand
        {
            get => changeThemeCommand ?? (changeThemeCommand = new RelayCommand(() =>
            {
                var curTheme = ApplicationThemeManager.GetAppTheme();
                if (curTheme == ApplicationTheme.Dark)
                {
                    ApplyTheme(ApplicationTheme.Light);
                }
                else if (curTheme == ApplicationTheme.Light)
                {
                    ApplyTheme(ApplicationTheme.Dark);
                }
            }));
        }

        #endregion


        #region method
        public void UpdateThemeBasedOnTime()
        {
            var currentHour = DateTime.Now.Hour;

            if (currentHour >= 9 && currentHour < 17) // 9AM - 5PM is light theme
            {
                ApplyTheme(Wpf.Ui.Appearance.ApplicationTheme.Light);

            }
            else
            {
                ApplyTheme(Wpf.Ui.Appearance.ApplicationTheme.Dark);

            }
        }

        public void ApplyTheme(Wpf.Ui.Appearance.ApplicationTheme curTheme)
        {


            BackgroundBrush = BackgroundBrushes[curTheme];

            BorderBackgroundBrush = BorderBackgroundBrushes[curTheme];

            ParticleBackgroundBrush = PartcleBackgroundPointBrushes[curTheme];

            TrayForegroundBrush = TrayForegroundBrushes[curTheme];

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Wpf.Ui.Appearance.ApplicationThemeManager.Apply(curTheme);
            });


            WeakReferenceMessenger.Default.Send(new ThemeChangedMessage(curTheme));
        }

        #endregion

    }
}
