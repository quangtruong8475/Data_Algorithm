using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace AssociationRule
{
    public partial class AssciationRule : Form
    {
        private List<int[]> datas;
        private List<(Tuple<int[], int[]>, double)> validRules;

        private string[] columnHeaders;
        public AssciationRule()
        {
            InitializeComponent();
            richtexboxKQ.Dock = DockStyle.Fill;
            richTextBoxMoTa.Dock = DockStyle.Fill;

        }
        private void AssciationRule_Load(object sender, EventArgs e)
        {
            comboxAssciationRule.SelectedIndex = 0;
        }

        private static IEnumerable<IEnumerable<T>> TaoToHop<T>(IEnumerable<T> elements, int k)
        {
            if (k == 0)
            {
                yield return new T[0];
                yield break;
            }
            var list = elements.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                var head = list.Skip(i).Take(1);
                var tailCombinations = TaoToHop(list.Skip(i + 1), k - 1);
                foreach (var tail in tailCombinations)
                {
                    yield return head.Concat(tail);
                }
            }
        }
        private List<int[]> TaoDanhSachToHop(List<int> items, int k)
        {
            var dsToHop = TaoToHop(items, k);
            var itemsets = dsToHop.Select(c => c.ToArray()).ToList();
            return itemsets;
        }
        private List<int[]> DanhSachToHop(List<int> items)
        {
            int k = 2;
            List<int[]> allItemsets = new List<int[]>();
            while (true)
            {
                var itemsets = TaoDanhSachToHop(items, k);
                if (!itemsets.Any()) break;
                allItemsets.AddRange(itemsets);
                k++;
            }
            return allItemsets;
        }
        /// <summary>
        /// Luận kết hợp cơ bản
        /// </summary>
        /// <param name="items"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        private double CalculateSupport(List<int[]> datas, int[] items)
        {
            int count = datas.Count(data => items.All(item => data[item - 1] == 1));
            double supp = (double)count / datas.Count;
            return supp;
        }
        private int CalculateCount(List<int[]> datas, int[] items)
        {
            int count = datas.Count(data => items.All(item => data[item - 1] == 1));
            return count;
        }
        private double CalculateConfidence(List<int[]> datas, Tuple<int[], int[]> rule)
        {
            var left = rule.Item1;
            var right = rule.Item2;
            double leftSupport = CalculateSupport(datas, left);
            double ruleSupport = CalculateSupport(datas, left.Concat(right).ToArray());

            return leftSupport == 0 ? 0 : ruleSupport / leftSupport;
        }
        /// <summary>
        /// hàm chạy cơ bản
        /// </summary>
        /// <param name="itemset"></param>
        /// <returns></returns>
        private List<Tuple<int[], int[]>> TaoLuatKetHop(List<int[]> itemset)
        {
            var rules = new List<Tuple<int[], int[]>>();
            foreach (var item in itemset)
            {
                if (item.Length > 1)
                {
                    for (int i = 1; i < item.Length; i++)
                    {
                        var left = TaoToHop(item, i);
                        foreach (var leftItem in left)
                        {
                            var right = item.Except(leftItem).ToArray();
                            rules.Add(new Tuple<int[], int[]>(leftItem.ToArray(), right));
                        }
                    }
                }
            }
            return rules;
        }
        private List<(List<int[]>, List<(Tuple<int[], int[]>, double)>)> RunAssociationRules(List<int[]> datas, double minSup, double minConf)
        {
            var items = Enumerable.Range(1, datas[0].Length).ToList();
            var results = new List<(List<int[]>, List<(Tuple<int[], int[]>, double)>)>();
            validRules = new List<(Tuple<int[], int[]>, double)>();
            var itemsets = DanhSachToHop(items);
            var frequentItemsets = itemsets.Where(itemset => CalculateSupport(datas, itemset) >= minSup).ToList();
            var rules = TaoLuatKetHop(frequentItemsets);
            var resultRules = rules.Select(rule => (rule, CalculateConfidence(datas, rule))).ToList();
            foreach (var rule in resultRules)
            {
                if (rule.Item2 >= minConf)
                {
                    validRules.Add(rule);
                }
            }
            results.Add((itemsets, resultRules));
            return results;
        }
        private void DisplayResults(List<(List<int[]>, List<(Tuple<int[], int[]>, double)>)> results, double minSup, double minConf, List<string> itemNames)
        {
            richTextBoxMoTa.Clear();

            for (int step = 0; step < results.Count; step++)
            {
                richTextBoxMoTa.AppendText($"Bước {step + 1}\n");
                richTextBoxMoTa.AppendText($"Tập hợp con có 2 phần tử trở lên:\n");
                int itemsetIndex = 1;
                foreach (var itemset in results[step].Item1)
                {
                    string itemsetStr = string.Join(", ", itemset.Select(index => itemNames[index - 1]));
                    richTextBoxMoTa.AppendText($"[{itemsetIndex++}] {{{itemsetStr}}}\n");
                }
                itemsetIndex = 1;
                richTextBoxMoTa.AppendText("Bước 2\nTính Support của các tập hợp:\n");
                foreach (var itemset in results[step].Item1)
                {
                    int suppCount = CalculateCount(datas, itemset);
                    double support = CalculateSupport(datas, itemset.ToArray());
                    string status = support >= minSup ? "Nhận" : "Loại";
                    richTextBoxMoTa.SelectionColor = support >= minSup ? Color.Red : Color.Black;
                    string itemsetStr = string.Join(", ", itemset.Select(index => itemNames[index - 1]));
                    richTextBoxMoTa.AppendText($"[{itemsetIndex++}] {{{itemsetStr}}} - Supp: {suppCount}/{datas.Count} = {support:F2} - {status}\n");
                }
                richTextBoxMoTa.AppendText("Bước 3\nLuật kết hợp (thỏa mãn điều kiện):\n");
                int ruleIndex = 1;
                foreach (var (rule, confidence) in results[step].Item2)
                {
                    var left = rule.Item1;
                    var right = rule.Item2;
                    int confCountLeft = CalculateCount(datas, left);
                    int confCountBoth = CalculateCount(datas, left.Concat(right).ToArray());
                    string ruleStatus = confidence >= minConf ? "Nhận" : "Loại";
                    richTextBoxMoTa.SelectionColor = confidence >= minConf ? Color.Red : Color.Black;
                    string leftStr = string.Join(", ", left.Select(index => itemNames[index - 1]));
                    string rightStr = string.Join(", ", right.Select(index => itemNames[index - 1]));
                    richTextBoxMoTa.AppendText($"[{ruleIndex++}] {{{leftStr}}} ==> {{{rightStr}}} - Conf: {confCountBoth}/{confCountLeft} = {confidence:F2} - {ruleStatus}\n");
                }
                richTextBoxMoTa.AppendText("\n");
            }

            richTextBoxMoTa.SelectionColor = Color.Black;
        }
        private void DisplayValidRules(List<string> itemNames)
        {
            richtexboxKQ.Clear();

            if (validRules == null || validRules.Count == 0)
            {
                richtexboxKQ.AppendText("Không có luật kết hợp nào thỏa mãn.\n");
                return;
            }

            richtexboxKQ.AppendText("Luật kết hợp (thỏa mãn điều kiện):\n");

            var displayedRules = new HashSet<string>();
            int ruleIndex = 1;

            foreach (var rule in validRules)
            {
                var antecedent = string.Join(", ", rule.Item1.Item1.Select(index => itemNames[index - 1]));
                var consequent = string.Join(", ", rule.Item1.Item2.Select(index => itemNames[index - 1]));
                var confidence = rule.Item2;

                // Xây dựng chuỗi luật theo cấu trúc yêu cầu
                var ruleString = $"{ruleIndex}. {{{antecedent}}} => {{{consequent}}} conf={confidence:F2} --> Nếu mua {{{antecedent}}} thì mua {{{consequent}}}\n";

                if (!displayedRules.Contains(ruleString))
                {
                    richtexboxKQ.AppendText(ruleString);
                    displayedRules.Add(ruleString);
                    ruleIndex++;
                }
            }
        }
        /// <summary>
        /// Run chạy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRun_Click(object sender, EventArgs e)
        {
            if (datas == null || datas.Count == 0)
            {
                MessageBox.Show("Vui lòng mở tệp dữ liệu trước!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!double.TryParse(txtMinSup.Text, out double minSup) || !double.TryParse(txtMinConf.Text, out double minConf))
            {
                MessageBox.Show("Vui lòng nhập giá trị hợp lệ cho MinSup và MinConf!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var itemNames = new List<string>(columnHeaders);
            int chooseIndex=comboxAssciationRule.SelectedIndex;
            switch (chooseIndex)
            {
                case 0:
                    var results = RunAssociationRules(datas, minSup, minConf);
                    DisplayResults(results, minSup, minConf, itemNames);
                    DisplayValidRules(itemNames);
                    break;

                case 1:
                    var aprioriResults = RunAprioriAlgorithm1(datas, minSup, minConf);

                    if (aprioriResults != null && aprioriResults.Count > 0)
                    {
                        DisplayResults1(aprioriResults, minSup, minConf, itemNames);
                        var associationRules = aprioriResults
                            .SelectMany(result => result.Item2)
                            .Select(rule => new Tuple<int[], int[], double>(rule.Item1.Item1, rule.Item1.Item2, rule.Item2))
                            .ToList();

                        DisplayAssociationRules1(associationRules, minConf, itemNames); // Truyền itemNames vào đây
                    }
                    else
                    {
                        richTextBoxMoTa.Clear();
                        richTextBoxMoTa.AppendText("Không có tập mục hoặc luật kết hợp nào để hiển thị.");
                    }
                    break;

                default:
                    MessageBox.Show("Lựa chọn không hợp lệ trong ComboBox.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }
        /// <summary>
        /// Nâng cao 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elements"></param>
        /// <param name="k"></param>
        /// <returns></returns>

        private void DisplayResults1(List<(List<(int[], double)>, List<(Tuple<int[], int[]>, double)>)> results, double minSup, double minConf, List<string> itemNames)
        {
            richTextBoxMoTa.Clear();

            // Giả sử bạn có một danh sách chứa tất cả dữ liệu (datas)
            List<int[]> datas = this.datas; // Thay đổi hàm này để lấy dữ liệu của bạn

            // Lọc tất cả các tập hợp con từ kết quả
            var frequentItemsets = results.SelectMany(r => r.Item1).ToList();

            // Nhóm các tập hợp con theo số lượng phần tử
            var itemsetsGroupedByLength = frequentItemsets
                .GroupBy(itemset => itemset.Item1.Length)
                .OrderBy(g => g.Key);

            // Đoạn mã này sẽ dùng để lưu trữ các tập hợp con đã được hiển thị
            int itemIndex = 1;

            // Bước 1: Liệt kê tất cả các tập hợp con có 2 phần tử mà không tính toán support
            var allItemsets = new HashSet<string>();
            var itemsetsList = new List<(int[], string)>();

            foreach (var group in itemsetsGroupedByLength)
            {
                if (group.Key == 2)
                {
                    richTextBoxMoTa.AppendText($"Tập hợp có {group.Key} phần tử:\n");

                    foreach (var itemset in group)
                    {
                        var itemsetStr = string.Join(", ", itemset.Item1
                            .Where(idx => idx > 0 && idx <= itemNames.Count)
                            .Select(idx => itemNames[idx - 1]));

                        var sortedItemsetStr = string.Join(", ", itemset.Item1.OrderBy(idx => idx));
                        if (!allItemsets.Contains(sortedItemsetStr))
                        {
                            allItemsets.Add(sortedItemsetStr);
                            itemsetsList.Add((itemset.Item1, itemsetStr));
                            richTextBoxMoTa.AppendText($"[{itemIndex++}] {{ {itemsetStr} }}\n");
                        }
                    }
                }
            }

            // Bước 2: Tính toán support cho các tập hợp con đã liệt kê
            richTextBoxMoTa.AppendText("Tính Supp:\n");
            itemIndex = 1; // Reset lại chỉ số
            var validItemsets = new List<int[]>();

            foreach (var itemset in itemsetsList)
            {
                var itemsetStr = itemset.Item2;
                int count = CalculateSupport1Count1(datas, itemset.Item1);
                double support = CalculateSupport1(datas, itemset.Item1);

                string status = support >= minSup ? "Nhận" : "Loại";
                Color color = support >= minSup ? Color.Red : Color.Black;

                // Lưu tập hợp con hợp lệ nếu support >= minSup
                if (support >= minSup)
                {
                    validItemsets.Add(itemset.Item1);
                }

                // Hiển thị kết quả tính toán support
                richTextBoxMoTa.SelectionColor = color;
                richTextBoxMoTa.AppendText($"[{itemIndex++}] {{ {itemsetStr} }} - Supp: {count}/{datas.Count} = {support:F2} - {status}\n");             
            }


            // Bước 3: Tiếp tục với các tập hợp con có 3 phần tử trở lên
            var currentItemsets = validItemsets;

            while (currentItemsets.Any())
            {
                int currentLength = currentItemsets.First().Length + 1;
                if (currentLength > itemNames.Count)
                    break;

                richTextBoxMoTa.AppendText($"Tập hợp có {currentLength} phần tử:\n");
                var newItemsets = new HashSet<string>();
                var newItemsetsList = new List<(int[], string)>();

                // Tạo các tập hợp con có kích thước hiện tại
                for (int i = 0; i < currentItemsets.Count; i++)
                {
                    for (int j = i + 1; j < currentItemsets.Count; j++)
                    {
                        var combined = currentItemsets[i].Union(currentItemsets[j]).Distinct().ToArray();
                        if (combined.Length == currentLength)
                        {
                            var itemsetStr = string.Join(", ", combined
                                .Where(idx => idx > 0 && idx <= itemNames.Count)
                                .Select(idx => itemNames[idx - 1]));

                            var sortedCombinedStr = string.Join(", ", combined.OrderBy(idx => idx));
                            if (!newItemsets.Contains(sortedCombinedStr))
                            {
                                newItemsets.Add(sortedCombinedStr);
                                newItemsetsList.Add((combined, itemsetStr));
                                richTextBoxMoTa.AppendText($"[{itemIndex++}] {{ {itemsetStr} }}\n");
                            }
                        }
                    }
                }

                // Bước 4: Tính toán support cho các tập hợp con đã liệt kê
                richTextBoxMoTa.AppendText("Tính Supp:\n");
                itemIndex = 1; // Reset lại chỉ số
                currentItemsets = new List<int[]>();

                foreach (var itemset in newItemsetsList)
                {
                    var itemsetStr = itemset.Item2;
                    int count = CalculateSupport1Count1(datas, itemset.Item1);
                    double support = CalculateSupport1(datas, itemset.Item1);

                    string status = support >= minSup ? "Nhận" : "Loại";
                    Color color = support >= minSup ? Color.Red : Color.Black;

                    // Chỉ thêm vào danh sách hợp lệ nếu support >= minSup
                    if (support >= minSup)
                    {
                        currentItemsets.Add(itemset.Item1);
                    }

                    // Hiển thị kết quả tính toán support
                    richTextBoxMoTa.SelectionColor = color;
                    richTextBoxMoTa.AppendText($"[{itemIndex++}] {{ {itemsetStr} }} - Supp: {count}/{datas.Count} = {support:F2} - {status}\n");

                               
                }
            }
            // Bước 5: Hiển thị các luật kết hợp
            richTextBoxMoTa.AppendText("Các luật kết hợp và tính Conf:\n");
            var rules = results.SelectMany(r => r.Item2).ToList();
            int ruleIndex = 1;
            foreach (var rule in rules)
            {
                var (left, right) = rule.Item1;
                double confidence = rule.Item2;
                int countLeft = CalculateSupport1Count1(datas, left);
                int countBoth = CalculateSupport1Count1(datas, left.Concat(right).ToArray());
                string leftStr = left.Length > 0 ? string.Join(", ", left.Where(index => index > 0 && index <= itemNames.Count).Select(index => itemNames[index - 1])) : "{}";
                string rightStr = right.Length > 0 ? string.Join(", ", right.Where(index => index > 0 && index <= itemNames.Count).Select(index => itemNames[index - 1])) : "{}";
                string ruleStatus = confidence >= minConf ? "Nhận" : "Loại";
                Color color = confidence >= minConf ? Color.Red : Color.Black;
                richTextBoxMoTa.SelectionColor = color;
                richTextBoxMoTa.AppendText(
                    $"{ruleIndex++}. {{ {leftStr} }} => {{ {rightStr} }} - Conf = {countBoth}/{countLeft} = {confidence:F2} {ruleStatus}\n");
            }

            richTextBoxMoTa.SelectionColor = Color.Black;
        }

        private List<(List<(int[], double)>, List<(Tuple<int[], int[]>, double)>)> RunAprioriAlgorithm1(List<int[]> datas, double minSup, double minConf)
        {
            var results = new List<(List<(int[], double)>, List<(Tuple<int[], int[]>, double)>)>();
            validRules = new List<(Tuple<int[], int[]>, double)>();


            var items = Enumerable.Range(0, datas[0].Length).Select(i => new int[] { i + 1 }).ToList();

            while (items.Any())
            {

                var currentFrequentItemsets = items
                    .Select(itemset => (itemset, support: CalculateSupport1(datas, itemset)))
                    .Where(tuple => tuple.support >= minSup)
                    .ToList();
                var rules = GenerateAssociationRules1(currentFrequentItemsets.Select(tuple => tuple.itemset).ToList());
                var resultRules = rules.Select(rule => (rule, CalculateConfidence1(datas, rule))).ToList();

                foreach (var rule in resultRules)
                {
                    if (rule.Item2 >= minConf)
                    {
                        validRules.Add(rule);
                    }
                }
                results.Add((currentFrequentItemsets, resultRules));
                items = GenerateNewCombinations1(currentFrequentItemsets.Select(tuple => tuple.itemset).ToList());
            }

            return results;
        }
        private List<int[]> GenerateNewCombinations1(List<int[]> frequentItemsets)
        {
            var newCombinations = new List<int[]>();

            if (frequentItemsets.Count == 0 || frequentItemsets[0] == null || frequentItemsets[0].Length == 0)
            {
                return newCombinations;
            }
            int length = frequentItemsets[0].Length;

            for (int i = 0; i < frequentItemsets.Count; i++)
            {
                for (int j = i + 1; j < frequentItemsets.Count; j++)
                {
                    var first = frequentItemsets[i];
                    var second = frequentItemsets[j];
                    if (first.Take(length - 1).SequenceEqual(second.Take(length - 1)))
                    {
                        var newItemset = first.Take(length - 1).Concat(new[] { first[length - 1], second[length - 1] }).Distinct().ToArray();
                        if (!newCombinations.Any(itemset => itemset.SequenceEqual(newItemset)))
                        {
                            newCombinations.Add(newItemset);
                        }
                    }
                }
            }
            return newCombinations;
        }
        private static IEnumerable<IEnumerable<T>> GenerateCombinations1<T>(IEnumerable<T> elements, int k)
        {
            if (k == 0)
            {
                yield return new T[0];
                yield break;
            }

            var list = elements.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                var head = list.Skip(i).Take(1);
                var tailCombinations = GenerateCombinations1(list.Skip(i + 1), k - 1);
                foreach (var tail in tailCombinations)
                {
                    yield return head.Concat(tail);
                }
            }
        }
        /// <summary>
        /// Hàm chạy Arigori
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        private int CalculateSupport1Count1(List<int[]> datas, int[] items)
        {
            return datas.Count(data => items.All(item =>
                item >= 1 && item <= data.Length && data[item - 1] == 1));
        }

        private double CalculateSupport1(List<int[]> datas, int[] items)
        {
            int count = CalculateSupport1Count1(datas, items);
            return datas.Count == 0 ? 0 : (double)count / datas.Count;
        }




        private double CalculateConfidence1(List<int[]> datas, Tuple<int[], int[]> rule)
        {
            var (left, right) = rule;


            int countLeft = CalculateSupport1Count1(datas, left);
            int countBoth = CalculateSupport1Count1(datas, left.Concat(right).ToArray());

            return countLeft == 0 ? 0 : (double)countBoth / countLeft;
        }
        private List<Tuple<int[], int[]>> GenerateAssociationRules1(List<int[]> itemsets)
        {
            var rules = new List<Tuple<int[], int[]>>();
            foreach (var itemset in itemsets)
            {
                if (itemset.Length > 1)
                {
                    for (int i = 1; i < itemset.Length; i++)
                    {
                        var leftCombinations = GenerateCombinations1(itemset, i);
                        foreach (var leftItemset in leftCombinations)
                        {
                            var rightItemset = itemset.Except(leftItemset).ToArray();
                            rules.Add(new Tuple<int[], int[]>(leftItemset.ToArray(), rightItemset));
                        }
                    }
                }
            }
            return rules;
        }

        private void DisplayAssociationRules1(List<Tuple<int[], int[], double>> rules, double minConf, List<string> itemNames)
        {
            richtexboxKQ.Clear();

            if (rules == null || rules.Count == 0)
            {
                richtexboxKQ.AppendText("Không có luật kết hợp nào để hiển thị.");
                return;
            }

            richtexboxKQ.AppendText("Luật kết hợp (thỏa mãn điều kiện):\n");
            int ruleIndex = 1;

            foreach (var rule in rules)
            {
                int[] antecedent = rule.Item1;
                int[] consequent = rule.Item2;
                double confidence = rule.Item3;

                if (confidence >= minConf)
                {
                    // Chuyển đổi các tập hợp thành chuỗi
                    string antecedentStr = string.Join(", ", antecedent.Where(index => index > 0 && index <= itemNames.Count).Select(index => itemNames[index - 1]));
                    string consequentStr = string.Join(", ", consequent.Where(index => index > 0 && index <= itemNames.Count).Select(index => itemNames[index - 1]));

                    // Xây dựng câu theo cấu trúc yêu cầu
                    string resultText = $"{ruleIndex++}. {{ {antecedentStr} }} => {{ {consequentStr} }} conf={confidence:F2} --> Nếu mua {{ {antecedentStr} }} thì mua {{ {consequentStr} }}\n";

                    richtexboxKQ.SelectionColor = Color.Black;
                    richtexboxKQ.AppendText(resultText);
                }
            }

            richtexboxKQ.SelectionColor = Color.Black;
        }



        /// <summary>
        /// Các hàm sử dụng  chung 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var result = XuLyData(openFileDialog.FileName);
                columnHeaders = result.Item1;
                datas = result.Item2;
                DisplayData();
                MessageBox.Show("Đã đọc dữ liệu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void updateDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (datas == null)
            {
                MessageBox.Show("Vui lòng mở tệp dữ liệu trước!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                UpdateData updateData = new UpdateData();
                updateData.DisplayDataInForm2(dataGridViewList);
                updateData.DataUpdated += DataUpdated;
                updateData.Show();
            }
        }
        private void DataUpdated(DataGridView updatedData)
        {
            datas.Clear();
            foreach (DataGridViewRow row in updatedData.Rows)
            {
                if (row.Cells[0].Value != null && row.Cells[0].Value.ToString() != "Total" && !row.IsNewRow)
                {
                    int[] rowData = new int[row.Cells.Count - 1];
                    for (int i = 1; i < row.Cells.Count; i++)
                    {
                        rowData[i - 1] = Convert.ToInt32(row.Cells[i].Value);
                    }
                    datas.Add(rowData);
                }
            }
            DisplayData();
        }
        private void DisplayData()
        {
            dataGridViewList.Columns.Clear();
            DataGridViewColumn sttColumn = new DataGridViewTextBoxColumn
            {
                Name = "stt",
                HeaderText = "STT",
                Width = 50
            };
            dataGridViewList.Columns.Add(sttColumn);

            if (columnHeaders != null)
            {

                foreach (var header in columnHeaders)
                {
                    DataGridViewColumn dataColumn = new DataGridViewTextBoxColumn
                    {
                        Name = header,
                        HeaderText = header,
                        Width = 100
                    };
                    dataGridViewList.Columns.Add(dataColumn);
                }

                if (datas != null && datas.Count > 0)
                {

                    dataGridViewList.Rows.Clear();


                    for (int i = 0; i < datas.Count; i++)
                    {
                        object[] row = new object[datas[i].Length + 1];
                        row[0] = i + 1; // STT
                        for (int j = 0; j < datas[i].Length; j++)
                        {
                            row[j + 1] = datas[i][j].ToString();
                        }
                        dataGridViewList.Rows.Add(row);
                    }
                    AddTotalRow();
                }
            }
        }
        private void AddTotalRow()
        {
            int columnCount = dataGridViewList.Columns.Count;
            object[] totalRow = new object[columnCount];
            totalRow[0] = "Total";

            for (int col = 1; col < columnCount; col++)
            {
                int sum = 0;
                foreach (DataGridViewRow row in dataGridViewList.Rows)
                {
                    if (row.Cells[col].Value != null && int.TryParse(row.Cells[col].Value.ToString(), out int value))
                    {
                        sum += value;
                    }
                }

                totalRow[col] = sum;
            }

            dataGridViewList.Rows.Add(totalRow);

            DataGridViewRow totalDataRow = dataGridViewList.Rows[dataGridViewList.Rows.Count - 1];
            totalDataRow.DefaultCellStyle.BackColor = Color.LightGray;
            totalDataRow.DefaultCellStyle.Font = new Font(dataGridViewList.Font, FontStyle.Bold);
        }
        private Tuple<string[], List<int[]>> XuLyData(string filePath)
        {
            var lines = File.ReadAllLines(filePath);


            var headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();


            List<int[]> data = new List<int[]>();
            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(',').Select(v =>
                {

                    if (int.TryParse(v.Trim(), out int result))
                        return result;
                    return 0;
                }).ToArray();
                data.Add(values);
            }

            return Tuple.Create(headers, data);
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtMinConf.Text = "";
            txtMinSup.Text = "";          
            richtexboxKQ.Clear();
            richTextBoxMoTa.Clear();


        }

       
    }
}
