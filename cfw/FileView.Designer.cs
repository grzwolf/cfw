namespace FileViewCtl
{
    partial class FileView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if ( disposing && (components != null) ) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileView));
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorizeFindTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetColorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wordWrapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findStopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.textBoxFind = new System.Windows.Forms.TextBox();
            this.richTextBox = new FileViewCtl.FileView.RTFScrolledBottom();
            this.buttonUp = new System.Windows.Forms.Button();
            this.buttonDown = new System.Windows.Forms.Button();
            this.buttonX = new System.Windows.Forms.Button();
            this.vScrollBar = new System.Windows.Forms.VScrollBar();
            this.panelFilePos = new System.Windows.Forms.Panel();
            this.labelStatus = new System.Windows.Forms.Label();
            this.contextMenuStrip.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.toolStripSeparator1,
            this.findToolStripMenuItem,
            this.colorizeFindTextToolStripMenuItem,
            this.resetColorsToolStripMenuItem,
            this.wordWrapToolStripMenuItem,
            this.printSelectionToolStripMenuItem,
            this.findStopToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            resources.ApplyResources(this.contextMenuStrip, "contextMenuStrip");
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            resources.ApplyResources(this.copyToolStripMenuItem, "copyToolStripMenuItem");
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.CheckOnClick = true;
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            resources.ApplyResources(this.findToolStripMenuItem, "findToolStripMenuItem");
            this.findToolStripMenuItem.Click += new System.EventHandler(this.findToolStripMenuItem_Click);
            // 
            // colorizeFindTextToolStripMenuItem
            // 
            this.colorizeFindTextToolStripMenuItem.Checked = true;
            this.colorizeFindTextToolStripMenuItem.CheckOnClick = true;
            this.colorizeFindTextToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.colorizeFindTextToolStripMenuItem.Name = "colorizeFindTextToolStripMenuItem";
            resources.ApplyResources(this.colorizeFindTextToolStripMenuItem, "colorizeFindTextToolStripMenuItem");
            this.colorizeFindTextToolStripMenuItem.Click += new System.EventHandler(this.colorizeFindTextToolStripMenuItem_Click);
            // 
            // resetColorsToolStripMenuItem
            // 
            this.resetColorsToolStripMenuItem.Name = "resetColorsToolStripMenuItem";
            resources.ApplyResources(this.resetColorsToolStripMenuItem, "resetColorsToolStripMenuItem");
            this.resetColorsToolStripMenuItem.Click += new System.EventHandler(this.resetColorsToolStripMenuItem_Click);
            // 
            // wordWrapToolStripMenuItem
            // 
            this.wordWrapToolStripMenuItem.Checked = true;
            this.wordWrapToolStripMenuItem.CheckOnClick = true;
            this.wordWrapToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.wordWrapToolStripMenuItem.Name = "wordWrapToolStripMenuItem";
            resources.ApplyResources(this.wordWrapToolStripMenuItem, "wordWrapToolStripMenuItem");
            this.wordWrapToolStripMenuItem.Click += new System.EventHandler(this.wordWrapToolStripMenuItem_Click);
            // 
            // printSelectionToolStripMenuItem
            // 
            resources.ApplyResources(this.printSelectionToolStripMenuItem, "printSelectionToolStripMenuItem");
            this.printSelectionToolStripMenuItem.Name = "printSelectionToolStripMenuItem";
            this.printSelectionToolStripMenuItem.Click += new System.EventHandler(this.printSelectionToolStripMenuItem_Click);
            // 
            // findStopToolStripMenuItem
            // 
            resources.ApplyResources(this.findStopToolStripMenuItem, "findStopToolStripMenuItem");
            this.findStopToolStripMenuItem.Name = "findStopToolStripMenuItem";
            this.findStopToolStripMenuItem.Click += new System.EventHandler(this.findStopToolStripMenuItem_Click);
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.textBoxFind, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.richTextBox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonUp, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.buttonDown, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.buttonX, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.vScrollBar, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.panelFilePos, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelStatus, 3, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // textBoxFind
            // 
            resources.ApplyResources(this.textBoxFind, "textBoxFind");
            this.textBoxFind.Name = "textBoxFind";
            this.textBoxFind.TextChanged += new System.EventHandler(this.textBoxFind_TextChanged);
            this.textBoxFind.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxFind_KeyDown);
            this.textBoxFind.MouseLeave += new System.EventHandler(this.textBoxFind_MouseLeave);
            this.textBoxFind.MouseHover += new System.EventHandler(this.textBoxFind_MouseHover);
            // 
            // richTextBox
            // 
            this.richTextBox.AcceptsTab = true;
            this.tableLayoutPanel1.SetColumnSpan(this.richTextBox, 4);
            this.richTextBox.ContextMenuStrip = this.contextMenuStrip;
            resources.ApplyResources(this.richTextBox, "richTextBox");
            this.richTextBox.HideSelection = false;
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.ReadOnly = true;
            this.richTextBox.SizeChanged += new System.EventHandler(this.richTextBox_SizeChanged);
            this.richTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.richTextBox_KeyDown);
            this.richTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.richTextBox_KeyUp);
            this.richTextBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.richTextBox_MouseDown);
            // 
            // buttonUp
            // 
            resources.ApplyResources(this.buttonUp, "buttonUp");
            this.buttonUp.Name = "buttonUp";
            this.buttonUp.UseVisualStyleBackColor = true;
            this.buttonUp.Click += new System.EventHandler(this.buttonUp_Click);
            // 
            // buttonDown
            // 
            resources.ApplyResources(this.buttonDown, "buttonDown");
            this.buttonDown.Name = "buttonDown";
            this.buttonDown.UseVisualStyleBackColor = true;
            this.buttonDown.Click += new System.EventHandler(this.buttonDown_Click);
            // 
            // buttonX
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.buttonX, 2);
            resources.ApplyResources(this.buttonX, "buttonX");
            this.buttonX.Name = "buttonX";
            this.buttonX.UseVisualStyleBackColor = true;
            this.buttonX.Click += new System.EventHandler(this.buttonX_Click);
            // 
            // vScrollBar
            // 
            resources.ApplyResources(this.vScrollBar, "vScrollBar");
            this.vScrollBar.Name = "vScrollBar";
            this.vScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScrollBar_Scroll);
            // 
            // panelFilePos
            // 
            resources.ApplyResources(this.panelFilePos, "panelFilePos");
            this.panelFilePos.Name = "panelFilePos";
            this.panelFilePos.Paint += new System.Windows.Forms.PaintEventHandler(this.panelFilePos_Paint);
            // 
            // labelStatus
            // 
            resources.ApplyResources(this.labelStatus, "labelStatus");
            this.labelStatus.Name = "labelStatus";
            // 
            // FileView
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "FileView";
            this.contextMenuStrip.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem colorizeFindTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetColorsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wordWrapToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox textBoxFind;
        private RTFScrolledBottom richTextBox;
        private System.Windows.Forms.Button buttonUp;
        private System.Windows.Forms.Button buttonDown;
        private System.Windows.Forms.Button buttonX;
        private System.Windows.Forms.VScrollBar vScrollBar;
        private System.Windows.Forms.Panel panelFilePos;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ToolStripMenuItem findStopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printSelectionToolStripMenuItem;
    }
}
