namespace PerformClustering
{
    partial class DrawPoints
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zedGraphControlDraw = new ZedGraph.ZedGraphControl();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportToolStripMenuItem,
            this.saveToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(842, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(66, 24);
            this.exportToolStripMenuItem.Text = "Export";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(54, 24);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // zedGraphControlDraw
            // 
            this.zedGraphControlDraw.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zedGraphControlDraw.Location = new System.Drawing.Point(0, 28);
            this.zedGraphControlDraw.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.zedGraphControlDraw.Name = "zedGraphControlDraw";
            this.zedGraphControlDraw.ScrollGrace = 0D;
            this.zedGraphControlDraw.ScrollMaxX = 0D;
            this.zedGraphControlDraw.ScrollMaxY = 0D;
            this.zedGraphControlDraw.ScrollMaxY2 = 0D;
            this.zedGraphControlDraw.ScrollMinX = 0D;
            this.zedGraphControlDraw.ScrollMinY = 0D;
            this.zedGraphControlDraw.ScrollMinY2 = 0D;
            this.zedGraphControlDraw.Size = new System.Drawing.Size(842, 785);
            this.zedGraphControlDraw.TabIndex = 1;
            this.zedGraphControlDraw.MouseClick += new System.Windows.Forms.MouseEventHandler(this.zedGraphControlDraw_MouseClick);
            // 
            // DrawPoints
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(842, 813);
            this.Controls.Add(this.zedGraphControlDraw);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "DrawPoints";
            this.Text = "DrawPoints";
            this.Load += new System.EventHandler(this.DrawPoints_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private ZedGraph.ZedGraphControl zedGraphControlDraw;
    }
}