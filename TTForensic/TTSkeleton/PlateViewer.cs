using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using TTSkeleton.Utility;

namespace TTSkeleton
{
    internal class PlateViewer : FrameworkElement
    {
     
        Size _size;
        int _col = 12;
        int _row = 8;
        Size _szMargin;
        Pen defaultPen;
        Point ptStart;
        Point ptEnd;
        Point ptCurrent = new Point(0, 0);
        bool bMouseDown = false;

        public int Columns
        {
            get { return _col; }
            set { _col = value; }
        }

        public int Rows
        {
            get { return _row; }
            set { _row = value; }
        }
        public bool HasSelection
        {
            get
            {
                return ptStart.X + ptStart.Y + ptEnd.X + ptEnd.Y != 0;
            }
        }

        public PlateViewer(Size sz, Size szMargin, int col = 12, int row = 8)
        {
            Log.Info("PlateViewer ctor");
            _size = sz;
            _szMargin = szMargin;
            _col = col;
            _row = row;
            defaultPen = new Pen(Brushes.Black, 1);
            //liquidSettingsInfo = info;
            this.MouseDown += new System.Windows.Input.MouseButtonEventHandler(MyFrameWorkElement_MouseDown);
            this.MouseUp += new System.Windows.Input.MouseButtonEventHandler(MyFrameWorkElement_MouseUp);
            this.MouseMove += new System.Windows.Input.MouseEventHandler(MyFrameWorkElement_MouseMove);
            this.SizeChanged += (o, e) =>
            {
                ptStart = new Point(0, 0);
                ptEnd = new Point(0, 0);
                _size = new Size(e.NewSize.Width - _szMargin.Width, e.NewSize.Height - _szMargin.Height);
                this.InvalidateVisual(); // cause a render
            };
        }

        void MyFrameWorkElement_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ptCurrent = e.GetPosition(this);
            ClipPoint(ref ptCurrent);
            if (bMouseDown)
                ptEnd = ptCurrent;
            this.InvalidateVisual(); // cause a render
        }

        void MyFrameWorkElement_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.ReleaseMouseCapture();
            bMouseDown = false;
            ptEnd = e.GetPosition(this);
            //clip the start & end
            ClipStartEnd();
            AvoidOverlap();
            POSITION start, end;
            AdjustStartEndPosition(out start, out end);
            PCRSettings.Instance.Set(start, end);
            this.InvalidateVisual();
        }

        private void AvoidOverlap()
        {
            if (ptStart.X == ptEnd.X) //shift x
            {
                ptEnd.X = ptStart.X + 1;
            }

            if (ptStart.Y == ptEnd.Y)
            {
                ptEnd.Y = ptStart.Y + 1;
            }
        }
        private void ClipPoint(ref Point pt)
        {
            pt.X = Math.Min(pt.X, _size.Width);
            pt.Y = Math.Min(pt.Y, _size.Height);
            pt.X = Math.Max(pt.X, _szMargin.Width);
            pt.Y = Math.Max(pt.Y, _szMargin.Height);
        }
        private void ClipStartEnd()
        {
            ClipPoint(ref ptStart);
            ClipPoint(ref ptEnd);
        }

        void MyFrameWorkElement_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ptStart = e.GetPosition(this);
            this.CaptureMouse();
            bMouseDown = true;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            //clip the point
            DrawAssays(drawingContext);

            //draw border
            DrawBorder(drawingContext);

            //grids
            DrawGrids(drawingContext);

            //label
            DrawLabels(drawingContext);



            if (bMouseDown)
            {
                drawingContext.DrawRectangle(Brushes.Wheat, defaultPen, new Rect(ptStart, ptEnd));
            }
            else
            {
                if (HasSelection)
                {
                    Pen pen = CreateBorderPen();
                    drawingContext.DrawRectangle(Brushes.Transparent, pen, new Rect(ptStart, ptEnd));
                }
            }
            //draw hint
            DrawHint(drawingContext);
        }

        private void DrawHint(DrawingContext drawingContext)
        {
            var wellWidth = GetWellWidth();
            var wellHeight = GetWellHeight();
            if (wellWidth == 0 || wellHeight == 0 || ptCurrent.X == 0)
                return;


            int height = (int)(this.ActualWidth / 800.0 * 20);

            Point pt2Draw = AdjustPosition(ptCurrent);
            pt2Draw.X = Math.Min(pt2Draw.X, _size.Width - wellWidth);
            pt2Draw.Y = Math.Min(pt2Draw.Y, _size.Height);
            Point ptText = new Point(pt2Draw.X + wellWidth / 3, pt2Draw.Y - wellHeight / 1.5);
            string desc = GetDescription(ptText);

            var txt = new FormattedText(desc,
                                        System.Globalization.CultureInfo.CurrentCulture,
                                        FlowDirection.LeftToRight,
                                        new Typeface("Courier new"),
                                        height,
                                        Brushes.Black);
            drawingContext.DrawText(txt, ptText);
            pt2Draw.Y -= wellHeight;
            var brush = new SolidColorBrush(Color.FromArgb(255, 50, 255, 120));
            drawingContext.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Wheat, 3), new Rect(pt2Draw, new Size(wellWidth, wellHeight)));
        }

        private void DrawAssays(DrawingContext drawingContext)
        {
            if (!PCRSettings.Instance.Vals.ContainsKey(GlobalVars.Instance.CurrentPlateID))
                return;
            Dictionary<POSITION, string> curPlatePosVals = PCRSettings.Instance.Vals[GlobalVars.Instance.CurrentPlateID];
            foreach (KeyValuePair<POSITION, string> pair in curPlatePosVals)
            {
                string pcrType = Common.RemoveUL(pair.Value).Trim();
                var pos = pair.Key;
                var color = PCRSettings.Instance.PCRType_Settings[pcrType].color;
                DrawWell(drawingContext, pos.x, pos.y, color, 100);
            }
        }

        //private void DrawLiquids(DrawingContext drawingContext)
        //{
        //    foreach (POSITION pos in liquidSettingsInfo.eachWellInfos.Keys)
        //    {
        //        DrawWell(pos, liquidSettingsInfo[pos], drawingContext);
        //    }
        //}

        //private void DrawWell(POSITION pos, WellInfo wellInfo, DrawingContext drawingContext)
        //{
        //    double sumVol = wellInfo.liquidsSetting.Sum(item => item.Value.thisLiquid);
        //    double startVol = 0;
        //    foreach (KeyValuePair<Reagent, VolumeSetting> pair in wellInfo.liquidsSetting)
        //    {
        //        //DrawWell(drawingContext, pos.x, pos.y, pair.Key.Color, pair.Value.thisLiquid/pair.Value.total);
        //        DrawPartWell(drawingContext, pos.x, pos.y, pair.Key.Color, startVol, pair.Value.thisLiquid, sumVol, pair.Value.total);
        //        startVol += pair.Value.thisLiquid;
        //    }
        //}

        private void DrawWell(DrawingContext drawingContext, int col, int row, Color color, double percent)
        {
            int xStart = (int)(col * GetWellWidth() + _szMargin.Width);
            int yStart = (int)(row * GetWellHeight() + _szMargin.Height);
            drawingContext.DrawRectangle(new SolidColorBrush(Color.FromArgb((byte)(255 * percent), color.R, color.G, color.B)), defaultPen,
                new Rect(new Point(xStart, yStart), new Size(GetWellWidth(), GetWellHeight())));
        }

        //private void DrawPartWell(DrawingContext drawingContext, int col, int row, Color color, double startVol, double curVol, double totalReagentVol, double totalLiquidVol)
        //{
        //    int xStart = (int)(col * GetWellWidth() + _szMargin.Width);
        //    int yStart = (int)((row + (startVol / totalReagentVol)) * GetWellHeight() + _szMargin.Height);
        //    double height = curVol / totalReagentVol * GetWellHeight();
        //    double percentColor = curVol / totalLiquidVol;

        //    drawingContext.DrawRectangle(new SolidColorBrush(Color.FromArgb((byte)(255 * percentColor), color.R, color.G, color.B)), defaultPen,
        //            new Rect(new Point(xStart, yStart), new Size(GetWellWidth(), height)));
        //}

        private Pen CreateBorderPen()
        {
            return new Pen(Brushes.DarkGray, 4);
        }


        private double GetWellWidth()
        {
            return (_size.Width - _szMargin.Width) / _col;
        }


        private double GetWellHeight()
        {
            return (_size.Height - _szMargin.Height) / _row;
        }

        public Point AdjustPosition(Point pt)
        {
            Point ptNew = new Point();
            int col = (int)((pt.X - _szMargin.Width) / GetWellWidth());
            ptNew.X = col * GetWellWidth() + _szMargin.Width;

            int row = (int)((pt.Y - _szMargin.Height) / GetWellHeight());
            ptNew.Y = (row + 1) * GetWellHeight() + _szMargin.Height;
            return ptNew;
        }

        private void AdjustStartEndPosition(out POSITION start, out POSITION end)
        {
            Point ptDownRight = new Point(Math.Max(ptStart.X, ptEnd.X), Math.Max(ptStart.Y, ptEnd.Y));
            Point ptUpLeft = new Point(Math.Min(ptStart.X, ptEnd.X), Math.Min(ptStart.Y, ptEnd.Y));
            ptStart = ptUpLeft;
            ptEnd = ptDownRight;

            int colStart = (int)((ptStart.X - _szMargin.Width) / GetWellWidth());
            ptStart.X = colStart * GetWellWidth() + _szMargin.Width;

            int rowStart = (int)((ptStart.Y - _szMargin.Height) / GetWellHeight());
            ptStart.Y = rowStart * GetWellHeight() + _szMargin.Height;

            int colEnd = (int)Math.Ceiling(((ptEnd.X - _szMargin.Width) / GetWellWidth()));
            ptEnd.X = colEnd * GetWellWidth() + _szMargin.Width;

            int rowEnd = (int)Math.Ceiling(((ptEnd.Y - _szMargin.Height) / GetWellHeight()));
            ptEnd.Y = rowEnd * GetWellHeight() + _szMargin.Height;

            start.x = colStart;
            start.y = rowStart;
            end.x = colEnd;
            end.y = rowEnd;

        }

        private void DrawLabels(DrawingContext drawingContext)
        {
            int height = (int)(this.ActualWidth / 800.0 * 20);
            for (int x = 1; x < _col + 1; x++)
            {
                var txt = new FormattedText(
                x.ToString(),
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Courier new"),
                height,
                Brushes.Black);

                int xPos = (int)((x - 0.6) * GetWellWidth()) + (int)_szMargin.Width;

                drawingContext.DrawText(txt,
                new Point(xPos, _szMargin.Height - height)
                );
                //drawingContext.DrawLine(new Pen(defaultLineBrush, 1), new Point(xPos, 0), new Point(xPos, _size.Height));
            }


            for (int y = 1; y < _row + 1; y++)
            {
                var txt = new FormattedText(
                ((char)('A' + y - 1)).ToString(),
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Courier new"),
                height,
                Brushes.Black);

                int yPos = (int)((y - 0.7) * GetWellHeight()) + (int)_szMargin.Height;

                drawingContext.DrawText(txt,
                new Point(_szMargin.Width - height, yPos)
                );

                //drawingContext.DrawLine(new Pen(defaultLineBrush, 1), new Point(xPos, 0), new Point(xPos, _size.Height));
            }
        }

        private void DrawGrids(DrawingContext drawingContext)
        {

            for (int x = 1; x < _col; x++)
            {
                int xPos = (int)(x * GetWellWidth()) + (int)_szMargin.Width;
                drawingContext.DrawLine(defaultPen, new Point(xPos, _szMargin.Height), new Point(xPos, _size.Height));
            }

            for (int y = 1; y < _row; y++)
            {
                int yPos = (int)(y * GetWellHeight()) + (int)_szMargin.Height;
                drawingContext.DrawLine(defaultPen, new Point(_szMargin.Width, yPos), new Point(_size.Width, yPos));
            }
        }



        private void DrawBorder(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(Brushes.Transparent, defaultPen,
                new Rect(_szMargin.Width, _szMargin.Height, _size.Width - _szMargin.Width, _size.Height - _szMargin.Height));
        }

        internal POSITION GetStartPos()
        {
            return GetPos(ptStart);

        }

        private POSITION GetPos(Point pt)
        {
            int xStart = (int)((pt.X - _szMargin.Width) / GetWellWidth());
            int yStart = (int)((pt.Y - _szMargin.Height) / GetWellHeight());
            return new POSITION(xStart, yStart);
        }

        internal POSITION GetEndPos()
        {
            return GetPos(ptEnd);
        }

        internal string GetDescription(Point pt)
        {
            POSITION pos = GetPos(pt);
            string sWellDesc = string.Format("{0}{1} ", (char)(pos.y + 'A'), (pos.x + 1));
            return sWellDesc;
        }
    }
    public struct POSITION
    {
        public int x;
        public int y;

        public int WellID
        {
            get
            {
                return Common.GetWellID(x, y);
            }
        }

        public POSITION(int xVal, int yVal)
        {
            x = xVal;
            y = yVal;
        }


        public string Description
        {
            get
            {
                return string.Format("{0}{1}", (char)(y + 'A'), (x + 1));
            }
        }
    }
}
