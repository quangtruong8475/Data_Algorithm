using PerformClustering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ZedGraph;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
namespace PerformClustering
{
    public partial class GraphForm : Form
    {
        Color[] colors;
        private const int UNCLASSIFIED = 0;
        private const int NOISE = -1;
        private System.Windows.Forms.ToolTip tooltip = new System.Windows.Forms.ToolTip();

        public GraphForm()
        {
            InitializeComponent();
            

        }
        private void GraphForm_Load(object sender, EventArgs e)
        {
            

        }
        public GraphForm( List<Diem>diemList, List<Cum> cumKetQua)
        {
            InitializeComponent();   
            InitializeColors();
            SetUpZedGraph(diemList);
            DrawPointsKMean(cumKetQua);
        }
        public GraphForm(List<Diem> dataPoints, int[] lables)
        {
            InitializeComponent();
            InitializeColors();
            SetUpZedGraph(dataPoints);
            DrawPointsDBScan(dataPoints, lables);
        }
        public GraphForm(List<Diem> diemList,Dictionary<Diem, List<Diem>> clusters)
        {
            InitializeComponent();
            InitializeColors();
            SetUpZedGraph(diemList);
            DrawPointsMeanShift(clusters);
        }
        private void InitializeColors()
        {
            colors = new Color[50]
            {
            Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Orange,
            Color.Purple, Color.Cyan, Color.Magenta, Color.Brown, Color.Pink,
            Color.Lime, Color.Maroon, Color.Navy, Color.Olive, Color.Teal,
            Color.Aqua, Color.Gray, Color.Silver, Color.Gold, Color.Beige,
            Color.Bisque, Color.Black, Color.Azure, Color.Coral, Color.Crimson,
            Color.Fuchsia, Color.Indigo, Color.Khaki, Color.Lavender, Color.LawnGreen,
            Color.LemonChiffon, Color.LightBlue, Color.LightCoral, Color.LightCyan, Color.LightGoldenrodYellow,
            Color.LightGray, Color.LightGreen, Color.LightPink, Color.LightSalmon, Color.LightSeaGreen,
            Color.LightSkyBlue, Color.LightSlateGray, Color.LightSteelBlue, Color.LightYellow, Color.LimeGreen,
            Color.MediumAquamarine, Color.MediumBlue, Color.MediumOrchid, Color.MediumPurple, Color.MediumSeaGreen
            };
        }
        public double FindMinValue(List<Diem> diemList)
        {
            double minValue = 0;

            foreach (var diem in diemList)
            {
                if (diem.X < minValue)
                    minValue = diem.X;
                if (diem.Y < minValue)
                    minValue = diem.Y;
            }

            return minValue;
        }
        public double FindMaxValue(List<Diem> diemList)
        {
            double maxValue = 0;

            foreach (var diem in diemList)
            {
                if (diem.X > maxValue)
                    maxValue = diem.X;
                if (diem.Y > maxValue)
                    maxValue = diem.Y;
            }

            return maxValue;
        }
        public void SetUpZedGraph(List<Diem> diemList)
        {
            double maxValue = FindMaxValue(diemList) + 2;
            double minValue = FindMinValue(diemList) - 0.5;

            GraphPane graphPane = zedGraphControl.GraphPane;

            graphPane.XAxis.Scale.Min = minValue;
            graphPane.XAxis.Scale.Max = maxValue;

            graphPane.YAxis.Scale.Min = minValue;
            graphPane.YAxis.Scale.Max = maxValue;

            zedGraphControl.AxisChange();
            zedGraphControl.Invalidate();
            VeTrucTungQuaGocToaDo();
        }
        public void VeTrucTungQuaGocToaDo()
        {
            // Truy cập GraphPane của ZedGraphControl
            GraphPane graphPane = zedGraphControl.GraphPane;

            // Tạo một đường thẳng đứng tại X = 0
            LineObj trucTung = new LineObj(
                Color.Black,    // Màu của đường
                0, graphPane.YAxis.Scale.Min,   // Điểm bắt đầu của đường (X = 0, Y = Min của trục Y)
                0, graphPane.YAxis.Scale.Max    // Điểm kết thúc của đường (X = 0, Y = Max của trục Y)
            );

            trucTung.Line.Style = System.Drawing.Drawing2D.DashStyle.Solid; // Kiểu đường nét liền
            trucTung.Line.Width = 1.0f; // Độ dày của đường

            // Thêm đường vào GraphPane
            graphPane.GraphObjList.Add(trucTung);

            // Làm mới ZedGraphControl để áp dụng các thay đổi
            zedGraphControl.AxisChange();
            zedGraphControl.Invalidate();
        }

        private void DrawPointsKMean(List<Cum> cumKetQua)
        {
            if (cumKetQua != null && zedGraphControl != null)
            {
                GraphPane myPane = zedGraphControl.GraphPane;
                myPane.Title.Text = "Biểu đồ phân cụm";
                myPane.XAxis.Title.Text = "X";
                myPane.YAxis.Title.Text = "Y";
                myPane.CurveList.Clear();

                // Sử dụng màu sắc từ danh sách
                for (int i = 0; i < cumKetQua.Count; i++)
                {
                    var cum = cumKetQua[i];
                    PointPairList cumList = new PointPairList();
                    foreach (var diem in cum.DiemCum)
                    {
                        cumList.Add(diem.X, diem.Y);
                    }

                    // Lấy màu từ danh sách màu sắc đã tạo
                    Color clusterColor = colors[i % colors.Length]; // Đảm bảo không vượt quá số lượng màu
                    LineItem cumCurve = myPane.AddCurve($"Cụm {i + 1}", cumList, clusterColor, SymbolType.Circle);
                    cumCurve.Line.IsVisible = false;
                    cumCurve.Symbol.Fill = new Fill(clusterColor);
                    cumCurve.Symbol.Size = 7;
                }

                zedGraphControl.AxisChange();
                zedGraphControl.Invalidate();
            }
        }
        private void DrawPointsDBScan(List<Diem> dataPointsDB, int[] labels)
        {
            GraphPane myPane = zedGraphControl.GraphPane;
            myPane.Title.Text = "DBScan Clustering";
            myPane.XAxis.Title.Text = "X";
            myPane.YAxis.Title.Text = "Y";

            myPane.CurveList.Clear();

            var clusterPoints = new Dictionary<int, PointPairList>();
            var unclassifiedPoints = new PointPairList(); // Danh sách các điểm chưa phân loại
            var noisePoints = new PointPairList(); // Danh sách các điểm nhiễu

            for (int i = 0; i < dataPointsDB.Count; i++)
            {
                int label = labels[i];
                if (label == UNCLASSIFIED)
                {
                    unclassifiedPoints.Add(dataPointsDB[i].X, dataPointsDB[i].Y);
                }
                else if (label == NOISE)
                {
                    noisePoints.Add(dataPointsDB[i].X, dataPointsDB[i].Y);
                }
                else
                {
                    if (!clusterPoints.ContainsKey(label))
                    {
                        clusterPoints[label] = new PointPairList();
                    }
                    clusterPoints[label].Add(dataPointsDB[i].X, dataPointsDB[i].Y);
                }
            }

            // Vẽ các cụm đã phân loại
            foreach (var entry in clusterPoints)
            {
                int label = entry.Key ;
                PointPairList pointList = entry.Value;
                // Sử dụng mảng màu
                Color color = colors[(label-1) % colors.Length];
                string curveName = $"Cụm {label}";
                LineItem myCurve = myPane.AddCurve(curveName, pointList, color, SymbolType.Circle);
                myCurve.Symbol.Fill = new Fill(color);
                myCurve.Line.IsVisible = false;
            }

            // Vẽ các điểm chưa phân loại
            if (unclassifiedPoints.Count > 0)
            {
                Color unclassifiedColor = Color.Gray; // Màu cho điểm chưa phân loại
                var unclassifiedCurve = myPane.AddCurve("Điểm chưa phân loại", unclassifiedPoints, unclassifiedColor, SymbolType.Circle);
                unclassifiedCurve.Symbol.Fill = new Fill(unclassifiedColor);
                unclassifiedCurve.Line.IsVisible = false;
            }

            // Vẽ các điểm nhiễu
            if (noisePoints.Count > 0)
            {
                Color noiseColor = Color.Black; // Màu cho điểm nhiễu
                var noiseCurve = myPane.AddCurve("Điểm nhiễu", noisePoints, noiseColor, SymbolType.Circle);
                noiseCurve.Symbol.Fill = new Fill(noiseColor);
                noiseCurve.Line.IsVisible = false;
            }

            zedGraphControl.AxisChange();
            zedGraphControl.Invalidate();
        }     
        private void DrawPointsMeanShift(Dictionary<Diem, List<Diem>> clusters)
        {
            GraphPane pane = zedGraphControl.GraphPane;
            pane.CurveList.Clear();
            pane.Title.Text = "Mean-Shift Clustering";
            pane.XAxis.Title.Text = "X";
            pane.YAxis.Title.Text = "Y";

            int clusterIndex = 0;

            foreach (var cluster in clusters)
            {
                // Sử dụng màu từ mảng màu đã định nghĩa
                Color clusterColor = colors[clusterIndex % colors.Length];
                var dataList = new PointPairList();

                foreach (var point in cluster.Value)
                {
                    dataList.Add(point.X, point.Y);
                }

                var dataCurve = pane.AddCurve($"Cụm {clusterIndex + 1}", dataList, clusterColor, SymbolType.Circle);
                dataCurve.Line.IsVisible = false;
                dataCurve.Symbol.Fill = new Fill(clusterColor);
                dataCurve.Symbol.Size = 7;

                clusterIndex++;
            }

            zedGraphControl.AxisChange();
            zedGraphControl.Invalidate();
        }
        private void ZedGraphControl_MouseMove(object sender, MouseEventArgs e)
        {
            GraphPane myPane = zedGraphControl.GraphPane;
            Diem mousePt = new Diem(e.X, e.Y);

            // Tìm điểm gần nhất với vị trí con trỏ chuột
            double x, y;
            myPane.ReverseTransform(new PointF((float)mousePt.X, (float)mousePt.Y), out x, out y);
            CurveItem nearestCurve;
            int index;
            if (myPane.FindNearestPoint(new PointF((float)mousePt.X, (float)mousePt.Y), out nearestCurve, out index))
            {
                PointPair nearestPoint = nearestCurve[index];
                string tooltipText = string.Format("X: {0}, Y: {1}", nearestPoint.X, nearestPoint.Y);
                tooltip.SetToolTip(zedGraphControl, tooltipText);
            }
            else
            {
                tooltip.SetToolTip(zedGraphControl, string.Empty);
            }
        }
    }
}
