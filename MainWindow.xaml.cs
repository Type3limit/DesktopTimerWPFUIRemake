using DesktopTimer.Views;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;
using Wpf.Ui;
using MahApps.Metro.Controls;
using Appearance = Wpf.Ui.Appearance;
using DesktopTimer.models;
using CommunityToolkit.Mvvm.Messaging;
using DesktopTimer.Helpers;
using System.Windows.Shell;
using Wpf.Ui.Interop;
using HotKey = DesktopTimer.Helpers.HotKey;
using DesktopTimer.Models;
using DeskTopTimer;
namespace DesktopTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        MainWorkModel? ModelInstance = null;

        static OptionsWindow? translateWindow = null;
        public MainWindow()
        {
            //Appearance.SystemThemeWatcher.Watch(this);
            InitializeComponent();
            Initilize();
            Loaded += MainWindow_Loaded;
            DataContextChanged += MainWindow_DataContextChanged;
            WeakReferenceMessenger.Default.Register<RequestCloseProgramMessage>(this, (o, e) =>
            {
                Environment.Exit(0);
            });

            NavigationCommands.BrowseBack.InputGestures.Clear();
            NavigationCommands.BrowseForward.InputGestures.Clear();
        }


        private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue is MainWorkModel mainModel)
            {
                ModelInstance = mainModel;
                ModelInstance?.SetShotKeyDiscribe(new List<HotKey>() { hiddenKey, flashKey, setKey, hiddenTimerKey, showWebFlyOut, /*showTranslate, showEmoji*/ });
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var TimerPage = new TimerPage();
            TimerPage.DataContext = this.DataContext;
            TimerPage.MouseMoveHandler += TimerPage_MouseMoveHandler ;
            ContentFrame.Navigate(TimerPage);

            windowInstance = this;
            
        }

        private void TimerPage_MouseMoveHandler(MouseEventArgs e)
        {
            if( Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }


        private void Initilize()
        {

        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu con)
            {
                con.DataContext = this.DataContext;
            }
        }



        HotKey hiddenKey = new HotKey(Key.H, KeyModifier.Shift | KeyModifier.Alt, new Action<HotKey>(OnHiddenKey), "窗口隐藏\\显示");
        HotKey flashKey = new HotKey(Key.F, KeyModifier.Shift | KeyModifier.Alt, new Action<HotKey>(OnFreshKey), "刷新");
        HotKey setKey = new HotKey(Key.S, KeyModifier.Shift | KeyModifier.Alt, new Action<HotKey>(OnSetKey), "设置显示\\隐藏");
        HotKey hiddenTimerKey = new HotKey(Key.T, KeyModifier.Shift | KeyModifier.Alt, new Action<HotKey>(OnHiddenTimerKey), "时间隐藏\\显示");
        HotKey showWebFlyOut = new HotKey(Key.U, KeyModifier.Shift | KeyModifier.Alt, new Action<HotKey>(OnShowWebFlyOut), "网页地址显示\\隐藏");
        //暂时屏蔽Everything api
        //HotKey showEveryThingFlyOut = new HotKey(Key.E, KeyModifier.Shift | KeyModifier.Alt, new Action<HotKey>(OnShowEveryThingFlyOut), "搜索本机文件");
        HotKey showTranslate = new HotKey(Key.Z, KeyModifier.Shift | KeyModifier.Alt, new Action<HotKey>(OnTranslate), "唤起翻译窗口");
        //HotKey showEmoji = new HotKey(Key.Q, KeyModifier.Shift | KeyModifier.Alt, new Action<HotKey>(OnEmoji), "唤起表情包窗口");

        static MainWindow windowInstance = null;
        static bool IsWindowShow = false;
        static public void OnHiddenKey(HotKey currentKey)
        {
            if (windowInstance == null)
                return;
            if (IsWindowShow)
            {
                windowInstance.Hide();
                IsWindowShow = false;
            }
            else
            {
                windowInstance.Show();
                windowInstance.Activate();
                IsWindowShow = true;
            }

        }

        static public void OnFreshKey(HotKey currentKey)
        {
            if (windowInstance == null)
                return;
            if (windowInstance?.DataContext is MainWorkModel mainWorkSpace)
            {
                mainWorkSpace?.DisplaySetting.RequestFlushCommand.Execute(null);
            }
        }

        static public void OnSetKey(HotKey currentKey)
        {
            if (windowInstance == null)
                return;
            if (windowInstance?.DataContext is MainWorkModel mainWorkSpace)
            {
                mainWorkSpace?.DisplaySetting.OpenSettingCommand.Execute(null);
            }
        }

        static public void OnHiddenTimerKey(HotKey currentKey)
        {
            if (windowInstance == null)
                return;
            if (windowInstance?.DataContext is MainWorkModel mainWorkSpace)
            {
                mainWorkSpace.DisplaySetting.IsTimerBorderVisiable = !(mainWorkSpace.DisplaySetting.IsTimerBorderVisiable);
            }
        }

        static public void OnShowWebFlyOut(HotKey currentKey)
        {
            if (windowInstance == null)
                return;
            if (windowInstance?.DataContext is MainWorkModel mainWorkSpace)
            {
                mainWorkSpace?.DisplaySetting.OpenExtraSettingCommand.Execute(null);
            }
        }

        static public void OnShowEveryThingFlyOut(HotKey currentKey)
        {
            //if (windowInstance == null)
            //    return;
            //if (windowInstance?.DataContext is MainWorkModel mainWorkSpace)
            //{
            //    mainWorkSpace.IsOpenSearchFlyOut = true;
            //}
        }


        static public void OnTranslate(HotKey currentKey)
        {
            if (translateWindow != null && !translateWindow.IsClosed)
            {
                translateWindow.WindowClose();
                return;
            }
            else
            {
                translateWindow = new OptionsWindow();
                translateWindow.DataContext = (windowInstance.DataContext as MainWorkModel)?.Translator;
                translateWindow.Show();
                translateWindow.Activate();
                translateWindow.Focus();
            }

        }

        static public void OnEmoji(HotKey currentKey)
        {
            //if (emojiWindow != null && !emojiWindow.IsClosed)
            //{
            //    emojiWindow.WindowClose();
            //    return;
            //}
            //else
            //{
            //    emojiWindow = new EmojiWindow();
            //    emojiWindow.DataContext = windowInstance.DataContext;
            //    emojiWindow.Show();
            //    emojiWindow.Activate();
            //    emojiWindow.Focus();
            //}

        }

    }
}