using CommunityToolkit.Mvvm.ComponentModel;
using DesktopTimer.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace DesktopTimer.Models.Everything
{
    public static class Everything
    {
        #region API constants and references

        const int EVERYTHING_OK = 0;
        const int EVERYTHING_ERROR_MEMORY = 1;
        const int EVERYTHING_ERROR_IPC = 2;
        const int EVERYTHING_ERROR_REGISTERCLASSEX = 3;
        const int EVERYTHING_ERROR_CREATEWINDOW = 4;
        const int EVERYTHING_ERROR_CREATETHREAD = 5;
        const int EVERYTHING_ERROR_INVALIDINDEX = 6;
        const int EVERYTHING_ERROR_INVALIDCALL = 7;

        const int EVERYTHING_REQUEST_FILE_NAME = 0x00000001;
        const int EVERYTHING_REQUEST_PATH = 0x00000002;
        const int EVERYTHING_REQUEST_FULL_PATH_AND_FILE_NAME = 0x00000004;
        const int EVERYTHING_REQUEST_EXTENSION = 0x00000008;
        const int EVERYTHING_REQUEST_SIZE = 0x00000010;
        const int EVERYTHING_REQUEST_DATE_CREATED = 0x00000020;
        const int EVERYTHING_REQUEST_DATE_MODIFIED = 0x00000040;
        const int EVERYTHING_REQUEST_DATE_ACCESSED = 0x00000080;
        const int EVERYTHING_REQUEST_ATTRIBUTES = 0x00000100;
        const int EVERYTHING_REQUEST_FILE_LIST_FILE_NAME = 0x00000200;
        const int EVERYTHING_REQUEST_RUN_COUNT = 0x00000400;
        const int EVERYTHING_REQUEST_DATE_RUN = 0x00000800;
        const int EVERYTHING_REQUEST_DATE_RECENTLY_CHANGED = 0x00001000;
        const int EVERYTHING_REQUEST_HIGHLIGHTED_FILE_NAME = 0x00002000;
        const int EVERYTHING_REQUEST_HIGHLIGHTED_PATH = 0x00004000;
        const int EVERYTHING_REQUEST_HIGHLIGHTED_FULL_PATH_AND_FILE_NAME = 0x00008000;

        const int EVERYTHING_SORT_NAME_ASCENDING = 1;
        const int EVERYTHING_SORT_NAME_DESCENDING = 2;
        const int EVERYTHING_SORT_PATH_ASCENDING = 3;
        const int EVERYTHING_SORT_PATH_DESCENDING = 4;
        const int EVERYTHING_SORT_SIZE_ASCENDING = 5;
        const int EVERYTHING_SORT_SIZE_DESCENDING = 6;
        const int EVERYTHING_SORT_EXTENSION_ASCENDING = 7;
        const int EVERYTHING_SORT_EXTENSION_DESCENDING = 8;
        const int EVERYTHING_SORT_TYPE_NAME_ASCENDING = 9;
        const int EVERYTHING_SORT_TYPE_NAME_DESCENDING = 10;
        const int EVERYTHING_SORT_DATE_CREATED_ASCENDING = 11;
        const int EVERYTHING_SORT_DATE_CREATED_DESCENDING = 12;
        const int EVERYTHING_SORT_DATE_MODIFIED_ASCENDING = 13;
        const int EVERYTHING_SORT_DATE_MODIFIED_DESCENDING = 14;
        const int EVERYTHING_SORT_ATTRIBUTES_ASCENDING = 15;
        const int EVERYTHING_SORT_ATTRIBUTES_DESCENDING = 16;
        const int EVERYTHING_SORT_FILE_LIST_FILENAME_ASCENDING = 17;
        const int EVERYTHING_SORT_FILE_LIST_FILENAME_DESCENDING = 18;
        const int EVERYTHING_SORT_RUN_COUNT_ASCENDING = 19;
        const int EVERYTHING_SORT_RUN_COUNT_DESCENDING = 20;
        const int EVERYTHING_SORT_DATE_RECENTLY_CHANGED_ASCENDING = 21;
        const int EVERYTHING_SORT_DATE_RECENTLY_CHANGED_DESCENDING = 22;
        const int EVERYTHING_SORT_DATE_ACCESSED_ASCENDING = 23;
        const int EVERYTHING_SORT_DATE_ACCESSED_DESCENDING = 24;
        const int EVERYTHING_SORT_DATE_RUN_ASCENDING = 25;
        const int EVERYTHING_SORT_DATE_RUN_DESCENDING = 26;

        const int EVERYTHING_TARGET_MACHINE_X86 = 1;
        const int EVERYTHING_TARGET_MACHINE_X64 = 2;
        const int EVERYTHING_TARGET_MACHINE_ARM = 3;

        [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
        public static extern UInt32 Everything_SetSearchW(string lpSearchString);
        [DllImport("Everything64.dll")]
        public static extern void Everything_SetMatchPath(bool bEnable);
        [DllImport("Everything64.dll")]
        public static extern void Everything_SetMatchCase(bool bEnable);
        [DllImport("Everything64.dll")]
        public static extern void Everything_SetMatchWholeWord(bool bEnable);
        [DllImport("Everything64.dll")]
        public static extern void Everything_SetRegex(bool bEnable);
        [DllImport("Everything64.dll")]
        public static extern void Everything_SetMax(UInt32 dwMax);
        [DllImport("Everything64.dll")]
        public static extern void Everything_SetOffset(UInt32 dwOffset);

        [DllImport("Everything64.dll")]
        public static extern bool Everything_GetMatchPath();
        [DllImport("Everything64.dll")]
        public static extern bool Everything_GetMatchCase();
        [DllImport("Everything64.dll")]
        public static extern bool Everything_GetMatchWholeWord();
        [DllImport("Everything64.dll")]
        public static extern bool Everything_GetRegex();
        [DllImport("Everything64.dll")]
        public static extern UInt32 Everything_GetMax();
        [DllImport("Everything64.dll")]
        public static extern UInt32 Everything_GetOffset();
        [DllImport("Everything64.dll")]
        public static extern IntPtr Everything_GetSearchW();
        [DllImport("Everything64.dll")]
        public static extern UInt32 Everything_GetLastError();
        [DllImport("Everything64.dll")]
        public static extern bool Everything_QueryW(bool bWait);
        [DllImport("Everything64.dll")]
        public static extern void Everything_SortResultsByPath();
        [DllImport("Everything64.dll")]
        public static extern UInt32 Everything_GetNumFileResults();
        [DllImport("Everything64.dll")]
        public static extern UInt32 Everything_GetNumFolderResults();
        [DllImport("Everything64.dll")]
        public static extern UInt32 Everything_GetNumResults();
        [DllImport("Everything64.dll")]
        public static extern UInt32 Everything_GetTotFileResults();
        [DllImport("Everything64.dll")]
        public static extern UInt32 Everything_GetTotFolderResults();
        [DllImport("Everything64.dll")]
        public static extern UInt32 Everything_GetTotResults();
        [DllImport("Everything64.dll")]
        public static extern bool Everything_IsVolumeResult(UInt32 nIndex);
        [DllImport("Everything64.dll")]
        public static extern bool Everything_IsFolderResult(UInt32 nIndex);
        [DllImport("Everything64.dll")]
        public static extern bool Everything_IsFileResult(UInt32 nIndex);
        [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
        public static extern void Everything_GetResultFullPathName(UInt32 nIndex, StringBuilder lpString, UInt32 nMaxCount);
        [DllImport("Everything64.dll")]
        public static extern void Everything_Reset();
        [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultFileName(UInt32 nIndex);
        [DllImport("Everything64.dll")]
        public static extern void Everything_SetSort(UInt32 dwSortType);
        [DllImport("Everything64.dll")]
        public static extern UInt32 Everything_GetSort();
        [DllImport("Everything64.dll")]
        public static extern UInt32 Everything_GetResultListSort();
        [DllImport("Everything64.dll")]
        public static extern void Everything_SetRequestFlags(UInt32 dwRequestFlags);
        [DllImport("Everything64.dll")]
        public static extern UInt32 Everything_GetRequestFlags();
        [DllImport("Everything64.dll")]
        public static extern UInt32 Everything_GetResultListRequestFlags();
        [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultExtension(UInt32 nIndex);
        [DllImport("Everything64.dll")]
        public static extern bool Everything_GetResultSize(UInt32 nIndex, out long lpFileSize);
        [DllImport("Everything64.dll")]
        public static extern bool Everything_GetResultDateCreated(UInt32 nIndex, out long lpFileTime);
        [DllImport("Everything64.dll")]
        public static extern bool Everything_GetResultDateModified(UInt32 nIndex, out long lpFileTime);
        [DllImport("Everything64.dll")]
        public static extern bool Everything_GetResultDateAccessed(UInt32 nIndex, out long lpFileTime);
        [DllImport("Everything64.dll")]
        public static extern UInt32 Everything_GetResultAttributes(UInt32 nIndex);
        [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultFileListFileName(UInt32 nIndex);
        [DllImport("Everything64.dll")]
        public static extern UInt32 Everything_GetResultRunCount(UInt32 nIndex);
        [DllImport("Everything64.dll")]
        public static extern bool Everything_GetResultDateRun(UInt32 nIndex, out long lpFileTime);
        [DllImport("Everything64.dll")]
        public static extern bool Everything_GetResultDateRecentlyChanged(UInt32 nIndex, out long lpFileTime);
        [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultHighlightedFileName(UInt32 nIndex);
        [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultHighlightedPath(UInt32 nIndex);
        [DllImport("Everything64.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultHighlightedFullPathAndFileName(UInt32 nIndex);
        [DllImport("Everything64.dll")]
        public static extern UInt32 Everything_GetRunCountFromFileName(string lpFileName);
        [DllImport("Everything64.dll")]
        public static extern bool Everything_SetRunCountFromFileName(string lpFileName, UInt32 dwRunCount);
        [DllImport("Everything64.dll")]
        public static extern UInt32 Everything_IncRunCountFromFileName(string lpFileName);

        #endregion

        const int MaxSize = 4096;

        public static Process? EveryThingProcess = null;


        private static bool PerformQuery(string qry)
        {
            Everything_SetSearchW(qry);
            Everything_SetRequestFlags(EVERYTHING_REQUEST_FILE_NAME | EVERYTHING_REQUEST_PATH | EVERYTHING_REQUEST_DATE_MODIFIED | EVERYTHING_REQUEST_SIZE);
            Everything_SetSort(EVERYTHING_SORT_DATE_RUN_DESCENDING);
            return Everything_QueryW(true);
        }


        private static Result? CreateResultFromIndex(uint index)
        {
            var sb = new StringBuilder(MaxSize);
            Everything_GetResultFullPathName(index, sb, MaxSize);
            Everything_GetResultDateModified(index, out long date_modified);
            Everything_GetResultSize(index, out long size);
            var ptr= Everything_GetResultFileName(index);
            if(ptr==IntPtr.Zero)
            {
                return null;
            }
            var curFileName = Marshal.PtrToStringUni(ptr);

            return new Result(sb.ToString())
            {
                DateModified = DateTime.FromFileTime(date_modified),
                Size = size,
                Filename = curFileName,
                RunCount = (int)Everything_IncRunCountFromFileName(curFileName)
            };
        }
        public static IEnumerable<Result?> SearchWithPaging(string qry, uint pageSize = 100)
        {
            if (EveryThingProcess == null)
            {
                yield return null;
                yield break;
            }

            uint offset = 0;
            bool moreResults = true;

            while (moreResults)
            {
                // 设置分页
                Everything_SetMax(pageSize);
                Everything_SetOffset(offset);

                if (PerformQuery(qry))
                {
                    var resultCount = Everything_GetNumResults();
                    Trace.WriteLine($"Fetched {resultCount} results starting from {offset}");

                    if (resultCount < pageSize)
                    {
                        moreResults = false; // 最后一页，停止查询
                    }

                    for (uint i = 0; i < resultCount; i++)
                    {
                        yield return CreateResultFromIndex(i);
                    }

                    offset += (uint)pageSize; // 更新偏移量
                }
                else
                {
                    var code = Everything_GetLastError();
                    Trace.WriteLine($"Search failed with {code},{GetErrorDescribe((int)code)}");
                    yield break;
                }
            }
        }
        public static async IAsyncEnumerable<Result?> SearchWithPagingAsync([EnumeratorCancellation] CancellationToken token,string qry,uint pageSize = 100)
        {
            if (EveryThingProcess == null)
            {
                yield return null;
                yield break;
            }

            uint offset = 0;
            bool moreResults = true;

            while (moreResults)
            {
                if (token.IsCancellationRequested)
                    yield break;

                // 设置分页
                Everything_SetMax(pageSize);
                Everything_SetOffset(offset);

                if (PerformQuery(qry))
                {
                    var resultCount = Everything_GetNumResults();
                    Trace.WriteLine($"Fetched {resultCount} results starting from {offset}");

                    if (resultCount < pageSize)
                    {
                        moreResults = false; // 最后一页，停止查询
                    }

                    for (uint i = 0; i < resultCount; i++)
                    {
                        if (token.IsCancellationRequested)
                            yield break;

                        yield return CreateResultFromIndex(i);
                    }

                    offset += (uint)pageSize; // 更新偏移量
                }
                else
                {
                    var code = Everything_GetLastError();
                    Trace.WriteLine($"Search failed with {code},{GetErrorDescribe((int)code)}");
                    yield break;
                }
            }
        }


        public static bool StartEverythingApp()
        {
            try
            {
                var FilePath = FileMapper.AssetsFileDir.PathCombine("everything.exe");
                if (FilePath.IsFileExist())
                {
                    EveryThingProcess = Process.GetProcessesByName("everything.exe").FirstOrDefault();
                    if (EveryThingProcess == null)
                    {
                        EveryThingProcess = new Process()
                        {
                            StartInfo = new ProcessStartInfo()
                            {
                                FileName = FilePath,
                                Arguments = "-startup -minimized",
                                Verb = "runas",
                            }
                        };
                        return EveryThingProcess.Start();
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return false;
            }

        }

        public static void StopEverythingApp()
        {
            if (EveryThingProcess != null)
            {
                EveryThingProcess.Close();
                EveryThingProcess.Dispose();
                EveryThingProcess = null;
            }
        }

        /// <summary>
        /// get error describe
        /// </summary>
        /// <param name="currentErrorCode"></param>
        /// <returns></returns>
        public static string GetErrorDescribe(int currentErrorCode) =>
          currentErrorCode switch
          {
              EVERYTHING_OK => "正常",
              EVERYTHING_ERROR_MEMORY => "内存问题导致失败",
              EVERYTHING_ERROR_IPC => "进程通信导致失败",
              EVERYTHING_ERROR_REGISTERCLASSEX => "注册失败",
              EVERYTHING_ERROR_CREATEWINDOW => "窗体创建失败",
              EVERYTHING_ERROR_CREATETHREAD => "线程创建失败",
              EVERYTHING_ERROR_INVALIDINDEX => "错误的序号",
              EVERYTHING_ERROR_INVALIDCALL => "错误的调用",
              _ => $"未知错误{currentErrorCode}"
          };


    }

    public partial class Result : ObservableObject
    {
        [ObservableProperty]
        long size; //in bytes
        [ObservableProperty]
        DateTime dateModified;
        [ObservableProperty]
        string filename = "";
        [ObservableProperty]
        string path = "";
        [ObservableProperty]
        BitmapSource? icon;
        [ObservableProperty]
        int runCount = 0;

        public bool Folder => Size < 0;

        public Result(string filePath)
        {
            Path = filePath;
            LoadIconAsync();
        }

        void LoadIconAsync()
        {
            Task.Run(async () =>
            {
                try
                {
                   
                    var curRes = await Task.Run(() =>
                    {
                        if (Path.IsFileExist())
                        {
                            using (Icon? ico = System.Drawing.Icon.ExtractAssociatedIcon(Path))
                            {
                                var bitmap = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                                bitmap.Freeze();
                                return bitmap;
                            }
                        }
                        else if(Directory.Exists(Path))
                        {
                            var ico = DefaultIcons.FolderLarge;
                            var bitmap = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                            bitmap.Freeze();
                            return bitmap;
                        }
                        return null;
                    });
                    _ = Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        Icon = curRes;
                    }, System.Windows.Threading.DispatcherPriority.Background);
                }
                catch
                {

                }

            });
        }

        public override string ToString()
            => $"Name: {Filename}\tSize (B): {(Folder ? "(Folder)" : Size)}\tModified: {DateModified:d}\t" +
            $"Path: {Path}...";
    }

    public static class DefaultIcons
    {
        private static readonly Lazy<Icon> _lazyFolderIcon = new Lazy<Icon>(FetchIcon, true);

        public static Icon FolderLarge
        {
            get { return _lazyFolderIcon.Value; }
        }

        private static Icon FetchIcon()
        {
            var tmpDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())).FullName;
            var icon = ExtractFromPath(tmpDir);
            Directory.Delete(tmpDir);
            return icon;
        }

        private static Icon ExtractFromPath(string path)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            SHGetFileInfo(
                path,
                0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                SHGFI_ICON | SHGFI_LARGEICON);
            return System.Drawing.Icon.FromHandle(shinfo.hIcon);
        }

        //Struct used by SHGetFileInfo function
        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x000000001;
    }
}
