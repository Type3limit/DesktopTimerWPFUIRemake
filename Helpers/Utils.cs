using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Expression = System.Linq.Expressions.Expression;
using System.IO.Compression;

namespace DesktopTimer.Helpers
{
    public class HotKey : IDisposable
    {
        private static Dictionary<int, HotKey>? _dictHotKeyToCalBackProc;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, UInt32 fsModifiers, UInt32 vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public const int WmHotKey = 0x0312;

        private bool _disposed = false;

        public Key Key { get; private set; }
        public KeyModifier KeyModifiers { get; private set; }
        public Action<HotKey> Action { get; private set; }
        public int Id { get; set; }
        public string name { set; get; }
        // ******************************************************************
        public HotKey(Key k, KeyModifier keyModifiers, Action<HotKey> action, string name, bool register = true)
        {
            Key = k;
            KeyModifiers = keyModifiers;
            Action = action;
            this.name = name;
            if (register)
            {
                Register();
            }
        }

        // ******************************************************************
        public bool Register()
        {
            int virtualKeyCode = KeyInterop.VirtualKeyFromKey(Key);
            Id = virtualKeyCode + ((int)KeyModifiers * 0x10000);
            bool result = RegisterHotKey(IntPtr.Zero, Id, (UInt32)KeyModifiers, (UInt32)virtualKeyCode);

            if (_dictHotKeyToCalBackProc == null)
            {
                _dictHotKeyToCalBackProc = new Dictionary<int, HotKey>();
                ComponentDispatcher.ThreadFilterMessage += new ThreadMessageEventHandler(ComponentDispatcherThreadFilterMessage);
            }

            _dictHotKeyToCalBackProc.Add(Id, this);

            Debug.Print(result.ToString() + ", " + Id + ", " + virtualKeyCode);
            return result;
        }

        // ******************************************************************
        public void Unregister()
        {
            if (_dictHotKeyToCalBackProc?.TryGetValue(Id, out HotKey hotKey)==true)
            {
                UnregisterHotKey(IntPtr.Zero, Id);
                _dictHotKeyToCalBackProc.Remove(Id);  // 从字典中移除
            }
        }

        // ******************************************************************
        private static void ComponentDispatcherThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            if (!handled)
            {
                if (msg.message == WmHotKey)
                {
                    HotKey hotKey;

                    if (_dictHotKeyToCalBackProc.TryGetValue((int)msg.wParam, out hotKey))
                    {
                        if (hotKey.Action != null)
                        {
                            hotKey.Action.Invoke(hotKey);
                        }
                        handled = true;
                    }
                }
            }
        }

        // ******************************************************************
        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // ******************************************************************
        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be _disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be _disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    Unregister();
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }

    // ******************************************************************
    [Flags]
    public enum KeyModifier
    {
        None = 0x0000,
        Alt = 0x0001,
        Ctrl = 0x0002,
        NoRepeat = 0x4000,
        Shift = 0x0004,
        Win = 0x0008
    }

    // ******************************************************************
    public static class ExtensionsMethods
    {
        #region StringRelated
        /// <summary>
        /// To check string is null or empty
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string obj)
        {
            return string.IsNullOrEmpty(obj);
        }
        /// <summary>
        /// To check target file exist
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsFileExist(this string path)
        {
            return !string.IsNullOrEmpty(path) && File.Exists(path);
        }
        /// <summary>
        /// Read target file as string
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string? ReadText(this string path)
        {
            if (path.IsFileExist())
            {
                return FileStrReader.Read(path, Encoding.UTF8);
            }
            return string.Empty;
        }
        /// <summary>
        /// Write content to target file
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="content"></param>
        public static void WriteText(this string Path, string content)
        {
            using (var Stream = File.Open(Path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                Stream.SetLength(0);
                var curBytes = Encoding.UTF8.GetBytes(content);
                Stream.Write(curBytes, 0, curBytes.Length);
            }
        }
        /// <summary>
        /// Serialize current object to json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string? ToJson<T>(this T obj) where T : class
        {
            try
            {
                return JsonSerializer.Serialize(obj, typeof(T));
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        /// Deserilize json string to object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T? DeSerialize<T>(this string str) where T : class
        {
            try
            {
                return JsonSerializer.Deserialize<T>(str);
            }
            catch (Exception ex)
            {
                Trace.Write(ex);
                return null;
            }
        }
        /// <summary>
        /// To check if directory is exist
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="shouldCreateIfNotExsit"></param>
        /// <returns></returns>
        public static bool IsDirctoryExist(this string Path, bool shouldCreateIfNotExsit = false)
        {
            bool res = Directory.Exists(Path);
            if (shouldCreateIfNotExsit)
                Directory.CreateDirectory(Path);
            return shouldCreateIfNotExsit ? true : res;
        }
        /// <summary>
        /// Combine pathes
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="combinedPath"></param>
        /// <returns></returns>
        public static string PathCombine(this string srcPath, string combinedPath)
        {
            return Path.Combine(srcPath, combinedPath);
        }
        /// <summary>
        /// Get file name of current path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string getName(this string path)
        {
            return Path.GetFileName(path);
        }
        /// <summary>
        /// Get extension name of current path
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string getExtension(this string ex)
        {
            return Path.GetExtension(ex);
        }
        /// <summary>
        /// Get file name without extension
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string getNameWithOutEx(this string name)
        {
            return Path.GetFileNameWithoutExtension(name);
        }

        /// <summary>
        /// To print a bunch of object 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objs"></param>
        /// <param name="Splitter"></param>
        /// <returns></returns>
        public static string SourcesToPrintString<T>(this IEnumerable<T> objs, char Splitter = ' ')
        {
            StringBuilder builder = new StringBuilder();
            foreach (var obj in objs)
            {
                builder.Append(obj?.ToString());
                builder.Append(Splitter);
            }
            builder.Remove(builder.Length - 1, 1);//remove last
            return builder.ToString();
        }
        /// <summary>
        /// sha encode
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string? SHA1(this string obj)
        {
            if (obj.IsNullOrEmpty())
            {
                return null;
            }
            byte[] cleanBytes = Encoding.Default.GetBytes(obj);
            byte[] hashedBytes = System.Security.Cryptography.SHA1.Create().ComputeHash(cleanBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "");

        }

        /// <summary>
        /// md5 encode
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string? MD5(this string obj)
        {
            if (obj.IsNullOrEmpty())
            {
                return null;
            }
            byte[] cleanBytes = Encoding.Default.GetBytes(obj);
            byte[] hashedBytes = System.Security.Cryptography.MD5.Create().ComputeHash(cleanBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "");

        }
        /// <summary>
        /// To check if current string contained chinese 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool ContainChinese(this string input)
        {
            string pattern = "[\u4e00-\u9fbb]";
            return Regex.IsMatch(input, pattern);
        }

        /// <summary>
        /// To check if current file is gif 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsGif(this string source)
        {
            if (!source.IsFileExist())
                return false;
            var gif = Encoding.ASCII.GetBytes("GIF");    // GIF
            var buffer = new byte[4];
            using (Stream stream = File.OpenRead(source))
            {
                stream.Read(buffer, 0, buffer.Length);
                var res = gif.SequenceEqual(buffer.Take(gif.Length));
                stream.Close();
                return res;
            }

        }

        /// <summary>
        /// get directory size
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static long GetDirectorySize(this string dirPath)
        {
            if (!System.IO.Directory.Exists(dirPath))
                return 0;
            long len = 0;
            DirectoryInfo di = new DirectoryInfo(dirPath);
            foreach (FileInfo item in di.GetFiles())
            {
                len += item.Length;
            }
            DirectoryInfo[] dis = di.GetDirectories();
            if (dis.Length > 0)
            {
                for (int i = 0; i < dis.Length; i++)
                {
                    len += GetDirectorySize(dis[i].FullName);
                }
            }
            return len;
        }


        /// <summary>
        /// get file size
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static long GetFileSize(this string filePath)
        {
            long temp = 0;
            if (!File.Exists(filePath))
            {
                string[] strs = Directory.GetFileSystemEntries(filePath);
                foreach (string item in strs)
                {
                    temp += GetFileSize(item);
                }
            }
            else
            {
                FileInfo fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }
            return temp;
        }

        /// <summary>
        /// compress with brotli
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string CompressBrotli(this string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            using (MemoryStream output = new MemoryStream())
            {
                using (BrotliStream brotli = new BrotliStream(output, CompressionMode.Compress))
                {
                    brotli.Write(inputBytes, 0, inputBytes.Length);
                }
                return Convert.ToBase64String(output.ToArray());
            }
        }

        /// <summary>
        /// decompress with brotli
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string DecompressBrotli(this string compressedInput)
        {
            byte[] compressedBytes = Convert.FromBase64String(compressedInput);
            using (MemoryStream input = new MemoryStream(compressedBytes))
            using (BrotliStream brotli = new BrotliStream(input, CompressionMode.Decompress))
            using (MemoryStream output = new MemoryStream())
            {
                brotli.CopyTo(output);
                return Encoding.UTF8.GetString(output.ToArray());
            }
        }

        #endregion


        #region AttachedProperty
        private static ConcurrentDictionary<int, ConcurrentDictionary<CompareableWeakReference, ConcurrentDictionary<string, object?>>> AttachedRecoreds =
            new ConcurrentDictionary<int, ConcurrentDictionary<CompareableWeakReference, ConcurrentDictionary<string, object?>>>();
        /// <summary>
        /// Set any attached properties to an object
        /// </summary>
        /// <typeparam name="T">Target object type</typeparam>
        /// <typeparam name="P">Attached object type</typeparam>
        /// <param name="key">Key to mark current attached object</param>
        /// <param name="obj">Attached object </param>
        /// <param name="property">Value of attached object </param>
        public static bool SetExProperty<T, P>(this T obj, string key, P property) where T : class
        {
            if (obj == null)
                return false;
            var HashCode = obj?.GetHashCode();
            if (HashCode == null)
                return false;
            if (!AttachedRecoreds.ContainsKey(HashCode.Value))
            {
                if (!AttachedRecoreds.TryAdd(HashCode.Value, new ConcurrentDictionary<CompareableWeakReference, ConcurrentDictionary<string, object?>>()))
                {
                    Trace.WriteLine($"Write ExProperty faild [Src:{obj?.GetType()}][Key:{key}][Value:{property?.GetType()}]");
                    return false;
                }
            }


            if (AttachedRecoreds[HashCode.Value].Count <= 0)
            {
                if (!AttachedRecoreds[HashCode.Value].TryAdd(new CompareableWeakReference(obj), new ConcurrentDictionary<string, object?>()))
                {
                    Trace.WriteLine($"Write ExProperty faild [Src:{obj?.GetType()}][Key:{key}][Value:{property?.GetType()}]");
                    return false;
                }
            }

            var storeKeyPairs = AttachedRecoreds[HashCode.Value].FirstOrDefault(o => o.Key.GetHashCode() == obj?.GetHashCode());

            if (storeKeyPairs.Value == null)
                return false;

            if (storeKeyPairs.Value.ContainsKey(key))
            {
                storeKeyPairs.Value[key] = property;
            }
            else
            {
                if (!storeKeyPairs.Value.TryAdd(key, property))
                {
                    Trace.WriteLine($"Write ExProperty faild [Src:{obj?.GetType()}][Key:{key}][Value:{property?.GetType()}]");
                    return false;
                }
            }
            return true;

        }

        /// <summary>
        /// Get attached properties
        /// </summary>
        /// <typeparam name="T">Target object type</typeparam>
        /// <typeparam name="P">Attached object type</typeparam>
        /// <param name="key">Key to mark current attached object</param>
        /// <param name="obj">Attached object </param>
        /// <param name="Property">Value of attached object </param>
        /// <returns>If current attached property get successful</returns>
        public static bool TryGetExProperty<T, P>(this T obj, string key, out P? Property) where T : class
        {
            Property = default(P);
            if (obj == null)
                return false;
            var HashCode = obj?.GetHashCode();
            if (HashCode == null)
                return false;
            if (!AttachedRecoreds.ContainsKey(HashCode.Value))
                return false;
            var Target = AttachedRecoreds[HashCode.Value].FirstOrDefault(x => x.Key.GetHashCode() == obj?.GetHashCode());

            if (Target.Key == null || Target.Value == null || !(Target.Value).ContainsKey(key))
                return false;

            if (Target.Value.TryGetValue(key, out object? data))
            {
                var con = TypeDescriptor.GetConverter(typeof(P));
                Property = (P?)con.ConvertTo(data, typeof(P));
                return true;
            }
            return false;
        }
        #endregion


        #region Actions
        /// <summary>
        /// Do next if current predicate returns true
        /// </summary>
        public static T IfDo<T>(this T status, Predicate<T> Predicate, Action<T>? ifTrue = null) where T : class
        {
            if (Predicate(status))
            {
                ifTrue?.Invoke(status);
            }
            return status;
        }

        /// <summary>
        /// With current objects, do next if predicate returns true,otherwise returned
        /// </summary>
        public static IEnumerable<T> IfDoElseBack<T>(this IEnumerable<T> srcs, Predicate<T> predicate, Action<T>? ifTrue = null) where T : class
        {
            List<T> restOf = new List<T>();
            srcs?.IfDo(p => p != null, p =>
            {
                foreach (var o in p)
                {
                    (predicate(o) ? ifTrue : restOf.Add)?.Invoke(o);
                }
            });
            return restOf;
        }

        /// <summary>
        /// Breakable foreach
        /// </summary>
        public static IEnumerable<T>? ForeachBreakable<T>(this IEnumerable<T> srcs, Predicate<T> breakWhile, Action<T>? action = null) where T : class
        {
            return srcs?.IfDo(o => o != null, o =>
            {
                foreach (var itr in o)
                {
                    if (breakWhile(itr))
                        break;
                    action?.Invoke(itr);
                }
            });
        }



        /// <summary>
        /// Get a tuple
        /// </summary>
        public static Tuple<T, V> GetTupleWith<T, V>(this T src, V src2) where T : class where V : class
        {
            return new Tuple<T, V>(src, src2);
        }

        /// <summary>
        /// Get current object copy with CopyedPropertyAttribute
        /// </summary>
        public static T GetMemberwiseCopy<T>(this T dst) where T : class
        {
            return MemberCopy.TransExp<T, T>(dst);
        }

        /// <summary>
        /// Pipe
        /// </summary>
        public static TResult Pipe<T, TResult>(this T arg, Func<T, TResult> method)
            => method.Invoke(arg);
        #endregion

        #region DelayTask

        /// <summary>
        /// Async task(void) with timeout
        /// </summary>
        public static async Task<bool> DelayTask(this Action proc, int millSecond)
        {

            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                var delayTask = Task.Delay(millSecond, timeoutCancellationTokenSource.Token);
                if (await Task.WhenAny(Task.Run(proc), delayTask) == delayTask)
                {
                    return true;
                }
                timeoutCancellationTokenSource.Cancel();
                return false;
            }
        }

        /// <summary>
        /// Async task(T) with timeout
        /// </summary>
        public static async Task<T?> DelayTask<T>(this Func<T> proc, int millSecond)
        {

            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                var delayTask = Task.Delay(millSecond, timeoutCancellationTokenSource.Token);
                var workTask = Task.Run(proc);
                if (await Task.WhenAny(workTask, delayTask) == delayTask)
                {
                    return await workTask;
                }
                timeoutCancellationTokenSource.Cancel();
                return default;
            }
        }

        #endregion

        #region SingleThreadTask

        private static ConcurrentDictionary<string, ConcurrentQueue<Action>> refTasks = new ConcurrentDictionary<string, ConcurrentQueue<Action>>();
        private static ConcurrentDictionary<string, CancellationTokenSource> taskMarks = new ConcurrentDictionary<string, CancellationTokenSource>();
        private static async void StartTask(string key)
        {
            if (!refTasks.ContainsKey(key))
                return;
            if (!taskMarks.ContainsKey(key) || taskMarks[key] == null)
            {
                taskMarks[key] = new CancellationTokenSource();
            }
            await Task.Run(() =>
            {
                var curTask = refTasks[key];
                var curToken = taskMarks[key];
                try
                {
                    while (curTask.Count <= 0 && !curToken.IsCancellationRequested)
                    {
                        if (curTask.TryDequeue(out var curAction))
                        {
                            curAction?.Invoke();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
                finally
                {
                    taskMarks[key] = null;
                }

            });
        }
        /// <summary>
        /// 同类方法集中，单线程统一调度执行
        /// </summary>
        /// <param name="proc">要执行的方法</param>
        /// <param name="taskKey">分组key</param>
        /// <param name="reftoken">外部提前终止量</param>
        /// <returns></returns>
        public static void StartActionTask(this Action proc, string taskKey, CancellationTokenSource reftoken = null)
        {
            try
            {
                if (!refTasks.ContainsKey(taskKey))
                {
                    refTasks[taskKey] = new ConcurrentQueue<Action>();
                }
                refTasks[taskKey].Enqueue(proc);
                if (taskMarks.ContainsKey(taskKey) && taskMarks[taskKey] != null)//not null means task with taskKey not fished,just add one and exit
                    return;
                else//build a cancellationTokenSource if refToken is null
                {
                    if (reftoken != null)
                    {
                        taskMarks[taskKey] = reftoken;
                    }
                    else
                    {
                        taskMarks[taskKey] = new CancellationTokenSource();
                    }
                    StartTask(taskKey);
                }
            }
            catch (Exception ex)
            {
                Trace.Write(ex);
            }
        }

        #endregion



        #region Parallel
        /// <summary>
        /// Get parallel query with actions
        /// </summary>
        /// <param name="act"></param>
        /// <param name="moreActs"></param>
        /// <returns></returns>
        public static ParallelQuery<T>? WithParllel<T>(this T act, params T[] moreActs) where T : class
        {
            if (act == null)
                return null;
            var actList = new List<T>();
            actList.Add(act);
            if (moreActs != null)
            {
                foreach (var itr in moreActs)
                {
                    actList.Add(itr);
                }
            }
            return actList.AsParallel();
        }
        /// <summary>
        /// Get parallel query with actions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="act"></param>
        /// <param name="moreActs"></param>
        /// <returns></returns>
        public static ParallelQuery<T>? WithParllel<T>(this ParallelQuery<T> act, params T[] moreActs) where T : class
        {
            if (act == null)
                return null;
            var actList = act.ToList();
            if (moreActs != null)
            {
                foreach (var itr in moreActs)
                {
                    actList.Add(itr);
                }
            }
            return actList.AsParallel();
        }
        #endregion

        #region DateTime

        public static DateTime GetNowTime()
        {
            string ID = TimeZoneInfo.Local.Id;
            DateTime NowTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById(ID));
            return NowTime;
        }
        /// <summary>
        /// DateTime to timestamp
        /// </summary>
        /// <param name="_dataTime">时间</param>
        /// <param name="MilliTime">毫秒计时</param>
        /// <returns></returns>
        public static long ToTimestamp(this DateTime _dataTime, bool Millisecond = true)
        {
            string ID = TimeZoneInfo.Local.Id;
            DateTime start = new DateTime(1970, 1, 1) + TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            DateTime startTime = TimeZoneInfo.ConvertTime(start, TimeZoneInfo.FindSystemTimeZoneById(ID));
            DateTime NowTime = TimeZoneInfo.ConvertTime(_dataTime, TimeZoneInfo.FindSystemTimeZoneById(ID));
            long timeStamp;
            if (Millisecond)
                timeStamp = (long)(NowTime - startTime).TotalMilliseconds; // 相差毫秒数
            else
                timeStamp = (long)(NowTime - startTime).TotalSeconds; // 相差秒数
            return timeStamp;
        }
        public static DateTime ToDateTime(this long stamp, bool Millisecond = true)
        {
            string ID = TimeZoneInfo.Local.Id;
            DateTime start = new DateTime(1970, 1, 1) + TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            DateTime startTime = TimeZoneInfo.ConvertTime(start, TimeZoneInfo.FindSystemTimeZoneById(ID));
            DateTime dt;
            if (Millisecond)
                dt = startTime.AddMilliseconds(stamp);
            else
                dt = startTime.AddSeconds(stamp);

            return dt;
        }

        #endregion


        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        public static T GetAttributeOfType<T>(this Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }

    }

    public class CompareableWeakReference
    {
        private int _hashCode;

        private WeakReference? innerData;

        public CompareableWeakReference(object? obj)
        {
            if (obj != null)
            {
                innerData = new WeakReference(obj);
                _hashCode = obj.GetHashCode();
            }
            else
            {
                _hashCode = -1;
            }
        }

        public object? GetTarget()
        {
            return innerData?.Target;
        }

        public bool IsAlive
        {
            get => innerData == null ? false : innerData.IsAlive;
        }

        public override bool Equals(object? obj)
        {
            return (obj as CompareableWeakReference)?._hashCode == _hashCode;
        }


        public override int GetHashCode()
        {
            return _hashCode;
        }

    }

    public static class MemberCopy
    {

        private static Dictionary<string, object> _CacheDic = new Dictionary<string, object>();

        /// <summary>
        /// copy properties by same name or with CopyedPropertyAttribute.
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="tIn"></param>
        /// <returns></returns>
        public static TOut TransExp<TIn, TOut>(TIn tIn)
            where TIn : class
            where TOut : class
        {

            string key = string.Format("trans_exp_{0}_{1}", typeof(TIn).FullName, typeof(TOut).FullName);

            if (!_CacheDic.ContainsKey(key))
            {
                ParameterExpression parameterExpression = Expression.Parameter(typeof(TIn), "p");
                List<MemberBinding> memberBindingList = new List<MemberBinding>();

                foreach (var item in typeof(TOut).GetProperties())
                {
                    MemberExpression? property = null;
                    if (!item.IsDefined(typeof(CopyedPropertyAttribute), false))
                    {
                        var curProperty = typeof(TIn).GetProperty(item.Name);
                        if (curProperty == null)
                        {
                            Trace.WriteLine($"CopyProperty ignore{item.Name}");
                            continue;
                        }

                        property = Expression.Property(parameterExpression, curProperty);
                    }
                    else
                    {
                        var curProperty = item.GetCustomAttributes(typeof(CopyedPropertyAttribute), false).FirstOrDefault();
                        var IsCopyByDefault = (curProperty?.GetType()?.GetProperty("IsDefaultCopyMode")?.GetValue(curProperty)) as bool?;
                        var TargetName = curProperty?.GetType().GetProperty("TargetName")?.GetValue(curProperty) as string;
                        var SourceName = curProperty?.GetType().GetProperty("SourceName")?.GetValue(curProperty) as string;
                        if (SourceName == null)
                        {
                            Trace.WriteLine($"With CopyedPropertyAttribute try get sourceName failed");
                            continue;
                        }
                        var mem = IsCopyByDefault == true && !string.IsNullOrEmpty(SourceName) ? typeof(TIn).GetProperty(item.Name) : typeof(TIn).GetProperty(SourceName);
                        if (mem == null)
                        {
                            Trace.WriteLine($"[{typeof(TIn).FullName}=>{typeof(TOut).FullName} ]CopyProperty ignore [{item.Name}]");
                            continue;
                        }

                        property = Expression.Property(parameterExpression, mem);

                    }

                    if (property == null || item.PropertyType != property.Type)
                    {


                        Trace.WriteLine($"[{typeof(TIn).FullName}=>{typeof(TOut).FullName} ]CopyProperty ignore [{item.Name}]");

                        continue;
                    }

                    MemberBinding memberBinding = Expression.Bind(item, property);
                    memberBindingList.Add(memberBinding);
                }

                MemberInitExpression memberInitExpression = Expression.MemberInit(Expression.New(typeof(TOut)), memberBindingList.ToArray());
                Expression<Func<TIn, TOut>> lambda = Expression.Lambda<Func<TIn, TOut>>(memberInitExpression, new ParameterExpression[] { parameterExpression });
                Func<TIn, TOut> func = lambda.Compile();
                _CacheDic[key] = func;
            }
            return ((Func<TIn, TOut>)_CacheDic[key])(tIn);
        }

    }

    public static class FileStrReader
    {
        #region Public Methods
        /// <summary>
        /// Read current file as string
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string? Read(string filePath, Encoding encodingType)
        {
            string? str = null;
            filePath.IfDo(o => o.IsFileExist(), o =>
            {
                try
                {
                    str = File.ReadAllText(filePath, encodingType);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"[{DateTime.Now.ToLocalTime()}]{ex}");
                }
            });
            return str;
        }
        #endregion
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class CopyedPropertyAttribute : Attribute
    {
        private string targetName = string.Empty;
        /// <summary>
        /// target name
        /// </summary>
        public string TargetName
        {
            get => targetName;
            set
            {
                targetName = value;
            }
        }
        private string sourceName = string.Empty;
        /// <summary>
        /// source name 
        /// </summary>
        public string SourceName
        {
            get => sourceName;
            set => sourceName = value;
        }
        /// <summary>
        /// is default copy mode
        /// </summary>
        public bool IsDefaultCopyMode
        {
            get => TargetName == SourceName;
        }

        public CopyedPropertyAttribute(string TargetName = "", string SourceName = "")
        {
            this.targetName = TargetName;
            this.sourceName = SourceName;
        }

    }

    public static class ImageHelper
    {
        static ImageHelper()
        {
            lock (typeof(ImageHelper))
            {
                _mapping = GetImageFormatMapping();
            }
        }
        private static IDictionary<Guid, String> _mapping;
        private static IDictionary<Guid, String> GetImageFormatMapping()
        {
            var dic = new Dictionary<Guid, String>();
            var properties = typeof(ImageFormat).GetProperties(
                BindingFlags.Static | BindingFlags.Public
            );
            foreach (var property in properties)
            {
                var format = property.GetValue(null, null) as ImageFormat;
                if (format == null) continue;
                dic[format.Guid] = "." + property.Name.ToLower();
            }
            return dic;
        }

        public static bool IsImageExtension(this string path)
        {
            try
            {
                string extension = Path.GetExtension(path).ToLower();
                if (_mapping.Values.Contains(extension))
                {
                    return true;
                }
                if (extension == ".jpg")
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static string GetImageExtension(this string path)
        {
            Image? img = null;
            try
            {
                if (!path.IsFileExist())
                    return string.Empty;
                img = Image.FromFile(path);
                var format = img.RawFormat;
                if (_mapping.ContainsKey(format.Guid))
                {
                    return _mapping[format.Guid];
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return string.Empty;
            }
            finally
            {
                img?.Dispose();
            }
        }
    }

    public class ImageTool
    {
        public struct Dpi
        {
            public double X { get; set; }

            public double Y { get; set; }

            public Dpi(double x, double y)
            {
                X = x;
                Y = y;
            }
        }

        public static async Task<BitmapImage?> LoadImg(string imagePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!File.Exists(imagePath))
                        return null;
                    BitmapImage bi = new BitmapImage();

                    // Begin initialization.
                    bi.BeginInit();
                    bi.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                    // Set properties.
                    bi.CacheOption = BitmapCacheOption.OnLoad;

                    using (Stream ms = new MemoryStream(File.ReadAllBytes(imagePath)))
                    {
                        if (ms.Length <= 0)
                        {
                            bi.EndInit();
                            bi.Freeze();
                            return bi;
                        }
                        bi.StreamSource = ms;
                        bi.EndInit();
                        bi.Freeze();
                    }
                    return bi;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                    return null;
                }

            });
        }


        public static Image? GetImageDirect(string imagePath)
        {
            byte[] byteImage = File.ReadAllBytes(imagePath);

            Image curImage;
            using (var ms = new MemoryStream(byteImage))
            {
                curImage = Image.FromStream(ms);
            }
            return curImage;
        }

        public static BitmapImage? GetImage(string imagePath)
        {
            try
            {
                BitmapImage bitmap = null;

                if (imagePath.StartsWith("pack://"))
                {
                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    if (Uri.TryCreate(imagePath, UriKind.RelativeOrAbsolute, out var current))
                    {
                        bitmap.UriSource = current;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }
                }
                else if (File.Exists(imagePath))
                {
                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    byte[] fileBytes = File.ReadAllBytes(imagePath);
                    using (Stream ms = new MemoryStream(fileBytes))
                    {
                        if (ms.Length <= 0)
                        {
                            return null;
                        }
                        ms.Position = 0;  // 确保流的起始位置是正确的
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }
                }

                return bitmap;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return null;
            }
        }



        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);


        public static Dpi GetDpiByGraphics()
        {
            double dpiX;
            double dpiY;

            using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                dpiX = graphics.DpiX;
                dpiY = graphics.DpiY;
            }

            return new Dpi(dpiX, dpiY);
        }

        public static ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }


        static public DrawingImage CreateABitMap(string DrawingText, double FontSize, Typeface cur)
        {
            var pixels = new byte[1080 * 1080 * 4];
            for (int i = 0; i < 1080 * 1080 * 4; i += 4)
            {
                pixels[i] = 0;
                pixels[i + 1] = 0;
                pixels[i + 2] = 0;
                pixels[i + 3] = 255;
            }
            BitmapSource bitmapSource = BitmapSource.Create(1080, 1080, 96, 96, PixelFormats.Pbgra32, null, pixels, 1080 * 4);
            var visual = new DrawingVisual();

            var CenterX = 540;
            var CenterY = 540;
            var Dpi = GetDpiByGraphics();//GetSystemDpi
            var formatText = new FormattedText(DrawingText, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                        cur, FontSize, System.Windows.Media.Brushes.White, Dpi.X / 96d);
            System.Windows.Point textLocation = new System.Windows.Point(CenterX - formatText.WidthIncludingTrailingWhitespace / 2, CenterY - formatText.Height / 2);

            using (DrawingContext drawingContext = visual.RenderOpen())
            {
                drawingContext.DrawImage(bitmapSource, new Rect(0, 0, 1080, 1080));
                drawingContext.DrawText(formatText, textLocation);
            }
            return new DrawingImage(visual.Drawing);
        }

        static public bool SaveDrawingToFile(DrawingImage drawing, string fileName, double scale = 1d)
        {
            drawing.Freeze();
            return System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    var drawingImage = new System.Windows.Controls.Image { Source = drawing };
                    var width = drawing.Width * scale;
                    var height = drawing.Height * scale;
                    drawingImage.Arrange(new Rect(0, 0, width, height));

                    var bitmap = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
                    bitmap.Render(drawingImage);

                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));

                    using (var stream = new FileStream(fileName, FileMode.Create))
                    {
                        encoder.Save(stream);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                    return false;
                }
            });


        }



        [DllImport("Shell32.dll")]
        private static extern IntPtr SHGetFileInfo
        (
            string pszPath, //一个包含要取得信息的文件相对或绝对路径的缓冲。它可以处理长或短文件名。（也就是指定的文件路径）注[1]
            uint dwFileAttributes,//资料上说，这个参数仅用于uFlags中包含SHGFI_USEFILEATTRIBUTES标志的情况(一般不使用)。如此，它应该是文件属性的组合：存档，只读，目录，系统等。
            out SHFILEINFO psfi,
            uint cbfileInfo,//简单地给出上项结构的尺寸。
            SHGFI uFlags//函数的核心变量，通过所有可能的标志，你就能驾驭函数的行为和实际地得到信息。
        );


        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public SHFILEINFO(bool b)
            {
                hIcon = IntPtr.Zero; iIcon = 0; dwAttributes = 0; szDisplayName = ""; szTypeName = "";
            }
            public IntPtr hIcon;//图标句柄
            public int iIcon;//系统图标列表的索引
            public uint dwAttributes; //文件的属性
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 260)]
            public string szDisplayName;//文件的路径等 文件名最长256（ANSI），加上盘符（X:\）3字节，259字节，再加上结束符1字节，共260
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 80)]
            public string szTypeName;//文件的类型名 固定80字节
        };



        private enum SHGFI
        {
            SmallIcon = 0x00000001,
            LargeIcon = 0x00000000,
            Icon = 0x00000100,
            DisplayName = 0x00000200,//Retrieve the display name for the file, which is the name as it appears in Windows Explorer. The name is copied to the szDisplayName member of the structure specified in psfi. The returned display name uses the long file name, if there is one, rather than the 8.3 form of the file name. Note that the display name can be affected by settings such as whether extensions are shown.
            Typename = 0x00000400,  //Retrieve the string that describes the file's type. The string is copied to the szTypeName member of the structure specified in psfi.
            SysIconIndex = 0x00004000, //Retrieve the index of a system image list icon. If successful, the index is copied to the iIcon member of psfi. The return value is a handle to the system image list. Only those images whose indices are successfully copied to iIcon are valid. Attempting to access other images in the system image list will result in undefined behavior.
            UseFileAttributes = 0x00000010 //Indicates that the function should not attempt to access the file specified by pszPath. Rather, it should act as if the file specified by pszPath exists with the file attributes passed in dwFileAttributes. This flag cannot be combined with the SHGFI_ATTRIBUTES, SHGFI_EXETYPE, or SHGFI_PIDL flags.
        }

        /// <summary>
        /// 根据文件扩展名得到系统扩展名的图标
        /// </summary>
        /// <param name="fileName">文件名(如：win.rar;setup.exe;temp.txt)</param>
        /// <param name="largeIcon">图标的大小</param>
        /// <returns></returns>
        public static Icon? GetFileIcon(string fileName, bool largeIcon)
        {
            SHFILEINFO info = new SHFILEINFO(true);
            int cbFileInfo = Marshal.SizeOf(info);
            SHGFI flags;
            if (largeIcon)
                flags = SHGFI.Icon | SHGFI.LargeIcon | SHGFI.UseFileAttributes;
            else
                flags = SHGFI.Icon | SHGFI.SmallIcon | SHGFI.UseFileAttributes;
            IntPtr IconIntPtr = SHGetFileInfo(fileName, 256, out info, (uint)cbFileInfo, flags);
            if (IconIntPtr.Equals(IntPtr.Zero))
                return null;
            return Icon.FromHandle(info.hIcon);
        }

        /// <summary>  
        /// 获取文件夹图标
        /// </summary>  
        /// <returns>图标</returns>  
        public static Icon? GetDirectoryIcon(string path, bool largeIcon)
        {
            SHFILEINFO _SHFILEINFO = new SHFILEINFO();
            int cbFileInfo = Marshal.SizeOf(_SHFILEINFO);
            SHGFI flags;
            if (largeIcon)
                flags = SHGFI.Icon | SHGFI.LargeIcon;
            else
                flags = SHGFI.Icon | SHGFI.SmallIcon;

            IntPtr IconIntPtr = SHGetFileInfo(path, 256, out _SHFILEINFO, (uint)cbFileInfo, flags);
            if (IconIntPtr.Equals(IntPtr.Zero))
                return null;
            Icon _Icon = Icon.FromHandle(_SHFILEINFO.hIcon);
            return _Icon;
        }





    }

    /// <summary>
    /// 正则枚举文件
    /// </summary>
    public static class RegexDirectoryEnumrator
    {   // Regex version
        public static IEnumerable<string> GetFiles(string path,
                            string searchPatternExpression = "",
                            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            Regex reSearchPattern = new Regex(searchPatternExpression, RegexOptions.IgnoreCase);
            return Directory.EnumerateFiles(path, "*", searchOption)
                            .Where(file =>
                                     reSearchPattern.IsMatch(Path.GetExtension(file)));
        }

        // Takes same patterns, and executes in parallel
        public static IEnumerable<string> GetFiles(string path,
                            string[] searchPatterns,
                            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return searchPatterns.AsParallel()
                   .SelectMany(searchPattern =>
                          Directory.EnumerateFiles(path, searchPattern, searchOption));
        }
    }
}
