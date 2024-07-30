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

namespace DesktopTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var TimerPage = new TimerPage();
            TimerPage.MouseMoveHandler += TimerPage_MouseMoveHandler; ;
            ContentFrame.Navigate(TimerPage);
        }

        private void TimerPage_MouseMoveHandler(MouseEventArgs e)
        {
            DragMove();
        }
    }
}