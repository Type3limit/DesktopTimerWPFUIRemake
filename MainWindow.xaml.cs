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
namespace DesktopTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        MainWorkModel? ModelInstance = null;
        public MainWindow()
        {
            //Appearance.SystemThemeWatcher.Watch(this);
            InitializeComponent();
            Initilize();
            Loaded += MainWindow_Loaded;
            DataContextChanged += MainWindow_DataContextChanged;
        }

        private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue is MainWorkModel mainModel)
            {
                ModelInstance = mainModel;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var TimerPage = new TimerPage();
            TimerPage.DataContext = this.DataContext;
            TimerPage.MouseMoveHandler += TimerPage_MouseMoveHandler ;
            ContentFrame.Navigate(TimerPage);
        }

        private void TimerPage_MouseMoveHandler(MouseEventArgs e)
        {
            if( Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseProgram_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Initilize()
        {
            SettingFlyout.Closed += (o, e) => 
            {
                ModelInstance?.DisplaySetting?.CloseSettingCommand?.Execute(null);
            };
        }
    }
}