using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopTimer.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;

namespace DesktopTimer.Models.Everything
{
    public partial class EverythingWrapper : ObservableObject
    {
        #region datas

        #endregion

        #region properties
        [ObservableProperty]
        string searchKey = "";

        [ObservableProperty]
        ObservableCollection<Result?>? currentResults = new ObservableCollection<Result?>();

        [ObservableProperty]
        Result? selectedResult;

        [ObservableProperty]
        bool shouldOpenFlyouts = false;
        #endregion

        #region constructor
        MainWorkModel? curModelInstance = null;
        public EverythingWrapper(MainWorkModel modelInstance)
        {
            curModelInstance = modelInstance;
            Everything.StartEverythingApp();
        }

        ~EverythingWrapper()
        {
            Everything.StopEverythingApp();
        }
        #endregion



        #region Command 
        ICommand? startSearchCommand = null;
        public ICommand? StartSearchCommand
        {
            get => startSearchCommand ?? (startSearchCommand = new RelayCommand(() =>
            {
                PerformSearch(SearchKey);
            }));
        }


        ICommand? openFileCommand = null;
        public ICommand? OpenFileCommand
        {
            get => openFileCommand ?? (openFileCommand = new RelayCommand(() =>
            {
                if (SelectedResult == null)
                    return;
                if (File.Exists(SelectedResult.Path))
                {
                    Process process = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo(SelectedResult.Path);
                    process.StartInfo = startInfo;
                    process.Start();
                }
                if (Directory.Exists(SelectedResult.Path))
                {
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
                    psi.Arguments = $"{SelectedResult.Path}";
                    System.Diagnostics.Process.Start(psi);
                }
            }));
        }

        ICommand? openDirectoryCommand = null;
        public ICommand OpenDirectoryCommand
        {
            get => openDirectoryCommand ?? (openDirectoryCommand = new RelayCommand(() =>
            {
                if (SelectedResult == null)
                    return;
                if (File.Exists(SelectedResult.Path))
                {
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
                    psi.Arguments = "/e,/select," + SelectedResult.Path;
                    System.Diagnostics.Process.Start(psi);
                }
                if(Directory.Exists(SelectedResult.Path))
                {
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
                    psi.Arguments = $"{SelectedResult.Path}"; 
                    System.Diagnostics.Process.Start(psi);
                }
            }));
        }
        #endregion

        #region methods

        private CancellationTokenSource? searchCanceller = null;
        private object searchLock = new object(); // 用于锁定操作

        private const int PageSize = 15; // 每页显示的结果数
        private int currentPage = 0;     // 当前页码
      
        Dictionary<string,IAsyncEnumerable<Result?>> everythingSearchCache = new 
            Dictionary<string, IAsyncEnumerable<Result?>>();

        public async void LoadMoreResults()
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            if(!everythingSearchCache.ContainsKey(SearchKey))
            {
                everythingSearchCache.Add(SearchKey, Everything.SearchAsync(SearchKey,CancellationToken.None));
            }
            var resultInUse = everythingSearchCache[SearchKey];
            try
            {
                int skipCount = currentPage * PageSize;
                int loadCount = 0;
                await foreach (var result in resultInUse)
                {

                    if(0<=(skipCount--))
                        continue;
                    if (result != null)
                    {
                        loadCount++;
                        _ = Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            CurrentResults?.Add(result);
                        });
                    }
                    if(loadCount>=PageSize)
                    {
                        currentPage++;
                        break;
                    }
                }

                Trace.WriteLine($"Loaded {PageSize} results");
            }
            catch (OperationCanceledException)
            {
                Trace.WriteLine("Search operation was canceled.");
            }
            sp.Stop();
            Trace.WriteLine($"Search spend {sp.ElapsedMilliseconds}ms");
        }

        public void PerformSearch(string searchKey,int debounceDelay = 300)
        {
            if (string.IsNullOrEmpty(searchKey))
                return;
            currentPage = 0;
            // 如果有进行中的搜索任务，取消它
            lock (searchLock)
            {
                if (searchCanceller != null)
                {
                    searchCanceller.Cancel();
                    searchCanceller.Dispose();
                    ShouldOpenFlyouts =false;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CurrentResults?.Clear();
                    });
                }

                searchCanceller = new CancellationTokenSource();
            }

            var token = searchCanceller.Token;

            // 延迟执行搜索任务，防止每次输入都触发
            Task.Delay(debounceDelay).ContinueWith(_ =>
            {
                if (token.IsCancellationRequested)
                    return;

                try
                {
                    // 开始搜索操作

                    if (!token.IsCancellationRequested)
                    {
                        // 更新搜索结果
                        LoadMoreResults();
                        SelectedResult = null;
                        ShouldOpenFlyouts = true;
                    }
                }
                catch (OperationCanceledException)
                {
                    // 搜索操作被取消
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
                finally
                {
                    lock (searchLock)
                    {
                        if (searchCanceller?.Token == token)
                        {
                            searchCanceller = null;
                        }
                    }
                }
            }, token);
        }

        public void CancelAll()
        {
            searchCanceller?.Cancel();
        }
        #endregion
    }
}
