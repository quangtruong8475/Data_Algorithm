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
using System.Xml.Linq;
using ZedGraph;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Classifier
{
    public partial class MainClassified : Form
    {
        private List<Customer> Customers = new List<Customer>();
        private int currentStep = 1;
        private List<(Customer Customer, double Distance)> distances;
        private List<(Customer Customer, double Distance)> sortedDistances;
        private List<(Customer Customer, double Distance)> nearestNeighbors;
        private string finalResult;
        //Mutli-Lables
        private List<ParsedData> parsedDatas = new List<ParsedData>();
        private List<string> labels = new List<string>();
        private Dictionary<string, List<Data>> datas = new Dictionary<string, List<Data>>();
        private List<(string label, List<(Data data, double Distance)> dataWithDistances)> distancesML = new List<(string label, List<(Data data, double Distance)> dataWithDistances)>();
        private string result;
        //Navie Bayes
        private List<Customer> CustomerList = new List<Customer>();
        private Customer newCustomer = new Customer();
        //DecisionTree
        public List<string> Lables = new List<string> { "Age", "LoanAmount", "Gender", "Income" };

        public MainClassified()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            tabPage1.Text = "K-NearestNeighbors";
            tabPage2.Text = "Navie Bayes";
            tabPage3.Text = "Cây quyết định";
            tabPage4.Text = "Mutli-Lables(KNN)";
            dgvData.ColumnCount = 5;
            dgvData.Columns[0].Name = "STT";
            dgvData.Columns[1].Name = "Tuổi";
            dgvData.Columns[2].Name = "Thu nhập";
            dgvData.Columns[3].Name = "Tiền vay";
            dgvData.Columns[4].Name = "Đánh giá";
            //
            dgvDataCQD.ColumnCount = 5;
            dgvDataCQD.Columns[0].Name = "STT";
            dgvDataCQD.Columns[1].Name = "Tuổi";
            dgvDataCQD.Columns[2].Name = "Thu nhập";
            dgvDataCQD.Columns[3].Name = "Tiền vay";
            dgvDataCQD.Columns[4].Name = "Đánh giá";
            //
            dgvResult.ColumnCount = 6;
            dgvResult.Columns[0].Name = "STT";
            dgvResult.Columns[1].Name = "Tuổi";
            dgvResult.Columns[2].Name = "Thu nhập";
            dgvResult.Columns[3].Name = "Tiền vay";
            dgvResult.Columns[4].Name = "Đánh giá";
            dgvResult.Columns[5].Name = "Khoảng cách";
            //Mutli-Lables
            dgvKhoangCachML.Columns.Add("GioiTinh", "Giới Tính");
            dgvKhoangCachML.Columns.Add("Tuoi", "Tuổi");
            dgvKhoangCachML.Columns.Add("ThuNhap", "Thu Nhập");
            dgvKhoangCachML.Columns.Add("SoGioRanh", "Số Giờ Rảnh");
            dgvKhoangCachML.Columns.Add("TinhTrangHonNhan", "Hôn Nhân");
            dgvKhoangCachML.Columns.Add("InterestCheckResult", "Đánh giá");
            dgvKhoangCachML.Columns.Add("distance", "Khoảng cách");
            //
            // Cấu hình các cột cho DataGridView
            dgvDataML.Columns.Clear();
            dgvDataML.Columns.Add("GioiTinh", "Giới Tính");
            dgvDataML.Columns.Add("Tuoi", "Tuổi");
            dgvDataML.Columns.Add("ThuNhap", "Thu Nhập");
            dgvDataML.Columns.Add("SoGioRanh", "Số Giờ Rảnh");
            dgvDataML.Columns.Add("TinhTrangHonNhan", "Tình Trạng Hôn Nhân");
            dgvDataML.Columns.Add("SoThich", "Sở Thích"); // Cột cho danh sách Sở Thích
            ColumnWidth();

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
                try
                {
                    Customers.Clear();
                    CustomerList.Clear();
                    var lines = File.ReadAllLines(openFileDialog.FileName);
                    foreach (var line in lines)
                    {
                        var parts = line.Split(',');
                        if (parts.Length == 5)
                        {
                            var Customer = new Customer(
                                int.Parse(parts[0].Trim()),
                                int.Parse(parts[1].Trim()),
                                int.Parse(parts[2].Trim()),
                                int.Parse(parts[3].Trim()),
                                parts[4].Trim()
                            );
                            Customers.Add(Customer);
                            CustomerList.Add(Customer);
                        }
                    }
                    DisplayCustomersInDataGridView();
                    DisplayCustomersDescsionTree();
                    DisplayDecisionTree();
                    rtbMoTa.AppendText("Dữ liệu đã được tải thành công.\n");
                }
                catch (Exception ex)
                {
                    rtbMoTa.AppendText($"Lỗi khi tải dữ liệu: {ex.Message}\n");
                }
            }
        }

        private void ColumnWidth()
        {
            // Cập nhật chiều rộng của cột "Sở Thích"
            int totalWidth = dgvDataML.ClientSize.Width;

            // Tính chiều rộng của các cột theo tỷ lệ phần trăm
            int columnWidth1 = (int)(totalWidth * 0.125); // 12.5%
            int columnWidth2 = (int)(totalWidth * 0.125); // 12.5%
            int columnWidth3 = (int)(totalWidth * 0.125); // 12.5%
            int columnWidth4 = (int)(totalWidth * 0.125); // 12.5%
            int columnWidth5 = (int)(totalWidth * 0.20);  // 20%
            int columnWidth6 = (int)(totalWidth * 0.30);  // 30%

            // Cập nhật chiều rộng cột
            dgvDataML.Columns["GioiTinh"].Width = columnWidth1;
            dgvDataML.Columns["Tuoi"].Width = columnWidth2;
            dgvDataML.Columns["ThuNhap"].Width = columnWidth3;
            dgvDataML.Columns["SoGioRanh"].Width = columnWidth4;
            dgvDataML.Columns["TinhTrangHonNhan"].Width = columnWidth5;
            dgvDataML.Columns["SoThich"].Width = columnWidth6;
        }

        private void RunKNNAlgorithmStep(Customer newPoint, int k)
        {
            if (currentStep == 1)
            {
                // Bước 1: Tính toán khoảng cách
                distances = KNNAlgorithm.CalculateDistances(newPoint, Customers);
                DisplayDistancesInDataGridView();
                rtbMoTa.AppendText("1. Tính toán khoảng cách:\n");
                foreach (var item in distances)
                {
                    rtbMoTa.AppendText($"   Khoảng cách đến Điểm dữ liệu({item.Customer.Gender}, {item.Customer.Age}, {item.Customer.Income}, {item.Customer.LoanAmount}): Khoảng cách = ");

                    // Hiển thị khoảng cách với hai chữ số thập phân
                    rtbMoTa.SelectionColor = Color.Black;
                    rtbMoTa.AppendText($"{item.Distance.ToString("F2")}, ");

                    // Hiển thị loại phân loại
                    rtbMoTa.SelectionColor = Color.Black;
                    rtbMoTa.AppendText($"Thuộc loại = {item.Customer.Classification}\n");

                    // Reset màu sắc cho văn bản tiếp theo
                    rtbMoTa.SelectionColor = Color.Black;
                }

                currentStep = 2; // Chuyển sang bước tiếp theo
            }
            else if (currentStep == 2)
            {
                sortedDistances = KNNAlgorithm.SortDistances(distances);
                rtbMoTa.AppendText("\n2. Sắp xếp khoảng cách:\n");
                foreach (var item in sortedDistances)
                {
                    rtbMoTa.AppendText($"   Điểm dữ liệu({item.Customer.Gender}, {item.Customer.Age}, {item.Customer.Income}, {item.Customer.LoanAmount}): Khoảng cách = ");
                    rtbMoTa.SelectionColor = Color.Black;
                    rtbMoTa.AppendText($"{item.Distance.ToString("F2")}, ");
                    rtbMoTa.SelectionColor = Color.Black;
                    rtbMoTa.AppendText($"Thuộc loại = {item.Customer.Classification}\n");
                    rtbMoTa.SelectionColor = Color.Black;
                }
                currentStep = 3;
            }
            else if (currentStep == 3)
            {
                nearestNeighbors = KNNAlgorithm.GetNearestNeighbors(sortedDistances, k);
                rtbMoTa.AppendText($"\n3. {k} Điểm gần nhất:\n");
                foreach (var neighbor in nearestNeighbors)
                {
                    rtbMoTa.AppendText($"   Điểm dữ liệu({neighbor.Customer.Gender}, {neighbor.Customer.Age}, {neighbor.Customer.Income}, {neighbor.Customer.LoanAmount}): ");
                    rtbMoTa.SelectionColor = Color.Black;
                    rtbMoTa.AppendText($"Thuộc loại = {neighbor.Customer.Classification}\n");

                    // Reset màu sắc cho văn bản tiếp theo
                    rtbMoTa.SelectionColor = Color.Black;
                }

                // Bước 4: Đếm số lượng mỗi loại phân loại và phân tích kết quả
                StringBuilder details = new StringBuilder();
                details.AppendLine($"\n4. Đếm số lượng mỗi loại phân loại trong {k} điểm gần nhất:");
                var groupedResults = nearestNeighbors.GroupBy(x => x.Customer.Classification)
                    .Select(g => new
                    {
                        Classification = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                foreach (var result in groupedResults)
                {
                    details.AppendLine($"   Phân loại: {result.Classification}, Số lượng: {result.Count}");
                }

                // Kết quả phân loại cuối cùng
                var finalResult = groupedResults.FirstOrDefault()?.Classification ?? "Không xác định";
                details.AppendLine($"\nKết quả phân loại: {finalResult}");

                rtbMoTa.AppendText(details.ToString());
                currentStep = 1; // Reset bước
            }
        }
        private void RunKNNAlgorithm(Customer newPoint, int k)
        {
            // Bước 1: Tính toán khoảng cách
            distances = KNNAlgorithm.CalculateDistances(newPoint, Customers);
            rtbMoTa.AppendText("1. Tính toán khoảng cách:\n");
            foreach (var item in distances)
            {
                rtbMoTa.AppendText($"   Khoảng cách tới Điểm dữ liệu ({item.Customer.Gender}, {item.Customer.Age}, {item.Customer.Income}, {item.Customer.LoanAmount}): Khoảng cách = {item.Distance.ToString("F2")}, Thuộc loại: {item.Customer.Classification}\n");
            }

            // Bước 2: Sắp xếp khoảng cách
            sortedDistances = KNNAlgorithm.SortDistances(distances);
            rtbMoTa.AppendText("\n2. Sắp xếp khoảng cách:\n");
            foreach (var item in sortedDistances)
            {
                rtbMoTa.AppendText($"   Điểm dữ liệu ({item.Customer.Gender}, {item.Customer.Age}, {item.Customer.Income}, {item.Customer.LoanAmount}): Khoảng cách = {item.Distance.ToString("F2")}, Thuộc loại: {item.Customer.Classification}\n");
            }

            // Bước 3: Lấy k hàng xóm gần nhất
            nearestNeighbors = KNNAlgorithm.GetNearestNeighbors(sortedDistances, k);
            rtbMoTa.AppendText($"\n3. {k} Điểm gần nhất:\n");
            foreach (var neighbor in nearestNeighbors)
            {
                rtbMoTa.AppendText($"   Điểm dữ liệu ({neighbor.Customer.Gender}, {neighbor.Customer.Age}, {neighbor.Customer.Income}, {neighbor.Customer.LoanAmount}): Khoảng cách = {neighbor.Distance.ToString("F2")}, Thuộc loại: {neighbor.Customer.Classification}\n");
            }

            // Bước 4: Đếm số lượng mỗi loại phân loại
            var details = new StringBuilder();
            details.AppendLine($"\n4. Đếm số lượng mỗi loại phân loại trong {k} điểm gần nhất:");
            var groupedResults = nearestNeighbors.GroupBy(x => x.Customer.Classification)
                .Select(g => new
                {
                    Classification = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count);

            foreach (var result in groupedResults)
            {
                details.AppendLine($"   Phân loại: {result.Classification}, Số lượng: {result.Count}");
            }

            // Kết quả phân loại cuối cùng
            finalResult = KNNAlgorithm.AnalyzeClassification(nearestNeighbors);
            details.AppendLine($"\nKết quả phân loại: {finalResult}");

            rtbMoTa.AppendText(details.ToString());
            rtbMoTa.ScrollToCaret();
            MessageBox.Show("Kết quả phân loại: " + finalResult.ToString());
        }


        private void btnReset_Click(object sender, EventArgs e)
        {
            txtK.Clear();
            cbGender.Checked = false;
            txtAge.Clear();
            txtIncome.Clear();
            txtAmount.Clear();
            rtbMoTa.Clear();
            Customers.Clear();
            dgvData.Rows.Clear();
            dgvResult.Rows.Clear();
            rtbMoTa.Clear();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {

            if (Customers.Count > 0)
            {
                if (!int.TryParse(txtK.Text, out int k) || k <= 0)
                {
                    rtbMoTa.AppendText("Giá trị không hợp lệ cho K. Vui lòng nhập số nguyên dương.\n");
                    return;
                }
                int gender = cbGender.Checked ? 1 : 0;
                if (!int.TryParse(txtAge.Text, out int age) ||
                    !int.TryParse(txtIncome.Text, out int income) ||
                    !int.TryParse(txtAmount.Text, out int loanAmount))
                {
                    MessageBox.Show("Giá trị đầu vào không hợp lệ. Vui lòng kiểm tra đầu vào của bạn.\n");
                    return;
                }

                if (age <= 18)
                {
                    MessageBox.Show("Tuổi phải lớn hơn 18. Vui lòng nhập lại.\n");
                    return;
                }
                int ageGroup = GetAgeGroup(age);
                var newPoint = new Customer
                {
                    Gender = gender,
                    Age = ageGroup,
                    Income = income,
                    LoanAmount = loanAmount
                };

                if (cbRunStep.Checked)
                {
                    // Nếu checkbox được tick, chạy từng bước
                    RunKNNAlgorithmStep(newPoint, k);
                }
                else
                {
                    rtbMoTa.Clear();
                    // Làm sạch richTextBox trước khi chạy toàn bộ
                    RunKNNAlgorithm(newPoint, k);
                    DisplayDistancesInDataGridView();
                }
            }
            else
            {
                MessageBox.Show(" Vui lòng kiểm tra thêm dữ liệu của bạn.\n");
            }
        }
        private int GetAgeGroup(int actualAge)
        {
            if (actualAge >= 18 && actualAge <= 25)
                return 1;
            else if (actualAge >= 26 && actualAge <= 35)
                return 2;
            else if (actualAge >= 36 && actualAge <= 50)
                return 3;
            else if (actualAge >= 51)
                return 4;
            else
                return -1;
        }
        private void DisplayCustomersInDataGridView()
        {
            dgvData.ColumnCount = 5;
            dgvData.Columns[0].Name = "STT";
            dgvData.Columns[1].Name = "Tuổi";
            dgvData.Columns[2].Name = "Thu nhập";
            dgvData.Columns[3].Name = "Tiền vay";
            dgvData.Columns[4].Name = "Đánh giá";

            dgvData.Rows.Clear();
            for (int i = 0; i < Customers.Count; i++)
            {
                Customer customer = Customers[i];
                dgvData.Rows.Add(i + 1, customer.Age, customer.Income, customer.LoanAmount, customer.Classification);
            }
        }
        private void DisplayDistancesInDataGridView()
        {
            dgvResult.Rows.Clear();
            for (int i = 0; i < distances.Count; i++)
            {
                var (customer, distance) = distances[i];
                dgvResult.Rows.Add(i + 1, customer.Age, customer.Income, customer.LoanAmount, customer.Classification, distance.ToString("F2"));
            }
        }



        private void iMenuExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MainClassified_Load(object sender, EventArgs e)
        {

        }
        //Mutli-Lables



        private void Mutli_Lable_Load(object sender, EventArgs e)
        {

        }
        private void btnOpenML(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Chọn Tệp Dữ Liệu"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                gbButton.Controls.Clear();
                var filePath = openFileDialog.FileName;
                var lines = System.IO.File.ReadAllLines(filePath).ToList();
                parsedDatas = ParseData(lines);
                datas = CheckInterest(parsedDatas, labels);
                DisplayParsedData();
                MessageBox.Show("Mở dữ liệu thành công");
            }
        }

        private List<ParsedData> ParseData(List<string> data)
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
        private List<string> GetUniqueLabels(List<ParsedData> parsedData)
        {
            var labels = new HashSet<string>();
            foreach (var data in parsedData)
            {
                foreach (var interest in data.SoThich)
                {
                    labels.Add(interest);
                }
            }
            return labels.ToList();
        }
        private void toolStripMenuReset_Click(object sender, EventArgs e)
        {
            dgvKhoangCachML.Rows.Clear();
            result = null;
            rtbMutliLabel.Clear();
            txtAgeML.Text = "";
            txtKML.Text = "";
            txtInComeML.Text = "";
            txtSoGioRanh.Text = "";
            cbGenderML.Checked = false;
            cbKetHon.Checked = false;
            gbButton.Controls.Clear();
        }
        private void AddCheckBoxesToGroupBox(List<string> labels, int columns)
        {
            int xOffset = 10;
            int yOffset = 20;
            int xSpacing = 120;
            int ySpacing = 30;

            int currentX = xOffset;
            int currentY = yOffset;

            CheckBox firstCheckBox = null; // Khai báo biến để giữ ô kiểm tra đầu tiên

            for (int i = 0; i < labels.Count; i++)
            {
                CheckBox checkBox = new CheckBox
                {
                    Text = labels[i],
                    Name = "checkBox" + labels[i].Replace(" ", ""),
                    Location = new Point(currentX, currentY)
                };
                checkBox.CheckedChanged += CheckBox_CheckedChanged;
                gbButton.Controls.Add(checkBox);

                // Ghi lại ô kiểm tra đầu tiên
                if (i == 0)
                {
                    firstCheckBox = checkBox;
                }

                currentX += xSpacing;
                if ((i + 1) % columns == 0)
                {
                    currentX = xOffset;
                    currentY += ySpacing;
                }
            }

            // Đánh dấu ô kiểm tra đầu tiên
            if (firstCheckBox != null)
            {
                firstCheckBox.Checked = true;
            }
            DisplayData(firstCheckBox.Text);
        }


        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox clickedCheckBox = sender as CheckBox;

            if (clickedCheckBox != null && clickedCheckBox.Checked)
            {
                // Bỏ chọn tất cả các CheckBox khác trong gbButton
                foreach (Control control in gbButton.Controls)
                {
                    if (control is CheckBox checkBox && checkBox != clickedCheckBox)
                    {
                        checkBox.Checked = false;
                    }
                }

                // Cập nhật nhãn lbNhan để phản ánh dữ liệu của CheckBox được chọn
                lbNhan.Text = $"Dữ liệu cho: {clickedCheckBox.Text}";

                // Xóa dữ liệu hiển thị hiện tại
                ClearDataDisplay();

                // Nhận nhãn từ CheckBox được chọn
                string label = clickedCheckBox.Text;

                // Hiển thị dữ liệu tương ứng với nhãn đã chọn
                DisplayData(label);
            }
        }


        private void DisplayData(string label)
        {
            dgvKhoangCachML.Rows.Clear();


            var filteredData = distancesML
                .Where(entry => entry.label == label)
                .SelectMany(entry => entry.dataWithDistances) // Flatten the list of (Data, Distance) tuples
                .ToList();


            foreach (var data in filteredData)
            {
                dgvKhoangCachML.Rows.Add(
                    data.data.GioiTinh,
                    data.data.Tuoi,
                    data.data.ThuNhap,
                    data.data.SoGioRanh,
                    data.data.TinhTrangHonNhan,
                    data.data.InterestCheckResult,
                    data.Distance.ToString("F2") // Khoảng cách
                );
            }
        }

        private void DisplayParsedData()
        {
            dgvDataML.Rows.Clear(); // Xóa dữ liệu hiện tại trong DataGridView

            foreach (var data in parsedDatas)
            {

                string soThich = string.Join(" - ", data.SoThich);

                dgvDataML.Rows.Add(
                    data.GioiTinh,
                    data.Tuoi,
                    data.ThuNhap,
                    data.SoGioRanh,
                    data.TinhTrangHonNhan ? "Đã kết hôn" : "Chưa kết hôn",
                    soThich
                );
            }
        }



        private void ClearDataDisplay()
        {
            dgvKhoangCachML.Rows.Clear();
        }

        private Dictionary<string, List<Data>> CheckInterest(List<ParsedData> parsedData, List<string> interests)
        {
            var results = new Dictionary<string, List<Data>>();

            foreach (var interest in interests)
            {
                var interestResults = new List<Data>();

                foreach (var data in parsedData)
                {
                    var hasInterest = data.SoThich.Contains(interest) ? "Yes" : "No";
                    var result = new Data
                    {
                        GioiTinh = data.GioiTinh,
                        Tuoi = data.Tuoi,
                        ThuNhap = data.ThuNhap,
                        SoGioRanh = data.SoGioRanh,
                        TinhTrangHonNhan = data.TinhTrangHonNhan ? 1 : 0,
                        InterestCheckResult = hasInterest
                    };
                    interestResults.Add(result);
                }

                results[interest] = interestResults;
            }

            return results;
        }

        private double CalculateDistance(Data a, Data b)
        {
            return Math.Sqrt(
                Math.Pow(a.GioiTinh - b.GioiTinh, 2) +
                Math.Pow(a.Tuoi - b.Tuoi, 2) +
                Math.Pow(a.ThuNhap - b.ThuNhap, 2) +
                Math.Pow(a.SoGioRanh - b.SoGioRanh, 2) +
                Math.Pow(a.TinhTrangHonNhan - b.TinhTrangHonNhan, 2)
            );
        }

        public List<(string label, List<(Data data, double Distance)> dataWithDistances)> Predict(Data testData)
        {
            distancesML.Clear();
            List<(string label, List<(Data data, double Distance)> dataWithDistances)> result = new List<(string label, List<(Data data, double Distance)> dataWithDistances)>();
            foreach (var kvp in datas)
            {
                string label = kvp.Key; // Nhãn của nhóm
                List<(Data data, double Distance)> dataWithDistances = new List<(Data data, double Distance)>();
                foreach (var trainData in kvp.Value)
                {
                    double distance = CalculateDistance(testData, trainData);
                    dataWithDistances.Add((trainData, distance));
                }
                result.Add((label, dataWithDistances));
            }

            return result;
        }
        private void DisplayMultiLabel(int k)
        {
            // Xóa nội dung RichTextBox
            rtbMutliLabel.Clear();

            // Tạo dictionary để lưu trữ k điểm gần nhất cho mỗi nhãn
            var nearestPointsByLabel = new Dictionary<string, List<(Data DataPoint, double Distance)>>();

            // Lặp qua từng nhãn trong danh sách nhãn
            foreach (string label in labels)
            {
                // Lọc ra các điểm có nhãn tương ứng và sắp xếp theo khoảng cách
                var filteredPoints = distancesML
                    .Where(entry => entry.label == label)
                    .SelectMany(entry => entry.dataWithDistances)
                    .OrderBy(data => data.Distance)
                    .Take(k)
                    .ToList();

                // Lưu kết quả vào dictionary
                nearestPointsByLabel[label] = filteredPoints;
            }

            // Hiển thị kết quả trong RichTextBox
            foreach (var label in nearestPointsByLabel.Keys)
            {
                rtbMutliLabel.AppendText($"Nhãn: {label}\n");

                foreach (var point in nearestPointsByLabel[label])
                {
                    rtbMutliLabel.AppendText($"-{point.DataPoint.GioiTinh}, {point.DataPoint.Tuoi}, {point.DataPoint.ThuNhap},  {point.DataPoint.SoGioRanh},  {point.DataPoint.TinhTrangHonNhan}, Đánh giá:  {point.DataPoint.InterestCheckResult} , - Khoảng cách: {point.Distance.ToString("F2")}\n");
                }

                rtbMutliLabel.AppendText("\n");
                int countYes = nearestPointsByLabel[label].Count(point => point.DataPoint.InterestCheckResult == "Yes");
                int countNo = nearestPointsByLabel[label].Count(point => point.DataPoint.InterestCheckResult == "No");

                rtbMutliLabel.AppendText($"Nhãn: {label}\n");
                rtbMutliLabel.AppendText($"- Số lượng 'Yes': {countYes}\n");
                rtbMutliLabel.AppendText($"- Số lượng 'No': {countNo}\n");
                if (countYes > countNo)
                {
                    if (result is null)
                    {
                        result += label;
                    }
                    else
                    {
                        result += " - " + label;
                    }
                }
                rtbMutliLabel.AppendText("\n");

            }
            rtbMutliLabel.AppendText("Kết quả phân loại là : " + result.ToString());
            rtbMutliLabel.ScrollToCaret();
            MessageBox.Show("Kết quả phân loại là : " + result.ToString());
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
        public class Data
        {
            public int GioiTinh { get; set; }
            public int Tuoi { get; set; }
            public int ThuNhap { get; set; }
            public int SoGioRanh { get; set; }
            public int TinhTrangHonNhan { get; set; }
            public string InterestCheckResult { get; set; }
        }

        private void btnRunML_Click(object sender, EventArgs e)
        {

            if (parsedDatas is null)
            {
                MessageBox.Show("Vui lòng mở tệp!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                if (!int.TryParse(txtKML.Text, out int k) || k <= 0)
                {
                    rtbMutliLabel.AppendText("Giá trị không hợp lệ cho K. Vui lòng nhập số nguyên dương.\n");
                    return;
                }
                int gender = cbGender.Checked ? 1 : 0;
                int isMarried = cbKetHon.Checked ? 1 : 0;
                if (!int.TryParse(txtAgeML.Text, out int age) ||
                    !int.TryParse(txtInComeML.Text, out int income) ||
                    !int.TryParse(txtSoGioRanh.Text, out int soGioRanh))
                {
                    MessageBox.Show("Giá trị đầu vào không hợp lệ. Vui lòng kiểm tra đầu vào của bạn.\n");
                    return;
                }
                if (age <= 18)
                {
                    MessageBox.Show("Tuổi phải lớn hơn 18. Vui lòng nhập lại.\n");
                    return;
                }
                int ageGroup = GetAgeGroup(age);
                Data data = new Data { GioiTinh = gender, Tuoi = ageGroup, ThuNhap = income, SoGioRanh = soGioRanh, TinhTrangHonNhan = isMarried };
                result = null;
                distancesML = Predict(data);
                AddCheckBoxesToGroupBox(labels, 7);
                DisplayMultiLabel(k);
            }
        }
        //Navie Bayes
        //public class Customer
        //{
        //    public int Gender { get; set; }
        //    public int Age { get; set; }
        //    public int Income { get; set; }
        //    public int LoanAmount { get; set; }
        //    public string Evaluation { get; set; }
        //}
        //private void ReadData(string filePath)
        //{
        //    try
        //    {

        //        var lines = File.ReadAllLines(filePath);
        //        foreach (var line in lines)
        //        {
        //            var parts = line.Split(',');
        //            if (parts.Length == 5)
        //            {
        //                var data = new Customer
        //                {
        //                    Gender = int.Parse(parts[0].Trim()),
        //                    Age = int.Parse(parts[1].Trim()),
        //                    Income = int.Parse(parts[2].Trim()),
        //                    LoanAmount = int.Parse(parts[3].Trim()),
        //                    Classification = parts[4].Trim()
        //                };
        //                CustomerList.Add(data);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Lỗi đọc tệp: {ex.Message}");
        //    }
        //}

        private void iMenuOpenNB_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Chọn Tệp Dữ Liệu"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Customers.Clear();
                    CustomerList.Clear();
                    var lines = File.ReadAllLines(openFileDialog.FileName);
                    foreach (var line in lines)
                    {
                        var parts = line.Split(',');
                        if (parts.Length == 5)
                        {
                            var Customer = new Customer(
                                int.Parse(parts[0].Trim()),
                                int.Parse(parts[1].Trim()),
                                int.Parse(parts[2].Trim()),
                                int.Parse(parts[3].Trim()),
                                parts[4].Trim()
                            );
                            Customers.Add(Customer);
                            CustomerList.Add(Customer);

                        }
                    }
                    DisplayCustomersInDataGridView();
                    rtbMoTa.AppendText("Dữ liệu đã được tải thành công.\n");
                }
                catch (Exception ex)
                {
                    rtbMoTa.AppendText($"Lỗi khi tải dữ liệu: {ex.Message}\n");
                }
            }
        }
        private void DisplayGroupedData()
        {
            // Tạo danh sách để lưu trữ dữ liệu nhóm
            var groupedData = new List<dynamic>();
            var attributeTotals = new Dictionary<string, (int T, int X)>();
            int overallTotalT = 0;
            int overallTotalX = 0;

            // Gom nhóm dữ liệu theo các thuộc tính và đánh giá
            var attributes = new[]
            {
        ("Giới tính", new[] { "Nam", "Nữ" }),
        ("Độ tuổi", new[] { "18-25", "26-35", "36-50", "51+" }),
        ("Thu nhập", new[] { "Dưới 10 triệu", "10-20 triệu", "Trên 20 triệu" }),
        ("Số tiền vay", new[] { "Dưới 20 triệu", "20-50 triệu", "Trên 50 triệu" })
    };

            // Tạo dữ liệu nhóm cho từng thuộc tính và giá trị của nó
            foreach (var attribute in attributes)
            {
                int attributeTotalT = 0;
                int attributeTotalX = 0;

                foreach (var value in attribute.Item2)
                {
                    int countT = CustomerList.Count(c =>
                    {
                        var attrValue = GetAttributeValue(c, Array.IndexOf(attributes, attribute));
                        return attrValue == value && c.Classification == "T";
                    });

                    int countX = CustomerList.Count(c =>
                    {
                        var attrValue = GetAttributeValue(c, Array.IndexOf(attributes, attribute));
                        return attrValue == value && c.Classification == "X";
                    });

                    // Thêm dữ liệu vào danh sách
                    groupedData.Add(new
                    {
                        Attribute = attribute.Item1,
                        Value = value,
                        CountT = countT,
                        CountX = countX
                    });

                    // Cập nhật tổng số lượng cho thuộc tính và tổng toàn cục
                    attributeTotalT += countT;
                    attributeTotalX += countX;
                    overallTotalT += countT;
                    overallTotalX += countX;
                }

                // Lưu tổng số lượng của thuộc tính hiện tại
                attributeTotals[attribute.Item1] = (attributeTotalT, attributeTotalX);
            }

            // Sắp xếp danh sách dữ liệu theo thuộc tính và giá trị
            groupedData = groupedData.OrderBy(d => Array.IndexOf(attributes.Select(a => a.Item1).ToArray(), d.Attribute))
                                     .ThenBy(d => Array.IndexOf(attributes.First(a => a.Item1 == d.Attribute).Item2, d.Value))
                                     .ToList();

            // Xóa DataGridView trước khi thêm dữ liệu mới
            dgvCount.Rows.Clear();
            dgvCount.Columns.Clear();

            // Thiết lập các cột cho DataGridView
            dgvCount.Columns.Add("Attribute", "Thuộc tính");
            dgvCount.Columns.Add("Value", "Giá trị");
            dgvCount.Columns.Add("Count_T", "Số lượng T");
            dgvCount.Columns.Add("Count_X", "Số lượng X");

            // Thêm dữ liệu vào DataGridView
            foreach (var data in groupedData)
            {
                dgvCount.Rows.Add(data.Attribute, data.Value, data.CountT, data.CountX);
            }
            var dataList = CustomerList.GroupBy(c => c.Classification);
            double countTNB = (dataList.FirstOrDefault(g => g.Key == "T")?.Count() ?? 0);
            double countXNB = (dataList.FirstOrDefault(g => g.Key == "X")?.Count() ?? 0);
            // Thêm dòng tổng cộng cho toàn bộ dữ liệu
            dgvCount.Rows.Add("Tổng cộng", "", countTNB, countXNB);
        }

        private void DisplayClassify()
        {
            var groupedData = CustomerList.GroupBy(c => c.Classification);
            var totalCustomers = CustomerList.Count();

            // Tính xác suất tiên nghiệm cho các lớp
            double probT = (groupedData.FirstOrDefault(g => g.Key == "T")?.Count() ?? 0) / (double)totalCustomers;
            double probX = (groupedData.FirstOrDefault(g => g.Key == "X")?.Count() ?? 0) / (double)totalCustomers;


            var attributes = new[]
            {
        ("Giới tính", new[] { "Nam", "Nữ" }),
        ("Độ tuổi", new[] { "18-25", "26-35", "36-50", "51+" }),
        ("Thu nhập", new[] { "Dưới 10 triệu", "10-20 triệu", "Trên 20 triệu" }),
        ("Số tiền vay", new[] { "Dưới 20 triệu", "20-50 triệu", "Trên 50 triệu" })
    };

            double[][] probTValues = new double[attributes.Length][];
            double[][] probXValues = new double[attributes.Length][];

            // Khởi tạo các giá trị cho probTValues và probXValues
            for (int i = 0; i < attributes.Length; i++)
            {
                probTValues[i] = new double[attributes[i].Item2.Length];
                probXValues[i] = new double[attributes[i].Item2.Length];
            }

            // Xóa DataGridView trước khi thêm dữ liệu mới
            dgvXacSuat.Rows.Clear();
            dgvXacSuat.Columns.Clear();

            // Thiết lập các cột cho DataGridView
            dgvXacSuat.Columns.Add("Attribute", "Thuộc tính");
            dgvXacSuat.Columns.Add("Value", "Giá trị");
            dgvXacSuat.Columns.Add("P_T", "P(T)");
            dgvXacSuat.Columns.Add("P_X", "P(X)");

            foreach (var attribute in attributes)
            {
                var attrIndex = Array.IndexOf(attributes, attribute);

                foreach (var value in attribute.Item2)
                {
                    double probValueT = (groupedData.FirstOrDefault(g => g.Key == "T")?.Count(c => GetAttributeValue(c, attrIndex) == value) ?? 0)
                        / (double)(groupedData.FirstOrDefault(g => g.Key == "T")?.Count() ?? 0);
                    double probValueX = (groupedData.FirstOrDefault(g => g.Key == "X")?.Count(c => GetAttributeValue(c, attrIndex) == value) ?? 0)
                        / (double)(groupedData.FirstOrDefault(g => g.Key == "X")?.Count() ?? 0);

                    probTValues[attrIndex][Array.IndexOf(attribute.Item2, value)] = probValueT;
                    probXValues[attrIndex][Array.IndexOf(attribute.Item2, value)] = probValueX;

                    // Thêm dữ liệu vào DataGridView
                    dgvXacSuat.Rows.Add(attribute.Item1, value, probValueT.ToString("F3"), probValueX.ToString("F3"));
                }
            }

            // Tính xác suất cuối cùng cho từng lớp
            double finalProbT = probT;
            double finalProbX = probX;

            // Thêm một hàng để hiển thị giá trị tiên nghiệm
            dgvXacSuat.Rows.Add("Tiên nghiệm", "", finalProbT.ToString("F3"), finalProbX.ToString("F3"));
        }

        private string Classify(Customer newCustomer)
        {
            var groupedData = CustomerList.GroupBy(c => c.Classification);
            var totalCustomers = CustomerList.Count();

            // Tính xác suất tiên nghiệm cho các lớp
            double probT = (groupedData.FirstOrDefault(g => g.Key == "T")?.Count() ?? 0) / (double)totalCustomers;
            double probX = (groupedData.FirstOrDefault(g => g.Key == "X")?.Count() ?? 0) / (double)totalCustomers;


            var attributes = new[]
            {
        ("Giới tính", new[] { "Nam", "Nữ" }),
        ("Độ tuổi", new[] { "18-25", "26-35", "36-50", "51+" }),
        ("Thu nhập", new[] { "Dưới 10 triệu", "10-20 triệu", "Trên 20 triệu" }),
        ("Số tiền vay", new[] { "Dưới 20 triệu", "20-50 triệu", "Trên 50 triệu" })
    };

            double[][] probTValues = new double[attributes.Length][];
            double[][] probXValues = new double[attributes.Length][];

            // Khởi tạo các giá trị cho probTValues và probXValues
            for (int i = 0; i < attributes.Length; i++)
            {
                probTValues[i] = new double[attributes[i].Item2.Length];
                probXValues[i] = new double[attributes[i].Item2.Length];
            }

            // Xóa DataGridView trước khi thêm dữ liệu mới
            dgvXacSuat.Rows.Clear();
            dgvXacSuat.Columns.Clear();

            // Thiết lập các cột cho DataGridView
            dgvXacSuat.Columns.Add("Attribute", "Thuộc tính");
            dgvXacSuat.Columns.Add("Value", "Giá trị");
            dgvXacSuat.Columns.Add("P_T", "P(T)");
            dgvXacSuat.Columns.Add("P_X", "P(X)");

            foreach (var attribute in attributes)
            {
                var attrIndex = Array.IndexOf(attributes, attribute);

                foreach (var value in attribute.Item2)
                {
                    double probValueT = (groupedData.FirstOrDefault(g => g.Key == "T")?.Count(c => GetAttributeValue(c, attrIndex) == value) ?? 0)
                        / (double)(groupedData.FirstOrDefault(g => g.Key == "T")?.Count() ?? 0);
                    double probValueX = (groupedData.FirstOrDefault(g => g.Key == "X")?.Count(c => GetAttributeValue(c, attrIndex) == value) ?? 0)
                        / (double)(groupedData.FirstOrDefault(g => g.Key == "X")?.Count() ?? 0);

                    probTValues[attrIndex][Array.IndexOf(attribute.Item2, value)] = probValueT;
                    probXValues[attrIndex][Array.IndexOf(attribute.Item2, value)] = probValueX;

                    // Thêm dữ liệu vào DataGridView
                    dgvXacSuat.Rows.Add(attribute.Item1, value, probValueT.ToString("F3"), probValueX.ToString("F3"));
                }

            }


            // Tính xác suất cuối cùng cho từng lớp
            double finalProbT = probT;
            double finalProbX = probX;
            dgvXacSuat.Rows.Add("Tiên nghiệm", "", finalProbT.ToString("F3"), finalProbX.ToString("F3"));

            // Các danh sách để lưu công thức và kết quả tính toán
            var formulaTParts = new List<string> { $"P(T) = {probT:F3}" };
            var formulaXParts = new List<string> { $"P(X) = {probX:F3}" };

            var calTParts = new List<string> { $"\t = {probT:F3}" };
            var calXParts = new List<string> { $"\t = {probX:F3}" };
            foreach (var (attribute, values) in attributes.Select((attr, index) => (attr, probTValues[index])))
            {
                var attributeValue = GetAttributeValue(newCustomer, Array.IndexOf(attributes, attribute));
                var valueIndex = Array.IndexOf(attribute.Item2, attributeValue);
                finalProbT *= probTValues[Array.IndexOf(attributes, attribute)][valueIndex];
                finalProbX *= probXValues[Array.IndexOf(attributes, attribute)][valueIndex];
                formulaTParts.Add($"P({attribute.Item1}={attributeValue}|T)\t\t\n");
                calTParts.Add($"{probTValues[Array.IndexOf(attributes, attribute)][valueIndex]:F3}");

                formulaXParts.Add($"P({attribute.Item1}={attributeValue}|X)\t\t\n");
                calXParts.Add($"{probXValues[Array.IndexOf(attributes, attribute)][valueIndex]:F3}");
            }
            string finalFormulaT = string.Join(" * ", formulaTParts);
            string finalFormulaX = string.Join(" * ", formulaXParts);
            string finalCalculationsT = string.Join(" * ", calTParts);
            string finalCalculationsX = string.Join(" * ", calXParts);
            string message = $"Công thức tính xác suất cho lớp T:\n" +
                             $"{finalFormulaT}" +
                             $"{finalCalculationsT}" + $" = {finalProbT:F3}\n\n" +
                             $"Công thức tính xác suất cho lớp X:\n" +
                             $"{finalFormulaX}" +
                             $"{finalCalculationsX}" + $" = {finalProbX:F3}\n\n" +
                             $"Xác suất cuối cùng cho lớp T: {finalProbT:F3}\n" +
                             $"Xác suất cuối cùng cho lớp X: {finalProbX:F3}\n\n" +
                             $"Kết quả phân loại: {(finalProbT > finalProbX ? "T" : "X")}";
            rtbKetQuaNB.AppendText("\n" + message);
            return finalProbT > finalProbX ? "Tốt" : "Xấu";

        }
        private string GetAttributeValue(Customer customer, int index)
        {
            switch (index)
            {
                case 0:
                    return customer.Gender == 1 ? "Nam" : "Nữ";
                case 1:
                    if (customer.Age == 1)
                        return "18-25";
                    else if (customer.Age == 2)
                        return "26-35";
                    else if (customer.Age == 3)
                        return "36-50";
                    else
                        return "51+";
                case 2:
                    if (customer.Income < 10)
                        return "Dưới 10 triệu";
                    else if (customer.Income < 20)
                        return "10-20 triệu";
                    else
                        return "Trên 20 triệu";
                case 3:
                    if (customer.LoanAmount < 20)
                        return "Dưới 20 triệu";
                    else if (customer.LoanAmount < 50)
                        return "20-50 triệu";
                    else
                        return "Trên 50 triệu";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void btnHuanLuyen_Click(object sender, EventArgs e)
        {
            DisplayGroupedData();
            DisplayClassify();
        }

        private void btnRunClassified_Click(object sender, EventArgs e)
        {
            if (CustomerList.Count > 0)
            {

                int gender = cbGenderNB.Checked ? 1 : 0;
                if (!int.TryParse(txtAgeNB.Text, out int age) ||
                    !int.TryParse(txtIncomeNB.Text, out int income) ||
                    !int.TryParse(txtAmountNB.Text, out int loanAmount))
                {
                    MessageBox.Show("Giá trị đầu vào không hợp lệ. Vui lòng kiểm tra đầu vào của bạn.\n");
                    return;
                }

                if (age <= 18)
                {
                    MessageBox.Show("Tuổi phải lớn hơn 18. Vui lòng nhập lại.\n");
                    return;
                }
                int ageGroup = GetAgeGroup(age);
                newCustomer = new Customer
                {
                    Gender = gender,
                    Age = ageGroup,
                    Income = income,
                    LoanAmount = loanAmount
                };
                string result = Classify(newCustomer);
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedChoose = tabControl1.SelectedIndex;
            switch (selectedChoose)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
            }
        }
        /// <summary>
        /// Cây quyết định
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // Lớp đại diện cho khách hàng        

        private void btnRunCQD_Click(object sender, EventArgs e)
        {

            int gender = cbGenderCQD.Checked ? 1 : 0;
            if (!int.TryParse(txtAgeCQD.Text, out int age) ||
                !int.TryParse(txtIncomeCQD.Text, out int income) ||
                !int.TryParse(txtAmountCQD.Text, out int loanAmount))
            {
                MessageBox.Show("Giá trị đầu vào không hợp lệ. Vui lòng kiểm tra đầu vào của bạn.\n");
                return;
            }

            if (age <= 18)
            {
                MessageBox.Show("Tuổi phải lớn hơn 18. Vui lòng nhập lại.\n");
                return;
            }
            int ageGroup = GetAgeGroup(age);
            Customer newCustomerCQD = new Customer(gender, ageGroup, income, loanAmount);

            Node tree = DecisionTree.BuildTree(Customers, Lables);
            string classification = DecisionTree.Classify(newCustomerCQD, tree);

            rtbResultCQD.AppendText($"Thông tin khách hàng:\n{newCustomerCQD}+Khách hàng mới được phân loại là: {classification}");
            MessageBox.Show($"Khách hàng mới được phân loại là: {classification}");
        }
        private void DrawTreeView(Node root)
        {
            treeView1.Nodes.Clear();
            TreeNode rootNode = CreateTreeNode(root, null);
            treeView1.Nodes.Add(rootNode);
            treeView1.ExpandAll();
        }
        private TreeNode CreateTreeNode(Node node, string attributeLabel)
        {
            if (node == null)
                return null;
            string nodeText = node.LeafValue ?? node.Attribute;
            if (attributeLabel != null)
            {
                nodeText = $"[{attributeLabel}]--------->{nodeText} ";
            }

            TreeNode treeNode = new TreeNode(nodeText);
            if (node.Children != null && node.Children.Count > 0)
            {
                foreach (var child in node.Children)
                {
                    string childAttributeLabel = GetAttributeLabel(node.Attribute, child.Key);
                    TreeNode childNode = CreateTreeNode(child.Value, childAttributeLabel);
                    treeNode.Nodes.Add(childNode);
                }
            }

            return treeNode;
        }

        private string GetAttributeLabel(string attribute, int value)
        {
            switch (attribute)
            {
                case "Gender":
                    return value == 1 ? "Nam" : "Nữ";

                case "Age":
                    return value == 1 ? "Độ tuổi từ 18->25" :
                         value == 2 ? "Độ tuổi từ 26->35" :
                         value == 3 ? "Độ tuổi từ 36->50" : "Độ tuổi từ 51+";

                case "Income":
                    return value == 1 ? "Thu nhập 0->10 triệu" :
                          value == 2 ? "Thu nhập 10->20 triệu" :
                          value == 3 ? "Thu nhập 20->50 triệu" : "Thu nhập 51+ triệu";

                case "LoanAmount":
                    return value == 1 ? "Tiền vay 0->10 triệu" :
                            value == 2 ? "Tiền vay 10->20 triệu" :
                            value == 3 ? "Tiền vay 20->50 triệu" : "Tiền vay 51+ triệu";
                default:
                    return value.ToString();
            }
        }
        private void DisplayDecisionTree()
        {
            Node tree = DecisionTree.BuildTree(Customers, Lables);
            DrawTreeView(tree);
        }

        private void DisplayCustomersDescsionTree()
        {
            dgvDataCQD.ColumnCount = 5;
            dgvDataCQD.Columns[0].Name = "STT";
            dgvDataCQD.Columns[1].Name = "Tuổi";
            dgvDataCQD.Columns[2].Name = "Thu nhập";
            dgvDataCQD.Columns[3].Name = "Tiền vay";
            dgvDataCQD.Columns[4].Name = "Đánh giá";

            dgvDataCQD.Rows.Clear();
            for (int i = 0; i < Customers.Count; i++)
            {
                Customer customer = Customers[i];
                dgvDataCQD.Rows.Add(i + 1, customer.Age, customer.Income, customer.LoanAmount, customer.Classification);
            }
        }


    }
}

