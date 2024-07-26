using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Windows.Media;

namespace DesktopTimerWPFUIRemake.models.displayModel
{
    public class GlobalDisplaySettingModel : ObservableObject
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


        #region Timer

        private bool isTimerBorderVisiable = true;
        /// <summary>
        /// To mark if timer border visiable
        /// </summary>
        public bool IsTimerBorderVisiable
        {
            get=>isTimerBorderVisiable;
            set
            {
                SetProperty(ref isTimerBorderVisiable , value);
            }
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
            set
            {
                SetProperty(ref timeFontColor, value);
                OnPropertyChanged("TimeFontBrush");
            }
        }


        private Color timerBackgroundColor = Color.FromArgb(128,0,0,0);
        /// <summary>
        /// Timer background color
        /// </summary>
        private Color TimerBackgroundColor
        {
            get=> timerBackgroundColor;
            set
            {
                SetProperty(ref timerBackgroundColor, value);
                OnPropertyChanged("TimerBackgroundBrush");
            }
        }

        private Color weekendFontColor = Colors.White;
        /// <summary>
        /// Font color of weekend text
        /// </summary>
        public Color WeekendFontColor
        {
            get => weekendFontColor;
            set
            {
                SetProperty(ref weekendFontColor, value);
                OnPropertyChanged("WeekendFontBrush");
            }
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


        #endregion


        #endregion
    }
}
