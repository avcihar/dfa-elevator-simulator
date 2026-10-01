using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace IntelligentElevatorSystem
{
    public class DfaVisualizerPanel : Panel
    {
        public string DfaType { get; set; } = "Cabin"; // Cabin, Door, Dispatcher
        public string ActiveState { get; set; } = "Idle";
        public string LastSymbol { get; set; } = "";

        private int nodeRadius = 24;
        private Dictionary<string, Point> positions = new Dictionary<string, Point>();
        private List<Tuple<string, string, string>> edges = new List<Tuple<string, string, string>>();
        private HashSet<string> acceptStates = new HashSet<string>();

        public DfaVisualizerPanel()
        {
            this.DoubleBuffered = true; // Ekran kırpışmasını engellemek için
            this.BackColor = Color.FromArgb(10, 10, 15);
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Font = new Font("Consolas", 9, FontStyle.Bold);
        }

        public void SetupDfa()
        {
            positions.Clear();
            edges.Clear();
            acceptStates.Clear();

            if (DfaType == "Cabin")
            {
                // DfaData.ts ve Asansor_haraket.json koordinat matrisi
                positions["Idle"] = new Point(60, 110);
                positions["Up"] = new Point(180, 50);
                positions["Down"] = new Point(180, 170);
                positions["Maint"] = new Point(300, 50);
                positions["Emrg"] = new Point(300, 170);

                acceptStates.Add("Idle");

                edges.Add(Tuple.Create("Idle", "Up", "U"));
                edges.Add(Tuple.Create("Idle", "Down", "D"));
                edges.Add(Tuple.Create("Idle", "Idle", "A R"));
                edges.Add(Tuple.Create("Idle", "Emrg", "E"));
                edges.Add(Tuple.Create("Idle", "Maint", "F"));
                edges.Add(Tuple.Create("Up", "Up", "U D R"));
                edges.Add(Tuple.Create("Up", "Idle", "A"));
                edges.Add(Tuple.Create("Up", "Emrg", "E"));
                edges.Add(Tuple.Create("Up", "Maint", "F"));
                edges.Add(Tuple.Create("Down", "Down", "U D R"));
                edges.Add(Tuple.Create("Down", "Idle", "A"));
                edges.Add(Tuple.Create("Down", "Emrg", "E"));
                edges.Add(Tuple.Create("Down", "Maint", "F"));
                edges.Add(Tuple.Create("Emrg", "Emrg", "U D A E F"));
                edges.Add(Tuple.Create("Emrg", "Idle", "R"));
                edges.Add(Tuple.Create("Maint", "Maint", "U D A F"));
                edges.Add(Tuple.Create("Maint", "Emrg", "E"));
                edges.Add(Tuple.Create("Maint", "Idle", "R"));
            }
            else if (DfaType == "Door")
            {
                // kapi.json matrisi
                positions["Closed"] = new Point(50, 110);
                positions["Opening"] = new Point(150, 40);
                positions["Open"] = new Point(270, 40);
                positions["OvrLoad"] = new Point(360, 110);
                positions["Closing"] = new Point(230, 180);
                positions["Obstcle"] = new Point(110, 180);

                acceptStates.Add("Closed");

                edges.Add(Tuple.Create("Closed", "Opening", "A"));
                edges.Add(Tuple.Create("Closed", "Closed", "O C T V L B R"));
                edges.Add(Tuple.Create("Opening", "Open", "O"));
                edges.Add(Tuple.Create("Opening", "Opening", "A C T V L B R"));
                edges.Add(Tuple.Create("Open", "Closing", "T"));
                edges.Add(Tuple.Create("Open", "OvrLoad", "V"));
                edges.Add(Tuple.Create("Open", "Open", "A O C L B R"));
                edges.Add(Tuple.Create("Closing", "Closed", "C"));
                edges.Add(Tuple.Create("Closing", "Obstcle", "B"));
                edges.Add(Tuple.Create("Closing", "Opening", "R"));
                edges.Add(Tuple.Create("Closing", "Closing", "A O T V L"));
                edges.Add(Tuple.Create("OvrLoad", "Open", "L"));
                edges.Add(Tuple.Create("OvrLoad", "OvrLoad", "A O C T V B R"));
                edges.Add(Tuple.Create("Obstcle", "Open", "O"));
                edges.Add(Tuple.Create("Obstcle", "Obstcle", "A C T V L B R"));
            }
            else if (DfaType == "Dispatcher")
            {
                // Dispatcher.json matrisi
                positions["Avail"] = new Point(70, 110);
                positions["Full"] = new Point(260, 50);
                positions["Halt"] = new Point(260, 170);

                acceptStates.Add("Avail");

                edges.Add(Tuple.Create("Avail", "Avail", "R A S"));
                edges.Add(Tuple.Create("Avail", "Full", "F"));
                edges.Add(Tuple.Create("Avail", "Halt", "E"));
                edges.Add(Tuple.Create("Full", "Full", "R F S"));
                edges.Add(Tuple.Create("Full", "Avail", "A"));
                edges.Add(Tuple.Create("Full", "Halt", "E"));
                edges.Add(Tuple.Create("Halt", "Halt", "R F A E"));
                edges.Add(Tuple.Create("Halt", "Avail", "S"));
            }
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias; // Pürüzsüz çizim aktif

            // Özel Ok Başı (LineCap) Tanımlama
            AdjustableArrowCap arrowCap = new AdjustableArrowCap(5, 5);

            // 1. TRANSITION (KENARLAR / OKLAR) ÇİZİMİ
            using (Pen defaultPen = new Pen(Color.FromArgb(50, 70, 95), 2))
            using (Pen activePen = new Pen(Color.Cyan, 3))
            {
                defaultPen.CustomEndCap = arrowCap;
                activePen.CustomEndCap = arrowCap;

                foreach (var edge in edges)
                {
                    if (!positions.ContainsKey(edge.Item1) || !positions.ContainsKey(edge.Item2)) continue;

                    Point p1 = positions[edge.Item1];
                    Point p2 = positions[edge.Item2];

                    bool isActiveEdge = (edge.Item1 == ActiveState && edge.Item3.Contains(LastSymbol) && !string.IsNullOrEmpty(LastSymbol));
                    Pen currentPen = isActiveEdge ? activePen : defaultPen;
                    Brush textBrush = isActiveEdge ? Brushes.Cyan : Brushes.LightSlateGray;

                    if (edge.Item1 == edge.Item2) // Kendi kendine dönüş (Self-loop)
                    {
                        g.DrawBezier(currentPen,
                            new Point(p1.X - 8, p1.Y - 18),
                            new Point(p1.X - 25, p1.Y - 45),
                            new Point(p1.X + 25, p1.Y - 45),
                            new Point(p1.X + 8, p1.Y - 18));

                        g.DrawString(edge.Item3, this.Font, textBrush, p1.X - 15, p1.Y - 55);
                    }
                    else // İki farklı durum arası çizgi
                    {
                        // Çizginin çemberin tam merkezine değil kenarına gelmesi için açı hesaplama
                        float angle = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
                        Point startPoint = new Point(p1.X + (int)(Math.Cos(angle) * nodeRadius), p1.Y + (int)(Math.Sin(angle) * nodeRadius));
                        Point endPoint = new Point(p2.X - (int)(Math.Cos(angle) * (nodeRadius + 4)), p2.Y - (int)(Math.Sin(angle) * (nodeRadius + 4)));

                        g.DrawLine(currentPen, startPoint, endPoint);

                        // Ortadaki sembol yazısı
                        int midX = (startPoint.X + endPoint.X) / 2;
                        int midY = (startPoint.Y + endPoint.Y) / 2 - 8;
                        g.DrawString(edge.Item3, this.Font, textBrush, midX, midY);
                    }
                }
            }

            // 2. STATES (DURUMLAR / DÜĞÜMLER) ÇİZİMİ
            foreach (var state in positions)
            {
                bool isActive = (state.Key == ActiveState);
                Point pt = state.Value;

                // Arka plan parlaması ve rengi
                Color nodeColor = isActive ? Color.FromArgb(15, 52, 96) : Color.FromArgb(30, 41, 59);
                Color strokeColor = isActive ? Color.Cyan : Color.FromArgb(100, 116, 139);

                using (Brush brush = new SolidBrush(nodeColor))
                using (Pen pen = new Pen(strokeColor, isActive ? 3 : 2))
                {
                    // Ana Çember
                    g.FillEllipse(brush, pt.X - nodeRadius, pt.Y - nodeRadius, nodeRadius * 2, nodeRadius * 2);
                    g.DrawEllipse(pen, pt.X - nodeRadius, pt.Y - nodeRadius, nodeRadius * 2, nodeRadius * 2);

                    // Kabul Durumu ise Çift Çember Çiz (JFLAP Kuralı)
                    if (acceptStates.Contains(state.Key))
                    {
                        g.DrawEllipse(pen, pt.X - (nodeRadius - 4), pt.Y - (nodeRadius - 4), (nodeRadius - 4) * 2, (nodeRadius - 4) * 2);
                    }
                }

                // Durum İsmi Yazısı
                Brush labelBrush = isActive ? Brushes.White : Brushes.LightGray;
                SizeF txtSize = g.MeasureString(state.Key, this.Font);
                g.DrawString(state.Key, this.Font, labelBrush, pt.X - (txtSize.Width / 2), pt.Y - (txtSize.Height / 2));
            }
        }
    }
}