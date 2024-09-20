using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopTimer.Helpers
{
    /// <summary>
    /// Path definitions
    /// </summary>
    public class FileMapper
    {

        #region dir
        /// <summary>
        /// Picture cache directory
        /// </summary>
        public static string PictureCacheDir
        {
            get
            {
                string currentDir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "PictureCache";
                if (!Directory.Exists(currentDir))
                    Directory.CreateDirectory(currentDir);
                return currentDir;
            }
        }
        /// <summary>
        /// Configuration file directory
        /// </summary>
        public static string ConfigureDir
        {
            get
            {
#pragma warning disable CS8604 
                string currentFile = Path.Combine(AppDomain.CurrentDomain?.SetupInformation?.ApplicationBase, "Configuration");
#pragma warning restore CS8604 
                if (!Directory.Exists(currentFile))
                    Directory.CreateDirectory(currentFile);
                return currentFile;
            }
        }
        /// <summary>
        /// Chat room related files
        /// </summary>
        public static string ChatRoomDir
        {
            get
            {
#pragma warning disable CS8604 
                string currentFile = Path.Combine(ConfigureDir, ".ChatRoom");
#pragma warning restore CS8604 
                if (!Directory.Exists(currentFile))
                    Directory.CreateDirectory(currentFile);
                return currentFile;
            }
        }

        /// <summary>
        /// Video cache directory
        /// </summary>
        public static string VideoCacheDir
        {
            get
            {
                string currentDir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "Videos";
                if (!Directory.Exists(currentDir))
                    Directory.CreateDirectory(currentDir);
                return currentDir;
            }
        }
        /// <summary>
        /// Normal background directory
        /// </summary>
        public static string NormalPictureDir
        {
            get
            {
                var cur = Path.Combine(PictureCacheDir, "Normal");
                if (!Directory.Exists(cur))
                    Directory.CreateDirectory(cur);
                return cur;
            }
        }
        /// <summary>
        /// Generated background directory
        /// </summary>
        public static string GeneratedPictureDir
        {
            get
            {
                var cur = Path.Combine(PictureCacheDir, "Generate");
                if (!Directory.Exists(cur))
                    Directory.CreateDirectory(cur);
                return cur;
            }
        }

        /// <summary>
        /// Local background directory
        /// </summary>
        public static string LocalPictureDir
        {
            get
            {
#pragma warning disable CS8604 // Possible null reference argument.
                string currentFile = Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Local");
#pragma warning restore CS8604 // Possible null reference argument.
                if (!Directory.Exists(currentFile))
                    Directory.CreateDirectory(currentFile);
                return currentFile;
            }
        }
        /// <summary>
        /// Collection directory
        /// </summary>
        public static string CollectionPictureDir
        {
            get
            {
#pragma warning disable CS8604 // Possible null reference argument.
                string currentFile = Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Collect");
#pragma warning restore CS8604 // Possible null reference argument.
                if (!Directory.Exists(currentFile))
                    Directory.CreateDirectory(currentFile);
                return currentFile;
            }
        }
        /// <summary>
        /// JSON configuration file path
        /// </summary>
        public static string ConfigureJson
        {
            get
            {
                string currentFile = Path.Combine(ConfigureDir, "Configuration.Json");
                if (!File.Exists(currentFile))
                    File.Create(currentFile).Close();
                return currentFile;
            }
        }


        /// <summary>
        /// URL record JSON path
        /// </summary>
        public static string BackTypesJson
        {
            get
            {
                string currentFile = Path.Combine(ConfigureDir, "BackgroundTypes.Json");
                if (!File.Exists(currentFile))
                    File.Create(currentFile).Close();
                return currentFile;
            }
        }
        /// <summary>
        /// CefBrowser cache data root directory
        /// </summary>
        public static string CefBrowserDataDir
        {
            get
            {
#pragma warning disable CS8604 // Possible null reference argument.
                string cefBrowserData = Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, @"CefBrowserData\");
#pragma warning restore CS8604 // Possible null reference argument.
                if (!Directory.Exists(cefBrowserData))
                {
                    Directory.CreateDirectory(cefBrowserData);
                }
                return cefBrowserData;
            }
        }
        /// <summary>
        /// Network log directory
        /// </summary>
        public static string CefBrowserLogPath
        {
            get
            {
                string cefBrowserLogPath = Path.Combine(CefBrowserDataDir, "CefBrowser.log");
                if (!File.Exists(cefBrowserLogPath))
                {
                    File.Create(cefBrowserLogPath);
                }
                return cefBrowserLogPath;
            }
        }
        /// <summary>
        /// Network cache directory
        /// </summary>
        public static string CefBrowserCacheDir
        {
            get
            {
                string cefBrowserCache = Path.Combine(CefBrowserDataDir, @"Cache\");
                if (!Directory.Exists(cefBrowserCache))
                {
                    Directory.CreateDirectory(cefBrowserCache);
                }
                return cefBrowserCache;
            }
        }
        /// <summary>
        /// Network user information cache
        /// </summary>
        public static string CefBrowserUserDataDir
        {
            get
            {
                string cefBrowserUserData = Path.Combine(CefBrowserDataDir, @"UserData\");
                if (!Directory.Exists(cefBrowserUserData))
                {
                    Directory.CreateDirectory(cefBrowserUserData);
                }
                return cefBrowserUserData;
            }
        }
        /// <summary>
        /// Log file directory
        /// </summary>
        public static string CurrentLogFileDir
        {
            get
            {
                string currentDir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "Logs";
                if (!Directory.Exists(currentDir))
                    Directory.CreateDirectory(currentDir);
                return currentDir;
            }
        }

        /// <summary>
        /// Preset file directory
        /// </summary>
        public static string AssetsFileDir
        {
            get
            {
                string currentDir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "Assets";
                if (!Directory.Exists(currentDir))
                    Directory.CreateDirectory(currentDir);
                return currentDir;
            }
        }
        #endregion

        #region file
        private static string currentLogFile = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
        /// <summary>
        /// Current log file
        /// </summary>
        public static string CurrentLogFile
        {
            get
            {
                var FileName = Path.Combine(CurrentLogFileDir, currentLogFile + ".log");
                if (!File.Exists(FileName))
                {
                    File.Create(FileName).Close();
                }
                return FileName;
            }
        }
        public static string CurrentEmojiCacheDir
        {
            get
            {
                string currentDir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "EmojiCache";
                if (!Directory.Exists(currentDir))
                    Directory.CreateDirectory(currentDir);
                return currentDir;
            }
        }

        /// <summary>
        /// Program configuration file
        /// </summary>
        public static string ProgramSettingFile
        {
            get
            {
                var file = ConfigureDir.PathCombine("ProgramSetting.json");
                if (!file.IsFileExist())
                {
                    File.Create(file).Close();
                }
                return file;
            }
        }
        /// <summary>
        /// User configuration file
        /// </summary>
        public static string UserConfigureFile
        {
            get
            {
                var file = ConfigureDir.PathCombine("UserSetting.json");
                if (!file.IsFileExist())
                {
                    File.Create(file).Close();
                }
                return file;
            }
        }
        /// <summary>
        /// User configuration file
        /// </summary>
        public static string TranslateConfigFile
        {
            get
            {
                var file = ConfigureDir.PathCombine("TranslateConfig.json");
                if (!file.IsFileExist())
                {
                    File.Create(file).Close();
                }
                return file;
            }
        }
        /// <summary>
        /// SD preset file
        /// </summary>
        public static string StableDiffusionPresetFile
        {
            get
            {
                var file = AssetsFileDir.PathCombine("StableDiffsionPresets.json");
                if (!file.IsFileExist())
                {
                    File.Create(file).Close();
                }
                return file;
            }
        }

        /// <summary>
        /// Chat room file
        /// </summary>
        public static string ChatRoomUserInfoListsFile
        {
            get
            {
                var file = ChatRoomDir.PathCombine("UserInfoLists.json");
                if (!file.IsFileExist())
                {
                    File.Create(file).Close();
                }
                return file;
            }
        }


        #region dbFile

        /// <summary>
        /// Chat User Info
        /// </summary>
        public static string ChatRoomUserInfoDBFile
        {
            get
            {
                var file = ChatRoomDir.PathCombine("ChatUserInfo.db");
                if (!file.IsFileExist())
                {
                    File.Create(file).Close();
                }
                return file;
            }
        }

        #endregion
        #endregion
    }

}
