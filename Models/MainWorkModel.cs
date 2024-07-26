using CommunityToolkit.Mvvm.ComponentModel;
using DesktopTimerWPFUIRemake.models.displayModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopTimerWPFUIRemake.models
{
    public class MainWorkModel: ObservableObject
    {
        #region Properties

        private GlobalDisplaySettingModel? displaySettingModel = null;
        /// <summary>
        /// For Ui display properties setting;
        /// </summary>
        public GlobalDisplaySettingModel DisplaySettingModel
        {
            get=>displaySettingModel??(displaySettingModel = new GlobalDisplaySettingModel());
        }

        #endregion
    }
}
