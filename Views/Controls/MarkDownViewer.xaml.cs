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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DesktopTimer.Views.Window.Conversation
{
    /// <summary>
    /// MarkDownViewer.xaml 的交互逻辑
    /// </summary>
    public partial class MarkDownViewer : UserControl
    {

        public string MarkdownDocment
        {
            get { return (string)GetValue(MarkdownDocmentProperty); }
            set { SetValue(MarkdownDocmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MarkdownDocment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MarkdownDocmentProperty =
            DependencyProperty.Register("MarkdownDocment", typeof(string), typeof(MarkDownViewer), new PropertyMetadata(""));



        public MarkDownViewer()
        {
            InitializeComponent();
        }
    }
}
