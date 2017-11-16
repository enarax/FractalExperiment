using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nito.AsyncEx;
using Timer = System.Windows.Forms.Timer;

namespace IFS
{
    public partial class Form1 : Form
    {
        private readonly Pen _pen = new Pen(Color.FromArgb(50, Color.Green));

        private readonly AsyncProducerConsumerQueue<PointF> _pointsToDraw = new AsyncProducerConsumerQueue<PointF>(100);

        private readonly Task _producerTask;
        private readonly Task _drawTask;

        private const int BITMAP_WIDTH = 1000;
        private const int BITMAP_HEIGTH = 2000;
        private Bitmap _bitmap = new Bitmap(BITMAP_WIDTH, BITMAP_HEIGTH);

        public Form1()
        {
            InitializeComponent();
            _producerTask = CreatePoints();
            _drawTask = DrawFractal();
            Timer t = new Timer();
            t.Interval = 50;
            t.Tick += (s,e) => DrawBitmap();
            t.Start();

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawBitmap();
        }

        private void DrawBitmap()
        {
            using( Graphics g = CreateGraphics())
                g.DrawImage(_bitmap, new PointF());
        }

        private async Task DrawFractal()
        {
            int count = 0;
            while (true)
            {
                using (Graphics g = Graphics.FromImage(_bitmap))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    PointF pont = (await _pointsToDraw.TryDequeueAsync()).Item;
                    const float radius = 0.3f;
                    g.DrawEllipse(_pen, pont.X * 80 + 300, pont.Y * 80, radius * 2, radius * 2);
                    if (count++ % 200 == 0)
                    {
                        await Task.Delay(5);
                    }
                }
            }
        }

        private async Task CreatePoints()
        {

            double x = 0;
            double y = 0;
            Random r = new Random();
            while(true)
            {
                double random = r.NextDouble();
                double a, b, c, d, e, f;
                if (random <= 0.01)
                {
                    a = 0;
                    b = 0;
                    c = 0;
                    d = 0.16;
                    e = 0;
                    f = 0;
                }
                else if (random <= 0.86)
                {
                    a = 0.85;
                    b = 0.04;
                    c = -0.04;
                    d = 0.85;
                    e = 0;
                    f = 1.6;
                }
                else if (random <= 0.93)
                {
                    a = 0.2;
                    b = -0.26;
                    c = 0.23;
                    d = 0.22;
                    e = 0;
                    f = 1.6;
                }
                else
                {
                    a = -0.15;
                    b = 0.28;
                    c = 0.26;
                    d = 0.24;
                    e = 0;
                    f = 0.44;
                }

                double oldX = x;
                double oldY = y;
                x = a * oldX + b * oldY + e;
                y = c * oldX + d * oldY + f;

                await _pointsToDraw.TryEnqueueAsync(new PointF((float)x, (float)y));
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _bitmap = new Bitmap(BITMAP_WIDTH, BITMAP_HEIGTH);
        }
    }
}