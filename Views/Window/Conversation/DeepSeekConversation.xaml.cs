using DesktopTimer.Models.DeepSeek;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace DesktopTimer.Views.Window.Conversation
{
    /// <summary>
    /// DeepSeekConversation.xaml 的交互逻辑
    /// </summary>
    public partial class DeepSeekConversation : FluentWindow
    {
        public DeepSeekConversation()
        {
            InitializeComponent();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                (this.DataContext as DeepSeek)?.StartCompletionsCommand.Execute(null);
            }
        }
    }
}
