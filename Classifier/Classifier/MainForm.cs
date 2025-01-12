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
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            comboBoxChoose.SelectedIndex = 0;
        }

        private void comboBoxChoose_SelectedIndexChanged(object sender, EventArgs e)
        {
            int indexChoose = comboBoxChoose.SelectedIndex;
            if (indexChoose == 0)
            {
                K_NearestNeighbors k_NearestNeighbors = new K_NearestNeighbors();
                panelMain.Controls.Clear();
                panelMain.Controls.Add(k_NearestNeighbors);
            }
            else if (indexChoose == 1)
            {

            }
        }
    }
}
