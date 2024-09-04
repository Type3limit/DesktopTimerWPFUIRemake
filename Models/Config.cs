using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DesktopTimer.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DesktopTimer.models;
using DesktopTimer.Views.Controls;
using System.Windows.Media;

namespace DesktopTimer.Models
{
    public partial class LocalConfig : ObservableObject
    {

        #region properties

        [ObservableProperty]
        UserSetting userConfigData = new UserSetting();

        [ObservableProperty]
        ProgramSetting programConfigData = new ProgramSetting();

        #endregion


        #region constructor
        MainWorkModel? mainWorkModelInstance = null;

        public LocalConfig(MainWorkModel modelInstance)
        {
            mainWorkModelInstance = modelInstance;
            WeakReferenceMessenger.Default.Register<RequestSaveConfigMessage>(this, (e, t) =>
            {

                (t.Value switch
                {
                    ConfigType.User => (Action?)WriteUserSetting,
                    ConfigType.Program => (Action?)WriteProgramSetting,
                    _ => null
                })?.Invoke();
            });
        }

        ~LocalConfig()
        {
            WeakReferenceMessenger.Default.Unregister<RequestSaveConfigMessage>(this);
        }
        #endregion

        #region method
        public void Initialize()
        {
            //read program setting
            ReadProgramSetting();

            //read user config
            ReadUserSetting();

            WeakReferenceMessenger.Default.Send(new ConfigReadComplecateMessage());
        }

        public void ReadProgramSetting()
        {
            try
            {
                  
                var str = FileMapper.ProgramSettingFile.ReadText();
                if (!str.IsNullOrEmpty())
                {
                    var curData = JsonSerializer.Deserialize<ProgramSetting>(str);
                    if (curData != null)
                    {
                        ProgramConfigData = curData;
                        if (ProgramConfigData.LocalCollectPath.IsNullOrEmpty())
                        {
                            ProgramConfigData.LocalCollectPath = FileMapper.CollectionPictureDir;
                        }
                        ReadTimerBackgroundSetting();
                    }
                }
                else
                {
                    if (ProgramConfigData.LocalCollectPath.IsNullOrEmpty())
                    {
                        ProgramConfigData.LocalCollectPath = FileMapper.CollectionPictureDir;
                    }
                    WriteProgramSetting();
                }
                
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

        }

        public void ReadTimerBackgroundSetting()
        {
            if(mainWorkModelInstance==null)
                return;
            mainWorkModelInstance.DisplaySetting.TimerBackgroundWidth = ProgramConfigData.TimerSetting.Width;
            mainWorkModelInstance.DisplaySetting.TimerBackgroundHeight = ProgramConfigData.TimerSetting.Height;
            mainWorkModelInstance.DisplaySetting.TimerBackgroundOpacity = ProgramConfigData.TimerSetting.BackgroundOpacity;
            mainWorkModelInstance.DisplaySetting.IsTimerBorderVisiable = ProgramConfigData.TimerSetting.IsTimerBackgroundVisiable;
            mainWorkModelInstance.DisplaySetting.SelectedFontFamily= mainWorkModelInstance.DisplaySetting.FontFamilies[ProgramConfigData.TimerSetting.TimeFontIndex];
            mainWorkModelInstance.DisplaySetting.SelectedWeekendFontFamily = mainWorkModelInstance.DisplaySetting.FontFamilies[ProgramConfigData.TimerSetting.WeekendFontIndex];
            mainWorkModelInstance.DisplaySetting.TimeCenterFontSize = ProgramConfigData.TimerSetting.TimeFontSize;
            mainWorkModelInstance.DisplaySetting.WeekendCenterFontSize = ProgramConfigData.TimerSetting.WeekendFontSize;
            mainWorkModelInstance.DisplaySetting.TimeFontColor = ColorConverter.ConvertFromString(ProgramConfigData.TimerSetting.TimeFontColor) as Color? ?? Colors.White;
            mainWorkModelInstance.DisplaySetting.WeekendFontColor = ColorConverter.ConvertFromString(ProgramConfigData.TimerSetting.WeekendFontColor) as Color? ?? Colors.White;
            mainWorkModelInstance.DisplaySetting.BackgroundCornerRadiusValue = ProgramConfigData.TimerSetting.BackgroundRaidus;
        }

        public void WriteTimerBackgroundSetting()
        {
            if(mainWorkModelInstance==null)
                return;
            ProgramConfigData.TimerSetting.Width = mainWorkModelInstance.DisplaySetting.TimerBackgroundWidth;
            ProgramConfigData.TimerSetting.Height = mainWorkModelInstance.DisplaySetting.TimerBackgroundHeight;
            ProgramConfigData.TimerSetting.BackgroundOpacity = mainWorkModelInstance.DisplaySetting.TimerBackgroundOpacity;
            ProgramConfigData.TimerSetting.IsTimerBackgroundVisiable = mainWorkModelInstance.DisplaySetting.IsTimerBorderVisiable;
            ProgramConfigData.TimerSetting.TimeFontIndex = mainWorkModelInstance.DisplaySetting.FontFamilies.IndexOf(mainWorkModelInstance?.DisplaySetting?.SelectedFontFamily);
            ProgramConfigData.TimerSetting.WeekendFontIndex = mainWorkModelInstance.DisplaySetting.FontFamilies.IndexOf(mainWorkModelInstance?.DisplaySetting?.SelectedWeekendFontFamily);
            ProgramConfigData.TimerSetting.TimeFontSize = mainWorkModelInstance.DisplaySetting.TimeCenterFontSize;
            ProgramConfigData.TimerSetting.WeekendFontSize = mainWorkModelInstance.DisplaySetting.WeekendCenterFontSize;
            ProgramConfigData.TimerSetting.TimeFontColor = mainWorkModelInstance.DisplaySetting.TimeFontColor.ToString();
            ProgramConfigData.TimerSetting.WeekendFontColor = mainWorkModelInstance.DisplaySetting.WeekendFontColor.ToString();
            ProgramConfigData.TimerSetting.BackgroundRaidus = mainWorkModelInstance.DisplaySetting.BackgroundCornerRadiusValue;
        }

        static object programSettingLock = new object();

        public void WriteProgramSetting()
        {
            try
            {
                WriteTimerBackgroundSetting();
                var str = JsonSerializer.Serialize(ProgramConfigData);
                lock (programSettingLock)
                {
                    FileMapper.ProgramSettingFile.WriteText(str);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

        }

        public void ReadUserSetting()
        {
            try
            {
                var str = FileMapper.UserConfigureFile.ReadText();
                if (!str.IsNullOrEmpty())
                {
                    var curData = JsonSerializer.Deserialize<UserSetting>(str);
                    if (curData != null)
                    {
                        UserConfigData = curData;
                    }
                }
                else
                {
                    WriteUserSetting();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

        }
        static object userSettingLocker = new object();
        public void WriteUserSetting()
        {
            try
            {
                var str = JsonSerializer.Serialize(UserConfigData);
                lock (userSettingLocker)
                {
                    FileMapper.UserConfigureFile.WriteText(str);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

        }
        #endregion


        #region command 

        ICommand? saveProgramSettingCommand = null;

        public ICommand SaveProgramSettingCommand
        {
            get => saveProgramSettingCommand ?? (saveProgramSettingCommand = new RelayCommand(() =>
            {
                WriteProgramSetting();
            }));
        }


        ICommand? saveUserConfigCommand = null;

        public ICommand SaveUserConfigCommand
        {
            get => saveUserConfigCommand ?? (saveUserConfigCommand = new RelayCommand(() =>
            {
                WriteUserSetting();
            }));
        }
        #endregion
    }

    public class UserSetting : ObservableObject
    {
    }

    public class ProgramSetting : ObservableObject
    {


        public string? LocalCollectPath { set; get; }

        [JsonIgnore]
        BackgroundSetting timerSetting = new BackgroundSetting();
        /// <summary>
        /// Timer
        /// </summary>
        public BackgroundSetting TimerSetting
        {
            get=>timerSetting; 
            set => SetProperty(ref timerSetting,value);
        }

        [JsonIgnore]
        ParticleBackgroundSetting particleBackgroundSetting = new ParticleBackgroundSetting();
        /// <summary>
        /// Background particle
        /// </summary>
        public ParticleBackgroundSetting ParticleBackgroundSetting
        {
            get => particleBackgroundSetting;
            set => SetProperty(ref particleBackgroundSetting, value);
        }

        [JsonIgnore]
        AudioWaveSetting audioWaveSetting = new AudioWaveSetting();
        /// <summary>
        /// Audio Wave
        /// </summary>
        public AudioWaveSetting AudioWaveSetting
        {
            get=>audioWaveSetting;
            set=>SetProperty(ref audioWaveSetting,value);
        }

    }

    public class BackgroundSetting:ObservableObject
    {

        public bool IsTimerBackgroundVisiable { set;get;} = true;

        public double Width { set; get; } = 0.25;

        public double Height { set; get; }=0.15;

        public double BackgroundOpacity { set; get; }=1;

        public long MaxCacheCount { set; get; } = 20;

        public long FlushTime { set; get; } = 20;

        public bool IsTopmost { set; get; } = false;

        public int TimeFontIndex { set; get; } = 0;

        public int WeekendFontIndex { set; get; } = 0;

        public int TimeFontSize { set; get; } = 20;

        public int WeekendFontSize { set; get; } = 12;

        public int BackgroundRaidus { set;get;} = 5;

        public string? TimeFontColor { set; get; }

        public string? WeekendFontColor { set; get; }


    }

    public class ParticleBackgroundSetting : ObservableObject
    {
        [JsonIgnore]
        bool enableParticleBackground = true;
      
        public bool EnableParticleBackground
        {
            get => enableParticleBackground;
            set => SetProperty(ref enableParticleBackground, value);
        }

        [JsonIgnore]
        int particleCount = 75;

        public int ParticleCount
        {
            get => particleCount;
            set => SetProperty(ref particleCount, value);
        }

        [JsonIgnore]
        int particleLinkDistance = 80;

        public int ParticleLinkDistance
        {
            get => particleLinkDistance;
            set => SetProperty(ref particleLinkDistance, value);
        }

        [JsonIgnore]
        int particleMaxConnection = 5;

        public int ParticleMaxConnection
        {
            get => particleMaxConnection;
            set => SetProperty(ref particleMaxConnection, value);
        }

        [JsonIgnore]
        int mouseLinkDistance = 80;

        public int MouseLinkDistance
        {
            get => mouseLinkDistance;
            set => SetProperty(ref mouseLinkDistance, value);
        }

        [JsonIgnore]
        double particleBackgroundOpacity = 0.2;

        public double ParticleBackgroundOpacity
        {
            get => particleBackgroundOpacity;
            set => SetProperty(ref particleBackgroundOpacity, value);
        }

        [JsonIgnore]
        bool enableColorfulBackground = false;

        public bool EnableColorfulBackground
        {
            get => enableColorfulBackground;
            set => SetProperty(ref enableColorfulBackground, value);
        }
        [JsonIgnore]
        bool trackingMouse = true;

        public bool TrackingMouse
        {
            get => trackingMouse;
            set => SetProperty(ref trackingMouse, value);
        }

        [JsonIgnore]
        bool enableColorfulParticle = true;

        public bool EnableColorfulParticle
        {
            get => enableColorfulParticle;
            set => SetProperty(ref enableColorfulParticle, value);
        }

        [JsonIgnore]
        long updateMilliSecond = (long)(1000 / 60.0);

        public long UpdateMilliSecond
        {
            get => updateMilliSecond;
            set => SetProperty(ref updateMilliSecond, value);
        }

        [JsonIgnore]
        double mouseAttractionDistance = 5.0d;
        public double MouseAttractionDistance
        {
            get => mouseAttractionDistance;
            set => SetProperty(ref mouseAttractionDistance, value);
        }

        [JsonIgnore]
        double cornerRadius = 4;

        public double CornerRadius
        {
            get=>cornerRadius;
            set=>SetProperty(ref cornerRadius,value);
        }
    }


    public class AudioWaveSetting :ObservableObject
    {

        [JsonIgnore]
        bool enableAudioWave = true;

        public bool EnableAudioWave
        {
            get=>enableAudioWave;
            set=>SetProperty(ref enableAudioWave,value);
        }

        [JsonIgnore]
        List<AudioWaveControl.ApperenceType> apperenceTypes =  
            Enum.GetValues(typeof(AudioWaveControl.ApperenceType)).Cast<AudioWaveControl.ApperenceType>().ToList();
        [JsonIgnore]
        public List<AudioWaveControl.ApperenceType> ApperenceTypes
        {
            get=>apperenceTypes;

        }


        [JsonIgnore]
        AudioWaveControl.ApperenceType apperenceType = AudioWaveControl.ApperenceType.BrokenLinesCircle;

        public AudioWaveControl.ApperenceType ApperenceType
        {
            get=>apperenceType;
            set=>SetProperty(ref apperenceType,value);
        }

        [JsonIgnore]
        int unitCount = 100;

        public int UnitCount
        {
            get=>unitCount;
            set=>SetProperty(ref unitCount,value);
        }

        [JsonIgnore]
        double unitRadius = 2.0d;

        public double UnitRadius
        {
            get=>unitRadius;
            set=>SetProperty(ref unitRadius,value);
        }

        [JsonIgnore]
        double unitStrokeWidth =1.0d;

        public double UnitStrokeWidth
        {
            get=>unitStrokeWidth;
            set=>SetProperty(ref unitStrokeWidth,value);
        }

        [JsonIgnore]
        double unitOpacity = 1.0d;

        public double UnitOpacity
        {
            get=>unitOpacity;
            set=>SetProperty(ref unitOpacity,value);
        }

        [JsonIgnore]
        bool usingRandomUnitColor = false;

        public bool UsingRandomUnitColor
        {
            get=>usingRandomUnitColor; set => SetProperty(ref usingRandomUnitColor, value);
        }

        [JsonIgnore]
        Color specificUnitColor = Colors.White;

        public Color SpecificUnitColor
        {
            get=>specificUnitColor;
            set=>SetProperty(ref specificUnitColor,value);
        }

        [JsonIgnore]
        Color specificUnitStrokeColor = Colors.White;

        public Color SpecificUnitStrokeColor
        {
            get=>specificUnitStrokeColor;
            set=>SetProperty(ref specificUnitStrokeColor,value);
        }


        [JsonIgnore]
        double dataWeight = 0.5d;
        public double DataWeight
        {
            get=>dataWeight;
            set=>SetProperty(ref dataWeight,value);
        }


        [JsonIgnore]
        double ellipseRadius = 150;

        public double EllipseRadius
        {
            get=>ellipseRadius;
            set=>SetProperty(ref ellipseRadius,value);
        }
    }
}
