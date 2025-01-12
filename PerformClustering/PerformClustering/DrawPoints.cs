using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;

namespace PerformClustering
{
    public partial class DrawPoints : Form
    {
        public List<Diem> diemList { get; set; }

        public DrawPoints(List<Diem>diemList)
        {
            InitializeComponent();
            InitializeGraph(diemList);
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private void InitializeGraph(List<Diem> diemList)
        {
            var pane = zedGraphControlDraw.GraphPane;
            pane.Title.Text = "Đồ thị của tôi";
            pane.XAxis.Title.Text = "Trục X";
            pane.YAxis.Title.Text = "Trục Y";

            var list = new PointPairList();
            if (diemList != null && diemList.Count > 0)
            {
                foreach (var diem in diemList)
                {
                    list.Add(diem.X, diem.Y);
                }
            }
            else
            {
                pane.XAxis.Scale.MaxAuto = false;
                pane.XAxis.Scale.MinAuto = false;
                pane.YAxis.Scale.MaxAuto = false;
                pane.YAxis.Scale.MinAuto = false;
                pane.XAxis.Scale.Min = -10;
                pane.XAxis.Scale.Max = 10;
                pane.YAxis.Scale.Min = -10;
                pane.YAxis.Scale.Max = 10;
            }

            var curve = pane.AddCurve("Điểm", list, Color.Blue, SymbolType.Circle);
            curve.Line.IsVisible = false;
            
          

            zedGraphControlDraw.AxisChange();
            zedGraphControlDraw.Invalidate();
        }

        private void zedGraphControlDraw_MouseClick(object sender, MouseEventArgs e)
        {
            // Lấy GraphPane từ ZedGraphControl
            var pane = zedGraphControlDraw.GraphPane;

            // Chuyển đổi tọa độ màn hình sang tọa độ biểu đồ
            pane.ReverseTransform(new PointF(e.X, e.Y), out double x, out double y);

            // Tạo điểm mới
            var point = new PointPair(x, y);

            // Thêm điểm mới vào đồ thị
            if (pane.CurveList.Count > 0)
            {
                // Giả sử đồ thị đầu tiên trong danh sách là đồ thị bạn muốn thêm điểm vào
                var curve = pane.CurveList[0] as LineItem;
                if (curve != null)
                {
                    curve.AddPoint(point);
                    zedGraphControlDraw.AxisChange();
                    zedGraphControlDraw.Invalidate();
                }
            }
        }
        private List<Diem> SavePointsToList()
        {
             diemList = new List<Diem>();
            var pane = zedGraphControlDraw.GraphPane;

            if (pane.CurveList.Count > 0)
            {
                // Giả sử đồ thị đầu tiên trong danh sách là đồ thị bạn muốn xuất điểm
                var curve = pane.CurveList[0] as LineItem;
                if (curve != null)
                {
                    var points = curve.Points as PointPairList;
                    if (points != null)
                    {
                        foreach (var point in points)
                        {
                            diemList.Add(new Diem(point.X, point.Y));
                        }
                    }
                }
            }
            return diemList;
        }

       

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            diemList=SavePointsToList();
            this.Close();
        }


        private void DrawPoints_Load(object sender, EventArgs e)
        {

        }
    }
}
