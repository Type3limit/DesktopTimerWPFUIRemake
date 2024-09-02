using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;
using UserControl = System.Windows.Controls.UserControl;
using Vector = System.Windows.Vector;


namespace DesktopTimer.Views.Controls
{
    public enum SpecificThemeMode
    {
        Auto,
        Light,
        Dark
    }

    public interface IParticle
    {
        public abstract Color PointColor();

        public abstract Point Position();

        public abstract void Update(double width, double height, Point mousePosition, double MaxMouseConnectionDistance,
            double MouseAttractionDistance, bool ShouldIgnoreMouse);

        public abstract void Draw(DrawingContext dc, Brush pointColor);

        public abstract void ResetColor(Color curColor);

        public abstract void AdjustPosition(double width, double height);

        public static double GetDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }
    }

    public interface IColorGenerator
    {
        public abstract List<Color> GetRandomBackgroundPreset(SpecificThemeMode themeMode = SpecificThemeMode.Auto);

        public abstract System.Windows.Media.Brush GetBackgroundGradientBrush(List<Color> colorSets,double opacity =1);

        public abstract Color GetRandomPaticlePointPreset(SpecificThemeMode themeMode = SpecificThemeMode.Auto);

        public abstract Color GetLineColor(Color color1, Color color2, double opacity);
    }


    public interface ILinePainter
    {
        public abstract bool ConnectParticles(DrawingContext dc, IParticle particle1, IParticle particle2, IColorGenerator colorGenerator, double maxDistance, bool useColorfulPoint, Color defaultLineColor);


        public abstract void ConnectToMouse(DrawingContext dc, Point mousePosition, IParticle particle, IColorGenerator colorGenerator, double maxDistance, bool useColorfulPoint, Color defaultLineColor);
    }

    public partial class ParticleBackgroundControl : UserControl
    {

        #region data
        private List<IParticle>? particles;

        private Point mousePosition;

        private DrawingVisual? drawingVisual;

        private VisualHost visualHost;

        bool UnLinkMousePosition = false;

        private System.Windows.Media.Brush? backgroundBrush = null;

        System.Timers.Timer PointUpdateTimer = new System.Timers.Timer();

        #endregion

        #region dependency properties
        public double MaxConnectionDistance
        {
            get { return (double)GetValue(MaxConnectionDistanceProperty); }
            set { SetValue(MaxConnectionDistanceProperty, value); }
        }
        // Using a DependencyProperty as the backing store for MaxConnectionDistance.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxConnectionDistanceProperty =
            DependencyProperty.Register("MaxConnectionDistance", typeof(double), typeof(ParticleBackgroundControl), new PropertyMetadata(100d));



        public double MaxMouseConnectionDistance
        {
            get { return (double)GetValue(MaxMouseConnectionDistanceProperty); }
            set { SetValue(MaxMouseConnectionDistanceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxMouseConnectionDistance.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxMouseConnectionDistanceProperty =
            DependencyProperty.Register("MaxMouseConnectionDistance", typeof(double), typeof(ParticleBackgroundControl), new PropertyMetadata(100d));




        public static readonly DependencyProperty ParticleCountProperty =
        DependencyProperty.Register("ParticleCount", typeof(int), typeof(ParticleBackgroundControl), new PropertyMetadata(100, OnParticleCountChanged));

        public int ParticleCount
        {
            get => (int)GetValue(ParticleCountProperty);
            set
            {
                SetValue(ParticleCountProperty, value);
            }
        }




        public System.Windows.Media.Brush LineColor
        {
            get { return (System.Windows.Media.Brush)GetValue(LineColorProperty); }
            set { SetValue(LineColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineColorProperty =
            DependencyProperty.Register("LineColor", typeof(System.Windows.Media.Brush),
                typeof(ParticleBackgroundControl), new PropertyMetadata(new SolidColorBrush(Colors.Gray)));




        public System.Windows.Media.Brush PointColor
        {
            get { return (System.Windows.Media.Brush)GetValue(PointColorProperty); }
            set { SetValue(PointColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PointColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointColorProperty =
            DependencyProperty.Register("PointColor", typeof(System.Windows.Media.Brush), typeof(ParticleBackgroundControl),
                new PropertyMetadata(new SolidColorBrush(Colors.Gray)));




        public bool TrackMouse
        {
            get { return (bool)GetValue(TrackMouseProperty); }
            set
            {
                SetValue(TrackMouseProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for TrackMouse.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TrackMouseProperty =
            DependencyProperty.Register("TrackMouse", typeof(bool), typeof(ParticleBackgroundControl), new PropertyMetadata(true, OnMouseTracking));



        public bool UseColorfulPointLines
        {
            get { return (bool)GetValue(UseColorfulPointLinesProperty); }
            set { SetValue(UseColorfulPointLinesProperty, value); }
        }

        public static readonly DependencyProperty UseColorfulPointLinesProperty =
            DependencyProperty.Register("UseColorfulPointLines", typeof(bool), typeof(ParticleBackgroundControl), new PropertyMetadata(false));

        public bool UseColorfulBackground
        {
            get { return (bool)GetValue(UseColorfulBackgroundProperty); }
            set
            {
                SetValue(UseColorfulBackgroundProperty, value);
            }
        }


        public static readonly DependencyProperty UseColorfulBackgroundProperty =
            DependencyProperty.Register("UseColorfulBackground", typeof(bool), typeof(ParticleBackgroundControl), 
                new PropertyMetadata(false, OnEnableColorfulBackgroundChanged));




        public SpecificThemeMode Theme
        {
            get { return (SpecificThemeMode)GetValue(ThemeProperty); }
            set
            {
                SetValue(ThemeProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Theme.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThemeProperty =
            DependencyProperty.Register("Theme", typeof(SpecificThemeMode), typeof(ParticleBackgroundControl),
                new PropertyMetadata(SpecificThemeMode.Auto, OnThemeChange));





        public double ColorBackgroundOpacity
        {
            get { return (double)GetValue(ColorBackgroundOpacityProperty); }
            set { SetValue(ColorBackgroundOpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColorBackgroundOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorBackgroundOpacityProperty =
            DependencyProperty.Register("ColorBackgroundOpacity", typeof(double), typeof(ParticleBackgroundControl), new PropertyMetadata(0.2d));




        public int MaxPointConnection
        {
            get { return (int)GetValue(MaxPointConnectionProperty); }
            set { SetValue(MaxPointConnectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxPointConnection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxPointConnectionProperty =
            DependencyProperty.Register("MaxPointConnection", typeof(int), typeof(ParticleBackgroundControl), new PropertyMetadata(10));





        public long ParticleUpdateMilliseconds
        {
            get { return (long)GetValue(ParticleUpdateMillisecondsProperty); }
            set { SetValue(ParticleUpdateMillisecondsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ParticleUpdateMilliseconds.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParticleUpdateMillisecondsProperty =
            DependencyProperty.Register("ParticleUpdateMilliseconds", typeof(long), typeof(ParticleBackgroundControl),
                new PropertyMetadata((long)(1000 / 60.0), OnUpDateTimeChanged));

        public double MouseAttractionDistance
        {
            get { return (double)GetValue(MouseAttractionDistanceProperty); }
            set { SetValue(MouseAttractionDistanceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseAttractionDistanceProperty =
            DependencyProperty.Register("MouseAttractionDistance", typeof(double), typeof(ParticleBackgroundControl), new PropertyMetadata(5.0d));



        public Func<IParticle> ParticleCreator
        {
            get
            {
                if (GetValue(ParticleCreatorProperty) is not Func<IParticle> func)
                {
                    func = CreateDefaultParticleCreator();
                    SetValue(ParticleCreatorProperty, func);
                }
                return func;
            }
            set { SetValue(ParticleCreatorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ParticleCreator.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParticleCreatorProperty =
            DependencyProperty.Register("ParticleCreator", typeof(Func<IParticle>), typeof(ParticleBackgroundControl),
                new PropertyMetadata(null, OnCreatorChanged));





        public IColorGenerator ColorGenrator
        {
            get
            {
                if (GetValue(ColorGenratorProperty) is not IColorGenerator colorGenerator)
                {
                    colorGenerator = new ColorHelper();
                    SetValue(ColorGenratorProperty, colorGenerator);
                }
                return colorGenerator;
            }
            set { SetValue(ColorGenratorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColorGenrator.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorGenratorProperty =
            DependencyProperty.Register("ColorGenrator", typeof(IColorGenerator), typeof(ParticleBackgroundControl), new PropertyMetadata(new ColorHelper()));



        public ILinePainter LinePainter
        {
            get
            {
                if (GetValue(LinePainterProperty) is not ILinePainter linePainter)
                {
                    linePainter = new LinePainter();
                    SetValue(LinePainterProperty, linePainter);
                }
                return linePainter;
            }
            set { SetValue(LinePainterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LinePainter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LinePainterProperty =
            DependencyProperty.Register("LinePainter", typeof(ILinePainter), typeof(ParticleBackgroundControl), new PropertyMetadata(new LinePainter()));



        public double CornorRadius
        {
            get { return (double)GetValue(CornorRadiusProperty); }
            set { SetValue(CornorRadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CornorRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CornorRadiusProperty =
            DependencyProperty.Register("CornorRadius", typeof(double), typeof(ParticleBackgroundControl), new PropertyMetadata(4.0d));



        #endregion

        #region value change callback

        private static void OnParticleCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ParticleBackgroundControl;
            control?.OnParticleCountChanged();
        }
        private static void OnThemeChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ParticleBackgroundControl;
            control?.OnThemeChange();
        }
        private static void OnBackgroundOpacity(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ParticleBackgroundControl;
            control?.OnBackgroundOpacity();
        }

        private static void OnMouseTracking(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ParticleBackgroundControl;
            control?.OnMouseTracking();
        }

        private static void OnEnableColorfulBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ParticleBackgroundControl;
            control?.OnEnableColorfulBackgroundChanged();
        }

        private static void OnUpDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ParticleBackgroundControl;
            control?.OnUpdateTimeChanged();
        }

        private static void OnCreatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            var control = d as ParticleBackgroundControl;
            control?.OnCreatorChanged();
        }

        private void OnMouseTracking()
        {
            if (TrackMouse)
            {
                UnLinkMousePosition = false;
            }
            else
            {
                UnLinkMousePosition = true;
            }
        }

        private void OnThemeChange()
        {
            UpdateBackgroundBrush();
            if (particles != null)
            {
                foreach (var itr in particles)
                {
                    itr.ResetColor(ColorGenrator.GetRandomPaticlePointPreset(Theme));
                }
            }
        }

        private Func<IParticle> CreateDefaultParticleCreator()
        {
            return () => new Particle(ActualWidth, ActualHeight, ColorGenrator.GetRandomPaticlePointPreset(Theme));
        }

        private void OnParticleCountChanged()
        {
            var curParticleCount = particles?.Count ?? 0;
            var diff = Math.Abs(ParticleCount - curParticleCount);
            if (ParticleCount > curParticleCount)
            {
                for (int i = 0; i < diff; i++)
                {
                    particles?.Add(ParticleCreator());
                }
            }
            else
            {
                for (int i = 0; i < diff; i++)
                {
                    particles?.RemoveAt(0);
                }
            }
        }

        private void OnBackgroundOpacity()
        {
            if (backgroundBrush != null)
            {
                backgroundBrush.Opacity = ColorBackgroundOpacity;
            }
        }

        private void OnEnableColorfulBackgroundChanged()
        {
            UpdateBackgroundBrush();
        }
        private void OnUpdateTimeChanged()
        {
            PointUpdateTimer.Stop();
            PointUpdateTimer.Interval = ParticleUpdateMilliseconds;
            PointUpdateTimer.Start();
        }
        private void OnCreatorChanged()
        {
            InitializeParticles();
        }
        #endregion

        public ParticleBackgroundControl()
        {

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            SizeChanged += OnSizeChanged;
            visualHost = new VisualHost();
            Content = visualHost;
            UpdateBackgroundBrush();
            PointUpdateTimer.Interval = ParticleUpdateMilliseconds;
            PointUpdateTimer.Elapsed += PointUpdateTimer_Elapsed;
            PointUpdateTimer.Start();
        }

        private void PointUpdateTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            UpdateParticles();
        }

        private void InitializeParticles()
        {
            particles = new List<IParticle>();
            for (int i = 0; i < ParticleCount; i++)
            {
                var particle = ParticleCreator();
                particles.Add(particle);
            }
        }

        private void UpdateBackgroundBrush()
        {

            var curColorSet = ColorGenrator.GetRandomBackgroundPreset(Theme);
            this.backgroundBrush = ColorGenrator.GetBackgroundGradientBrush(curColorSet);
            OnBackgroundOpacity();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (particles == null || particles.Count <= 0 || ActualWidth <= 0 || ActualHeight <= 0)
                return;

            //QuadTree = new QuadTree(new Rect(0, 0, ActualWidth, ActualHeight));

            foreach (var particle in particles)
            {
                particle.AdjustPosition(ActualWidth, ActualHeight);
                //QuadTree.Insert(particle);
            }

        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {

            //QuadTree = new QuadTree(new Rect(0, 0, Width, Height));

            InitializeParticles();

            drawingVisual = new DrawingVisual();

            visualHost.AddVisual(drawingVisual);

            CompositionTarget.Rendering += OnRendering;

        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {

            // 注销 CompositionTarget.Rendering 事件


            CompositionTarget.Rendering -= OnRendering;
            if (drawingVisual == null)
                return;
            visualHost.RemoveVisual(drawingVisual);
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseMove(e);
            // e.GetPosition(this);
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            mousePosition = Mouse.GetPosition(this);
            //UpdateParticles();
            DrawParticles();
        }

        #region drawing
        private void UpdateParticles()
        {
            if (particles == null) return;
            var distance = System.Windows.Application.Current.Dispatcher.Invoke(() => MaxMouseConnectionDistance);
            var Attraction = System.Windows.Application.Current.Dispatcher.Invoke(() => MouseAttractionDistance);
            Parallel.ForEach(particles, (o) =>
            {
                o.Update(ActualWidth, ActualHeight, mousePosition, distance, Attraction, UnLinkMousePosition);
                //QuadTree?.UpdateParticle(o);
            });
        }



        private void DrawParticles()
        {
            if (drawingVisual == null || this.Visibility != Visibility.Visible)
                return;

            using (var dc = drawingVisual.RenderOpen())
            {
                // 设置背景色
                dc.DrawRoundedRectangle(UseColorfulBackground ? backgroundBrush : new SolidColorBrush(Colors.Transparent),
                    null, new Rect(0, 0, ActualWidth, ActualHeight),CornorRadius,CornorRadius);

                //dc.DrawEllipse(new SolidColorBrush(Colors.Red),null,mousePosition,5,5);
                if (particles != null)
                {

                    var ConnectionDistance = MaxConnectionDistance;
                    var MouseDistance = MaxMouseConnectionDistance;
                    var PointConnectionDistance = MaxPointConnection;
                    bool UseColorPointLine = UseColorfulPointLines;
                    var CurLineColor = ((SolidColorBrush)LineColor).Color;

                    var quadTree= new QuadTree(new Rect(0,0,ActualWidth,ActualHeight));
                    foreach(var particle in particles)
                    {
                        quadTree.Insert(particle);
                    }

                    foreach (var particle in particles)
                    {
                        var searchArea = new Rect(particle.Position().X - ConnectionDistance, particle.Position().Y - ConnectionDistance,
                                                 2 * ConnectionDistance, 2 * ConnectionDistance);



                        var neighboringParticles = quadTree?.QueryRange(searchArea);

                        var connectionsMade = 0;
                        if (neighboringParticles != null)
                        {
                            foreach (var otherParticle in neighboringParticles)
                            {
                                if (otherParticle == particle || connectionsMade >= PointConnectionDistance)
                                    continue;
                                if(LinePainter.ConnectParticles(dc,particle,otherParticle,ColorGenrator,ConnectionDistance,UseColorPointLine,CurLineColor))
                                {
                                    connectionsMade++;
                                }
                            }
                        }


                        if (!UnLinkMousePosition)
                        {
                            LinePainter.ConnectToMouse(dc, mousePosition,particle,ColorGenrator, MouseDistance, UseColorPointLine, CurLineColor);
                        }

                        var curBrush = UseColorPointLine ? new SolidColorBrush(particle.PointColor()) : PointColor;
                        particle.Draw(dc, curBrush);
                    };

                }
            }
        }

        private void ConnectToMouse(DrawingContext dc, IParticle particle, double maxDistance,
            bool useColorfulPoint, Color lineColor)
        {
            double distance = Particle.GetDistance(particle.Position(), mousePosition);
            if (distance < maxDistance)
            {
                double opacity = (maxDistance - distance) / maxDistance;
                Color curlineColor = useColorfulPoint ? ColorGenrator.GetLineColor(particle.PointColor(), lineColor, opacity) :
                    lineColor;

                dc.DrawLine(new Pen(new SolidColorBrush(curlineColor) { Opacity = opacity }, 0.5), particle.Position(), mousePosition);

            }
        }

        #endregion

    }

    public class Particle : IParticle
    {
        private static readonly Random random = new Random();

        public Point Position;
        private Vector velocity;
        private Vector acceleration; // Acceleration vector
        private Vector initialVelocity; // Store the initial velocity
        public Color pointColor;
        private const double MaxVelocity = 5;
        private const double MaxAcceleration = 0.5;

        public double Radius { get; private set; } = 5.0;

        public Particle(double width, double height, Color pointColor)
        {
            Position = new Point(random.NextDouble() * width, random.NextDouble() * height);

            // Initial random direction and speed
            velocity = new Vector(random.NextDouble() * 2 - 1, random.NextDouble() * 2 - 1);
            velocity.Normalize();
            velocity *= random.NextDouble() * 2 + 0.5; // Random speed

            initialVelocity = velocity; // Store the initial velocity

            this.pointColor = pointColor;

            acceleration = new Vector(0, 0); // Initialize acceleration to 0
        }

        private Vector Reflect(Vector velocity, Vector normal)
        {
            double dotProduct = Vector.Multiply(velocity, normal); // Calculate V · N
            return velocity - 2 * dotProduct * normal;
        }

        public void Update(double width, double height, Point mousePosition, double MaxMouseConnectionDistance,
            double MouseAttractionDistance, bool ShouldIgnoreMouse)
        {
            if (!ShouldIgnoreMouse)
            {
                Vector distanceVector = mousePosition - Position;
                double distanceToMouse = distanceVector.Length;
                double MaxAttraction = MaxMouseConnectionDistance * 1.5;
                double tangentialSpeed = 0.04;

                if (distanceToMouse < MaxMouseConnectionDistance)
                {
                    distanceVector.Normalize();
                    velocity += -distanceVector * tangentialSpeed;  // Adjust particle to move away from the mouse
                }
                else if (distanceToMouse - MaxMouseConnectionDistance <= MouseAttractionDistance)
                {
                    Vector radialVector = distanceVector;
                    radialVector.Normalize();

                    Vector tangentialVector = new Vector(-radialVector.Y, radialVector.X);
                    tangentialSpeed = velocity.Length;

                    velocity = tangentialVector * tangentialSpeed;

                    double attractionForce = 1; // Attraction coefficient
                    velocity += radialVector * attractionForce;
                }
                else if (distanceToMouse > MaxMouseConnectionDistance && distanceToMouse <= MaxAttraction)
                {
                    distanceVector.Normalize();
                    velocity += distanceVector * tangentialSpeed;  // Adjust particle to move towards the mouse
                }
            }

            // Gradually return velocity to initial velocity
            double velocityReturnRate = 0.001; // Adjust this rate as needed
            velocity += (initialVelocity - velocity) * velocityReturnRate;

            // limit acceleration
            if (acceleration.Length > MaxAcceleration)
            {
                acceleration.Normalize();
                acceleration *= MaxAcceleration;
            }

            // Apply acceleration to velocity
            velocity += acceleration;

            if (velocity.X > MaxVelocity)
            {
                velocity.X = MaxVelocity;
            }
            if (velocity.Y > MaxVelocity)
            {
                velocity.Y = MaxVelocity;
            }

            // Update position
            Position += velocity;

            if (Position.X <= 0 || Position.X >= width)
            {
                velocity.X = -velocity.X * 1.1;  // 反转并稍微增加速度
                acceleration.X = 0;
                Position.X = Position.X <= 0 ? 1 : width - 1;  // 确保粒子在边界内
            }
            if (Position.Y <= 0 || Position.Y >= height)
            {
                velocity.Y = -velocity.Y * 1.1;  // 反转并稍微增加速度
                acceleration.Y = 0;
                Position.Y = Position.Y <= 0 ? 1 : height - 1;  // 确保粒子在边界内
            }
            Position = new Point(
                Math.Clamp(Position.X, 0, width),
                Math.Clamp(Position.Y, 0, height)
            );

            // Dampen the acceleration over time
            acceleration *= 0.9;
        }

        public void Draw(DrawingContext dc, Brush pointColor)
        {
            dc.DrawEllipse(pointColor, null, Position, 2, 2);
        }

        public void ResetColor(Color curColor)
        {
            pointColor = curColor;
        }

        public void AdjustPosition(double width, double height)
        {
            Position = new Point(
                Math.Clamp(Position.X, 0, width),
                Math.Clamp(Position.Y, 0, height)
            );
        }

        public double DistanceTo(Point other)
        {
            return (Position - other).Length;
        }

        public double DistanceTo(Particle other)
        {
            return (Position - other.Position).Length;
        }

        public static double GetDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

        public Color PointColor()
        {
            return pointColor;
        }

        Point IParticle.Position()
        {
            return Position;
        }
    }

    public class VisualHost : FrameworkElement
    {
        private readonly VisualCollection _children;

        public VisualHost()
        {
            _children = new VisualCollection(this);
        }

        protected override int VisualChildrenCount => _children.Count;

        protected override Visual GetVisualChild(int index)
        {
            return _children[index];
        }

        public void AddVisual(Visual visual)
        {
            _children.Add(visual);
        }

        public void RemoveVisual(Visual visual)
        {
            _children.Remove(visual);
        }

        public void ClearVisuals()
        {
            _children.Clear();
        }
    }

    public class QuadTree
    {
        private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        private const int MaxParticlesPerNode = 4;
        private const int MaxDepth = 5;

        private readonly Rect bounds;
        private List<IParticle> particles;
        private QuadTree[]? nodes;

        public QuadTree(Rect bounds)
        {
            this.bounds = bounds;
            this.particles = new List<IParticle>();
            this.nodes = null;
        }

        public void Insert(IParticle particle)
        {
            rwLock.EnterWriteLock();
            try
            {
                if (!bounds.Contains(particle.Position()))
                    return;

                if (particles.Count < MaxParticlesPerNode || MaxDepth == 0)
                {
                    particles.Add(particle);
                }
                else
                {
                    if (nodes == null)
                    {
                        Subdivide();
                    }
                    if (nodes != null)
                    {
                        foreach (var node in nodes)
                        {
                            node.Insert(particle);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                rwLock.ExitWriteLock();
            }

        }

        public void Remove(IParticle particle)
        {
            rwLock.EnterWriteLock();
            try
            {
                if (!particles.Remove(particle) && nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        node?.Remove(particle);
                    }
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public void UpdateParticle(IParticle particle)
        {
            Remove(particle);
            Insert(particle);
        }

        public List<IParticle>? QueryRange(Rect range)
        {
            rwLock.EnterReadLock();
            try
            {
                var foundParticles = new List<IParticle>();

                if (!bounds.IntersectsWith(range))
                    return foundParticles;

                foreach (var particle in particles)
                {
                    if (range.Contains(particle.Position()))
                    {
                        foundParticles.Add(particle);
                    }
                }

                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        foundParticles.AddRange(node.QueryRange(range));
                    }
                }

                return foundParticles;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return null;
            }
            finally
            {
                rwLock.ExitReadLock();
            }
            
        }

        private void Subdivide()
        {
            double halfWidth = bounds.Width / 2.0;
            double halfHeight = bounds.Height / 2.0;
            double x = bounds.X;
            double y = bounds.Y;

            nodes = new QuadTree[4];
            nodes[0] = new QuadTree(new Rect(x, y, halfWidth, halfHeight)); // Top Left
            nodes[1] = new QuadTree(new Rect(x + halfWidth, y, halfWidth, halfHeight)); // Top Right
            nodes[2] = new QuadTree(new Rect(x, y + halfHeight, halfWidth, halfHeight)); // Bottom Left
            nodes[3] = new QuadTree(new Rect(x + halfWidth, y + halfHeight, halfWidth, halfHeight)); // Bottom Right
        }
    }

    public class ColorHelper : IColorGenerator
    {
        #region light preset

        public enum LightBackgroundPreset
        {
            MintSunset = 0,        // (198, 255, 221) -> (251, 215, 134) -> (247, 121, 125)
            SpringMeadow,      // (168, 255, 120) -> (120, 255, 214)
            ElectricDream,     // (239, 50, 217) -> (137, 255, 253)
            SkySunshine,       // (0, 195, 255) -> (255, 255, 28)
            TropicalBreeze     // (170, 255, 169) -> (17, 255, 189)
        }

        public enum LightParticlePreset
        {
            OceanBlue = 0,         // (0, 100, 200)
            ForestGreen,       // (0, 150, 100)
            RoyalPurple,       // (100, 0, 200)
            SunsetOrange,      // (255, 165, 0)
            DeepSea,           // (0, 50, 150)
            TealBlue,          // (0, 100, 150)
            OrchidPurple,      // (150, 0, 150)
            Amber              // (255, 140, 0)
        }

        public static readonly Dictionary<LightBackgroundPreset, List<Color>> LightBackgroundColorDictionary =
            new Dictionary<LightBackgroundPreset, List<Color>>()
    {
        { LightBackgroundPreset.MintSunset, new List<Color>(){ Color.FromRgb(198, 255, 221), Color.FromRgb(251, 215, 134), Color.FromRgb(247, 121, 125) } },
        { LightBackgroundPreset.SpringMeadow, new List<Color>(){ Color.FromRgb(168, 255, 120), Color.FromRgb(120, 255, 214) } },
        { LightBackgroundPreset.ElectricDream, new List<Color>(){ Color.FromRgb(239, 50, 217), Color.FromRgb(137, 255, 253) } },
        { LightBackgroundPreset.SkySunshine, new List<Color>(){ Color.FromRgb(0, 195, 255), Color.FromRgb(255, 255, 28) } },
        { LightBackgroundPreset.TropicalBreeze, new List<Color>(){ Color.FromRgb(170, 255, 169), Color.FromRgb(17, 255, 189) } }
    };


        public static readonly Dictionary<LightParticlePreset, Color> LightParticleColorDictionary =
            new Dictionary<LightParticlePreset, Color>()
            {
        { LightParticlePreset.OceanBlue, Color.FromRgb(0, 100, 200) },
        { LightParticlePreset.ForestGreen, Color.FromRgb(0, 150, 100) },
        { LightParticlePreset.RoyalPurple, Color.FromRgb(100, 0, 200) },
        { LightParticlePreset.SunsetOrange, Color.FromRgb(255, 165, 0) },
        { LightParticlePreset.DeepSea, Color.FromRgb(0, 50, 150) },
        { LightParticlePreset.TealBlue, Color.FromRgb(0, 100, 150) },
        { LightParticlePreset.OrchidPurple, Color.FromRgb(150, 0, 150) },
        { LightParticlePreset.Amber, Color.FromRgb(255, 140, 0) }
            };
        #endregion
        #region dark preset

        public enum DarkBackgroundPreset
        {
            SlateGrey = 0,         // (35, 37, 38) -> (65, 67, 69)
            Charcoal,          // (0, 0, 0) -> (67, 67, 67)
            Twilight,          // (58, 97, 134) -> (137, 37, 62)
            DustyRose,         // (53, 92, 125) -> (108, 91, 123) -> (192, 108, 132)
            MidnightOcean      // (15, 32, 39) -> (32, 58, 67) -> (44, 83, 100)
        }

        public enum DarkParticlePreset
        {
            Ivory = 0,             // (255, 239, 213)
            Peach,             // (255, 218, 185)
            Pink,              // (255, 192, 203)
            Plum,              // (221, 160, 221)
            LemonChiffon,      // (255, 250, 205)
            Khaki,             // (240, 230, 140)
            LightPink,         // (255, 182, 193)
            Violet             // (238, 130, 238)
        }

        public static readonly Dictionary<DarkBackgroundPreset, List<Color>> DarkBackgroundColorDictionary =
            new Dictionary<DarkBackgroundPreset, List<Color>>()
            {
        { DarkBackgroundPreset.SlateGrey, new List<Color>(){ Color.FromRgb(35, 37, 38), Color.FromRgb(65, 67, 69) } },
        { DarkBackgroundPreset.Charcoal, new List<Color>(){ Color.FromRgb(0, 0, 0), Color.FromRgb(67, 67, 67) } },
        { DarkBackgroundPreset.Twilight, new List<Color>(){ Color.FromRgb(58, 97, 134), Color.FromRgb(137, 37, 62) } },
        { DarkBackgroundPreset.DustyRose, new List<Color>(){ Color.FromRgb(53, 92, 125), Color.FromRgb(108, 91, 123), Color.FromRgb(192, 108, 132) } },
        { DarkBackgroundPreset.MidnightOcean, new List<Color>(){ Color.FromRgb(15, 32, 39), Color.FromRgb(32, 58, 67), Color.FromRgb(44, 83, 100) } }
            };

        public static readonly Dictionary<DarkParticlePreset, Color> DarkParticleColorDictionary =
    new Dictionary<DarkParticlePreset, Color>()
    {
        { DarkParticlePreset.Ivory, Color.FromRgb(255, 239, 213) },
        { DarkParticlePreset.Peach, Color.FromRgb(255, 218, 185) },
        { DarkParticlePreset.Pink, Color.FromRgb(255, 192, 203) },
        { DarkParticlePreset.Plum, Color.FromRgb(221, 160, 221) },
        { DarkParticlePreset.LemonChiffon, Color.FromRgb(255, 250, 205) },
        { DarkParticlePreset.Khaki, Color.FromRgb(240, 230, 140) },
        { DarkParticlePreset.LightPink, Color.FromRgb(255, 182, 193) },
        { DarkParticlePreset.Violet, Color.FromRgb(238, 130, 238) }
    };
        #endregion
        /// <summary>
        /// 获取预设的渐变背景
        /// </summary>
        /// <returns></returns>
        public List<Color> GetRandomBackgroundPreset(SpecificThemeMode themeMode = SpecificThemeMode.Auto)
        {

            Random rand = new Random();
            List<List<Color>>? colorSchemes = null;
            switch (themeMode)
            {
                case SpecificThemeMode.Light:
                    {
                        colorSchemes = LightBackgroundColorDictionary.Values.ToList();
                        break;
                    }
                case SpecificThemeMode.Dark:
                    {
                        colorSchemes = DarkBackgroundColorDictionary.Values.ToList();
                        break;
                    }
                default:
                    {
                        int hour = DateTime.Now.Hour;

                        colorSchemes = hour >= 9 && hour < 17 ? LightBackgroundColorDictionary.Values.ToList() :
                            DarkBackgroundColorDictionary.Values.ToList();
                        break;
                    }
            }
            return colorSchemes[rand.Next(colorSchemes.Count)];
        }
        /// <summary>
        /// 获取渐变画刷
        /// </summary>
        /// <param name="colorSets"></param>
        /// <returns></returns>
        public System.Windows.Media.Brush GetBackgroundGradientBrush(List<Color> colorSets, double opacity = 1)
        {


            var backgroundBrush = new System.Windows.Media.LinearGradientBrush();
            backgroundBrush.MappingMode = BrushMappingMode.RelativeToBoundingBox;
            backgroundBrush.SpreadMethod = GradientSpreadMethod.Repeat;


            for (int i = 0; i < colorSets.Count; i++)
            {
                var curColor = colorSets[i];
                backgroundBrush.GradientStops.Add(new GradientStop(curColor, i / (double)(colorSets.Count)));

            }
            for (int i = 0; i < colorSets.Count; i++)
            {
                var curColor = colorSets[i];
                var nextColor = colorSets[(i + 1) % colorSets.Count];
                ColorAnimation animation = new ColorAnimation
                {
                    From = curColor,
                    To = nextColor,
                    Duration = TimeSpan.FromSeconds(10),
                    RepeatBehavior = RepeatBehavior.Forever,
                    AutoReverse = true
                };
                animation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };
                backgroundBrush.GradientStops[i].BeginAnimation(GradientStop.ColorProperty, animation);
            }
            backgroundBrush.Opacity = opacity;
            return backgroundBrush;
        }
        /// <summary>
        /// 获取预设的粒子色
        /// </summary>
        /// <returns></returns>
        public Color GetRandomPaticlePointPreset(SpecificThemeMode themeMode = SpecificThemeMode.Auto)
        {
            Random rand = new Random();

            List<Color> colorSchemes;
            switch (themeMode)
            {
                case SpecificThemeMode.Light:
                    {
                        colorSchemes = LightParticleColorDictionary.Values.ToList();
                        break;
                    }
                case SpecificThemeMode.Dark:
                    {
                        colorSchemes = DarkParticleColorDictionary.Values.ToList();
                        break;
                    }
                default:
                    {
                        int hour = DateTime.Now.Hour;

                        colorSchemes = hour >= 9 && hour < 17 ? LightParticleColorDictionary.Values.ToList() :
                            DarkParticleColorDictionary.Values.ToList();
                        break;
                    }
            }
            return colorSchemes[rand.Next(colorSchemes.Count)];
        }
        /// <summary>
        /// 获取两点间线色
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <param name="opacity"></param>
        /// <returns></returns>
        public Color GetLineColor(Color color1, Color color2, double opacity)
        {
            var r = (byte)((color1.R + color2.R) / 2);
            var g = (byte)((color1.G + color2.G) / 2);
            var b = (byte)((color1.B + color2.B) / 2);
            return Color.FromArgb((byte)(opacity * 255), r, g, b);
        }
    }

    public class LinePainter : ILinePainter
    {
        public bool ConnectParticles(DrawingContext dc, IParticle particle1, IParticle particle2, IColorGenerator colorGenerator,double maxDistance, bool useColorfulPoint, Color defaultLineColor)
        {
            double distance = Particle.GetDistance(particle1.Position(), particle2.Position());
            if (distance < maxDistance)
            {
                double opacity = (maxDistance - distance) / maxDistance;
                Color curlineColor = useColorfulPoint ? colorGenerator.GetLineColor(particle1.PointColor(), particle2.PointColor(), opacity) : defaultLineColor;

                dc.DrawLine(new Pen(new SolidColorBrush(curlineColor) { Opacity = opacity }, 0.5), particle1.Position(), particle2.Position());

                return true;
            }
            return false;
        }

        public void ConnectToMouse(DrawingContext dc, Point mousePosition, IParticle particle, IColorGenerator colorGenerator, double maxDistance,bool useColorfulPoint, Color defaultLineColor)
        {
            double distance = Particle.GetDistance(particle.Position(), mousePosition);
            if (distance < maxDistance)
            {
                double opacity = (maxDistance - distance) / maxDistance;
                Color curlineColor = useColorfulPoint ? colorGenerator.GetLineColor(particle.PointColor(), defaultLineColor, opacity) :
                    defaultLineColor;

                dc.DrawLine(new Pen(new SolidColorBrush(curlineColor) { Opacity = opacity }, 0.5), particle.Position(), mousePosition);

            }
        }

    }
}