using System;
using System.Drawing;
using System.Windows.Forms;

namespace UltimateFishBot.Forms
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    using UltimateFishBot.Classes;
    using UltimateFishBot.Properties;

    public partial class frmOverlay : Form
    {
        private static frmOverlay inst;

        public Point ClickPoint;

        public ClickAction CurrentAction;

        public Point CurrentBottomRight;

        public Point CurrentTopLeft;

        public Point DragClickRelative;

        public bool LeftButtonDown;

        public int PrimMon;

        public bool ReadyToDrag;

        public bool RectangleDrawn;

        private Graphics g;
        private Pen MyPen = new Pen(Color.White, 1);
        private Pen EraserPen = new Pen(Color.FromArgb(0, 0, 0), 20);

        public int RectangleWidth;

        public int showMon;

        private Pen EraserPen = new Pen(Color.FromArgb(0, 0, 0), 20);

        private readonly Graphics g;

        private Pen MyPen = new Pen(Color.White, 1);

        private readonly frmSettings settings;

        public frmOverlay(frmSettings settings)
        {
            this.InitializeComponent();
            this.settings = settings;
            this.MouseDown += this.mouse_Click;
            this.MouseUp += this.mouse_Up;
            this.MouseMove += this.mouse_Move;
            this.KeyUp += this.key_press;
            this.DoubleClick += this.mouse_dClick;
            this.g = this.CreateGraphics();

            this.PrimMon = this.GetPrimaryMonIdx();
            this.showMon = this.PrimMon;
        }

        public enum ClickAction
        {
            NoClick = 0, 

            Dragging, 

            Outside, 

            TopSizing, 

            BottomSizing, 

            LeftSizing, 

            TopLeftSizing, 

            BottomLeftSizing, 

            RightSizing, 

            TopRightSizing, 

            BottomRightSizing
        }

        public enum CursPos
        {
            WithinSelectionArea = 0, 

            OutsideSelectionArea, 

            TopLine, 

            BottomLine, 

            LeftLine, 

            RightLine, 

            }
            else
            {

                Properties.Settings.Default.minScanXY = CurrentTopLeft;
                Properties.Settings.Default.maxScanXY = CurrentBottomRight;
                settings.txtMinXY.Text = CurrentTopLeft.ToString();
                settings.txtMaxXY.Text = CurrentBottomRight.ToString();

            BottomLeft, 

            BottomRight
        }

        public static frmOverlay GetForm(frmSettings settings)
        {
            if (inst == null || inst.IsDisposed) inst = new frmOverlay(settings);
            return inst;
        }

        public void key_press(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
            else if (e.KeyCode == Keys.Z) this.InvertColors();
            else if (e.KeyCode == Keys.Enter) this.SaveSelection();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                e = null;
            }

            base.OnMouseClick(e);
        }

        private CursPos CursorPosition()
        {
            if (Cursor.Position.X > this.CurrentTopLeft.X - 10 && Cursor.Position.X < this.CurrentTopLeft.X + 10
                && (Cursor.Position.Y > this.CurrentTopLeft.Y + 10) && (Cursor.Position.Y < this.CurrentBottomRight.Y - 10))
            {
                this.Cursor = Cursors.SizeWE;
                return CursPos.LeftLine;
            }

            if (Cursor.Position.X >= this.CurrentTopLeft.X - 10 && Cursor.Position.X <= this.CurrentTopLeft.X + 10
                && (Cursor.Position.Y >= this.CurrentTopLeft.Y - 10) && (Cursor.Position.Y <= this.CurrentTopLeft.Y + 10))
            {
                this.Cursor = Cursors.SizeNWSE;
                return CursPos.TopLeft;
            }

            if (Cursor.Position.X >= this.CurrentTopLeft.X - 10 && Cursor.Position.X <= this.CurrentTopLeft.X + 10
                && (Cursor.Position.Y >= this.CurrentBottomRight.Y - 10) && (Cursor.Position.Y <= this.CurrentBottomRight.Y + 10))
            {
                this.Cursor = Cursors.SizeNESW;
                return CursPos.BottomLeft;
            }

            if (Cursor.Position.X > this.CurrentBottomRight.X - 10 && Cursor.Position.X < this.CurrentBottomRight.X + 10
                && (Cursor.Position.Y > this.CurrentTopLeft.Y + 10) && (Cursor.Position.Y < this.CurrentBottomRight.Y - 10))
            {
                this.Cursor = Cursors.SizeWE;
                return CursPos.RightLine;
            }

            if (Cursor.Position.X >= this.CurrentBottomRight.X - 10 && Cursor.Position.X <= this.CurrentBottomRight.X + 10
                && (Cursor.Position.Y >= this.CurrentTopLeft.Y - 10) && (Cursor.Position.Y <= this.CurrentTopLeft.Y + 10))
            {
                this.Cursor = Cursors.SizeNESW;
                return CursPos.TopRight;
            }

            if (Cursor.Position.X >= this.CurrentBottomRight.X - 10 && Cursor.Position.X <= this.CurrentBottomRight.X + 10
                && (Cursor.Position.Y >= this.CurrentBottomRight.Y - 10) && (Cursor.Position.Y <= this.CurrentBottomRight.Y + 10))
            {
                this.Cursor = Cursors.SizeNWSE;
                return CursPos.BottomRight;
            }

            if ((Cursor.Position.Y > this.CurrentTopLeft.Y - 10) && (Cursor.Position.Y < this.CurrentTopLeft.Y + 10)
                && Cursor.Position.X > this.CurrentTopLeft.X + 10 && Cursor.Position.X < this.CurrentBottomRight.X - 10)
            {
                this.Cursor = Cursors.SizeNS;
                return CursPos.TopLine;
            }

            if ((Cursor.Position.Y > this.CurrentBottomRight.Y - 10) && (Cursor.Position.Y < this.CurrentBottomRight.Y + 10)
                && Cursor.Position.X > this.CurrentTopLeft.X + 10 && Cursor.Position.X < this.CurrentBottomRight.X - 10)
            {
                this.Cursor = Cursors.SizeNS;
                return CursPos.BottomLine;
            }

            if (Cursor.Position.X >= this.CurrentTopLeft.X + 10 && Cursor.Position.X <= this.CurrentBottomRight.X - 10
                && Cursor.Position.Y >= this.CurrentTopLeft.Y + 10 && Cursor.Position.Y <= this.CurrentBottomRight.Y - 10)
            {
                this.Cursor = Cursors.Hand;
                return CursPos.WithinSelectionArea;
            }

            this.Cursor = Cursors.No;
            return CursPos.OutsideSelectionArea;
        }

        private void DragSelection()
        {
            // Ensure that the rectangle stays within the bounds of the screen

            // Erase the previous rectangle
            this.g.DrawRectangle(this.EraserPen, this.GetX(this.CurrentTopLeft.X), this.CurrentTopLeft.Y, this.RectangleWidth, this.RectangleHeight);

            if (this.GetX(Cursor.Position.X) - this.DragClickRelative.X > 0
                && this.GetX(Cursor.Position.X) - this.DragClickRelative.X + this.RectangleWidth < this.Width
                
                /*System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width*/)
            {
                this.CurrentTopLeft.X = Cursor.Position.X - this.DragClickRelative.X;
                this.CurrentBottomRight.X = this.CurrentTopLeft.X + this.RectangleWidth;
            }
            else

            // Selection area has reached the right side of the screen
                if (this.GetX(Cursor.Position.X) - this.DragClickRelative.X > 0)
                {
                    this.CurrentTopLeft.X = this.Width /*System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width*/- this.RectangleWidth;
                    this.CurrentBottomRight.X = this.CurrentTopLeft.X + this.RectangleWidth;
                }

                // Selection area has reached the left side of the screen
                else
                {
                    this.CurrentTopLeft.X = this.Left;
                    this.CurrentBottomRight.X = this.CurrentTopLeft.X + this.RectangleWidth;
                }

            if (Cursor.Position.Y - this.DragClickRelative.Y > 0 && Cursor.Position.Y - this.DragClickRelative.Y + this.RectangleHeight < this.Width
                
                /*System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height*/)
            {
                this.CurrentTopLeft.Y = Cursor.Position.Y - this.DragClickRelative.Y;
                this.CurrentBottomRight.Y = this.CurrentTopLeft.Y + this.RectangleHeight;
            }
            else

            // Selection area has reached the bottom of the screen
                if (Cursor.Position.Y - this.DragClickRelative.Y > 0)
                {
                    this.CurrentTopLeft.Y = this.Height /*System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height*/- this.RectangleHeight;
                    this.CurrentBottomRight.Y = this.CurrentTopLeft.Y + this.RectangleHeight;
                }

                // Selection area has reached the top of the screen
                else
                {
                    this.CurrentTopLeft.Y = 0;
                    this.CurrentBottomRight.Y = this.CurrentTopLeft.Y + this.RectangleHeight;
                }

            // Draw a new rectangle
            this.g.DrawRectangle(this.MyPen, this.GetX(this.CurrentTopLeft.X), this.CurrentTopLeft.Y, this.RectangleWidth, this.RectangleHeight);
        }

        private void DrawSelection()
        {
            this.Cursor = Cursors.Arrow;

            // Erase the previous rectangle
            this.g.DrawRectangle(
                this.EraserPen, 
                this.GetX(this.CurrentTopLeft.X), 
                this.CurrentTopLeft.Y, 
                this.GetX(this.CurrentBottomRight.X) - this.GetX(this.CurrentTopLeft.X), 
                this.CurrentBottomRight.Y - this.CurrentTopLeft.Y);

            // Calculate X Coordinates
            if (Cursor.Position.X < this.ClickPoint.X)
            {
                this.CurrentTopLeft.X = Cursor.Position.X;
                this.CurrentBottomRight.X = this.ClickPoint.X;
            }
            else
            {
                this.CurrentTopLeft.X = this.ClickPoint.X;
                this.CurrentBottomRight.X = Cursor.Position.X;
            }

            // Calculate Y Coordinates
            if (Cursor.Position.Y < this.ClickPoint.Y)
            {
                this.CurrentTopLeft.Y = Cursor.Position.Y;
                this.CurrentBottomRight.Y = this.ClickPoint.Y;
            }
            else
            {
                this.CurrentTopLeft.Y = this.ClickPoint.Y;
                this.CurrentBottomRight.Y = Cursor.Position.Y;
            }

            // Draw a new rectangle
            this.g.DrawRectangle(
                this.MyPen, 
                this.GetX(this.CurrentTopLeft.X), 
                this.CurrentTopLeft.Y, 
                this.GetX(this.CurrentBottomRight.X) - this.GetX(this.CurrentTopLeft.X), 
                this.CurrentBottomRight.Y - this.CurrentTopLeft.Y);
        }

        private int GetPrimaryMonIdx()
        {
            Screen[] sc;
            sc = Screen.AllScreens;
            var idx = 0;

            foreach (var s in sc)
            {
                if (s.Bounds.Left == Screen.PrimaryScreen.Bounds.Left) break;
                idx++;
            }

            return idx <= sc.Length ? idx : 0;
        }

        private int GetX(int X)
        {
            if (this.showMon == this.PrimMon) return X;
            if (this.Left < 0) return X + Math.Abs(this.Left);
            return X - Screen.PrimaryScreen.Bounds.Width;
        }

        private void init_FullScreen()
        {
            Screen[] sc;
            sc = Screen.AllScreens;

            this.CurrentTopLeft.X = sc[this.showMon].Bounds.Left;
            this.CurrentTopLeft.Y = sc[this.showMon].Bounds.Top;
            if (this.showMon != this.PrimMon) this.CurrentBottomRight.X = sc[this.showMon].Bounds.Width + sc[this.showMon].Bounds.Left;
            else this.CurrentBottomRight.X = sc[this.showMon].Bounds.Width;
            this.CurrentBottomRight.Y = sc[this.showMon].Bounds.Height;
        }

        private void initPoints()
        {
            this.ClickPoint.X = 0;
            this.ClickPoint.Y = 0;

            this.DragClickRelative.X = 0;
            this.DragClickRelative.Y = 0;

            this.CurrentTopLeft.X = 0;
            this.CurrentTopLeft.Y = 0;
            this.CurrentBottomRight.X = 0;
            this.CurrentBottomRight.Y = 0;
            this.RectangleWidth = 0;
            this.RectangleHeight = 0;

            this.LeftButtonDown = false;
            this.RectangleDrawn = false;
            this.ReadyToDrag = false;

            this.Cursor = Cursors.Arrow;
        }

        private void InvertColors()
        {
            this.g.DrawRectangle(
                this.EraserPen, 
                this.GetX(this.CurrentTopLeft.X), 
                this.CurrentTopLeft.Y, 
                this.GetX(this.CurrentBottomRight.X) - this.GetX(this.CurrentTopLeft.X), 
                this.CurrentBottomRight.Y - this.CurrentTopLeft.Y);

            if (this.BackColor == Color.Black)
            {
                this.BackColor = Color.Yellow;
                this.MyPen.Dispose();
                this.EraserPen.Dispose();
                this.MyPen = new Pen(Color.Black, 1);
                this.EraserPen = new Pen(Color.Yellow, 20);
            }
            else
                //Selection area has reached the right side of the screen
                if (GetX(Cursor.Position.X) - DragClickRelative.X > 0)
            {

                CurrentTopLeft.X = this.Width/*System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width*/ - RectangleWidth;
                CurrentBottomRight.X = CurrentTopLeft.X + RectangleWidth;

            }
            //Selection area has reached the left side of the screen
            else
            {

                CurrentTopLeft.X = this.Left;
                CurrentBottomRight.X = CurrentTopLeft.X + RectangleWidth;

            }

        private void mouse_dClick(object sender, EventArgs e)
        {
            this.SaveSelection();
        }

        private void mouse_Move(object sender, MouseEventArgs e)
        {
            if (this.LeftButtonDown && !this.RectangleDrawn)
            {
                this.DrawSelection();
            }
            else
                //Selection area has reached the bottom of the screen
                if (Cursor.Position.Y - DragClickRelative.Y > 0)
            {

                CurrentTopLeft.Y = this.Height/*System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height*/ - RectangleHeight;
                CurrentBottomRight.Y = CurrentTopLeft.Y + RectangleHeight;

            }
            //Selection area has reached the top of the screen
            else
            {

                CurrentTopLeft.Y = 0;
                CurrentBottomRight.Y = CurrentTopLeft.Y + RectangleHeight;

            }

                if (this.CurrentAction != ClickAction.Dragging && this.CurrentAction != ClickAction.Outside)
                {
                    this.ResizeSelection();
                }
            }
        }
        #endregion

        private void mouse_Up(object sender, MouseEventArgs e)
        {
            this.RectangleDrawn = true;
            this.LeftButtonDown = false;
            this.CurrentAction = ClickAction.NoClick;
        }

        private void ResizeSelection()
        {
            if (this.CurrentAction == ClickAction.LeftSizing)
            {
                if (Cursor.Position.X < this.CurrentBottomRight.X - 10)
                {
                    // Erase the previous rectangle
                    this.g.DrawRectangle(this.EraserPen, this.GetX(this.CurrentTopLeft.X), this.CurrentTopLeft.Y, this.RectangleWidth, this.RectangleHeight);
                    this.CurrentTopLeft.X = Cursor.Position.X;
                    this.RectangleWidth = this.CurrentBottomRight.X - this.CurrentTopLeft.X;
                    this.g.DrawRectangle(this.MyPen, this.GetX(this.CurrentTopLeft.X), this.CurrentTopLeft.Y, this.RectangleWidth, this.RectangleHeight);
                }
            }

            if (this.CurrentAction == ClickAction.TopLeftSizing)
            {
                if (Cursor.Position.X < this.CurrentBottomRight.X - 10 && Cursor.Position.Y < this.CurrentBottomRight.Y - 10)
                {
                    // Erase the previous rectangle
                    this.g.DrawRectangle(this.EraserPen, this.GetX(this.CurrentTopLeft.X), this.CurrentTopLeft.Y, this.RectangleWidth, this.RectangleHeight);
                    this.CurrentTopLeft.X = Cursor.Position.X;
                    this.CurrentTopLeft.Y = Cursor.Position.Y;
                    this.RectangleWidth = this.CurrentBottomRight.X - this.CurrentTopLeft.X;
                    this.RectangleHeight = this.CurrentBottomRight.Y - this.CurrentTopLeft.Y;
                    this.g.DrawRectangle(this.MyPen, this.GetX(this.CurrentTopLeft.X), this.CurrentTopLeft.Y, this.RectangleWidth, this.RectangleHeight);
                }
            }

            if (this.CurrentAction == ClickAction.BottomLeftSizing)
            {
                if (Cursor.Position.X < this.CurrentBottomRight.X - 10 && Cursor.Position.Y > this.CurrentTopLeft.Y + 10)
                {
                    // Erase the previous rectangle
                    this.g.DrawRectangle(this.EraserPen, this.GetX(this.CurrentTopLeft.X), this.CurrentTopLeft.Y, this.RectangleWidth, this.RectangleHeight);
                    this.CurrentTopLeft.X = Cursor.Position.X;
                    this.CurrentBottomRight.Y = Cursor.Position.Y;
                    this.RectangleWidth = this.CurrentBottomRight.X - this.CurrentTopLeft.X;
                    this.RectangleHeight = this.CurrentBottomRight.Y - this.CurrentTopLeft.Y;
                    this.g.DrawRectangle(this.MyPen, this.GetX(this.CurrentTopLeft.X), this.CurrentTopLeft.Y, this.RectangleWidth, this.RectangleHeight);
                }
            }

            if (this.CurrentAction == ClickAction.RightSizing)
            {
                if (Cursor.Position.X > this.CurrentTopLeft.X + 10)
                {
                    // Erase the previous rectangle
                    this.g.DrawRectangle(this.EraserPen, this.GetX(this.CurrentTopLeft.X), this.CurrentTopLeft.Y, this.RectangleWidth, this.RectangleHeight);
                    this.CurrentBottomRight.X = Cursor.Position.X;
                    this.RectangleWidth = this.CurrentBottomRight.X - this.CurrentTopLeft.X;
                    this.g.DrawRectangle(this.MyPen, this.GetX(this.CurrentTopLeft.X), this.CurrentTopLeft.Y, this.RectangleWidth, this.RectangleHeight);
                }
            }

            if (this.CurrentAction == ClickAction.TopRightSizing)
            {
                if (Cursor.Position.X > this.CurrentTopLeft.X + 10 && Cursor.Position.Y < this.CurrentBottomRight.Y - 10)
                {
                    // Erase the previous rectangle
                    this.g.DrawRectangle(this.EraserPen, this.GetX(this.CurrentTopLeft.X), this.CurrentTopLeft.Y, this.RectangleWidth, this.RectangleHeight);
                    this.CurrentBottomRight.X = Cursor.Position.X;
                    this.CurrentTopLeft.Y = Cursor.Position.Y;
                    this.RectangleWidth = this.CurrentBottomRight.X - this.CurrentTopLeft.X;
                    this.RectangleHeight = this.CurrentBottomRight.Y - this.CurrentTopLeft.Y;
                    this.g.DrawRectangle(this.MyPen, this.GetX(this.CurrentTopLeft.X), this.CurrentTopLeft.Y, this.RectangleWidth, this.RectangleHeight);
                }
            }

            if (this.CurrentAction == ClickAction.BottomRightSizing)
            {
                if (Cursor.Position.X > this.CurrentTopLeft.X + 10 && Cursor.Position.Y > this.CurrentTopLeft.Y + 10)
                {
                    // Erase the previous rectangle
                    this.g.DrawRectangle(this.EraserPen, this.GetX(this.CurrentTopLeft.X), this.CurrentTopLeft.Y, this.RectangleWidth, this.RectangleHeight);
                    this.CurrentBottomRight.X = Cursor.Position.X;
                    this.CurrentBottomRight.Y = Cursor.Position.Y;
                    this.RectangleWidth = this.CurrentBottomRight.X - this.CurrentTopLeft.X;
                    this.RectangleHeight = this.CurrentBottomRight.Y - this.CurrentTopLeft.Y;
                    this.g.DrawRectangle(this.MyPen, this.GetX(this.CurrentTopLeft.X), this.CurrentTopLeft.Y, this.RectangleWidth, this.RectangleHeight);
                }
            }

            if (this.CurrentAction == ClickAction.TopSizing)
            {
                if (Cursor.Position.Y < this.CurrentBottomRight.Y - 10)
                {
                    // Erase the previous rectangle
                    this.g.DrawRectangle(this.EraserPen, this.GetX(this.CurrentTopLeft.X), this.CurrentTopLeft.Y, this.RectangleWidth, this.RectangleHeight);
                    this.CurrentTopLeft.Y = Cursor.Position.Y;
                    this.RectangleHeight = this.CurrentBottomRight.Y - this.CurrentTopLeft.Y;
                    this.g.DrawRectangle(this.MyPen, this.GetX(this.CurrentTopLeft.X), this.CurrentTopLeft.Y, this.RectangleWidth, this.RectangleHeight);
                }
            }

            if (this.CurrentAction == ClickAction.BottomSizing)
            {
                if (Cursor.Position.Y > this.CurrentTopLeft.Y + 10)
                {
                    // Erase the previous rectangle
                    this.g.DrawRectangle(this.EraserPen, this.GetX(this.CurrentTopLeft.X), this.CurrentTopLeft.Y, this.RectangleWidth, this.RectangleHeight);
                    this.CurrentBottomRight.Y = Cursor.Position.Y;
                    this.RectangleHeight = this.CurrentBottomRight.Y - this.CurrentTopLeft.Y;
                    this.g.DrawRectangle(this.MyPen, this.GetX(this.CurrentTopLeft.X), this.CurrentTopLeft.Y, this.RectangleWidth, this.RectangleHeight);
                }
            }
        }

        private void SaveSelection()
        {
            if (this.CurrentBottomRight.X - this.CurrentTopLeft.X < 60 || this.CurrentBottomRight.Y - this.CurrentTopLeft.Y < 60)
            {
                MessageBox.Show(Translate.GetTranslate("frmSettings", "AREA_SMALL"), "Error");
            }
            else
            {
                Settings.Default.minScanXY = this.CurrentTopLeft;
                Settings.Default.maxScanXY = this.CurrentBottomRight;
                this.settings.txtMinXY.Text = this.CurrentTopLeft.ToString();
                this.settings.txtMaxXY.Text = this.CurrentBottomRight.ToString();
            }

            this.Close();
        }

        private void SetClickAction()
        {
            switch (this.CursorPosition())
            {
                case CursPos.BottomLine:
                    this.CurrentAction = ClickAction.BottomSizing;
                    break;
                case CursPos.TopLine:
                    this.CurrentAction = ClickAction.TopSizing;
                    break;
                case CursPos.LeftLine:
                    this.CurrentAction = ClickAction.LeftSizing;
                    break;
                case CursPos.TopLeft:
                    this.CurrentAction = ClickAction.TopLeftSizing;
                    break;
                case CursPos.BottomLeft:
                    this.CurrentAction = ClickAction.BottomLeftSizing;
                    break;
                case CursPos.RightLine:
                    this.CurrentAction = ClickAction.RightSizing;
                    break;
                case CursPos.TopRight:
                    this.CurrentAction = ClickAction.TopRightSizing;
                    break;
                case CursPos.BottomRight:
                    this.CurrentAction = ClickAction.BottomRightSizing;
                    break;
                case CursPos.WithinSelectionArea:
                    this.CurrentAction = ClickAction.Dragging;
                    break;
                case CursPos.OutsideSelectionArea:
                    this.CurrentAction = ClickAction.Outside;
                    break;
            }
        }
    }
}