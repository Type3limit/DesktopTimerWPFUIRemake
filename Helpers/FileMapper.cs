using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopTimer.Helpers
{
    /// <summary>
    /// 路径定义
    /// </summary>
    public class FileMapper
    {

        #region dir
        /// <summary>
        /// 图片缓存目录
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
        /// 配置文件目录
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
        /// 视频缓存目录
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
        /// 普通背景目录
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
        /// 本地背景目录
        /// </summary>
        public static string LocalPictureDir
        {
            get
            {
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                string currentFile = Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Local");
#pragma warning restore CS8604 // 引用类型参数可能为 null。
                if (!Directory.Exists(currentFile))
                    Directory.CreateDirectory(currentFile);
                return currentFile;
            }
        }
        /// <summary>
        /// 收藏目录
        /// </summary>
        public static string CollectionPictureDir
        {
            get
            {
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                string currentFile = Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Collect");
#pragma warning restore CS8604 // 引用类型参数可能为 null。
                if (!Directory.Exists(currentFile))
                    Directory.CreateDirectory(currentFile);
                return currentFile;
            }
        }
        /// <summary>
        /// json配置文件路径
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
        /// 网址记录json路径
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
        /// CefBrowser缓存数据根目录
        /// </summary>
        public static string CefBrowserDataDir
        {
            get
            {
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                string cefBrowserData = Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, @"CefBrowserData\");
#pragma warning restore CS8604 // 引用类型参数可能为 null。
                if (!Directory.Exists(cefBrowserData))
                {
                    Directory.CreateDirectory(cefBrowserData);
                }
                return cefBrowserData;
            }
        }
        /// <summary>
        /// 网络日志目录
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
        /// 网络缓存目录
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
        /// 网络用户信息缓存
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
        /// 日志文件目录
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

        #endregion

        #region file
        private static string currentLogFile = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
        /// <summary>
        /// 当前日志文件
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
        /// 程序配置文件
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
        /// 用户配置文件
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
        /// 用户配置文件
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
        #endregion
    }

}
