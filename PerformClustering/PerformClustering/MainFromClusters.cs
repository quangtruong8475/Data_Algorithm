
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;

namespace PerformClustering
{
    public partial class MainFromClusters : Form
    {
        /// <summary>
        /// private of KMean
        /// </summary>
        Color[] colors;
        private int countStep = 1;
        private List<Diem> diemDuLieu = new List<Diem>();
        private List<Cum> cumKetQua = new List<Cum>();
        private DrawPoints drawPoints;
        private bool thayDoi;
        private int iteration;
        private bool isKMeansInitialized = false;
        private System.Windows.Forms.ToolTip tooltip = new System.Windows.Forms.ToolTip();
        /// <summary>
        /// private of Mean-Shifts
        /// </summary>
        /// 
       // private DrawPoints drawPoints;
        private List<Diem> dataPoints = new List<Diem>();
        private List<Diem> clustersCenterOld = new List<Diem>();
        private List<Diem> clustersCenterNew = new List<Diem>();
        private Dictionary<Diem, int> originalIndices = new Dictionary<Diem, int>();
        private bool thayDoiMS = false;
        private bool isMeanShiftInitialized = false;
        // private System.Windows.Forms.ToolTip tooltip;
        /// <summary>
        /// private of DBScan
        /// </summary>
       // private DrawPoints drawPoints;
        private List<Diem> dataPointsDB = new List<Diem>();
        private int[] labels;
        private const int UNCLASSIFIED = 0;
        private const int NOISE = -1;
        private int clusterId = 0;
        private int currentPointIndex = 0;
        public MainFromClusters()
        {
            InitializeComponent();
        }
        /// <summary>
        /// code public
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainFromClusters_Load(object sender, EventArgs e)
        {
            comboxChoose.SelectedIndex = 0;
            rtbMoTa.SelectionStart = rtbMoTa.Text.Length;
            rtbMoTa.ScrollToCaret();

            //
            dgvData.Columns.Add("STT", "STT");
            dgvData.Columns.Add("X", "X");
            dgvData.Columns.Add("Y", "Y");
            dgvData.Columns["STT"].Width = 50;
            //
            dgvResult.Columns.Add("stt", "STT");
            dgvResult.Columns.Add("x", "X");
            dgvResult.Columns.Add("y", "Y");
            dgvResult.Columns.Add("phancap", "Phân loại");
            //
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
        private void btnChooseFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Chọn File Dữ Liệu",
                Filter = "Tệp Tin Text|*.txt"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string duongDan = openFileDialog.FileName;
                diemDuLieu = DocDuLieu(duongDan);
                dataPoints = diemDuLieu;
                InitializeOriginalIndices(dataPoints);
                dataPointsDB = diemDuLieu;
                if (diemDuLieu != null)
                {
                    dgvData.Rows.Clear();
                    DrawInitialPoints(diemDuLieu);
                    labels = new int[dataPointsDB.Count];
                    for (int i = 0; i < labels.Length; i++)
                    {
                        labels[i] = UNCLASSIFIED;
                    }
                    int stt = 1;
                    foreach (var diem in diemDuLieu)
                    {
                        dgvData.Rows.Add(stt++, diem.X, diem.Y);
                    }
                    SetUpZedGraph(diemDuLieu);
                    MessageBox.Show($"Đọc thành công {diemDuLieu.Count} điểm dữ liệu từ {duongDan}");
                }
                else
                {
                    MessageBox.Show("Không thể đọc dữ liệu từ tệp tin.");
                }
            }
        }
        private List<Diem> DocDuLieu(string filePath)
        {
            diemDuLieu.Clear();
            var lines = File.ReadAllLines(filePath);
            var diemSet = new HashSet<(double, double)>();

            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length == 2 && double.TryParse(parts[0], out double x) && double.TryParse(parts[1], out double y))
                {
                    var diem = (x, y);
                    if (!diemSet.Contains(diem))
                    {
                        diemDuLieu.Add(new Diem(x, y));
                        diemSet.Add(diem);
                    }
                }
            }
            return diemDuLieu;
        }
        private double KhoangCach(Diem a, Diem b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }
        private void btnReset_Click(object sender, EventArgs e)
        {
            countStep = 1;
            txtBanKinh.Text = "";
            txtK.Text = "";
            txtMinPoints.Text = "";
            rtbMoTa.Clear();
            txtK.Text = "";
            dgvResult.Rows.Clear();
            cbRunStep.Checked = false;
            clusterId = 0;
            currentPointIndex = 0;
            labels = new int[dataPointsDB.Count];
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i] = UNCLASSIFIED;
            }
            DrawZepReset(diemDuLieu);
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
        private void DrawZepReset(List<Diem> diemList)
        {
            zedGraphControl.GraphPane.CurveList.Clear();
            zedGraphControl.GraphPane.GraphObjList.Clear();
            zedGraphControl.Invalidate();
            SetUpZedGraph(diemList);
            DrawInitialPoints(diemList);
        }
        private void iMenuExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                saveFileDialog.DefaultExt = "txt";
                saveFileDialog.AddExtension = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;
                    try
                    {
                        using (StreamWriter writer = new StreamWriter(filePath))
                        {
                            foreach (var point in diemDuLieu)
                            {
                                writer.WriteLine($"{point.X},{point.Y}");
                            }
                        }
                        MessageBox.Show("Xuất file thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Có lỗi xảy ra khi xuất file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void btnVeDiem_Click(object sender, EventArgs e)
        {
            drawPoints = new DrawPoints(diemDuLieu);
            drawPoints.FormClosed += new FormClosedEventHandler(DrawPoints_FormClosed);
            drawPoints.Show();
        }

        private void DrawPoints_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (drawPoints.diemList != null && drawPoints.diemList.Count > 0)
            {
                dgvData.Rows.Clear();
                var diemSet = new HashSet<(double, double)>();
                var uniqueDiemList = new List<Diem>();

                foreach (var diem in drawPoints.diemList)
                {
                    var diemTuple = (diem.X, diem.Y);
                    if (!diemSet.Contains(diemTuple))
                    {
                        uniqueDiemList.Add(diem);
                        diemSet.Add(diemTuple);
                    }
                }
                diemDuLieu = uniqueDiemList;
                dataPoints = uniqueDiemList;
                dataPointsDB = uniqueDiemList;
                InitializeOriginalIndices(dataPoints);
                DrawInitialPoints(diemDuLieu);
                labels = new int[dataPointsDB.Count];
                for (int i = 0; i < labels.Length; i++)
                {
                    labels[i] = UNCLASSIFIED;
                }
                int stt = 1;
                foreach (var diem in diemDuLieu)
                {
                    dgvData.Rows.Add(stt++, diem.X, diem.Y);
                }
                SetUpZedGraph(diemDuLieu);
            }
        }
        private void DrawInitialPoints(List<Diem> points)
        {
            GraphPane pane = zedGraphControl.GraphPane;
            pane.CurveList.Clear();
            pane.Title.Text = "Đồ thị điểm dữ liệu";
            pane.XAxis.Title.Text = "X";
            pane.YAxis.Title.Text = "Y";

            var pointList = new PointPairList();
            foreach (var point in points)
            {
                pointList.Add(point.X, point.Y);
            }

            var myCurve = pane.AddCurve("Điểm dữ liệu", pointList, Color.Black, SymbolType.Circle);
            myCurve.Symbol.Fill = new Fill(Color.Black);
            myCurve.Line.IsVisible = false;

            zedGraphControl.AxisChange();
            zedGraphControl.Invalidate();
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
            GraphPane graphPane = zedGraphControl.GraphPane;
            LineObj trucTung = new LineObj(
                Color.Black,    // Màu của đường
                0, graphPane.YAxis.Scale.Min,  
                0, graphPane.YAxis.Scale.Max    
            );

            trucTung.Line.Style = System.Drawing.Drawing2D.DashStyle.Solid; // Kiểu đường nét liền
            trucTung.Line.Width = 1.0f; // Độ dày của đường
            graphPane.GraphObjList.Add(trucTung);
            zedGraphControl.AxisChange();
            zedGraphControl.Invalidate();
        }


        /// <summary>
        /// code of KMean
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        public void KhoiTaoKMeans(List<Diem> diemDuLieu, int k)
        {
            rtbMoTa.Clear();
            rtbMoTa.AppendText("Bắt đầu thuật toán K-Means\n");

            this.diemDuLieu = diemDuLieu;
            cumKetQua = new List<Cum>();
            var random = new Random();
            HashSet<int> selectedIndices = new HashSet<int>();

            while (cumKetQua.Count < k)
            {
                int index = random.Next(diemDuLieu.Count);
                if (!selectedIndices.Contains(index))
                {
                    var diemNgauNhien = diemDuLieu[index];
                    cumKetQua.Add(new Cum(diemNgauNhien));
                    selectedIndices.Add(index);
                    rtbMoTa.AppendText($"Khởi tạo trung tâm cụm {cumKetQua.Count}: ({diemNgauNhien.X:F2}, {diemNgauNhien.Y:F2})\n");
                }
            }

            thayDoi = true;
            iteration = 0;
        }
        public void KMeansStep()
        {
            if (!thayDoi)
            {
                rtbMoTa.AppendText("\nThuật toán K-Means hoàn thành\n");
                return;
            }

            iteration++;
            rtbMoTa.AppendText($"\nLặp lại lần thứ {iteration}:\n");
            thayDoi = false;  // Đặt lại thayDoi thành false trước khi tính toán

            if (iteration == 1)
            {
                thayDoi = true;
            }
            else
            {
                TinhDiemTrungTam();
            }

            foreach (var c in cumKetQua) c.DiemCum.Clear();

            foreach (var diem in diemDuLieu)
            {
                var cumGanNhat = cumKetQua.OrderBy(c => KhoangCach(diem, c.TrungTam)).First();
                cumGanNhat.DiemCum.Add(diem);
            }

            rtbMoTa.AppendText("Thông tin các điểm và khoảng cách đến các trung tâm:\n");
            rtbMoTa.AppendText("Điểm".PadRight(20));
            for (int i = 0; i < cumKetQua.Count; i++)
            {
                rtbMoTa.AppendText($"Cụm {i + 1}".PadRight(18));
            }
            rtbMoTa.AppendText("\n");

            foreach (var diem in diemDuLieu)
            {
                string diemStr = $"({diem.X:F2}, {diem.Y:F2})";
                int padRight = SetPadRight(diemStr);
                rtbMoTa.AppendText(diemStr.PadRight(padRight));

                for (int i = 0; i < cumKetQua.Count; i++)
                {
                    double khoangCach = KhoangCach(diem, cumKetQua[i].TrungTam);
                    rtbMoTa.AppendText(khoangCach.ToString("F2").PadRight(khoangCach >= 10 ? 19 : 20));
                }
                rtbMoTa.AppendText("\n");
            }

            rtbMoTa.AppendText("\nĐiểm thuộc các cụm trước khi cập nhật trung tâm:\n");
            for (int i = 0; i < cumKetQua.Count; i++)
            {
                var c = cumKetQua[i];
                rtbMoTa.AppendText($"Cụm {i + 1} với trung tâm hiện tại ({c.TrungTam.X:F2}, {c.TrungTam.Y:F2}):\n");
                foreach (var diem in c.DiemCum)
                {
                    rtbMoTa.AppendText($"\tĐiểm ({diem.X:F2}, {diem.Y:F2})\n");
                }
            }
        }

        private void TinhDiemTrungTam()
        {
            rtbMoTa.AppendText("\nCập nhật trung tâm cụm:\n");
            for (int i = 0; i < cumKetQua.Count; i++)
            {
                var c = cumKetQua[i];
                if (c.DiemCum.Any())
                {
                    var trungTamCu = c.TrungTam;
                    var trungTamMoi = new Diem(c.DiemCum.Average(d => d.X), c.DiemCum.Average(d => d.Y));
                    if (Math.Abs(trungTamMoi.X - trungTamCu.X) > 0.01 || Math.Abs(trungTamMoi.Y - trungTamCu.Y) > 0.01)
                    {
                        thayDoi = true;  // Đặt thayDoi thành true khi có sự thay đổi đáng kể
                        c.TrungTam = trungTamMoi;
                        rtbMoTa.AppendText($"Cụm {i + 1} cập nhật: Trung tâm mới ({trungTamMoi.X:F2}, {trungTamMoi.Y:F2})\n");
                    }
                }
            }
        }

        private List<Cum> KMeans(List<Diem> diemDuLieu, int k)
        {
            rtbMoTa.Clear();
            rtbMoTa.AppendText("Bắt đầu thuật toán K-Means\n");

            var cum = new List<Cum>();
            var random = new Random();
            HashSet<int> selectedIndices = new HashSet<int>();

            while (cum.Count < k)
            {
                int index = random.Next(diemDuLieu.Count);
                if (!selectedIndices.Contains(index))
                {
                    var diemNgauNhien = diemDuLieu[index];
                    cum.Add(new Cum(diemNgauNhien));
                    selectedIndices.Add(index);
                    rtbMoTa.AppendText($"Khởi tạo trung tâm cụm {cum.Count}: ({diemNgauNhien.X:F2}, {diemNgauNhien.Y:F2})\n");
                }
            }

            bool thayDoi;
            int iteration = 0;
            do
            {
                iteration++;
                rtbMoTa.AppendText($"\nLặp lại lần thứ {iteration}:\n");

                foreach (var c in cum) c.DiemCum.Clear();

                foreach (var diem in diemDuLieu)
                {
                    var cumGanNhat = cum.OrderBy(c => KhoangCach(diem, c.TrungTam)).First();
                    cumGanNhat.DiemCum.Add(diem);
                }

                rtbMoTa.AppendText("Thông tin các điểm và khoảng cách đến các trung tâm:\n");
                rtbMoTa.AppendText("Điểm".PadRight(20));
                for (int i = 0; i < k; i++)
                {
                    rtbMoTa.AppendText($"Cụm {i + 1}".PadRight(18));
                }
                rtbMoTa.AppendText("\n");

                foreach (var diem in diemDuLieu)
                {
                    string diemStr = $"({diem.X:F2}, {diem.Y:F2})";
                    int padRight = SetPadRight(diemStr);
                    rtbMoTa.AppendText(diemStr.PadRight(padRight));

                    for (int i = 0; i < k; i++)
                    {
                        double khoangCach = KhoangCach(diem, cum[i].TrungTam);
                        rtbMoTa.AppendText(khoangCach.ToString("F2").PadRight(khoangCach >= 10 ? 19 : 20));
                    }
                    rtbMoTa.AppendText("\n");
                }

                thayDoi = false;

                rtbMoTa.AppendText("\nĐiểm thuộc các cụm trước khi cập nhật trung tâm:\n");
                for (int i = 0; i < cum.Count; i++)
                {
                    var c = cum[i];
                    rtbMoTa.AppendText($"Cụm {i + 1} với trung tâm hiện tại ({c.TrungTam.X:F2}, {c.TrungTam.Y:F2}):\n");
                    foreach (var diem in c.DiemCum)
                    {
                        rtbMoTa.AppendText($"\tĐiểm ({diem.X:F2}, {diem.Y:F2})\n");
                    }
                }

                rtbMoTa.AppendText("\nCập nhật trung tâm cụm:\n");
                for (int i = 0; i < cum.Count; i++)
                {
                    var c = cum[i];
                    if (c.DiemCum.Any())
                    {
                        var trungTamCu = c.TrungTam;
                        var trungTamMoi = new Diem(c.DiemCum.Average(d => d.X), c.DiemCum.Average(d => d.Y));
                        if (Math.Abs(trungTamMoi.X - trungTamCu.X) > 0.01 || Math.Abs(trungTamMoi.Y - trungTamCu.Y) > 0.01)
                        {
                            thayDoi = true;
                            c.TrungTam = trungTamMoi;
                            rtbMoTa.AppendText($"Cụm {i + 1} cập nhật: Trung tâm mới ({trungTamMoi.X:F2}, {trungTamMoi.Y:F2})\n");
                        }
                    }
                }
            } while (thayDoi);

            rtbMoTa.AppendText("\nThuật toán K-Means hoàn thành\n");
            return cum;
        }
        private void DisplayResult(List<Cum> result)
        {
            dgvResult.Rows.Clear(); // Xóa các hàng cũ nếu có

            int stt = 1;
            foreach (var c in result)
            {
                foreach (var diem in c.DiemCum)
                {
                    dgvResult.Rows.Add(stt++, diem.X, diem.Y, $"Cụm {result.IndexOf(c) + 1}");
                }
            }
        }
        private int SetPadRight(string s)
        {
            int lenght = s.Length;
            if (lenght == 12)
            {
                return 20;
            }
            else if (lenght == 13)
            {
                return 19;
            }
            else if (lenght == 14)
            {
                return 18;
            }
            return 0;
        }
        private void DrawPointsKMean(List<Cum> cumKetQua, int count)
        {
            if (cumKetQua != null)
            {
                GraphPane myPane = zedGraphControl.GraphPane;
                myPane.Title.Text = "Biểu đồ phân cụm lần "+ count.ToString();
                myPane.XAxis.Title.Text = "X";
                myPane.YAxis.Title.Text = "Y"; 
                myPane.CurveList.Clear();

                // Mảng màu cố định với 50 màu đã được tạo trước đó              
                for (int i = 0; i < cumKetQua.Count; i++)
                {
                    var cum = cumKetQua[i];
                    PointPairList cumList = new PointPairList();
                    foreach (var diem in cum.DiemCum)
                    {
                        cumList.Add(diem.X, diem.Y);
                    }

                    // Sử dụng màu theo thứ tự từ mảng màu
                    Color clusterColor = colors[i % colors.Length];
                    LineItem cumCurve = myPane.AddCurve($"Cụm {i + 1}", cumList, clusterColor, SymbolType.Circle);
                    cumCurve.Line.IsVisible = false;
                    cumCurve.Symbol.Fill = new Fill(clusterColor);
                    cumCurve.Symbol.Size = 7;
                }

                zedGraphControl.AxisChange();
                zedGraphControl.Invalidate();
            }
        }
        //KMean mở rộng
        public List<Diem> KhoiTaoTamCumNgauNhien(List<Diem> diemList, int k)
        {
            Random rand = new Random();
            List<Diem> centers = new List<Diem>();
            HashSet<int> usedIndices = new HashSet<int>();

            for (int i = 0; i < k; i++)
            {
                int index;
                do
                {
                    index = rand.Next(diemList.Count);
                } while (usedIndices.Contains(index));

                usedIndices.Add(index);
                centers.Add(diemList[index]);
            }

            return centers;
        }

        // Tính tổng khoảng cách bình phương từ các điểm đến tâm cụm
        public double TinhTongKhoangCachBinhPhuong(List<Cum> clusters)
        {
            double tongKC = 0;

            foreach (var cluster in clusters)
            {
                Diem tamCum = cluster.TrungTam;
                foreach (var diem in cluster.DiemCum)
                {
                    double kc = Math.Pow(diem.X - tamCum.X, 2) + Math.Pow(diem.Y - tamCum.Y, 2);
                    tongKC += kc;
                }
            }

            return tongKC;
        }

        // Thực hiện K-Means và chọn K tối ưu
        public int ChonKToiUu(List<Diem> diemList, double threshold , int maxK )
        {
            rtbMoTa.Clear();
            List<double> tongKCList = new List<double>();
            List<double> deltaList = new List<double>();
            int kToiUu = 2;

        
            for (int k = 2; k <= maxK; k++)
            {
               
                var clusters = KMeansClustering(diemList, k);
                double tongKC = TinhTongKhoangCachBinhPhuong(clusters);
                tongKCList.Add(tongKC);

            
                rtbMoTa.AppendText($"K = {k}\n\tTổng bình phương khoảng cách = {tongKC}\n");

                
                if (k > 2)
                {
                    double delta = tongKCList[k - 3] - tongKCList[k - 2];
                    double phanTramChenhLech = delta / tongKCList[k - 3];
                    deltaList.Add(delta);

                    rtbMoTa.AppendText($"\tDelta = {tongKCList[k - 3]} - {tongKCList[k - 2]} = {delta}  \n\tPhần trăm chênh lệch = {delta} / {tongKCList[k - 3]} = {phanTramChenhLech * 100}%\n");
                                      
                    if (phanTramChenhLech <= threshold)
                    {
                        if (phanTramChenhLech < 0)
                        {
                            kToiUu = k - 1;
                            rtbMoTa.AppendText($"K tối ưu đã tìm thấy: {kToiUu}\n");
                            break;
                        }
                        kToiUu = k;
                        rtbMoTa.AppendText($"K tối ưu đã tìm thấy: {kToiUu}\n");
                        break;
                    }
                }
            }
            return kToiUu;
        }



        // Thực hiện thuật toán K-Means
        public List<Cum> KMeansClustering(List<Diem> diemList,int k)
        {
            var cum = new List<Cum>();
            var random = new Random();
            HashSet<int> selectedIndices = new HashSet<int>();

            // Chọn k điểm ngẫu nhiên để khởi tạo trung tâm cụm
            while (cum.Count < k)
            {
                int index = random.Next(diemDuLieu.Count);
                if (!selectedIndices.Contains(index))
                {
                    var diemNgauNhien = diemList[index];
                    cum.Add(new Cum(diemNgauNhien));
                    selectedIndices.Add(index);
                }
            }

            bool isChange;
            do
            {           
                foreach (var c in cum) c.DiemCum.Clear();             
                foreach (var diem in diemList)
                {
                    var cumGanNhat = cum.OrderBy(c => KhoangCach(diem, c.TrungTam)).First();
                    cumGanNhat.DiemCum.Add(diem);
                }

                isChange = false;             
                for (int i = 0; i < cum.Count; i++)
                {
                    var c = cum[i];
                    if (c.DiemCum.Any())
                    {
                        var trungTamCu = c.TrungTam;
                        var trungTamMoi = new Diem(c.DiemCum.Average(d => d.X), c.DiemCum.Average(d => d.Y));                      
                        if (Math.Abs(trungTamMoi.X - trungTamCu.X) > 0.01 || Math.Abs(trungTamMoi.Y - trungTamCu.Y) > 0.01)
                        {
                            isChange = true;
                            c.TrungTam = trungTamMoi;
                        }
                    }
                }
            } while (isChange);

            return cum;
        }


        /// <summary>
        /// code of Mean-Shifts
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InitializeOriginalIndices(List<Diem> points)
        {
            originalIndices = points
                .Distinct()
                .Select((point, index) => new { point, index = index + 1 })
                .ToDictionary(x => x.point, x => x.index);
        }

        private List<Diem> MeanShiftClustering(float bandwidth)
        {
            List<Diem> clusterCenters = new List<Diem>(dataPoints);
            bool hasConverged = false;

            while (!hasConverged)
            {
                hasConverged = true;
                var newCenters = new List<Diem>();
                var clusters = clusterCenters.ToDictionary(center => center, center => new List<Diem>());

                foreach (var center in clusterCenters)
                {
                    var nearbyPoints = GetNearbyPoints(center, bandwidth);

                    if (nearbyPoints.Count == 0) continue;

                    var newCenter = new Diem(
                        Math.Round(nearbyPoints.Average(p => p.X), 2),
                        Math.Round(nearbyPoints.Average(p => p.Y), 2)
                    );

                    if (Distance(center, newCenter) > 1e-3)
                    {
                        hasConverged = false;
                    }

                    newCenters.Add(newCenter);
                }

                clusterCenters = newCenters
                    .Distinct(new DiemEqualityComparer())
                    .ToList();
            }

            return clusterCenters;
        }

        private List<Diem> GetNearbyPoints(Diem center, float radius)
        {
            return dataPoints.Where(p => Distance(p, center) <= radius).ToList();
        }

        private float Distance(Diem p1, Diem p2)
        {
            return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        private Dictionary<Diem, List<Diem>> AssignClusters(List<Diem> clusterCenters)
        {
            var clusters = clusterCenters.ToDictionary(center => center, center => new List<Diem>());

            foreach (var point in dataPoints)
            {
                var closestCenter = clusterCenters
                    .OrderBy(c => Distance(c, point))
                    .First();

                clusters[closestCenter].Add(point);
            }

            return clusters;
        }
        private List<Diem> DisplayMeanShiftStep(List<Diem> clusterC, float bandwidth)
        {


            List<Diem> clusterCenter = clusterC;
            rtbMoTa.AppendText("====================Danh sách các điểm trung tâm hiện tại========================= :\n");
            int countClusterCenter = 1;
            foreach (var point in clusterCenter)
            {
                rtbMoTa.AppendText($"[{countClusterCenter++}]. ({point.X:F2}, {point.Y:F2})\n");
            }
            rtbMoTa.AppendText("\n");

            var newCenters = new List<Diem>();
            rtbMoTa.AppendText($"Liệt kê các điểm gần kề (trong phạm vi bán kính {bandwidth:F2}):\n");
            int nearByPointIndex = 1;
            foreach (var center in clusterCenter)
            {
                var nearbyPoints = GetNearbyPoints(center, bandwidth);
                rtbMoTa.AppendText($"\n[{nearByPointIndex++}]. Điểm trung tâm ({center.X:F2}, {center.Y:F2}):\n");

                if (nearbyPoints.Count == 0)
                {
                    rtbMoTa.AppendText("Không có điểm gần kề.\n\n");
                    newCenters.Add(center); // Giữ nguyên điểm trung tâm nếu không có điểm gần kề
                    continue;
                }

                foreach (var point in nearbyPoints)
                {
                    rtbMoTa.AppendText($" - Điểm ({point.X:F2}, {point.Y:F2}): Bán kính = {Distance(center, point):F2}\n");
                }

                var newCenter = new Diem(
                    Math.Round(nearbyPoints.Average(p => p.X), 2),
                    Math.Round(nearbyPoints.Average(p => p.Y), 2)
                );
                newCenters.Add(newCenter);
                if (Distance(center, newCenter) > 1e-3)
                {
                    thayDoiMS = true;
                }
            }
            rtbMoTa.AppendText("\nTính lại các điểm trung tâm:\n");
            int pointCenterIndex = 1;
            foreach (var center in newCenters)
            {
                rtbMoTa.AppendText($"[{pointCenterIndex++}]. ({center.X:F2}, {center.Y:F2})\n");
            }
            clusterCenter = newCenters
                .Distinct(new DiemEqualityComparer())
                .ToList();
            rtbMoTa.AppendText("\nDanh sách sau khi loại bỏ các điểm trung tâm trùng nhau:\n");
            pointCenterIndex = 1;
            foreach (var center in clusterCenter)
            {
                rtbMoTa.AppendText($"[{pointCenterIndex++}]. ({center.X:F2}, {center.Y:F2})\n");
            }
            rtbMoTa.AppendText("\n");
            //Kết quả
            Dictionary<Diem, List<Diem>> clusters = AssignClusters(clusterCenter);
            rtbMoTa.AppendText("\n======Kết quả gom cụm========\n");
            int clusterIndex = 1;
            foreach (var cluster in clusters)
            {
                var center = cluster.Key;
                var points = cluster.Value;

                rtbMoTa.AppendText($"Cụm {clusterIndex++} (Trung tâm: ({center.X:F2}, {center.Y:F2})):\n");

                foreach (var point in points)
                {
                    rtbMoTa.AppendText($" - Điểm ({point.X:F2}, {point.Y:F2})\n");
                }

                rtbMoTa.AppendText("\n");
            }
            return clusterCenter;
        }

        private void DisplayMeanShift(float bandwidth)
        {
            List<Diem> clusterCenters = new List<Diem>(dataPoints);
            bool hasConverged = false;
            int count = 1;
            rtbMoTa.Clear();

            rtbMoTa.AppendText("\t\t\tDanh sách các điểm dữ liệu :\n");
            foreach (var point in clusterCenters)
            {
                rtbMoTa.AppendText($"[{originalIndices[point]}]. ({point.X:F2}, {point.Y:F2})\n");
            }
            rtbMoTa.AppendText("\n");
            while (!hasConverged)
            {
                int countClusterCenter = 1;
                rtbMoTa.AppendText(string.Format("LẦN LẶP THỨ  {0}", count++) + "\n");
                rtbMoTa.AppendText("==>Danh sách các điểm trung tâm :\n");
                foreach (var point in clusterCenters)
                {
                    rtbMoTa.AppendText($"[{countClusterCenter++}]. ({point.X:F2}, {point.Y:F2})\n");
                }
                hasConverged = true;
                var newCenters = new List<Diem>();
                var clusters = clusterCenters.ToDictionary(center => center, center => new List<Diem>());

                rtbMoTa.AppendText($"\n==>Điểm trung tâm hiện tại và các điểm gần kề (trong phạm vi bán kính {bandwidth:F2}):\n");
                int nearByPointIndex = 1;

                foreach (var center in clusterCenters)
                {
                    var nearbyPoints = GetNearbyPoints(center, bandwidth);
                    rtbMoTa.AppendText($"\n[{nearByPointIndex++}]. Điểm trung tâm ({center.X:F2}, {center.Y:F2}):\n");

                    if (nearbyPoints.Count == 0)
                    {
                        rtbMoTa.AppendText("Không có điểm gần kề.\n\n");
                        continue;
                    }

                    foreach (var point in nearbyPoints)
                    {
                        rtbMoTa.AppendText($" - {originalIndices[point]}. Điểm ({point.X:F2}, {point.Y:F2}): Bán kính = {Distance(center, point):F2}\n");
                    }

                    var newCenter = new Diem(
                        Math.Round(nearbyPoints.Average(p => p.X), 2),
                        Math.Round(nearbyPoints.Average(p => p.Y), 2)
                    );

                    if (Distance(center, newCenter) > 1e-3)
                    {
                        hasConverged = false;
                    }

                    newCenters.Add(newCenter);
                }

                rtbMoTa.AppendText("\n==>Tính lại các điểm trung tâm:\n");
                int pointCenterIndex = 1;
                foreach (var center in newCenters)
                {
                    rtbMoTa.AppendText($"[{pointCenterIndex++}]. ({center.X:F2}, {center.Y:F2})\n");
                }

                clusterCenters = newCenters
                    .Distinct(new DiemEqualityComparer())
                    .ToList();

                if (hasConverged)
                {
                    rtbMoTa.AppendText("\n==CÁC ĐIỂM TRUNG TÂM KHÔNG THAY ĐỔI => THUẬT TOÁN KẾT THÚC===\n");
                }
                else
                {
                    pointCenterIndex = 1;
                    rtbMoTa.AppendText("\n==>Danh sách sau khi loại bỏ các điểm trung tâm trùng nhau:\n");
                    foreach (var center in clusterCenters)
                    {
                        rtbMoTa.AppendText($"[{pointCenterIndex++}]. ({center.X:F2}, {center.Y:F2})\n");
                    }
                    rtbMoTa.AppendText("\n");
                }
            }

            rtbMoTa.AppendText($"==>Danh sách các điểm gần kề trong bán kính {bandwidth}:\n");
            int pointIndex = 1;
            foreach (var center in clusterCenters)
            {
                var nearbyPoints = GetNearbyPoints(center, bandwidth);
                rtbMoTa.AppendText($"[{pointIndex++}]. Điểm trung tâm ({center.X:F2}, {center.Y:F2}):\n");

                if (nearbyPoints.Count == 0)
                {
                    rtbMoTa.AppendText("==>Không có điểm gần kề.\n");
                    continue;
                }

                foreach (var point in nearbyPoints)
                {
                    rtbMoTa.AppendText($" - {originalIndices[point]}. Điểm ({point.X:F2}, {point.Y:F2}): Bán kính = {Distance(center, point):F2}\n");
                }
            }
        }

        private void DisplayClusters(Dictionary<Diem, List<Diem>> clusters, float badwidth)
        {
            DisplayMeanShift(badwidth);
            rtbMoTa.AppendText("\n======Kết quả gom cụm========\n");
            int clusterIndex = 1;

            foreach (var cluster in clusters)
            {
                var center = cluster.Key;
                var points = cluster.Value;

                rtbMoTa.AppendText($"Cụm {clusterIndex++} (Trung tâm: ({center.X:F2}, {center.Y:F2})):\n");

                foreach (var point in points)
                {
                    rtbMoTa.AppendText($" - Điểm ({point.X:F2}, {point.Y:F2})\n");
                }

                rtbMoTa.AppendText("\n");
            }
        }
        private void DrawPointsMeanShift(Dictionary<Diem, List<Diem>> clusters)
        {
            GraphPane pane = zedGraphControl.GraphPane;
            pane.CurveList.Clear();
            pane.Title.Text = "Mean-Shift Clustering lần "+countStep.ToString();
            pane.XAxis.Title.Text = "X";
            pane.YAxis.Title.Text = "Y";
            int clusterIndex = 0;

            foreach (var cluster in clusters)
            {
                // Lấy màu từ mảng màu đã định nghĩa
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

        private void DisplayResult(Dictionary<Diem, List<Diem>> clusters)
        {

            dgvResult.Rows.Clear();

            int stt = 1;

            int clusterIndex = 1;

            foreach (var cluster in clusters)
            {
                string clusterName = $"Cụm {clusterIndex++}";
                List<Diem> points = cluster.Value;

                foreach (var point in points)
                {
                    dgvResult.Rows.Add(stt++, point.X, point.Y, clusterName);
                }
            }
        }

        private class DiemEqualityComparer : IEqualityComparer<Diem>
        {
            public bool Equals(Diem p1, Diem p2)
            {
                return Math.Abs(p1.X - p2.X) < 1e-3 && Math.Abs(p1.Y - p2.Y) < 1e-3;
            }

            public int GetHashCode(Diem p)
            {
                return p.X.GetHashCode() ^ p.Y.GetHashCode();
            }
        }
        /// <summary>
        /// code of DBScan
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
       
        private void DisplayRunDBScan(float epsilon, int minPts)
        {
            labels = new int[dataPointsDB.Count];
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i] = UNCLASSIFIED;
            }

            int clusterId = 0;

            for (int i = 0; i < dataPointsDB.Count; i++)
            {
                if (labels[i] == UNCLASSIFIED)
                {
                    List<int> seeds = RegionQuery(i, epsilon);

                    // Hiển thị thông tin về điểm đang được xử lý
                    rtbMoTa.AppendText(string.Format("===Xử lý điểm ({0:F2}, {1:F2})===\n",
                        Math.Round(dataPointsDB[i].X, 2),
                        Math.Round(dataPointsDB[i].Y, 2)));

                    if (seeds.Count < minPts)
                    {
                        labels[i] = NOISE;
                        rtbMoTa.AppendText(string.Format("Số lượng điểm lân cận là {0} - Điểm này bị phân loại là nhiễu (NOISE).\n\n", seeds.Count));
                    }
                    else
                    {
                        rtbMoTa.AppendText(string.Format("Số lượng điểm lân cận là {0} - Điểm này là trung tâm của một cụm mới (Cụm {1}).\n", seeds.Count, clusterId + 1));
                        if (DisplayExpandCluster(i, clusterId + 1, epsilon, minPts))
                        {
                            clusterId++;
                        }
                        rtbMoTa.AppendText("\n");
                    }
                }
                else
                {
                    rtbMoTa.AppendText(string.Format("===Xử lý điểm ({0:F2}, {1:F2})===\n",
                       Math.Round(dataPointsDB[i].X, 2),
                       Math.Round(dataPointsDB[i].Y, 2)));
                    rtbMoTa.AppendText(string.Format("Điểm này là đã thuộc Cụm {0}.\n", labels[i]));
                }
            }
        }

        private bool DisplayExpandCluster(int pointIndex, int clusterId, float epsilon, int minPts)
        {
            List<int> seeds = RegionQuery(pointIndex, epsilon);

            rtbMoTa.AppendText(string.Format("Điểm ({0:F2}, {1:F2}) là trung tâm của một cụm mới (Cụm {2}).\n",
                Math.Round(dataPointsDB[pointIndex].X, 2),
                Math.Round(dataPointsDB[pointIndex].Y, 2),
                clusterId));

            rtbMoTa.AppendText(string.Format("Danh sách các điểm lân cận trong phạm vi epsilon ({0:F2}):\n", epsilon));
            if (seeds.Count < minPts)
            {
                rtbMoTa.AppendText("Không đủ số lượng điểm để tạo thành cụm.\n");
                labels[pointIndex] = NOISE;
                return false;
            }

            foreach (var seed in seeds)
            {
                rtbMoTa.AppendText(string.Format(" - Điểm lân cận ({0:F2}, {1:F2})\n",
                    Math.Round(dataPointsDB[seed].X, 2),
                    Math.Round(dataPointsDB[seed].Y, 2)));
            }

            rtbMoTa.AppendText("\n");

            foreach (var seed in seeds)
            {
                labels[seed] = clusterId;
            }

            seeds.Remove(pointIndex);

            while (seeds.Count > 0)
            {
                int currentPoint = seeds[0];
                List<int> result = RegionQuery(currentPoint, epsilon);

                rtbMoTa.AppendText(string.Format("Điểm ({0:F2}, {1:F2}) trong cụm {2}:\n",
                    Math.Round(dataPointsDB[currentPoint].X, 2),
                    Math.Round(dataPointsDB[currentPoint].Y, 2),
                    clusterId));

                rtbMoTa.AppendText(string.Format("Danh sách các điểm lân cận trong phạm vi epsilon ({0:F2}):\n", epsilon));
                if (result.Count >= minPts)
                {
                    foreach (var resultPoint in result)
                    {
                        if (labels[resultPoint] == UNCLASSIFIED || labels[resultPoint] == NOISE)
                        {
                            if (labels[resultPoint] == UNCLASSIFIED)
                            {
                                seeds.Add(resultPoint);
                            }
                            labels[resultPoint] = clusterId;
                        }
                    }

                    foreach (var resultPoint in result)
                    {
                        rtbMoTa.AppendText(string.Format(" - Điểm lân cận ({0:F2}, {1:F2})\n",
                            Math.Round(dataPointsDB[resultPoint].X, 2),
                            Math.Round(dataPointsDB[resultPoint].Y, 2)));
                    }
                }
                else
                {
                    rtbMoTa.AppendText("Không đủ số lượng điểm để mở rộng cụm.\n");
                }

                rtbMoTa.AppendText("\n");

                seeds.Remove(currentPoint);
            }

            return true;
        }
        private void RunDBScan(float epsilon, int minPts)
        {
            while (true)
            {
                if (currentPointIndex >= dataPointsDB.Count)
                {

                    clusterId = 0;
                    currentPointIndex = 0;
                    MessageBox.Show("Tất cả điểm đã được phân cụm - Thuật toán kết thúc!");
                    GraphForm graphForm = new GraphForm(dataPointsDB, labels);
                    graphForm.Show();
                    break;
                }
                // Nếu điểm đã được phân loại, bỏ qua và tiếp tục với điểm tiếp theo
                if (labels[currentPointIndex] != UNCLASSIFIED)
                {
                    currentPointIndex++;
                    continue;
                }

                if (currentPointIndex < dataPointsDB.Count)
                {
                    if (labels[currentPointIndex] == NOISE || labels[currentPointIndex] == UNCLASSIFIED)
                    {
                        DisplayPointUnclassified();
                        rtbMoTa.AppendText(string.Format("\n=== Xử lý điểm ({0:F2}, {1:F2}) ===\n",
                              Math.Round(dataPointsDB[currentPointIndex].X, 2),
                              Math.Round(dataPointsDB[currentPointIndex].Y, 2)));

                        if (labels[currentPointIndex] == UNCLASSIFIED)
                        {
                            List<int> seeds = RegionQuery(currentPointIndex, epsilon);
                            if (seeds.Count < minPts)
                            {
                                labels[currentPointIndex] = NOISE;
                                rtbMoTa.AppendText(string.Format("Số lượng điểm lân cận là {0} - Điểm này bị phân loại là nhiễu (NOISE).\n", seeds.Count));
                            }
                            else
                            {
                                rtbMoTa.AppendText(string.Format("Số lượng điểm lân cận là {0} - Điểm này là gốc của một cụm mới (Cụm {1}).\n", seeds.Count, clusterId + 1));

                                foreach (var seed in seeds)
                                {
                                    rtbMoTa.AppendText(string.Format(" - Điểm lân cận ({0:F2}, {1:F2})\n",
                                        Math.Round(dataPointsDB[seed].X, 2),
                                        Math.Round(dataPointsDB[seed].Y, 2)));
                                }

                                if (ExpandCluster(currentPointIndex, clusterId + 1, epsilon, minPts))
                                {
                                    clusterId++;
                                }
                            }
                        }

                        currentPointIndex++;
                    }
                }
                break;
            }
        }
        private void DisplayPointUnclassified()
        {
            rtbMoTa.AppendText("========Danh sách các điểm chưa phân cụm==========\n");

            bool hasUnclassifiedOrNoise = false;

            for (int i = 0; i < dataPointsDB.Count; i++)
            {
                if (labels[i] == UNCLASSIFIED || labels[i] == NOISE)
                {
                    hasUnclassifiedOrNoise = true;

                    rtbMoTa.AppendText(string.Format("Điểm ({0:F2}, {1:F2}) - Nhãn: {2}\n",
                        Math.Round(dataPointsDB[i].X, 2),
                        Math.Round(dataPointsDB[i].Y, 2),
                        labels[i] == UNCLASSIFIED ? "UNCLASSIFIED" : "NOISE"));
                }
            }
            if (!hasUnclassifiedOrNoise)
            {
                rtbMoTa.AppendText("Không có điểm nào chưa phân loại hoặc bị nhiễu.\n");
            }

        }
        private bool ExpandCluster(int pointIndex, int clusterCurrent, float epsilon, int minPts)
        {
            List<int> seeds = RegionQuery(pointIndex, epsilon);
            List<Diem> pointsNew = new List<Diem>();

            if (seeds.Count < minPts)
            {
                labels[pointIndex] = NOISE;
                return false;
            }

            foreach (var seed in seeds)
            {
                labels[seed] = clusterCurrent;
            }

            seeds.Remove(pointIndex);
            rtbMoTa.AppendText(string.Format("Mở rộng cụm{0}\n", clusterCurrent));
            while (seeds.Count > 0)
            {
                int currentPoint = seeds[0];
                rtbMoTa.AppendText(string.Format("\tĐiểm ({0:F2}, {1:F2}) trong cụm {2}:\n",
                            Math.Round(dataPointsDB[currentPoint].X, 2),
                            Math.Round(dataPointsDB[currentPoint].Y, 2),
                            clusterCurrent));
                List<int> result = RegionQuery(currentPoint, epsilon);

                if (result.Count >= minPts)
                {
                    foreach (var resultPoint in result)
                    {
                        if (labels[resultPoint] == UNCLASSIFIED || labels[resultPoint] == NOISE)
                        {
                            if (labels[resultPoint] == UNCLASSIFIED)
                            {
                                seeds.Add(resultPoint);
                            }
                            labels[resultPoint] = clusterCurrent;
                            pointsNew.Add(dataPointsDB[resultPoint]);
                        }
                        rtbMoTa.AppendText(string.Format(" - Điểm lân cận " + dataPointsDB[resultPoint].ToString()) + "\n");
                    }
                }
                seeds.Remove(currentPoint);
            }
            rtbMoTa.AppendText("Các điểm mới sau khi mở rộng:\n");
            if (pointsNew.Count > 0)
            {
                foreach (var point in pointsNew)
                {
                    rtbMoTa.AppendText("-" + point.ToString() + "\n");
                }
            }
            else
            {
                rtbMoTa.AppendText("==>Không có điểm mới nào thêm\n");
            }
            return true;
        }
        private List<int> RegionQuery(int pointIndex, float epsilon)
        {
            List<int> neighbors = new List<int>();

            for (int i = 0; i < dataPointsDB.Count; i++)
            {
                if (Distance(dataPointsDB[pointIndex], dataPointsDB[i]) <= epsilon)
                {
                    neighbors.Add(i);
                }
            }
            return neighbors;
        }


        private void DisplayDBScanDetails(float epsilon, int minPts)
        {

            rtbMoTa.Clear();
            rtbMoTa.AppendText("Thông số của DBScan:\n");
            rtbMoTa.AppendText(string.Format("Epsilon (Khoảng cách tối đa): {0:F2}\n", epsilon));
            rtbMoTa.AppendText(string.Format("MinPts (Số điểm tối thiểu để tạo thành một cụm): {0}\n\n", minPts));

            rtbMoTa.AppendText("Danh sách các điểm dữ liệu ban đầu:\n");
            foreach (var point in dataPointsDB)
            {
                rtbMoTa.AppendText(string.Format("- ({0:F2}, {1:F2})\n", Math.Round(point.X, 2), Math.Round(point.Y, 2)));
            }
            rtbMoTa.AppendText("\n");

            rtbMoTa.AppendText("Bắt đầu chạy thuật toán DBScan...\n");
            DisplayRunDBScan(epsilon, minPts);

            rtbMoTa.AppendText("\nKết quả phân loại các điểm:\n");
            for (int i = 0; i < dataPointsDB.Count; i++)
            {
                rtbMoTa.AppendText(string.Format("- ({0:F2}, {1:F2}) - Nhãn: {2}\n",
                    Math.Round(dataPointsDB[i].X, 2),
                    Math.Round(dataPointsDB[i].Y, 2),
                    labels[i] == NOISE ? "Nhiễu" : labels[i].ToString()));
            }
            rtbMoTa.AppendText("\n");

            var clusters = labels.Distinct().Where(label => label != NOISE).ToList();
            rtbMoTa.AppendText(string.Format("Số lượng cụm phát hiện được: {0}\n\n", clusters.Count));

            foreach (var clusterId in clusters)
            {
                rtbMoTa.AppendText(string.Format("Cụm {0}:\n", clusterId));
                for (int i = 0; i < dataPointsDB.Count; i++)
                {
                    if (labels[i] == clusterId)
                    {
                        rtbMoTa.AppendText(string.Format(" - ({0:F2}, {1:F2})\n",
                            Math.Round(dataPointsDB[i].X, 2),
                            Math.Round(dataPointsDB[dataPointsDB.IndexOf(dataPointsDB[i])].Y, 2)));
                    }
                }
                rtbMoTa.AppendText("\n");
            }

            var noisePoints = labels.Where(label => label == NOISE).ToList();
            if (noisePoints.Count > 0)
            {
                rtbMoTa.AppendText("Danh sách các điểm nhiễu:\n");
                for (int i = 0; i < dataPointsDB.Count; i++)
                {
                    if (labels[i] == NOISE)
                    {
                        rtbMoTa.AppendText(string.Format("- ({0:F2}, {1:F2})\n",
                            Math.Round(dataPointsDB[i].X, 2),
                            Math.Round(dataPointsDB[i].Y, 2)));
                    }
                }
            }
            else
            {
                rtbMoTa.AppendText("Không có điểm nào bị phân loại là nhiễu.\n");
            }
        }
        private void DisplayDataInDataGridView()
        {
            dgvResult.Rows.Clear(); // Xóa các hàng hiện tại nếu có

            for (int i = 0; i < dataPointsDB.Count; i++)
            {
                string phanLoai;
                if (labels[i] == UNCLASSIFIED)
                {
                    phanLoai = "UNCLASSIFIED";
                }
                else if (labels[i] == NOISE)
                {
                    phanLoai = "NOISE";
                }
                else
                {
                    phanLoai = $"Cluster {labels[i]}";
                }

                dgvResult.Rows.Add(i + 1, dataPointsDB[i].X, dataPointsDB[i].Y, phanLoai);
            }
        }
        private void DrawPointsDBScan(List<Diem> dataPointsDB, int[] labels)
        {
            GraphPane myPane = zedGraphControl.GraphPane;
            myPane.Title.Text = "DBScan Clustering lần "+countStep.ToString();
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
                int label = entry.Key;
                PointPairList pointList = entry.Value;
                // Sử dụng mảng màu
                Color color = colors[(label - 1) % colors.Length];
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


        /// <summary>
        /// main run
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void btnRun_Click(object sender, EventArgs e)
        {
            int indexChoose = comboxChoose.SelectedIndex;
            switch (indexChoose)
            {
                case 0:
                    if (diemDuLieu == null || diemDuLieu.Count == 0)
                    {
                        MessageBox.Show("Bạn chưa chọn dữ liệu!", "Cảnh Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else if (int.TryParse(txtK.Text, out int k))
                    {
                        if (cbRunStep.Checked == true)
                        {

                            if (!isKMeansInitialized)
                            {
                                KhoiTaoKMeans(diemDuLieu, k);
                                isKMeansInitialized = true;
                                iMenuChooseFile.Enabled = false;
                                iMenuDrawPoints.Enabled = false;
                                rtbMoTa.SelectionStart = rtbMoTa.Text.Length;
                                rtbMoTa.ScrollToCaret();
                            }
                            else
                            {
                                KMeansStep();
                                DisplayResult(cumKetQua);
                                DrawPointsKMean(cumKetQua,countStep);
                                if (!thayDoi)
                                {
                                    GraphForm graphForm = new GraphForm(diemDuLieu, cumKetQua);
                                    graphForm.Show();
                                    isKMeansInitialized = false;
                                    iMenuChooseFile.Enabled = true;
                                    iMenuDrawPoints.Enabled = true;
                                    MessageBox.Show("Thuật toán K-Means hoàn thành", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                }
                                rtbMoTa.SelectionStart = rtbMoTa.Text.Length;
                                rtbMoTa.ScrollToCaret();
                                countStep++;
                            }
                        }
                        else
                        {
                            cumKetQua = KMeans(diemDuLieu, k);
                            DisplayResult(cumKetQua);
                            DrawPointsKMean(cumKetQua,countStep);
                            GraphForm graphForm = new GraphForm(diemDuLieu, cumKetQua);
                            graphForm.Show();
                            rtbMoTa.SelectionStart = rtbMoTa.Text.Length;
                            rtbMoTa.ScrollToCaret();
                            countStep++;
                        }
                        
                    }
                    else
                    {
                        MessageBox.Show("Vui lòng nhập số cụm hợp lệ.", "Cảnh Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    break;
                case 1:
                    if (dataPoints == null || dataPoints.Count == 0)
                    {
                        MessageBox.Show("Bạn chưa chọn dữ liệu!", "Cảnh Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (float.TryParse(txtBanKinh.Text, out float bandwidth))
                    {
                        if (cbRunStep.Checked == true)
                        {

                            if (!isMeanShiftInitialized)
                            {
                                rtbMoTa.Clear();
                                clustersCenterNew = DisplayMeanShiftStep(dataPoints, bandwidth);
                                Dictionary<Diem, List<Diem>> clusters = AssignClusters(clustersCenterNew);
                                isMeanShiftInitialized = true;
                                DisplayResult(clusters);
                                DrawPointsMeanShift(clusters);
                                rtbMoTa.SelectionStart = rtbMoTa.Text.Length;
                                rtbMoTa.ScrollToCaret();
                                countStep++;

                            }
                            else
                            {
                                if (!thayDoiMS)
                                {
                                    Dictionary<Diem, List<Diem>> c = AssignClusters(clustersCenterNew);//phân cụm
                                    MessageBox.Show("Các điểm trung tâm đã hội tụ - Thuật toán kết thúc!");
                                    isMeanShiftInitialized = false;
                                    GraphForm graphForm = new GraphForm(dataPoints, c);
                                    graphForm.Show();
                                    countStep = 1;
                                    return;
                                   
                                }
                                thayDoiMS = false;
                                clustersCenterOld = clustersCenterNew;
                                clustersCenterNew = DisplayMeanShiftStep(clustersCenterNew, bandwidth);
                                Dictionary<Diem, List<Diem>> clusters = AssignClusters(clustersCenterNew);
                                DisplayResult(clusters);
                                DrawPointsMeanShift(clusters);
                                rtbMoTa.SelectionStart = rtbMoTa.Text.Length;
                                rtbMoTa.ScrollToCaret();
                                countStep++;

                            }
                        }
                        else
                        {
                            List<Diem> clusterCenters = MeanShiftClustering(bandwidth);
                            Dictionary<Diem, List<Diem>> clusters = AssignClusters(clusterCenters);
                            DisplayClusters(clusters, bandwidth);
                            DrawPointsMeanShift(clusters);
                            GraphForm graphForm = new GraphForm(dataPoints, clusters);
                            graphForm.Show();
                            rtbMoTa.SelectionStart = rtbMoTa.Text.Length;
                            rtbMoTa.ScrollToCaret();
                        }

                    }
                    else
                    {
                        MessageBox.Show("Vui lòng nhập bán kính hợp lệ.", "Cảnh Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    break;
                case 2:
                    if (dataPointsDB == null || dataPointsDB.Count == 0)
                    {
                        MessageBox.Show("Bạn chưa chọn dữ liệu!", "Cảnh Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (!float.TryParse(txtBanKinh.Text, out float epsilon))
                    {
                        MessageBox.Show("Vui lòng nhập số bán kính hợp lệ.", "Cảnh Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (!int.TryParse(txtMinPoints.Text, out int minPts))
                    {
                        MessageBox.Show("Vui lòng nhập số điểm tối thiểu hợp lệ.", "Cảnh Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }



                    if (cbRunStep.Checked)
                    {

                        RunDBScan(epsilon, minPts);
                        DisplayDataInDataGridView();
                        DrawPointsDBScan(dataPointsDB, labels);
                        rtbMoTa.SelectionStart = rtbMoTa.Text.Length;
                        rtbMoTa.ScrollToCaret();
                        countStep++;
                    }
                    else
                    {
                        DisplayDBScanDetails(epsilon, minPts);
                        DrawPointsDBScan(dataPointsDB, labels);
                        rtbMoTa.SelectionStart = rtbMoTa.Text.Length;
                        rtbMoTa.ScrollToCaret();
                        GraphForm graphForm = new GraphForm(dataPointsDB, labels);
                        graphForm.Show();
                    }
                    break;
                case 3:
                    if (diemDuLieu == null || diemDuLieu.Count == 0)
                    {
                        MessageBox.Show("Bạn chưa chọn dữ liệu!", "Cảnh Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        if (!int.TryParse(txtBanKinh.Text, out int clusterMax))
                        {
                            MessageBox.Show("Vui lòng nhập số cụm cần xét hợp lệ.", "Cảnh Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (!int.TryParse(txtMinPoints.Text, out int doLech))
                        {
                            MessageBox.Show("Vui lòng nhập độ lệch hợp lệ.", "Cảnh Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        int kToiUu = ChonKToiUu(diemDuLieu, doLech/100, clusterMax);
                        MessageBox.Show("K tối ưu nhất là: " + kToiUu.ToString());
                        cumKetQua = KMeans(diemDuLieu, kToiUu);
                        DisplayResult(cumKetQua);
                        DrawPointsKMean(cumKetQua, countStep);
                        GraphForm graphForm = new GraphForm(diemDuLieu, cumKetQua);
                        graphForm.Show();
                        rtbMoTa.SelectionStart = rtbMoTa.Text.Length;
                        rtbMoTa.ScrollToCaret();                     
                    }

                    break;

            }
        }

        private void comboxChoose_SelectedIndexChanged(object sender, EventArgs e)
        {
            int indexChoose = comboxChoose.SelectedIndex;
            switch (indexChoose)
            {
                case 0:
                    countStep = 1;
                    txtK.Enabled = true;
                    txtBanKinh.Enabled = false;
                    txtMinPoints.Enabled = false;
                    cbRunStep.Visible = true;
                    txtBanKinh.Text = "";
                    txtK.Text = "";
                    txtMinPoints.Text = "";
                    dgvResult.Rows.Clear();
                    rtbMoTa.Text = "";
                    DrawZepReset(diemDuLieu);
                    break;
                case 1:
                    countStep = 1;
                    txtK.Enabled = false;
                    txtBanKinh.Enabled = true;
                    txtMinPoints.Enabled = false;
                    cbRunStep.Visible = true;
                    txtBanKinh.Text = "";
                    txtK.Text = "";
                    txtMinPoints.Text = "";
                    dgvResult.Rows.Clear();
                    rtbMoTa.Text = "";
                    DrawZepReset(diemDuLieu);
                    break;
                case 2:
                    countStep = 1;
                    txtK.Enabled = false;
                    txtBanKinh.Enabled = true;
                    txtMinPoints.Enabled = true;
                    cbRunStep.Visible = true;
                    lbMinPoint.Text = "Nhập MinPoints";
                    lbRadis.Text = "Nhập bán kính";
                    txtBanKinh.Text = "";
                    txtK.Text = "";
                    txtMinPoints.Text = "";
                    labels = new int[dataPointsDB.Count];
                    for (int i = 0; i < labels.Length; i++)
                    {
                        labels[i] = UNCLASSIFIED;
                    }
                    dgvResult.Rows.Clear();
                    rtbMoTa.Text = "";
                    DrawZepReset(diemDuLieu);
                    break;
                case 3:
                    countStep = 1;
                    txtK.Enabled = false;
                    txtBanKinh.Enabled = true;
                    txtMinPoints.Enabled = true;
                    lbMinPoint.Text = "Độ lệch(%)";
                    lbRadis.Text = "Số cụm cần xét";
                    txtBanKinh.Text = "";
                    txtK.Text = "";
                    txtMinPoints.Text = "";
                    labels = new int[dataPointsDB.Count];
                    for (int i = 0; i < labels.Length; i++)
                    {
                        labels[i] = UNCLASSIFIED;
                    }
                    dgvResult.Rows.Clear();
                    rtbMoTa.Text = "";
                    cbRunStep.Visible= false;
                    DrawZepReset(diemDuLieu);
                    break;

            }
        }
        private void iMenuExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        
    }
}
