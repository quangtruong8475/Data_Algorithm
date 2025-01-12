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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Classifier
{
    public partial class K_NearestNeighbors : UserControl
    {
        private List<Customer> Customers = new List<Customer>();
        private int currentStep = 1; 
        private List<(Customer Customer, double Distance)> distances;
        private List<(Customer Customer, double Distance)> sortedDistances;
        private List<(Customer Customer, double Distance)> nearestNeighbors;
        private string finalResult;
        public K_NearestNeighbors()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            dgvData.ColumnCount = 5;
            dgvData.Columns[0].Name = "STT";
            dgvData.Columns[1].Name = "Tuổi";
            dgvData.Columns[2].Name = "Thu nhập";
            dgvData.Columns[3].Name = "Tiền vay";
            dgvData.Columns[4].Name = "Đánh giá";
            //
            dgvResult.ColumnCount = 6;
            dgvResult.Columns[0].Name = "STT";
            dgvResult.Columns[1].Name = "Tuổi";
            dgvResult.Columns[2].Name = "Thu nhập";
            dgvResult.Columns[3].Name = "Tiền vay";
            dgvResult.Columns[4].Name = "Đánh giá";
            dgvResult.Columns[5].Name = "Khoảng cách";

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

                            // Hiển thị thông tin Customer trong RichTextBox
                            rtbMoTa.AppendText(
                                $"Giới tính: {Customer.Gender}, Tuổi: {Customer.Age}, Thu nhập: {Customer.Income}, Số tiền vay: {Customer.LoanAmount}, Phân loại: {Customer.Classification}\n"
                            );
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

        private void buttonRunAlgorithm_Click(object sender, EventArgs e)
        {
           
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
                // Bước 2: Sắp xếp khoảng cách
                sortedDistances = KNNAlgorithm.SortDistances(distances);
                rtbMoTa.AppendText("\n2. Sắp xếp khoảng cách:\n");
                foreach (var item in sortedDistances)
                {
                    rtbMoTa.AppendText($"   Điểm dữ liệu({item.Customer.Gender}, {item.Customer.Age}, {item.Customer.Income}, {item.Customer.LoanAmount}): Khoảng cách = ");

                    // Hiển thị khoảng cách
                    rtbMoTa.SelectionColor = Color.Black;
                    rtbMoTa.AppendText($"{item.Distance.ToString("F2")}, ");

                    // Hiển thị loại phân loại
                    rtbMoTa.SelectionColor = Color.Black;
                    rtbMoTa.AppendText($"Thuộc loại = {item.Customer.Classification}\n");

                    // Reset màu sắc cho văn bản tiếp theo
                    rtbMoTa.SelectionColor = Color.Black;
                }
                currentStep = 3; // Chuyển sang bước tiếp theo
            }
            else if (currentStep == 3)
            {
                // Bước 3: Lấy k hàng xóm gần nhất
                nearestNeighbors = KNNAlgorithm.GetNearestNeighbors(sortedDistances, k);
                rtbMoTa.AppendText($"\n3. {k} Điểm gần nhất:\n");
                foreach (var neighbor in nearestNeighbors)
                {
                    rtbMoTa.AppendText($"   Điểm dữ liệu({neighbor.Customer.Gender}, {neighbor.Customer.Age}, {neighbor.Customer.Income}, {neighbor.Customer.LoanAmount}): ");

                    // Hiển thị loại phân loại
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
                rtbMoTa.AppendText($"   Điểm dữ liệu ({item.Customer.Gender}, {item.Customer.Age}, {item.Customer.Income}, {item.Customer.LoanAmount}): Khoảng cách = {item.Distance}, Thuộc loại: {item.Customer.Classification}\n");
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



        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void iMenuExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
