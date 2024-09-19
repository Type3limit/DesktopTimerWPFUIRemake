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
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;

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
        ObservableCollection<Result?> currentResults = new ObservableCollection<Result?>();

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
                if (Directory.Exists(SelectedResult.Path))
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
        private object searchLock = new object();

        private const int PageSize = 15;
        private int currentPage = 0;

        ConcurrentDictionary<string, IEnumerable<Result?>> everythingSearchCache = new
            ConcurrentDictionary<string, IEnumerable<Result?>>();

        bool HasOneLoadedAct = false;
        public void LoadMoreResults(string curSKey)
        {
            if (HasOneLoadedAct)
                return;
            HasOneLoadedAct = true;

            Stopwatch sp = new Stopwatch();
            sp.Start();
            try
            {
                var curKey = $"{curSKey}_{currentPage}";
                if (!everythingSearchCache.ContainsKey(curKey))
                {
                    everythingSearchCache.TryAdd(curKey, Everything.SearchWithPaging(curSKey, PageSize));
                }

                var resultInUse = everythingSearchCache[curKey];

                int skipCount = (currentPage) * PageSize;
                resultInUse.Skip(skipCount).Take(PageSize).ToList().ForEach((result) =>
                {
                    if (result != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if(!CurrentResults.Contains(result))
                            {
                                CurrentResults?.Add(result);
                            }
                        });
                    }
                });
                Trace.WriteLine($"Loaded {PageSize} results,current page:{currentPage++}");
            }
            catch (OperationCanceledException)
            {
                Trace.WriteLine("Search operation was canceled.");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Exception occurred: {ex}");
            }
            finally
            {
                HasOneLoadedAct = false;
            }
            sp.Stop();
            Trace.WriteLine($"Search completed in {sp.ElapsedMilliseconds}ms");
        }

        public void PerformSearch(string searchKey)
        {
            if (string.IsNullOrEmpty(searchKey))
                return;

            currentPage = 0; // 重置页码
            lock (searchLock)
            {
                // 清除上次的搜索
                if (searchCanceller != null)
                {
                    searchCanceller.Cancel();
                    searchCanceller.Dispose();
                }
                ShouldOpenFlyouts = false;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CurrentResults?.Clear();
                });
                // 新建 CancellationTokenSource，虽然这里同步，但保留逻辑可以方便未来扩展
                searchCanceller = new CancellationTokenSource();
            }

            try
            {
                if (searchCanceller?.IsCancellationRequested == false)
                {
                    // 执行搜索
                    LoadMoreResults(SearchKey);
                    SelectedResult = null;
                    ShouldOpenFlyouts = true;
                }
            }
            catch (OperationCanceledException)
            {
                Trace.WriteLine("Search operation was canceled.");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Search error: {ex}");
            }
            finally
            {
                // 清理资源
                lock (searchLock)
                {
                    if (searchCanceller != null && !searchCanceller.IsCancellationRequested)
                    {
                        searchCanceller = null;
                    }
                }
            }
        }



        public void RequestForLoadMoreResults()
        {
            LoadMoreResults(SearchKey);
        }
        public void CancelAll()
        {
            searchCanceller?.Cancel();
        }
        #endregion
    }
}
