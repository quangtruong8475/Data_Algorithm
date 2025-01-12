using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Classifier
{
    public partial class Mutli_Lable : Form
    {
        private List<ParsedData> parsedDatas = new List<ParsedData>();
        private string[] labels = new string[0];
        private Dictionary<string, List<Data>> Datas = new Dictionary<string, List<Data>>();
        public Mutli_Lable()
        {
            InitializeComponent();
        }

        private void Mutli_Lable_Load(object sender, EventArgs e)
        {
            
        }
        private void btnLoadData_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Chọn Tệp Dữ Liệu"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var filePath = openFileDialog.FileName;
                var lines = System.IO.File.ReadAllLines(filePath).ToList();
                parsedDatas = ParseData(lines);
                var result= CheckInterest(parsedDatas, "Reading");
                dataGridView1.DataSource= result;
                foreach(string line in labels) 
                {
                    MessageBox.Show(line);
                }
            }
        }

        private  List<ParsedData> ParseData(List<string> data)
        {
            var parsedData = new List<ParsedData>();

            foreach (var line in data)
            {
                var parts = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 6)
                    continue;
                var parsed = new ParsedData
                {
                    GioiTinh = int.Parse(parts[0].Trim()),
                    Tuoi = int.Parse(parts[1].Trim()),
                    ThuNhap = int.Parse(parts[2].Trim()),
                    SoGioRanh = int.Parse(parts[3].Trim()),
                    TinhTrangHonNhan = parts[4].Trim() == "1",
                    SoThich = parts[5].Trim().Split('-').Select(s => s.Trim()).ToList()
                };
                parsedData.Add(parsed);
            }
            labels = GetUniqueLabels(parsedData);
            return parsedData;
        }
        private string[] GetUniqueLabels(List<ParsedData> parsedData)
        {              
            var labels = new HashSet<string>();
            foreach (var data in parsedData)
            {
                foreach (var interest in data.SoThich)
                {
                    labels.Add(interest);
                }
            }
            return labels.ToArray();
        }

        private List<Data> CheckInterest(List<ParsedData> parsedData, string interest)
        {
            var results = new List<Data>();

            foreach (var data in parsedData)
            {
                // Kiểm tra sở thích
                var hasInterest = data.SoThich.Contains(interest) ? "Yes" : "No";

                // Tạo đối tượng Data với kết quả kiểm tra sở thích
                var result = new Data
                {
                    GioiTinh = data.GioiTinh,
                    Tuoi = data.Tuoi,
                    ThuNhap = data.ThuNhap,
                    SoGioRanh = data.SoGioRanh,
                    TinhTrangHonNhan = data.TinhTrangHonNhan ? 1 : 0,
                    InterestCheckResult = hasInterest
                };

                results.Add(result);
            }

            return results;
        }

        class ParsedData
        {
            public int GioiTinh { get; set; }
            public int Tuoi { get; set; }
            public int ThuNhap { get; set; }
            public int SoGioRanh { get; set; }
            public bool TinhTrangHonNhan { get; set; }
            public List<string> SoThich { get; set; }
        }
    }
    public class Data
    {
        public int GioiTinh { get; set; }
        public int Tuoi { get; set; }
        public int ThuNhap { get; set; }
        public int SoGioRanh { get; set; }
        public int TinhTrangHonNhan { get; set; }
        public string InterestCheckResult { get; set; }
    }
}
