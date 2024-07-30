using CommunityToolkit.Mvvm.ComponentModel;
using DesktopTimer.models.displayModel;
using DesktopTimer.Models.BackgroundWorkingModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopTimer.models
{
    public class MainWorkModel: ObservableObject
    {
        #region Properties

        private GlobalDisplaySettingModel? displaySetting = null;
        /// <summary>
        /// For UI display properties setting;
        /// </summary>
        public GlobalDisplaySettingModel DisplaySetting
        {
            get=>displaySetting??(displaySetting = new GlobalDisplaySettingModel(this));
        }

        private WebRequestModel? webRequest = null;
        /// <summary>
        /// Unit of any web request 
        /// </summary>
        public WebRequestModel? WebRequest
        {
            get => webRequest??(webRequest = new WebRequestModel(this));
        }

        #endregion

        #region delegate 
        public delegate void onTimer();
        public event onTimer? TimerHandler;
        #endregion

        #region constructor

        public MainWorkModel()
        {
            StartTimer();
        }

        #endregion


        #region methods
        System.Timers.Timer? timer = null;
        /// <summary>
        /// start timer event for 1 second
        /// </summary>
        void StartTimer()
        {
            if(timer!=null)
            {
                timer.Enabled = false;
                timer.Stop();
                timer.Dispose();
            }
            timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += (o,e) => { 
                TimerHandler?.BeginInvoke(new AsyncCallback((ar) => { TimerHandler?.EndInvoke(ar); }), null);
                };
            timer.Start();
        }

        #endregion
    }
}
