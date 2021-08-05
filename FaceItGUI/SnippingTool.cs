// https://stackoverflow.com/questions/3123776/net-equivalent-of-snipping-tool/3124252#3124252


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

namespace FaceItGUI
{
    public partial class SnippingTool : Form
    {

        public static SnippingTool Snip()
        {
            var rc = Screen.PrimaryScreen.Bounds;
            using (Bitmap bmp = new Bitmap(rc.Width, rc.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb))
            {
                using (Graphics gr = Graphics.FromImage(bmp))
                    gr.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                using (var snipper = new SnippingTool(bmp))
                {
                    if (snipper.ShowDialog() == DialogResult.OK)
                    {
                        return snipper;
                    }
                }
                return null;
            }
        }

        public SnippingTool(Image screenShot)
        {
            InitializeComponent();
            this.BackgroundImage = screenShot;
            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.DoubleBuffered = true;
        }
        public SnippingTool Clone()
        {
            var rc = Screen.PrimaryScreen.Bounds;
            Rectangle recc = new Rectangle(Frame.X, Frame.Y, Frame.Width, Frame.Height);
            Bitmap bmp = new Bitmap(rc.Width, rc.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            SnippingTool tool = new SnippingTool(bmp);
            tool.FirstImage = (Image)this.FirstImage.Clone();
            tool.Frame = new Rectangle(this.Frame.X, this.Frame.Y, this.Frame.Width, this.Frame.Height);


            return tool;
        }
        public Image FirstImage { get; set; }

        private Point pntStart;
        public string TheName { get; set; }

        public Rectangle Frame { get; set; }


        protected override void OnMouseDown(MouseEventArgs e)
        {
            // Start the snip on mouse down
            if (e.Button != MouseButtons.Left) return;
            pntStart = e.Location;
            Frame = new Rectangle(e.Location, new Size(0, 0));
            this.Invalidate();
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            // Modify the selection on mouse move
            if (e.Button != MouseButtons.Left) return;
            int x1 = Math.Min(e.X, pntStart.X);
            int y1 = Math.Min(e.Y, pntStart.Y);
            int x2 = Math.Max(e.X, pntStart.X);
            int y2 = Math.Max(e.Y, pntStart.Y);
            Frame = new Rectangle(x1, y1, x2 - x1, y2 - y1);
            this.Invalidate();
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            FirstImage = GetImage();
        }
        public Image GetImage()
        {
            // Complete the snip on mouse-up
            if (Frame == null)
            {
                return null;
            }
            if (Frame.Width <= 0 || Frame.Height <= 0) return null;
            System.Diagnostics.Debug.WriteLine("here 1");

            Image image = new Bitmap(Frame.Width, Frame.Height);
            System.Diagnostics.Debug.WriteLine("here 2");

            using (Graphics gr = Graphics.FromImage(image))
            {
                System.Diagnostics.Debug.WriteLine("here 3");

                gr.DrawImage(this.BackgroundImage, new Rectangle(0, 0, image.Width, image.Height),
                    Frame, GraphicsUnit.Pixel);
                System.Diagnostics.Debug.WriteLine("here 4");

            }
            DialogResult = DialogResult.OK;
            System.Diagnostics.Debug.WriteLine("here 5");

            return image;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            // Draw the current selection
            using (Brush br = new SolidBrush(Color.FromArgb(120, Color.White)))
            {
                int x1 = Frame.X; int x2 = Frame.X + Frame.Width;
                int y1 = Frame.Y; int y2 = Frame.Y + Frame.Height;
                e.Graphics.FillRectangle(br, new Rectangle(0, 0, x1, this.Height));
                e.Graphics.FillRectangle(br, new Rectangle(x2, 0, this.Width - x2, this.Height));
                e.Graphics.FillRectangle(br, new Rectangle(x1, 0, x2 - x1, y1));
                e.Graphics.FillRectangle(br, new Rectangle(x1, y2, x2 - x1, this.Height - y2));
            }
            using (Pen pen = new Pen(Color.Red, 3))
            {
                e.Graphics.DrawRectangle(pen, Frame);
            }
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Allow canceling the snip with the Escape key
            if (keyData == Keys.Escape) this.DialogResult = DialogResult.Cancel;
            return base.ProcessCmdKey(ref msg, keyData);
        }
        public static Image FromRectangle(Rectangle rectangle)
        {
            Bitmap bmp = new Bitmap(rectangle.Width, rectangle.Height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(rectangle.Left, rectangle.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
            g.Flush();
            g.Dispose();
            return bmp;
        }
    }
}
