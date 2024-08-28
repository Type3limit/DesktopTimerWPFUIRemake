using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DesktopTimer.Helpers;
using DesktopTimer.models.displayModel;
using DesktopTimer.Models.BackgroundWorkingModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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

        private BackgroundImageRequestModel? backgroundImageRequest = null;
        /// <summary>
        /// Unit of any web request 
        /// </summary>
        public BackgroundImageRequestModel? BackgroundImageRequest
        {
            get => backgroundImageRequest??(backgroundImageRequest = new BackgroundImageRequestModel(this));
        }

        #endregion

        #region delegate 
        #endregion

        #region constructor

        public MainWorkModel()
        {
            BackgroundImageRequest?.Initialize();
            DisplaySetting?.Initilize();
            StartTimer();
        }

        #endregion


        #region command 
        private ICommand? exitProgramCommand;
        /// <summary>
        /// To exit current app
        /// </summary>
        public ICommand ExitProgramCommand
        {
            get => exitProgramCommand ?? (exitProgramCommand = new RelayCommand(() =>
            {
                WeakReferenceMessenger.Default.Send(new RequestCloseProgramMessage());
            }));
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
                WeakReferenceMessenger.Default.Send(new TimeUpdateMessage());
            };
            timer.Start();
        }

        #endregion
    }
}
