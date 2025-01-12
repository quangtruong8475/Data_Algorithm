using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ComboBox = System.Windows.Forms.ComboBox;

namespace AssociationRule
{
    public partial class UpdateData : Form
    {
        private List<ComboBox> comboBoxes = new List<ComboBox>();
        private int selectedRowIndex = -1;
        public UpdateData()
        {
            InitializeComponent();
           // CreateComboBoxes(dgvData);
        }
        public event Action<DataGridView> DataUpdated;
        private void UpdateData_Load(object sender, EventArgs e)
        {

        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (comboBoxes.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để thêm!");
                return;
            }

            // Kiểm tra từng ComboBox để đảm bảo rằng tất cả đều có giá trị được chọn
            foreach (var comboBox in comboBoxes)
            {
                if (comboBox.SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn giá trị cho tất cả các cột!");
                    return;
                }
            }

            RemoveTotalRow();

            object[] rowValues = new object[comboBoxes.Count + 1];
            rowValues[0] = dgvDataForm2.Rows.Count;

            for (int i = 0; i < comboBoxes.Count; i++)
            {
                rowValues[i + 1] = comboBoxes[i].SelectedItem;
            }

            dgvDataForm2.Rows.Add(rowValues);
            AddTotalRow();
        }

        private void RemoveTotalRow()
        {          
            foreach (DataGridViewRow row in dgvDataForm2.Rows)
            {
                if (row.Cells[0].Value != null && row.Cells[0].Value.ToString() == "Total")
                {
                    dgvDataForm2.Rows.Remove(row);
                    break;
                }
            }
        }

        private void AddTotalRow()
        {
            // Tạo mảng object để lưu trữ giá trị tổng
            int columnCount = dgvDataForm2.Columns.Count;
            object[] totalRow = new object[columnCount];

            // Đặt giá trị cột đầu tiên là "Total"
            totalRow[0] = "Total";

            // Tính tổng cho từng cột
            for (int col = 1; col < columnCount; col++)
            {
                int sum = 0;
                foreach (DataGridViewRow row in dgvDataForm2.Rows)
                {
                    if (row.Cells[col].Value != null && row.Cells[0].Value.ToString() != "Total")
                    {
                        sum += Convert.ToInt32(row.Cells[col].Value);
                    }
                }
                totalRow[col] = sum;
            }

            // Thêm hàng tổng vào dgvDataForm2
            dgvDataForm2.Rows.Add(totalRow);

            // Định dạng hàng tổng (tùy chọn)
            DataGridViewRow totalDataRow = dgvDataForm2.Rows[dgvDataForm2.Rows.Count - 1];
            totalDataRow.DefaultCellStyle.BackColor = Color.LightGray;
            totalDataRow.DefaultCellStyle.Font = new Font(dgvDataForm2.Font, FontStyle.Bold);
        }

        public void DisplayDataInForm2(DataGridView dgvData)
        {
            dgvDataForm2.Columns.Clear();
            foreach (DataGridViewColumn column in dgvData.Columns)
            {
                dgvDataForm2.Columns.Add((DataGridViewColumn)column.Clone());
            }

            foreach (DataGridViewRow row in dgvData.Rows)
            {
                if (!row.IsNewRow)
                {
                    int rowIndex = dgvDataForm2.Rows.Add();
                    for (int i = 0; i < row.Cells.Count; i++)
                    {
                        dgvDataForm2.Rows[rowIndex].Cells[i].Value = row.Cells[i].Value;
                    }
                }
            }
            CreateComboBoxes(dgvData);
        }
        private void CreateComboBoxes(DataGridView dgvData)
        {
            // Xóa các ComboBox và Label cũ
            foreach (var comboBox in comboBoxes)
            {
                panel1.Controls.Remove(comboBox);
            }
            comboBoxes.Clear();

            var labels = panel1.Controls.OfType<Label>().ToList();
            foreach (var label in labels)
            {
                panel1.Controls.Remove(label);
            }

            // Tạo các ComboBox và Label mới
            int columnCount = dgvData.Columns.Count;
            int comboBoxWidth = (int)(panel1.Width * 0.7);
            int labelWidth = 50;
            int spacing = 10; // Khoảng cách giữa các ComboBox và Label
            int totalWidth = comboBoxWidth + labelWidth + spacing;

            // Tính toán chiều cao mỗi dòng và khoảng cách giữa các dòng
            int lineHeight = (panel1.Height - spacing * (columnCount - 1)) / columnCount;
            int verticalSpacing = (panel1.Height - (lineHeight * columnCount)) / (columnCount + 1);

            for (int i = 1; i < columnCount; i++)
            {
                int yPos = verticalSpacing + i * (lineHeight + verticalSpacing);

                Label label = new Label
                {
                    Text = "Cột " + i,
                    AutoSize = true,
                    Location = new Point((panel1.Width - totalWidth) / 2, yPos)
                };
                panel1.Controls.Add(label);

                ComboBox comboBox = new ComboBox
                {
                    Name = "cmbColumn" + i,
                    Width = comboBoxWidth,
                    Location = new Point(label.Right + spacing, yPos)
                };
                comboBox.Items.Add("0");
                comboBox.Items.Add("1");
                comboBox.SelectedIndex = 1; // Đặt giá trị mặc định là 1
                panel1.Controls.Add(comboBox);
                comboBoxes.Add(comboBox);
            }
        }
        private void dgvData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Kiểm tra xem người dùng đã nhấp vào hàng hợp lệ hay chưa
            if (e.RowIndex >= 0 && e.RowIndex < dgvDataForm2.Rows.Count)
            {
                DataGridViewRow row = dgvDataForm2.Rows[e.RowIndex];

                // Lấy dữ liệu từ hàng và gán cho các ComboBox
                for (int i = 0; i < comboBoxes.Count; i++)
                {
                    comboBoxes[i].SelectedItem = row.Cells[i + 1].Value?.ToString();
                }
                selectedRowIndex = e.RowIndex;
            }
        }
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // Kiểm tra xem có hàng nào được chọn hay không
            if (selectedRowIndex >= 0 && selectedRowIndex < dgvDataForm2.Rows.Count)
            {
                DataGridViewRow row = dgvDataForm2.Rows[selectedRowIndex];

                // Cập nhật giá trị của hàng được chọn bằng giá trị từ các ComboBox
                for (int i = 0; i < comboBoxes.Count; i++)
                {
                    row.Cells[i + 1].Value = comboBoxes[i].SelectedItem;
                }

                // Cập nhật lại hàng "Total"
                RemoveTotalRow();
                AddTotalRow();
            }
            else
            {
                MessageBox.Show("Chọn một hàng để cập nhật!");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Kiểm tra xem có hàng nào được chọn hay không
            if (selectedRowIndex >= 0 && selectedRowIndex < dgvDataForm2.Rows.Count)
            {
                // Xóa hàng được chọn
                dgvDataForm2.Rows.RemoveAt(selectedRowIndex);

                // Cập nhật lại hàng "Total"
                RemoveTotalRow();
                AddTotalRow();

                // Đặt selectedRowIndex về giá trị ban đầu
                selectedRowIndex = -1;
            }
            else
            {
                MessageBox.Show("Chọn một hàng để xóa!");
            }
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            // Kích hoạt sự kiện khi nhấn nút Save
            DataUpdated?.Invoke(dgvDataForm2);
            this.Close();
        }
    }
}
