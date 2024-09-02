using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata;
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

namespace DesktopTimer.Views.Controls
{
    /// <summary>
    /// AudioWaveControl.xaml 的交互逻辑
    /// </summary>
    public partial class AudioWaveControl : UserControl
    {
        public enum ApperenceType
        {
            /// <summary>
            /// 矩形
            /// </summary>
            [Display(Name ="矩形")]
            Rectangles = 0,
            /// <summary>
            /// 折线
            /// </summary>
            [Display(Name ="折线")]
            BrokenLines,
            /// <summary>
            /// 折线圆
            /// </summary>
            [Display(Name ="圆形")]
            BrokenLinesCircle
        }

        #region Datas

        AudioWaveDataModel WaveDataModel = new AudioWaveDataModel();

        Random random = new Random();

        List<Rectangle> rects = new List<Rectangle>();

        Polyline polyLine = new Polyline();

        Polygon polygon = new Polygon();

        #endregion

        #region dependcy properties



        public ApperenceType Apperence
        {
            get { return (ApperenceType)GetValue(ApperenceProperty); }
            set { SetValue(ApperenceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Apperence.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ApperenceProperty =
            DependencyProperty.Register("Apperence", typeof(ApperenceType), typeof(AudioWaveControl),
                new PropertyMetadata(ApperenceType.Rectangles, OnApperenceTypeChanged));



        public int UnitCount
        {
            get { return (int)GetValue(UnitCountProperty); }
            set { SetValue(UnitCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UnitCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnitCountProperty =
            DependencyProperty.Register("UnitCount", typeof(int), typeof(AudioWaveControl), new PropertyMetadata(100, OnUnitCountChanged));



        public double UnitRadius
        {
            get { return (double)GetValue(UnitRadiusProperty); }
            set { SetValue(UnitRadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Radius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnitRadiusProperty =
            DependencyProperty.Register("UnitRadius", typeof(double), typeof(AudioWaveControl), new PropertyMetadata(2.0d));



        public double UnitStrokeWidth
        {
            get { return (double)GetValue(UnitStrokeWidthProperty); }
            set { SetValue(UnitStrokeWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UnitBorderWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnitStrokeWidthProperty =
            DependencyProperty.Register("UnitStrokeWidth", typeof(double), typeof(AudioWaveControl), new PropertyMetadata(1.0d));




        public double UnitOpacity
        {
            get { return (double)GetValue(UnitOpacityProperty); }
            set { SetValue(UnitOpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UnitOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnitOpacityProperty =
            DependencyProperty.Register("UnitOpacity", typeof(double), typeof(AudioWaveControl), new PropertyMetadata(1.0d));



        public bool UsingRandomUnitColor
        {
            get { return (bool)GetValue(UsingRandomUnitColorProperty); }
            set { SetValue(UsingRandomUnitColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UsingRandomUnitColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UsingRandomUnitColorProperty =
            DependencyProperty.Register("UsingRandomUnitColor", typeof(bool), typeof(AudioWaveControl), new PropertyMetadata(false));




        public Color? SpecificUnitColor
        {
            get { return (Color?)GetValue(SpecificUnitColorProperty); }
            set { SetValue(SpecificUnitColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SpecificUnitColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpecificUnitColorProperty =
            DependencyProperty.Register("SpecificUnitColor", typeof(Color?), typeof(AudioWaveControl), new PropertyMetadata(null));



        public Color? SpecificUnitStrokeColor
        {
            get { return (Color?)GetValue(SpecificUnitStrokeColorProperty); }
            set { SetValue(SpecificUnitStrokeColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SpecificUnitStrokeColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpecificUnitStrokeColorProperty =
            DependencyProperty.Register("SpecificUnitStrokeColor", typeof(Color?), typeof(AudioWaveControl), new PropertyMetadata(null));




        public double DataWeight
        {
            get { return (double)GetValue(DataWeightProperty); }
            set { SetValue(DataWeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DataWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataWeightProperty =
            DependencyProperty.Register("DataWeight", typeof(double), typeof(AudioWaveControl), new PropertyMetadata(0.5, OnDataWeightChanged));




        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Enabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.Register("Enabled", typeof(bool), typeof(AudioWaveControl), new PropertyMetadata(true));



        public double EllipseRadius
        {
            get { return (double)GetValue(EllipseRadiusProperty); }
            set { SetValue(EllipseRadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EllipseRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EllipseRadiusProperty =
            DependencyProperty.Register("EllipseRadius", typeof(double), typeof(AudioWaveControl), new PropertyMetadata(20.0d));


        #endregion

        #region dep prop callback
        public static void OnApperenceTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AudioWaveControl aw)
            {
                aw.OnApperenceTypeChanged();
            }
        }

        void OnApperenceTypeChanged()
        {
            //clear all childs
            ClearAllChildOnCanvas();
            OnUnitCountChanged();
        }

        public static void OnDataWeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AudioWaveControl aw)
            {
                aw.OnDataWeightChanged();
            }
        }

        void OnDataWeightChanged()
        {
            WaveDataModel.DataWeight = DataWeight;
        }

        public static void OnUnitCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AudioWaveControl aw)
            {
                aw.OnUnitCountChanged();
            }
        }
        void OnUnitCountChanged()
        {
            var curApperence = System.Windows.Application.Current.Dispatcher.Invoke(() => Apperence);
            Action? act = curApperence switch
            {
                ApperenceType.Rectangles => OnRectangleApperenceCountChanged,
                ApperenceType.BrokenLines => OnBrokenLineApperenceCountChanged,
                ApperenceType.BrokenLinesCircle => OnBrokenLineCircleApperenceCountChanged,
                _ => null
            };
            System.Windows.Application.Current?.Dispatcher?.Invoke(act);
        }

        public static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AudioWaveControl aw)
            {
                aw.OnUnitCountChanged();
            }
        }

        void OnEnableChanged()
        {
            if(Enabled)
            {
                StartRecord();
            }
            else
            {
                StopRecord();
            }
        }
        #endregion

        #region constructor
        public AudioWaveControl()
        {
            InitializeComponent();
            Loaded += AudioWaveControl_Loaded;
            Unloaded += AudioWaveControl_Unloaded;
        }

        private void AudioWaveControl_Unloaded(object sender, RoutedEventArgs e)
        {
            WaveDataModel.StopRecord();
        }

        private void AudioWaveControl_Loaded(object sender, RoutedEventArgs e)
        {
            WaveDataModel.WaveDataChanged += WaveDataModel_WaveDataChanged;
            WaveDataModel.WaveRecordsStarted+= OnUnitCountChanged;
            if (Enabled)
            {
                WaveDataModel.StartRecord();
            }
        }


        #endregion

        #region methods


        public void StartRecord()
        {
            WaveDataModel.StartRecord();
        }

        public void StopRecord()
        {
            WaveDataModel.StopRecord();
        }

        Color GetRandomColor()
        {
            return Color.FromRgb(Convert.ToByte(random.Next() % 255), Convert.ToByte(random.Next() % 255), Convert.ToByte(random.Next() % 255));
        }



        private void WaveDataModel_WaveDataChanged(float[] samples)
        {
            if (!this.IsVisible || Visibility != Visibility.Visible)
                return;
            var curApperence = System.Windows.Application.Current.Dispatcher.Invoke(() => Apperence);
            Action<float[]>? act = curApperence switch
            {
                ApperenceType.Rectangles => OnRectangleApperenceDataChanged,
                ApperenceType.BrokenLines => OnBrokenLinesApperenceDataChanged,
                ApperenceType.BrokenLinesCircle => OnBrokenLinesCircleApperenceDataChanged,
                _ => null
            };

            System.Windows.Application.Current.Dispatcher.BeginInvoke(act, samples);
        }

        void ClearAllChildOnCanvas()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var itr in rects)
                {
                    if (AudioVisualizerDrawArea.Children.Contains(itr))
                        AudioVisualizerDrawArea.Children.Remove(itr);
                }
                if (AudioVisualizerDrawArea.Children.Contains(polyLine))
                    AudioVisualizerDrawArea.Children.Remove(polyLine);
                if (AudioVisualizerDrawArea.Children.Contains(polygon))
                    AudioVisualizerDrawArea.Children.Remove(polygon);
            });
        }

        void OnRectangleApperenceCountChanged()
        {
            for (int i = 0; i < UnitCount; i++)
            {
                if(rects.Count>i)
                {
                    if (AudioVisualizerDrawArea.Children.Contains(rects[i]))
                        AudioVisualizerDrawArea.Children.Remove(rects[i]);
                }

            }
            rects.Clear();
            for (int i = 0; i < UnitCount; i++)
            {
                var Brush = new SolidColorBrush(SpecificUnitColor ?? Colors.White);
                if (UsingRandomUnitColor)
                    Brush = new SolidColorBrush(GetRandomColor());

                rects.Add(new Rectangle()
                {
                    Stroke = new SolidColorBrush(Colors.White),
                    StrokeThickness = UnitStrokeWidth,
                    SnapsToDevicePixels = true,
                    UseLayoutRounding = true,
                });
            }
        }

        void OnBrokenLineApperenceCountChanged()
        {
            if(AudioVisualizerDrawArea.Children.Contains(polyLine))
                AudioVisualizerDrawArea.Children.Remove(polyLine);
        }

        void OnBrokenLineCircleApperenceCountChanged()
        {
            if (AudioVisualizerDrawArea.Children.Contains(polygon))
                AudioVisualizerDrawArea.Children.Remove(polygon);
        }

        void OnRectangleApperenceDataChanged(float[] samples)
        {
            if (samples.Length == 0)
            {
                System.Windows.Application.Current?.Dispatcher?.BeginInvoke(new Action(() =>
                {
                    for (int i = 0; i < UnitCount; i++)
                    {

                        if (AudioVisualizerDrawArea.Children.Contains(rects[i]))
                            AudioVisualizerDrawArea.Children.Remove(rects[i]);

                    }
                }));
                return;
            }

            var widthPercent = AudioVisualizerDrawArea.ActualWidth / (UnitCount - 1);//ActualWidth / finalData.Count();

            int diffCount = samples.Length / UnitCount;


            for (int i = 0; i < UnitCount; i++)
            {
                var curRect = rects[i];
                curRect.Width = widthPercent;
                curRect.Height = (i * diffCount >= samples.Length) ? 1 : ((samples[i * diffCount]) < 0 ? 0 : (samples[i * diffCount]));
                curRect.RenderTransform = new RotateTransform() { Angle = 180 };
                curRect.RadiusX = UnitRadius;
                curRect.RadiusY = UnitRadius;
                curRect.StrokeThickness = UnitStrokeWidth;
                curRect.Stroke = new SolidColorBrush(SpecificUnitStrokeColor ?? Colors.White);
                curRect.Fill = UsingRandomUnitColor ? ((curRect.Fill as SolidColorBrush)?.Color == SpecificUnitColor ?
                    new SolidColorBrush(GetRandomColor()) : curRect.Fill) : new SolidColorBrush(SpecificUnitColor ?? Colors.White);
                Canvas.SetLeft(curRect, (double)(i * widthPercent));
                Canvas.SetTop(curRect, ActualHeight);
                if (AudioVisualizerDrawArea.Children.Contains(curRect))
                    AudioVisualizerDrawArea.Children.Remove(curRect);
                AudioVisualizerDrawArea.Children.Add(curRect);
            }
        }

        void OnBrokenLinesApperenceDataChanged(float[] samples)
        {
            if (samples.Length == 0)
            {
                if(AudioVisualizerDrawArea.Children.Contains(polyLine))
                {
                    AudioVisualizerDrawArea.Children.Remove(polyLine);
                }
                return;
            }
            var widthPercent = AudioVisualizerDrawArea.ActualWidth / (UnitCount - 1);//ActualWidth / finalData.Count();

            int diffCount = samples.Length / UnitCount;

            polyLine.Points.Clear();
            polyLine.Fill= new SolidColorBrush(Colors.Transparent);
            polyLine.Stroke = UsingRandomUnitColor ?
                ((polyLine.Stroke as SolidColorBrush)?.Color == SpecificUnitColor ?
                    new SolidColorBrush(GetRandomColor()) : polyLine.Stroke) : 
                    new SolidColorBrush(SpecificUnitColor ?? Colors.White);
            polyLine.StrokeThickness = UnitStrokeWidth;
            for (int i = 0; i < UnitCount; i++)
            {
                var curX = (i*widthPercent)+(widthPercent/2);
                var curY = this.ActualHeight- ( (i * diffCount >= samples.Length) ? 1 : ((samples[i * diffCount]) < 0 ? 0 : (samples[i * diffCount])));
                polyLine.Points.Add(new Point(curX, curY));
            }
            Canvas.SetLeft(polyLine,0);
            Canvas.SetTop(polyLine,0);

            if(!AudioVisualizerDrawArea.Children.Contains(polyLine))
            {
                AudioVisualizerDrawArea.Children.Add(polyLine);
            }
        }


        void OnBrokenLinesCircleApperenceDataChanged(float[] samples)
        {
            if (samples.Length == 0)
            {
                polygon.Points.Clear();
                if (AudioVisualizerDrawArea.Children.Contains(polygon))
                {
                    AudioVisualizerDrawArea.Children.Remove(polygon);
                }
                return;
            }

            polygon.Points.Clear();
            double anglePerUnit = 360.0 / UnitCount;
            var centerX = ActualWidth / 2;
            var centerY = ActualHeight / 2;
            polygon.Stroke = UsingRandomUnitColor ? ((polygon.Stroke as SolidColorBrush)?.Color == SpecificUnitColor ?
                    new SolidColorBrush(GetRandomColor()) : polygon.Stroke) : 
                    new SolidColorBrush(SpecificUnitColor ?? Colors.White);

            polygon.StrokeThickness = UnitStrokeWidth;
            polygon.Fill = new SolidColorBrush(Colors.Transparent);
            for (int i = 0; i < UnitCount; i++)
            {
                double sampleValue = (i * samples.Length / UnitCount >= samples.Length) ? 1 : samples[i * samples.Length / UnitCount];
                double height = 0.1 * sampleValue;

                var a = Math.PI / 180 * anglePerUnit * i;

                double x = centerX + (EllipseRadius + height) * Math.Cos(a);
                double y = centerY + (EllipseRadius + height) * Math.Sin(a);

                polygon.Points.Add(new Point(x, y));
            }

            if(!AudioVisualizerDrawArea.Children.Contains(polygon))
                AudioVisualizerDrawArea.Children.Add(polygon);
        }
        #endregion
    }
}
