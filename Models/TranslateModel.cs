using CommunityToolkit.Mvvm.ComponentModel;
using DesktopTimer.Helpers;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Text.Json.Nodes;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text.Json;
using DesktopTimer.Models;
using CommunityToolkit.Mvvm.Messaging;

namespace DesktopTimer.Views.Models
{
    public class TranslateModel : ObservableObject
    {
        #region property
        private string translateSource = "";
        public string TranslateSource
        {
            get => translateSource;
            set
            {
                if (translateSource != value)
                {

                    SetProperty(ref translateSource, value);
                }

            }
        }


        private bool shouldOpenTranslateResult = false;
        /// <summary>
        /// 标识是否需要开启翻译结果视图
        /// </summary>
        public bool ShouldOpenTranslateResult
        {
            get => shouldOpenTranslateResult;
            set => SetProperty(ref shouldOpenTranslateResult, value);
        }

        private ObservableCollection<string?> translateResult = new ObservableCollection<string?>();
        /// <summary>
        /// 翻译结果
        /// </summary>
        public ObservableCollection<string?> TranslateResult
        {
            get => translateResult;
            set => SetProperty(ref translateResult, value);
        }

        private string? selectedTranslateResult = null;
        /// <summary>
        /// 选中的翻译结果
        /// </summary>
        public string? SelectedTranslateResult
        {
            get => selectedTranslateResult;
            set => SetProperty(ref selectedTranslateResult, value);
        }

        private volatile string? curTanslateObject = null;
        public string? CurTranslateObject
        {
            get => curTanslateObject;
            set => curTanslateObject = value;

        }
        #endregion

        #region Baidu

        /// <summary>
        /// baidu translate
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public async Task<string?> BaiduTranslate(string? src, CancellationTokenSource? canceller)
        {
            try
            {
                if (true == src?.IsNullOrEmpty())
                    return null;
                var appId = mainModelInstance.Config.TranslateConfigData.BaiduAppId;
                var securety = mainModelInstance.Config.TranslateConfigData.BaiduSecretKey;
                var url = mainModelInstance.Config.TranslateConfigData.BaiduTranslateUrl;

                url += "?q=" + HttpUtility.UrlEncode(src);
                var from = "en";
                var to = "zh";
                if (true == src?.ContainChinese())
                {
                    from = "zh";
                    to = "en";
                }
                Random random = new Random();
                string salt = random.Next(100000).ToString();
                string sign = EncryptString(appId + src + salt + securety);
                url += "&from=" + from;
                url += "&to=" + to;
                url += "&appid=" + appId;
                url += "&salt=" + salt;
                url += "&sign=" + sign;
                if (canceller == null)
                    return null;
                var Res = await url.GetAsync(cancellationToken: canceller.Token);
                if (Res.ResponseMessage.IsSuccessStatusCode)
                {
                    var ResponStream = await Res.GetStreamAsync();
                    StreamReader curStreamReader = new StreamReader(ResponStream, Encoding.GetEncoding("utf-8"));
                    string retString = curStreamReader.ReadToEnd();
                    curStreamReader.Close();
                    ResponStream.Close();
                    var jsonRes = JsonObject.Parse(HttpUtility.HtmlDecode(retString));
                    if (jsonRes == null)
                        return null;
                    var result = jsonRes["trans_result"];
                    if (jsonRes != null && result != null)
                    {
                        var arrayNode = JsonArray.Parse(result.ToJsonString());
                        if (arrayNode != null)
                        {
                            var array = (JsonArray)arrayNode;
                            var firstRes = array?.First();
                            if (firstRes != null && firstRes is JsonObject)
                            {
                                return firstRes["dst"]?.GetValue<string>();
                            }
                        }
                        return null;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return null;
            }

        }
        /// <summary>
        /// MD5
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EncryptString(string str)
        {
            MD5 md5 = MD5.Create();
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            byte[] byteNew = md5.ComputeHash(byteOld);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
        #endregion

        #region YouDao

        public async Task<string?> YouDaoTranslate(string? src, CancellationTokenSource? canceller)
        {
            try
            {
                if (true == src?.IsNullOrEmpty())
                    return null;
                var appId = mainModelInstance.Config.TranslateConfigData.YoudaoAppId;
                var securety = mainModelInstance.Config.TranslateConfigData.YouDaoSecretKey;
                var url = mainModelInstance.Config.TranslateConfigData.YoudaoTranslateUrl;

                var dic = new Dictionary<string, string>();
                string? q = src;
                string appKey = appId;
                string appSecret = securety;
                string salt = DateTime.Now.Millisecond.ToString();
                var from = "en";
                var to = "zh-CHS";
                if (true == src?.ContainChinese())
                {
                    from = "zh-CHS";
                    to = "en";
                }
                url += "?from=Auto";
                url += "&to=" + to;
                url += "&signType=v3";
                TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                long millis = (long)ts.TotalMilliseconds;
                string curtime = Convert.ToString(millis / 1000);
                url += "&curtime=" + curtime;
                string signStr = appKey + Truncate(q) + salt + curtime + appSecret;
                string sign = ComputeHash(signStr, SHA256.Create());
                url += "&q=" + System.Web.HttpUtility.UrlEncode(q);
                url += "&appKey=" + appKey;
                url += "&salt=" + salt;
                url += "&sign=" + sign;
                if (canceller == null)
                    return null;
                var Res = await url.PostAsync(null, cancellationToken: canceller.Token);
                if (Res.ResponseMessage.IsSuccessStatusCode)
                {
                    var ResponStream = await Res.GetStreamAsync();
                    StreamReader curStreamReader = new StreamReader(ResponStream, Encoding.GetEncoding("utf-8"));
                    string retString = curStreamReader.ReadToEnd();
                    curStreamReader.Close();
                    ResponStream.Close();
                    var jsonRes = JsonObject.Parse(HttpUtility.HtmlDecode(retString));
                    if(jsonRes==null)
                        return null;
                    var curRes = jsonRes["translation"];
                    if (jsonRes != null && curRes != null)
                    {
                        var arrayNode = JsonArray.Parse(curRes.ToJsonString());
                        if (arrayNode != null)
                        {
                            var array = arrayNode as JsonArray;
                            return array?.First()?.ToString();
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return null;
            }


        }
        protected static string ComputeHash(string input, HashAlgorithm algorithm)
        {
            Byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            Byte[] hashedBytes = algorithm.ComputeHash(inputBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "");
        }
        protected static string? Truncate(string q)
        {
            if (q == null)
            {
                return null;
            }
            int len = q.Length;
            return len <= 20 ? q : (q.Substring(0, 10) + len + q.Substring(len - 10, 10));
        }
        #endregion

        #region constructor
        MainWorkModel? mainModelInstance = null;

        public TranslateModel(MainWorkModel modelInstance)
        {
            mainModelInstance = modelInstance;
           
        }

        #endregion

        #region command

        CancellationTokenSource? translatedCanceller = null;
        ICommand? runTranslateCommand = null;
        public ICommand? RunTranslateCommand
        {
            get => runTranslateCommand ?? (runTranslateCommand = new RelayCommand<string?>(async (str) =>
            {
                if (string.IsNullOrEmpty(str) || CurTranslateObject == str)
                    return;

                InitializeTranslation();

                BaiduRequestedWords.Add(str);
                YouDaoRequestedWords.Add(str);
                CurTranslateObject = str;

                StartTranslationProcesses();


            }));
        }


        #endregion


        #region methods

        private void InitializeTranslation()
        {
            translatedCanceller = new CancellationTokenSource();
            BaiduRequestedWords.Clear();
            YouDaoRequestedWords.Clear();
            ShouldOpenTranslateResult = false;
            SelectedTranslateResult = null;
            System.Windows.Application.Current.Dispatcher.Invoke(() => TranslateResult.Clear());
        }

        private void StartTranslationProcesses()
        {
            Task.Run(() => StartYouDaoTranslate());
            Task.Run(() => StartBaiduTranslate());
        }

        List<string?> BaiduRequestedWords = new List<string?>();
        bool IsBaiduTranslateStarted = false;
        void StartBaiduTranslate()
        {
            if (IsBaiduTranslateStarted)
                return;
            while (!IsBaiduTranslateStarted)
                IsBaiduTranslateStarted = true;
            Task.Run(() =>
            {
                try
                {
                    while (BaiduRequestedWords?.Count > 0 && false == translatedCanceller?.IsCancellationRequested)
                    {
                        string? curRequestwords = null;
                        lock (BaiduRequestedWords)
                        {
                            curRequestwords = BaiduRequestedWords?.FirstOrDefault();
                            BaiduRequestedWords?.Remove(curRequestwords);
                        }
                        curRequestwords?.IfDo(x => !x.IsNullOrEmpty(), async (x) =>
                        {
                            Trace.WriteLine("request with baidu");
                            var TransResult = await BaiduTranslate(x, translatedCanceller);
                            if (false == TransResult?.IsNullOrEmpty())
                            {
                                Trace.WriteLine($"Baidu present a result with {x}");
                                SubmitTranslateResult(x, TransResult);
                                if (SelectedTranslateResult == null)
                                    SelectedTranslateResult = TransResult;
                                ShouldOpenTranslateResult = TranslateResult.Count > 0;
                            }
                            Trace.WriteLine($"request baidu end with {TransResult}");
                        });

                        Thread.Sleep(300);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
                finally
                {
                    while (IsBaiduTranslateStarted)
                        IsBaiduTranslateStarted = false;
                }

            });
        }


        List<string?> YouDaoRequestedWords = new List<string?>();
        bool IsYouDaoTranslateStarted = false;
        void StartYouDaoTranslate()
        {
            if (IsYouDaoTranslateStarted)
                return;
            while (!IsYouDaoTranslateStarted)
                IsYouDaoTranslateStarted = true;
            try
            {
                while (YouDaoRequestedWords?.Count > 0 && false == translatedCanceller?.IsCancellationRequested)
                {
                    string? curRequestwords = null;
                    lock (YouDaoRequestedWords)
                    {
                        curRequestwords = YouDaoRequestedWords?.FirstOrDefault();
                        YouDaoRequestedWords?.Remove(curRequestwords);
                    }
                    curRequestwords?.IfDo(x => !x.IsNullOrEmpty(), async (x) =>
                    {
                        var TransResult = await YouDaoTranslate(x, translatedCanceller);
                        if (false == TransResult?.IsNullOrEmpty())
                        {
                            SubmitTranslateResult(x, TransResult);
                            if (SelectedTranslateResult == null)
                                SelectedTranslateResult = TransResult;
                            ShouldOpenTranslateResult = TranslateResult.Count > 0;
                        }
                    });

                    Thread.Sleep(300);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                while (IsYouDaoTranslateStarted)
                    IsYouDaoTranslateStarted = false;
            }

        }


        void SubmitTranslateResult(string? key, string? result)
        {
            if (key == curTanslateObject)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    TranslateResult.Add(result);
                });
            }
        }



        public void CancelAll()
        {
            translatedCanceller?.Cancel();
        }
        #endregion
    }


}
   