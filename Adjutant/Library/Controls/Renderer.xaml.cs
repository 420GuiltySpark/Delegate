using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace Adjutant.Library.Controls
{
    /// <summary>
    /// Interaction logic for Renderer.xaml
    /// </summary>
    public partial class Renderer : System.Windows.Controls.UserControl
    {
        #region Init
        private const double RAD_089 = 1.5706217940;
        private const double RAD_090 = 1.5707963268;
        private const double RAD_360 = 6.2831853072;

        private bool mouseDown;
        private DispatcherTimer timer = new DispatcherTimer();
        private double forwardEx;
        private Point lastPos = new Point();
        private double yaw;
        private double pitch;
        private double backEx;
        private double leftEx;
        private double rightEx;
        private double upEx;
        private double downEx;
        private double aSpeed = 0.1;
        private double bSpeed = 0.1;


        public double CameraSpeed = 0.015;
        public Viewport3D Viewport
        {
            get { return this.viewport; }
        }
        public double MaxCameraSpeed = 1.5;
        public Point3D MaxPosition = new Point3D(500, 500, 500);
        public Point3D MinPosition = new Point3D(-500, -500, -500);
        public double FarPlane { get { return ((PerspectiveCamera)viewport.Camera).FarPlaneDistance; } set { ((PerspectiveCamera)viewport.Camera).FarPlaneDistance = value; } }
        public double FarPlaneMin = 200;
        public double FarPlaneMax = 5000;
        public bool Running
        {
            get { return timer.IsEnabled; }
        }

        public Renderer()
        {
            InitializeComponent();
        }
        #endregion

        #region Event Handlers
        private void Renderer_Loaded(object sender, RoutedEventArgs e)
        {
            ClearViewport();
            NormalizeSet();
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(dispatcherTimer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            var CursorPos = new System.Drawing.Point();
            GetCursorPos(out CursorPos);
            var point2 = new System.Windows.Point(CursorPos.X, CursorPos.Y);

            if (base.IsFocused)
                UpdateCameraPosition();

            UpdateCameraDirection(point2);
        }

        private void Renderer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            base.Focus();
            if (e.LeftButton == MouseButtonState.Pressed)
                mouseDown = true;
        }

        private void Renderer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mouseDown = false;
        }

        private void Renderer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //cameraSpeed += (double)e.Delta / 120000.0;

            if (e.Delta > 0)
                CameraSpeed = Math.Ceiling(CameraSpeed * 1050) / 1000;
            else
                CameraSpeed = Math.Floor(CameraSpeed * 0950) / 1000;

            if (CameraSpeed < 0.001)
                CameraSpeed = 0.001;

            if (CameraSpeed > MaxCameraSpeed)
                CameraSpeed = MaxCameraSpeed;
        }
        #endregion

        #region Methods
        public void Start()
        {
            timer.Start();
        }

        public void Stop(string Message)
        {
            timer.Stop();
            ClearViewport();
            lblStats.Content = Message;
            Refresh();
        }

        public void ClearViewport()
        {
            for (int i = 0; i < viewport.Children.Count; i++)
            {
                ModelVisual3D visuald = (ModelVisual3D)viewport.Children[i];
                if (!(visuald.Content is Light))
                    viewport.Children.Remove(visuald);

                if (visuald.Content is AmbientLight)
                {
                    System.Windows.Media.Color color = new System.Windows.Media.Color
                    {
                        A = 0xff,
                        R = 100,
                        G = 100,
                        B = 100
                    };
                    ((AmbientLight)visuald.Content).Color = color;
                }
            }
        }

        public void Refresh()
        {
            Dispatcher.Invoke(DispatcherPriority.Render, (Action)delegate { });
        }

        private void NormalizeCamera(PerspectiveCamera Camera)
        {
            double len = Camera.LookDirection.Length;
            Camera.LookDirection = new Vector3D(Camera.LookDirection.X / len, Camera.LookDirection.Y / len, Camera.LookDirection.Z / len);
        }

        public void NormalizeSet()
        {
            PerspectiveCamera camera = (PerspectiveCamera)viewport.Camera;
            NormalizeCamera(camera);
            yaw = Math.Atan2(camera.LookDirection.X, camera.LookDirection.Z);
            pitch = Math.Atan(camera.LookDirection.Y);
        }

        public void MoveCamera(Point3D Position, Vector3D Direction)
        {
            lastPos = new Point();
            PerspectiveCamera camera = (PerspectiveCamera)viewport.Camera;
            camera.Position = Position;
            camera.LookDirection = Direction;
            NormalizeSet();
        }

        private void SetDebugInfo(PerspectiveCamera Camera)
        {
            lblStats.Content = string.Format(
                "{0,6:0.00}\x00b0, {1,6:0.00}\x00b0 X:{2:0.00} Y:{3:0.00} Z:{4:0.00} FOV: {5:0}, FPD: {6:0}, Speed: {7:0}",
                (360.0 * yaw) / RAD_360,
                (360.0 * pitch) / RAD_360,
                Camera.Position.X,
                Camera.Position.Y,
                Camera.Position.Z,
                Camera.FieldOfView,
                Camera.FarPlaneDistance,
                CameraSpeed * 1000);
        }

        private void UpdateCameraPosition()
        {
            PerspectiveCamera camera = (PerspectiveCamera)viewport.Camera;
            NormalizeCamera(camera);
            //lblHelp.Visibility = CheckKeyState(Keys.F1) ? Visibility.Visible : Visibility.Hidden;

            #region Set FOV
            if (CheckKeyState(Keys.NumPad6)) camera.FieldOfView += camera.FieldOfView / 100.0;
            if (CheckKeyState(Keys.NumPad4)) camera.FieldOfView -= camera.FieldOfView / 100.0;
            camera.FieldOfView = ClipValue(camera.FieldOfView, 40, 120);
            #endregion

            #region Set FPD
            if (CheckKeyState(Keys.NumPad8)) camera.FarPlaneDistance *= 1.01;
            if (CheckKeyState(Keys.NumPad2)) camera.FarPlaneDistance *= 0.99;
            camera.FarPlaneDistance = ClipValue(camera.FarPlaneDistance, FarPlaneMin, FarPlaneMax);
            #endregion

            #region Check WASD
            if (!CheckKeyState(Keys.W))
            {
                if (forwardEx > 0.0)
                    forwardEx -= CameraSpeed * bSpeed;
                else
                    forwardEx = 0.0;
            }
            else if (forwardEx < CameraSpeed)
                forwardEx += CameraSpeed * aSpeed;
            else
                forwardEx = CameraSpeed;

            if (forwardEx != 0.0)
                camera.Position = new Point3D(camera.Position.X + (camera.LookDirection.X * forwardEx), camera.Position.Y + (camera.LookDirection.Y * forwardEx), camera.Position.Z + (camera.LookDirection.Z * forwardEx));

            if (!CheckKeyState(Keys.A))
            {
                if (leftEx > 0.0)
                    leftEx -= CameraSpeed * bSpeed;
                else
                    leftEx = 0.0;
            }
            else if (leftEx < CameraSpeed)
                leftEx += CameraSpeed * aSpeed;
            else
                leftEx = CameraSpeed;

            if (leftEx != 0.0)
                camera.Position = new Point3D(camera.Position.X - (Math.Sin(yaw + RAD_090) * leftEx), camera.Position.Y - (Math.Cos(yaw + RAD_090) * leftEx), camera.Position.Z);

            if (!CheckKeyState(Keys.S))
            {
                if (backEx > 0.0)
                    backEx -= CameraSpeed * bSpeed;
                else
                    backEx = 0.0;
            }
            else if (backEx < CameraSpeed)
                backEx += CameraSpeed * aSpeed;
            else
                backEx = CameraSpeed;

            if (backEx != 0.0)
                camera.Position = new Point3D(camera.Position.X - (camera.LookDirection.X * backEx), camera.Position.Y - (camera.LookDirection.Y * backEx), camera.Position.Z - (camera.LookDirection.Z * backEx));

            if (!CheckKeyState(Keys.D))
            {
                if (rightEx > 0.0)
                    rightEx -= CameraSpeed * bSpeed;
                else
                    rightEx = 0.0;
            }
            else if (rightEx < CameraSpeed)
                rightEx += CameraSpeed * aSpeed;
            else
                rightEx = CameraSpeed;
            if (rightEx != 0.0)
                camera.Position = new Point3D(camera.Position.X + (Math.Sin(yaw + RAD_090) * rightEx), camera.Position.Y + (Math.Cos(yaw + RAD_090) * rightEx), camera.Position.Z);
            #endregion

            #region Check RF
            if (!CheckKeyState(Keys.R))
            {
                if (upEx > 0.0)
                    upEx -= CameraSpeed * bSpeed;
                else
                    upEx = 0.0;
            }
            else if (upEx < CameraSpeed)
                upEx += CameraSpeed * aSpeed;
            else
                upEx = CameraSpeed;
            if (upEx != 0.0)
            {
                Vector3D vectord = Vector3D.CrossProduct(camera.LookDirection, Vector3D.CrossProduct(camera.LookDirection, camera.UpDirection));
                vectord.Normalize();
                camera.Position = new Point3D(camera.Position.X - (vectord.X * upEx), camera.Position.Y - (vectord.Y * upEx), camera.Position.Z - (vectord.Z * upEx));
            }

            if (!CheckKeyState(Keys.F))
            {
                if (downEx > 0.0)
                    downEx -= CameraSpeed * bSpeed;
                else
                    downEx = 0.0;
            }
            else if (downEx < CameraSpeed)
                downEx += CameraSpeed * aSpeed;
            else
                downEx = CameraSpeed;

            if (downEx != 0.0)
            {
                Vector3D vectord2 = Vector3D.CrossProduct(camera.LookDirection, Vector3D.CrossProduct(camera.LookDirection, camera.UpDirection));
                vectord2.Normalize();
                camera.Position = new Point3D(camera.Position.X + (vectord2.X * downEx), camera.Position.Y + (vectord2.Y * downEx), camera.Position.Z + (vectord2.Z * downEx));
            }
            #endregion

            camera.Position = new Point3D(
                ClipValue(camera.Position.X, MinPosition.X, MaxPosition.X),
                ClipValue(camera.Position.Y, MinPosition.Y, MaxPosition.Y),
                ClipValue(camera.Position.Z, MinPosition.Z, MaxPosition.Z));

            SetDebugInfo(camera);
        }

        private void UpdateCameraDirection(System.Windows.Point point)
        {
            PerspectiveCamera camera = (PerspectiveCamera)viewport.Camera;
            Point locationFromWindow = base.TranslatePoint(new Point(0, 0), this);
            Point locationFromScreen = base.PointToScreen(locationFromWindow);
            if (mouseDown)
            {
                if (point.X < locationFromScreen.X + 1)
                    SetCursorPos((int)(locationFromScreen.X + base.ActualWidth - 2), (int)point.Y);
                else if (point.X > locationFromScreen.X + base.ActualWidth - 1)
                    SetCursorPos((int)(locationFromScreen.X + 2), (int)point.Y);

                if (point.Y < locationFromScreen.Y + 1)
                    SetCursorPos((int)point.X, (int)(locationFromScreen.Y + base.ActualHeight - 2));
                else if (point.Y > locationFromScreen.Y + base.ActualHeight - 1)
                    SetCursorPos((int)point.X, (int)(locationFromScreen.Y + 2));

                if (point.X < locationFromScreen.X + 1
                    || point.X > locationFromScreen.X + base.ActualWidth - 1
                    || point.Y < locationFromScreen.Y + 1
                    || point.Y > locationFromScreen.Y + base.ActualHeight - 1)
                {
                    System.Drawing.Point CursorPos = new System.Drawing.Point();
                    GetCursorPos(out CursorPos);
                    point = new System.Windows.Point(CursorPos.X, CursorPos.Y);
                    lastPos = point;
                }

                if (lastPos.X > point.X)
                    yaw -= (((lastPos.X - point.X) * RAD_360) / 54321.0) * camera.FieldOfView;
                else if (lastPos.X < point.X)
                    yaw += (((point.X - lastPos.X) * RAD_360) / 54321.0) * camera.FieldOfView;
                if (lastPos.Y > point.Y)
                    pitch += (((lastPos.Y - point.Y) * RAD_360) / 54321.0) * camera.FieldOfView;
                else if (lastPos.Y < point.Y)
                    pitch -= (((point.Y - lastPos.Y) * RAD_360) / 54321.0) * camera.FieldOfView;
            }

            yaw %= RAD_360;
            pitch = ClipValue(pitch, -RAD_089, RAD_089);

            SetDebugInfo(camera);
            camera.LookDirection = new Vector3D((camera.Position.X + Math.Sin(yaw)) - camera.Position.X, (camera.Position.Y + Math.Cos(yaw)) - camera.Position.Y, (camera.Position.Z + Math.Tan(pitch)) - camera.Position.Z);
            lastPos = point;
        }

        private double ClipValue(double val, double min, double max)
        {
            return Math.Min(Math.Max(min, val), max);
        }
        #endregion

        #region KeyState Shit
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetAsyncKeyState(int KeyID);
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out System.Drawing.Point Point);
        [DllImport("user32.dll")]
        private static extern int SetCursorPos(int X, int Y);

        private static bool CheckKeyState(Keys keys)
        {
            return ((GetAsyncKeyState((int)keys) & 32768) != 0);
        }
        #endregion
    }
}
