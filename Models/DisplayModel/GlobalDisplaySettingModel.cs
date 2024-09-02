using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Windows;
using Microsoft.VisualBasic;
using DesktopTimer.Helpers;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.IO;

namespace DesktopTimer.models.displayModel
{



    public partial class GlobalDisplaySettingModel : ObservableObject
    {
        #region Properties


        private List<FontFamily> fontFamilies = new List<FontFamily>();
        /// <summary>
        /// All fonts in current system
        /// </summary>
        public List<FontFamily> FontFamilies
        {
            get => fontFamilies;
            set => SetProperty(ref fontFamilies, value);
        }


        #region cache
        private string cacheSize = "0MB";
        /// <summary>
        /// 缓存大小
        /// </summary>
        public string CacheSize
        {
            get => cacheSize;
            set => SetProperty(ref cacheSize, value);
        }
        #endregion

        #region Timer

        private double timerBackgroundOpacity = 1;
        public double TimerBackgroundOpacity
        {
            get=>timerBackgroundOpacity;
            set=>SetProperty(ref timerBackgroundOpacity,value);
        }

        private double timerBackgroundWidth = 0.35;
        /// <summary>
        /// Timer background width
        /// </summary>
        public double TimerBackgroundWidth
        {
            get => timerBackgroundWidth;
            set => SetProperty(ref timerBackgroundWidth, value);
        }


        private double timerBackgroundHeight = 0.15;
        /// <summary>
        /// Timer background height
        /// </summary>
        public double TimerBackgroundHeight
        {
            get => timerBackgroundHeight;
            set => SetProperty(ref timerBackgroundHeight, value);
        }


        private bool isTimerBorderVisiable = true;
        /// <summary>
        /// To mark if timer border visiable
        /// </summary>
        public bool IsTimerBorderVisiable
        {
            get => isTimerBorderVisiable;
            set => SetProperty(ref isTimerBorderVisiable, value);

        }

        private FontFamily? selectedFontFamily = null;
        /// <summary>
        /// Font of timer text
        /// </summary>
        public FontFamily? SelectedFontFamily
        {
            get => selectedFontFamily;
            set => SetProperty(ref selectedFontFamily, value);
        }

        private FontFamily? selectedWeekendFontFamily = null;
        /// <summary>
        /// Font of weekend text
        /// </summary>
        public FontFamily? SelectedWeekendFontFamily
        {
            get => selectedWeekendFontFamily;
            set => SetProperty(ref selectedWeekendFontFamily, value);
        }

        private Color timeFontColor = Colors.White;
        /// <summary>
        /// Font color of timer text
        /// </summary>
        public Color TimeFontColor
        {
            get => timeFontColor;
            set => SetProperty(ref timeFontColor, value);
        }


        private Color timerBackgroundColor = Color.FromArgb(128, 0, 0, 0);
        /// <summary>
        /// Timer background color
        /// </summary>
        public Color TimerBackgroundColor
        {
            get => timerBackgroundColor;
            set => SetProperty(ref timerBackgroundColor, value);

        }

        private int timerBackgroundCornorRadiusValue = 6;
        public int TimerBackgroundCornorRadiusValue
        {
            get => timerBackgroundCornorRadiusValue;
            set
            {
                SetProperty(ref timerBackgroundCornorRadiusValue, value);
                OnPropertyChanged("TimerBackgroundCornorRadius");
            }
        }
        public CornerRadius TimerBackgroundCornorRadius
        {
            get => new CornerRadius(TimerBackgroundCornorRadiusValue);
        }


        private Color weekendFontColor = Colors.White;
        /// <summary>
        /// Font color of weekend text
        /// </summary>
        public Color WeekendFontColor
        {
            get => weekendFontColor;
            set => SetProperty(ref weekendFontColor, value);
        }

        private int timeCenterFontSize = 20;
        /// <summary>
        /// Font size of timer text
        /// </summary>
        public int TimeCenterFontSize
        {
            get => timeCenterFontSize;
            set => SetProperty(ref timeCenterFontSize, value);
        }

        private int weekendCenterFontSize = 12;
        /// <summary>
        /// Font size of weekend text
        /// </summary>
        public int WeekendCenterFontSize
        {
            get => weekendCenterFontSize;
            set => SetProperty(ref weekendCenterFontSize, value);
        }

        private string currentTimeStr = string.Empty;
        /// <summary>
        /// string of time
        /// </summary>
        public string CurrentTimeStr
        {
            get => currentTimeStr;
            set => SetProperty(ref currentTimeStr, value);

        }

        private string currentWeekTimeStr = string.Empty;
        /// <summary>
        /// string of week
        /// </summary>
        public string CurrentWeekTimeStr
        {
            get => currentWeekTimeStr;
            set => SetProperty(ref currentWeekTimeStr, value);
        }


        static string[] Day = new string[] { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
        #endregion



        #region Background

        private int backgroundCornerRadiusValue = 15;
        public int BackgroundCornerRadiusValue
        {
            get=>backgroundCornerRadiusValue;
            set
            {
                SetProperty(ref backgroundCornerRadiusValue, value);
                OnPropertyChanged("BackgroundCornerRadius");
            }
        }

        public CornerRadius BackgroundCornerRadius
        {
            get=>
                new CornerRadius(BackgroundCornerRadiusValue);
            
        }

        private double backgroundImageOpacity = 1d;
        /// <summary>
        /// opacity of current background
        /// </summary>
        public double BackgroundImageOpacity
        {
            get => backgroundImageOpacity;
            set => SetProperty(ref backgroundImageOpacity, value);
        }

        private long curCountDown = 0;
        /// <summary>
        /// fresh background count down
        /// </summary>
        public long CurCountDown
        {
            get => curCountDown;
            set => SetProperty(ref curCountDown, value);
        }

        private long totalCountDown = 20;//20 seconds by default
        /// <summary>
        /// count down limit
        /// </summary>
        public long TotalCountDown
        {
            get => totalCountDown;
            set => SetProperty(ref totalCountDown, value);
        }


        private long maxCacheCount = 20;
        /// <summary>
        /// max background cache count 
        /// </summary>
        public long MaxCacheCount
        {
            get => maxCacheCount;
            set => SetProperty(ref maxCacheCount, value);
        }

        private BitmapImage? backgroundView = null;
        /// <summary>
        /// current background image
        /// </summary>
        public BitmapImage? BackgroundView
        {
            get => backgroundView;
            set => SetProperty(ref backgroundView, value);
        }


        private List<string> backgroundImageLists = new List<string>();
        /// <summary>
        /// background image cache list
        /// </summary>
        public List<string> BackgroundImageLists
        {
            get => backgroundImageLists;
            set => SetProperty(ref backgroundImageLists, value);
        }


        private bool shouldPauseFresh = false;
        /// <summary>
        /// mark that should pause fresh background
        /// </summary>
        public bool ShouldPauseFresh
        {
            get => shouldPauseFresh;
            set => SetProperty(ref shouldPauseFresh, value);
        }

        #endregion

        #region setting 
        [ObservableProperty]
        private bool isSettingOpen = false;
        /// <summary>
        /// mark if setting flyout opened
        /// </summary>

        [ObservableProperty]
        private bool isOnlineBackgroundMode = true;

        #endregion
        #endregion

        #region constructor



        MainWorkModel? mainModelInstance = null;

        public GlobalDisplaySettingModel(MainWorkModel? modelInstance)
        {
            this.mainModelInstance = modelInstance;

            WeakReferenceMessenger.Default.Register<RequestAbandonCurrentCacheMessage>(this, (e, t) =>
            {
                if (t.Value == 0)
                {
                    BackgroundView = null;
                    BackgroundImageLists.Clear();
                    CheckToFillBackgroundCache();
                }
            });


        }


        private void MainModelInstance_TimerHandler()
        {
            //update timer str 
            var curDate = DateTime.Now.ToLocalTime();
            CurrentTimeStr = curDate.ToString("yyyy-MM-dd HH:mm:ss");
            string week = Day[Convert.ToInt32(DateTime.Now.DayOfWeek.ToString("d"))].ToString();
            week += (",今年的第" + DateTime.Now.DayOfYear + "天");
            CurrentWeekTimeStr = week;


            if (IsSettingOpen)
            {
                UpdateCacheSize();
            }


            //update background views 
            ++CurCountDown;
            if (CurCountDown < TotalCountDown)
                return;

            CurCountDown = 0;

            if (BackgroundImageLists.Count > 0)
            {
                FreshCurrentImage();

                CheckToFillBackgroundCache();
            }

        }

        #endregion

        #region command


        bool originTimerBorderStatus = false;
        private ICommand? openSettingCommand = null;
        public ICommand OpenSettingCommand
        {
            get => openSettingCommand ?? (openSettingCommand = new RelayCommand(() =>
            {
                UpdateCacheSize();
                IsSettingOpen = true;
            }));
        }

        private ICommand? closeSettingCommand = null;
        public ICommand CloseSettingCommand
        {
            get => closeSettingCommand ?? (closeSettingCommand = new RelayCommand(() =>
            {
                IsSettingOpen = false;
            }));
        }


        private ICommand? requestFlushCommand = null;
        public ICommand RequestFlushCommand
        {
            get => requestFlushCommand ?? (requestFlushCommand = new RelayCommand(() =>
            {
                CurCountDown = 0;
                FreshCurrentImage();
            }));
        }


        private ICommand? setBackgroundfreshCommand = null;
        public ICommand SetBackgroundfreshCommand
        {
            get => setBackgroundfreshCommand ?? (setBackgroundfreshCommand = new RelayCommand(() =>
            {

                ShouldPauseFresh = !ShouldPauseFresh;
            }));
        }

        private ICommand? clearCacheCommand;
        public ICommand ClearCacheCommand
        {
            get => clearCacheCommand ?? (clearCacheCommand = new RelayCommand(() =>
            {
                EmptyCache();
                BackgroundImageLists.Clear();
                CheckToFillBackgroundCache();
            }));
        }
        #endregion

        #region methods

        public void UpdateCacheSize()
        {
            CacheSize = (FileMapper.NormalPictureDir.GetDirectorySize() / 1024 / 1024.0).ToString("0.00") + "MB";
        }

        public void Initilize()
        {
            WeakReferenceMessenger.Default.Register<TimeUpdateMessage>(this, (t, message) =>
            {
                MainModelInstance_TimerHandler();
            });

            WeakReferenceMessenger.Default.Register<BackgroundSourceUpdateMessage>(this, (t, message) =>
            {
                if (message?.Value != null && message.Value.IsFileExist())
                {
                    AddBackgroundImageCache(message.Value);
                }
            });


            GetAllFont();
            CheckToFillBackgroundCache();
        }

        public async void FreshCurrentImage()
        {

            var cur = BackgroundImageLists.FirstOrDefault();
            if (cur == null || ShouldPauseFresh)
                return;
            BackgroundView = await ImageTool.LoadImg(cur);
            BackgroundImageLists.Remove(cur);
        }

        /// <summary>
        /// add background image to cache list
        /// </summary>
        /// <param name="path"></param>
        public void AddBackgroundImageCache(string path)
        {
            if (!BackgroundImageLists.Contains(path))
                BackgroundImageLists.Add(path);
            if (BackgroundView == null)
            {
                FreshCurrentImage();
            }
        }

        /// <summary>
        /// get all font in system
        /// </summary>
        private void GetAllFont()
        {
            FontFamilies = Fonts.SystemFontFamilies.ToList();
            SelectedFontFamily = FontFamilies.FirstOrDefault();
            SelectedWeekendFontFamily = FontFamilies.FirstOrDefault();
        }

        /// <summary>
        /// request for fill cache list
        /// </summary>
        private void CheckToFillBackgroundCache()
        {
            if (BackgroundImageLists.Count >= MaxCacheCount)
                return;//no need for cache
            WeakReferenceMessenger.Default.Send(new RequestFillBackgroundMessage((int)(MaxCacheCount - BackgroundImageLists.Count)));
        }

        /// <summary>
        /// clear cache directory
        /// </summary>
        void EmptyCache()
        {
            try
            {
                Directory.Delete(FileMapper.NormalPictureDir, true);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        #endregion
    }
}
