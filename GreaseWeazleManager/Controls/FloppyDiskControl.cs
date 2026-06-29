using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GwCopyPro.Models;

namespace GwCopyPro.Controls
{
    /// <summary>
    /// Owner-drawn control that renders a horizontal row of 84 coloured cells,
    /// one per cylinder, visualising track status for a single head of a floppy disk.
    /// Includes a side label, a cylinder-number ruler, and a status legend.
    /// </summary>
    public class FloppyDiskControl : Control
    {
        private const int CellW       = 8;
        private const int CellH       = 12;
        private const int CellPad     = 1;
        private const int HeaderH     = 22;
        private const int LabelW      = 30;
        private const int RightMargin = 6;
        private const int MaxCylinders = 84;

        /// <summary>Total pixel width required to display all 84 cylinder cells plus labels.</summary>
        public static int ControlWidth  => LabelW + MaxCylinders * (CellW + CellPad) + RightMargin;

        /// <summary>Total pixel height required for the header, cell row, and legend.</summary>
        public static int ControlHeight => HeaderH + CellH + CellPad + 18;

        private TrackCell[,]? _tracks;
        private int    _head       = 0;
        private string _sideLabel  = "Side A (Head 0)";
        private int    _highlightCyl = -1;

        /// <summary>
        /// Head index (0 or 1) determining which row of the <see cref="TrackCell"/> grid is rendered.
        /// Setting this property triggers a repaint.
        /// </summary>
        public int Head
        {
            get => _head;
            set { _head = value; Invalidate(); }
        }

        /// <summary>
        /// Label text displayed at the top-left of the control (e.g. "Side 0  (Head 0 — Upper)").
        /// Setting this property triggers a repaint.
        /// </summary>
        public string SideLabel
        {
            get => _sideLabel;
            set { _sideLabel = value; Invalidate(); }
        }

        /// <summary>
        /// Updates the track grid data source and schedules a repaint.
        /// </summary>
        /// <param name="tracks">An 84 × 2 array of <see cref="TrackCell"/> instances.</param>
        public void SetTracks(TrackCell[,] tracks)
        {
            _tracks = tracks;
            Invalidate();
        }

        /// <summary>
        /// Draws a white highlight ring around the specified cylinder cell and repaints.
        /// Pass <c>-1</c> to clear the highlight.
        /// </summary>
        /// <param name="cyl">Zero-based cylinder index to highlight.</param>
        public void HighlightCylinder(int cyl)
        {
            _highlightCyl = cyl;
            Invalidate();
        }

        /// <summary>
        /// Initialises the control with double-buffering, fixed size, and a dark background.
        /// </summary>
        public FloppyDiskControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);
            Size      = new Size(ControlWidth, ControlHeight);
            BackColor = Color.FromArgb(18, 20, 28);
        }

        /// <summary>
        /// Renders the background, side label, 84 cylinder cells, cylinder-number ruler,
        /// and the status-colour legend.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode      = SmoothingMode.AntiAlias;
            g.TextRenderingHint  = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            g.FillRectangle(new SolidBrush(Color.FromArgb(18, 20, 28)), ClientRectangle);

            using var titleFont  = new Font("Consolas", 7.5f, FontStyle.Bold);
            using var titleBrush = new SolidBrush(Color.FromArgb(140, 185, 255));
            g.DrawString(_sideLabel, titleFont, titleBrush, LabelW, 4);

            using var labelFont  = new Font("Consolas", 6f);
            using var labelBrush = new SolidBrush(Color.FromArgb(90, 120, 160));
            using var borderPen  = new Pen(Color.FromArgb(35, 55, 75), 0.5f);

            for (int c = 0; c < MaxCylinders; c++)
            {
                int x = LabelW + c * (CellW + CellPad);
                int y = HeaderH;

                if (c == _highlightCyl)
                {
                    using var hlPen = new Pen(Color.White, 1.2f);
                    g.DrawRectangle(hlPen, x - 1, y - 1, CellW + 1, CellH + 1);
                }

                using var cellBrush = new SolidBrush(GetCellColor(c));
                g.FillRectangle(cellBrush, x, y, CellW, CellH);
                g.DrawRectangle(borderPen, x, y, CellW, CellH);

                if (c % 10 == 0)
                    g.DrawString(c.ToString(), labelFont, labelBrush, x, y + CellH + 2);
            }

            DrawLegend(g, LabelW, HeaderH + CellH + 14);
        }

        /// <summary>
        /// Returns the fill colour for the cell at <paramref name="cylinder"/> on the current head.
        /// Falls back to the "unknown" colour when no track data is loaded.
        /// </summary>
        /// <param name="cylinder">Zero-based cylinder index.</param>
        /// <returns>The colour corresponding to the cell's <see cref="TrackStatus"/>.</returns>
        private Color GetCellColor(int cylinder)
        {
            if (_tracks == null) return Color.FromArgb(35, 42, 58);
            return _tracks[cylinder, _head].Status switch
            {
                TrackStatus.Unknown => Color.FromArgb(35, 42, 58),
                TrackStatus.Pending => Color.FromArgb(50, 60, 82),
                TrackStatus.Reading => Color.FromArgb(25, 135, 215),
                TrackStatus.Writing => Color.FromArgb(195, 135, 15),
                TrackStatus.Good    => Color.FromArgb(35, 175, 95),
                TrackStatus.Error   => Color.FromArgb(215, 45, 45),
                TrackStatus.Empty   => Color.FromArgb(55, 42, 28),
                _                   => Color.FromArgb(35, 42, 58)
            };
        }

        /// <summary>
        /// Draws the five-item colour legend (Unknown, Pending, Active, Good, Error)
        /// starting at the given coordinates.
        /// </summary>
        /// <param name="g">The <see cref="Graphics"/> context.</param>
        /// <param name="x">Left edge of the first legend swatch.</param>
        /// <param name="y">Top edge of the legend row.</param>
        private static void DrawLegend(Graphics g, int x, int y)
        {
            var items = new (Color c, string label)[]
            {
                (Color.FromArgb(35, 42, 58),   "Unknown"),
                (Color.FromArgb(50, 60, 82),   "Pending"),
                (Color.FromArgb(25, 135, 215), "Active"),
                (Color.FromArgb(35, 175, 95),  "Good"),
                (Color.FromArgb(215, 45, 45),  "Error"),
            };
            using var font = new Font("Consolas", 6f);
            int lx = x;
            foreach (var (color, label) in items)
            {
                using var b  = new SolidBrush(color);
                using var lb = new SolidBrush(Color.FromArgb(110, 140, 175));
                g.FillRectangle(b, lx, y, 8, 7);
                g.DrawString(label, font, lb, lx + 10, y - 1);
                lx += 68;
            }
        }
    }
}
