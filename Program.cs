using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MyProgram
{
    public class FlockingTestApp : Form
    {
        private Timer timer;
        private Swarm swarm;
        private Image iconRegular;
        private Image iconZombie;
        public int nBoundary = 700;
        public float fSpeed = 12f;

        [STAThread]
        private static void Main()
        {
            Application.Run(new FlockingTestApp());
        }

        public FlockingTestApp()
        {
            int boundary = nBoundary;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            Image imgFormBackground = new Bitmap("background.png", true);

            BackgroundImage = imgFormBackground;
            BackgroundImageLayout = ImageLayout.Stretch;

            iconRegular = CreateIcon(Brushes.Black, false);
            iconZombie = CreateIcon(Brushes.Red, true);
            swarm = new Swarm(boundary);
            timer = new Timer();
            timer.Tick += new EventHandler(this.timer_Tick);
            timer.Interval = 50;
            timer.Start();

        }

  

        protected override void OnMouseClick(MouseEventArgs e)
        {
            bool bZombie = e.Button == MouseButtons.Right;

            Point p = PointToClient(Cursor.Position);
            swarm.Boids.Add(new Boid(bZombie, p.X, p.Y, nBoundary));
            swarm.MoveBoids();

            FlockingTestApp.ActiveForm.Text = String.Format("{0}", swarm.Boids.Count);

            base.OnMouseClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            foreach (Boid boid in swarm.Boids)
            {
                float angle;
                if (boid.dX == 0) angle = 90f;
                else angle = (float)(Math.Atan(boid.dY / boid.dX) * /*57.3*/30.0);
                if (boid.dX < 0f) angle += 180f;
                Matrix matrix = new Matrix();
                matrix.RotateAt(angle, boid.Position);
                e.Graphics.Transform = matrix;

                if (boid.Zombie) e.Graphics.DrawImage(iconZombie, boid.Position);
                else e.Graphics.DrawImage(iconRegular, boid.Position);
            }
        }

        private static Image CreateIcon(Brush brush, bool bZombie)
        {
            Bitmap icon = null;
            Graphics g = null;
            if (bZombie)
            {
                icon = new Bitmap(48, 48);
                g = Graphics.FromImage(icon);

                //g.FillEllipse(brush, new Rectangle(0, 0, 16, 16));

                //g.FillEllipse(Brushes.White, new Rectangle(4, 10, 4, 4));
                //g.FillEllipse(Brushes.White, new Rectangle(10, 10, 4, 4));

                //Point p1 = new Point(4, 6);
                //Point p2 = new Point(6, 5);
                //Point p3 = new Point(8, 4);
                //Point p4 = new Point(10, 5);
                //Point p5 = new Point(12, 6);
                //Point[] pointsMouth = { p1, p2, p3, p4, p5 };

                //g.FillClosedCurve(Brushes.White, pointsMouth);

                Image image = Image.FromFile("z2.png");
                //g.DrawImage(image, new Point(0, 0));
                g.DrawImage(image, 0, 0, 48, 48);
            }
            else
            {
                icon = new Bitmap(24, 24);
                g = Graphics.FromImage(icon);

                
                Image image = Image.FromFile("b2.png");
                //g.DrawImage(image, new Point(0, 0));
                g.DrawImage(image, 0, 0, 24, 24);
            }

            return icon;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            swarm.MoveBoids();
            Invalidate();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // FlockingTestApp
            // 
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Name = "FlockingTestApp";
            this.ResumeLayout(false);

        }
    }
    public class Swarm
    {
        public List<Boid> Boids = new List<Boid>();

        public Swarm(int boundary)
        {
            for (int i = 0; i < 300; i++)
            {
                Boids.Add(new Boid((i > 295), boundary));
            }
        }

        public void MoveBoids()
        {
            foreach (Boid boid in Boids)
            {
                boid.Move(Boids);
            }
        }
    }

    public class Boid
    {
        private static Random rnd = new Random();
        private static float border = 100f;
        private static float sight = 75f;
        private static float space = 30f;
        public float speed = 12f;
        private float boundary;
        public float dX;
        public float dY;
        public bool Zombie;
        public PointF Position;

        public Boid(bool zombie, int boundary)
        {
            Position = new PointF(rnd.Next(boundary), rnd.Next(boundary));
            this.boundary = boundary;
            Zombie = zombie;
        }

        public Boid(bool zombie, int x, int y, int boundary)
        {
            Position = new PointF(x, y);
            this.boundary = boundary;
            Zombie = zombie;
        }

        public void Move(List<Boid> boids)
        {
            if (!Zombie) Flock(boids);
            else Hunt(boids);
            CheckBounds();
            CheckSpeed();
            Position.X += dX;
            Position.Y += dY;
        }

        private void Flock(List<Boid> boids)
        {
            foreach (Boid boid in boids)
            {
                float distance = Distance(Position, boid.Position);
                if (boid != this && !boid.Zombie)
                {
                    if (distance < space)
                    {
                        // Create space.
                        dX += Position.X - boid.Position.X;
                        dY += Position.Y - boid.Position.Y;
                    }
                    else if (distance < sight)
                    {
                        // Flock together.
                        dX += (boid.Position.X - Position.X) * 0.00001f;
                        dY += (boid.Position.Y - Position.Y) * 0.00001f;
                    }
                    if (distance < sight)
                    {
                        // Align movement.
                        dX += boid.dX * 0.7f;
                        dY += boid.dY * 0.7f;
                    }
                }
                if (boid.Zombie && distance < sight)
                {
                    // Avoid zombies.
                    dX += Position.X - boid.Position.X;
                    dY += Position.Y - boid.Position.Y;
                }
            }
        }

        private void Hunt(List<Boid> boids)
        {
            float range = float.MaxValue;
            Boid prey = null;
            foreach (Boid boid in boids)
            {
                if (!boid.Zombie)
                {
                    float distance = Distance(Position, boid.Position);
                    if (distance < sight && distance < range)
                    {
                        range = distance;
                        prey = boid;
                    }
                }
            }
            if (prey != null)
            {
                // Move towards closest prey.
                dX += prey.Position.X - Position.X;
                dY += prey.Position.Y - Position.Y;
            }
        }

        private static float Distance(PointF p1, PointF p2)
        {
            double val = Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2);
            return (float)Math.Sqrt(val);
        }

        private void CheckBounds()
        {
            float valX = Screen.PrimaryScreen.Bounds.Width - border;
            float valY = Screen.PrimaryScreen.Bounds.Height - border;
            if (Position.X < border) dX += border - Position.X;
            if (Position.Y < border) dY += border - Position.Y;
            if (Position.X > valX) dX += valX - Position.X;
            if (Position.Y > valY) dY += valY - Position.Y;
        }

        private void CheckSpeed()
        {
            float s;
            if (!Zombie) s = speed;
            else s = speed / 4f;
            float val = Distance(new PointF(0f, 0f), new PointF(dX, dY));
            if (val > s)
            {
                dX = dX * s / val;
                dY = dY * s / val;
            }
        }
    }
}