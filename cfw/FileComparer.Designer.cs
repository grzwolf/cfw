namespace cfw
{
    partial class FileComparer
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.textBoxB = new System.Windows.Forms.TextBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader0 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectNothingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.invertSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.changeStatusOfSelectedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCopyToLeft = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCopyToRight = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemIdle = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.restoreOriginalStatusAfterLastComparisonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lastSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeStatusOfAllFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_winmerge = new System.Windows.Forms.ToolStripMenuItem();
            this.bothFilesOpenEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_LeftEditor = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_RightFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.copySelectedTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textBoxA = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.checkBoxGrant5Sec = new System.Windows.Forms.CheckBox();
            this.checkBoxExistence = new System.Windows.Forms.CheckBox();
            this.checkBoxLastWrite = new System.Windows.Forms.CheckBox();
            this.comboBoxWildCard = new System.Windows.Forms.ComboBox();
            this.checkBoxLength = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBoxTopLevel = new System.Windows.Forms.CheckBox();
            this.checkBox1by1 = new System.Windows.Forms.CheckBox();
            this.checkBoxLiveView = new System.Windows.Forms.CheckBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.checkBoxToRight = new System.Windows.Forms.CheckBox();
            this.checkBoxEoNA = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonCopy = new System.Windows.Forms.Button();
            this.checkBoxToLeft = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.checkBoxXXX = new System.Windows.Forms.CheckBox();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.timerFirstStart = new System.Windows.Forms.Timer(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 13;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.Controls.Add(this.buttonRefresh, 5, 2);
            this.tableLayoutPanel1.Controls.Add(this.textBoxB, 11, 2);
            this.tableLayoutPanel1.Controls.Add(this.listView1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBoxA, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.progressBar1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 9, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 7, 2);
            this.tableLayoutPanel1.Controls.Add(this.splitter1, 6, 2);
            this.tableLayoutPanel1.Controls.Add(this.splitter2, 8, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(784, 562);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonRefresh.Location = new System.Drawing.Point(263, 475);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(138, 84);
            this.buttonRefresh.TabIndex = 1;
            this.buttonRefresh.Text = "Compare";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // textBoxB
            // 
            this.textBoxB.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tableLayoutPanel1.SetColumnSpan(this.textBoxB, 2);
            this.textBoxB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxB.Location = new System.Drawing.Point(714, 472);
            this.textBoxB.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.textBoxB.Multiline = true;
            this.textBoxB.Name = "textBoxB";
            this.textBoxB.ReadOnly = true;
            this.textBoxB.Size = new System.Drawing.Size(70, 90);
            this.textBoxB.TabIndex = 6;
            // 
            // listView1
            // 
            this.listView1.BackColor = System.Drawing.SystemColors.Window;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader0,
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.tableLayoutPanel1.SetColumnSpan(this.listView1, 13);
            this.listView1.ContextMenuStrip = this.contextMenuStrip;
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(3, 3);
            this.listView1.Name = "listView1";
            this.listView1.OwnerDraw = true;
            this.listView1.Size = new System.Drawing.Size(778, 456);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.listView1_DrawColumnHeader);
            this.listView1.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listView1_DrawItem);
            this.listView1.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.listView1_DrawSubItem);
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            this.listView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseDown);
            this.listView1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseMove);
            this.listView1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseUp);
            // 
            // columnHeader0
            // 
            this.columnHeader0.Text = "#";
            this.columnHeader0.Width = 40;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 400;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader2.Width = 100;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Width = 400;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectAllToolStripMenuItem,
            this.selectNothingToolStripMenuItem,
            this.invertSelectionToolStripMenuItem,
            this.toolStripSeparator1,
            this.changeStatusOfSelectedFilesToolStripMenuItem,
            this.changeStatusOfAllFilesToolStripMenuItem,
            this.toolStripSeparator2,
            this.toolStripMenuItem_winmerge,
            this.bothFilesOpenEditorToolStripMenuItem,
            this.toolStripMenuItem_LeftEditor,
            this.toolStripMenuItem_RightFile,
            this.toolStripSeparator3,
            this.copySelectedTextToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(279, 242);
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(278, 22);
            this.selectAllToolStripMenuItem.Text = "Select all";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // selectNothingToolStripMenuItem
            // 
            this.selectNothingToolStripMenuItem.Name = "selectNothingToolStripMenuItem";
            this.selectNothingToolStripMenuItem.Size = new System.Drawing.Size(278, 22);
            this.selectNothingToolStripMenuItem.Text = "Select nothing";
            this.selectNothingToolStripMenuItem.Click += new System.EventHandler(this.selectNothingToolStripMenuItem_Click);
            // 
            // invertSelectionToolStripMenuItem
            // 
            this.invertSelectionToolStripMenuItem.Name = "invertSelectionToolStripMenuItem";
            this.invertSelectionToolStripMenuItem.Size = new System.Drawing.Size(278, 22);
            this.invertSelectionToolStripMenuItem.Text = "Invert selection";
            this.invertSelectionToolStripMenuItem.Click += new System.EventHandler(this.invertSelectionToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(275, 6);
            // 
            // changeStatusOfSelectedFilesToolStripMenuItem
            // 
            this.changeStatusOfSelectedFilesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemCopyToLeft,
            this.toolStripMenuItemCopyToRight,
            this.toolStripMenuItemDelete,
            this.toolStripMenuItemIdle,
            this.toolStripSeparator4,
            this.restoreOriginalStatusAfterLastComparisonToolStripMenuItem,
            this.lastSelectionToolStripMenuItem});
            this.changeStatusOfSelectedFilesToolStripMenuItem.Name = "changeStatusOfSelectedFilesToolStripMenuItem";
            this.changeStatusOfSelectedFilesToolStripMenuItem.Size = new System.Drawing.Size(278, 22);
            this.changeStatusOfSelectedFilesToolStripMenuItem.Text = "Change \"Status\" of selected file(s) to ...";
            // 
            // toolStripMenuItemCopyToLeft
            // 
            this.toolStripMenuItemCopyToLeft.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripMenuItemCopyToLeft.Name = "toolStripMenuItemCopyToLeft";
            this.toolStripMenuItemCopyToLeft.Size = new System.Drawing.Size(275, 22);
            this.toolStripMenuItemCopyToLeft.Text = "<--- (copy to left side)";
            this.toolStripMenuItemCopyToLeft.Click += new System.EventHandler(this.toolStripMenuItemCopyToLeft_Click);
            // 
            // toolStripMenuItemCopyToRight
            // 
            this.toolStripMenuItemCopyToRight.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.toolStripMenuItemCopyToRight.Name = "toolStripMenuItemCopyToRight";
            this.toolStripMenuItemCopyToRight.Size = new System.Drawing.Size(275, 22);
            this.toolStripMenuItemCopyToRight.Text = "---> (copy to right side)";
            this.toolStripMenuItemCopyToRight.Click += new System.EventHandler(this.toolStripMenuItemCopyToRight_Click);
            // 
            // toolStripMenuItemDelete
            // 
            this.toolStripMenuItemDelete.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
            this.toolStripMenuItemDelete.Size = new System.Drawing.Size(275, 22);
            this.toolStripMenuItemDelete.Text = "XXX  (delete file)";
            this.toolStripMenuItemDelete.Click += new System.EventHandler(this.toolStripMenuItemDelete_Click);
            // 
            // toolStripMenuItemIdle
            // 
            this.toolStripMenuItemIdle.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.toolStripMenuItemIdle.Name = "toolStripMenuItemIdle";
            this.toolStripMenuItemIdle.Size = new System.Drawing.Size(275, 22);
            this.toolStripMenuItemIdle.Text = "<?>  (don\'t touch file)";
            this.toolStripMenuItemIdle.Click += new System.EventHandler(this.toolStripMenuItemIdle_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(272, 6);
            // 
            // restoreOriginalStatusAfterLastComparisonToolStripMenuItem
            // 
            this.restoreOriginalStatusAfterLastComparisonToolStripMenuItem.Name = "restoreOriginalStatusAfterLastComparisonToolStripMenuItem";
            this.restoreOriginalStatusAfterLastComparisonToolStripMenuItem.Size = new System.Drawing.Size(275, 22);
            this.restoreOriginalStatusAfterLastComparisonToolStripMenuItem.Text = "Restore original \"Status\"";
            this.restoreOriginalStatusAfterLastComparisonToolStripMenuItem.Click += new System.EventHandler(this.resetStatusOfAllSelectedFilesToolStripMenuItem_Click);
            // 
            // lastSelectionToolStripMenuItem
            // 
            this.lastSelectionToolStripMenuItem.Name = "lastSelectionToolStripMenuItem";
            this.lastSelectionToolStripMenuItem.Size = new System.Drawing.Size(275, 22);
            // 
            // changeStatusOfAllFilesToolStripMenuItem
            // 
            this.changeStatusOfAllFilesToolStripMenuItem.Name = "changeStatusOfAllFilesToolStripMenuItem";
            this.changeStatusOfAllFilesToolStripMenuItem.Size = new System.Drawing.Size(278, 22);
            this.changeStatusOfAllFilesToolStripMenuItem.Text = "Restore original \"Status\" of all files";
            this.changeStatusOfAllFilesToolStripMenuItem.Click += new System.EventHandler(this.resetStatusOfAllFilesToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(275, 6);
            // 
            // toolStripMenuItem_winmerge
            // 
            this.toolStripMenuItem_winmerge.Name = "toolStripMenuItem_winmerge";
            this.toolStripMenuItem_winmerge.Size = new System.Drawing.Size(278, 22);
            this.toolStripMenuItem_winmerge.Text = "WinMerge both files";
            this.toolStripMenuItem_winmerge.Click += new System.EventHandler(this.toolStripMenuItem_winmerge_Click);
            // 
            // bothFilesOpenEditorToolStripMenuItem
            // 
            this.bothFilesOpenEditorToolStripMenuItem.Name = "bothFilesOpenEditorToolStripMenuItem";
            this.bothFilesOpenEditorToolStripMenuItem.Size = new System.Drawing.Size(278, 22);
            this.bothFilesOpenEditorToolStripMenuItem.Text = "Edit both files";
            this.bothFilesOpenEditorToolStripMenuItem.Click += new System.EventHandler(this.bothFilesOpenEditorToolStripMenuItem_Click);
            // 
            // toolStripMenuItem_LeftEditor
            // 
            this.toolStripMenuItem_LeftEditor.Name = "toolStripMenuItem_LeftEditor";
            this.toolStripMenuItem_LeftEditor.Size = new System.Drawing.Size(278, 22);
            this.toolStripMenuItem_LeftEditor.Text = "Edit left file";
            this.toolStripMenuItem_LeftEditor.Click += new System.EventHandler(this.toolStripMenuItem_LeftEditor_Click);
            // 
            // toolStripMenuItem_RightFile
            // 
            this.toolStripMenuItem_RightFile.Name = "toolStripMenuItem_RightFile";
            this.toolStripMenuItem_RightFile.Size = new System.Drawing.Size(278, 22);
            this.toolStripMenuItem_RightFile.Text = "Edit right file";
            this.toolStripMenuItem_RightFile.Click += new System.EventHandler(this.toolStripMenuItem_RightFile_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(275, 6);
            // 
            // copySelectedTextToolStripMenuItem
            // 
            this.copySelectedTextToolStripMenuItem.Name = "copySelectedTextToolStripMenuItem";
            this.copySelectedTextToolStripMenuItem.Size = new System.Drawing.Size(278, 22);
            this.copySelectedTextToolStripMenuItem.Text = "Copy selected text";
            this.copySelectedTextToolStripMenuItem.Click += new System.EventHandler(this.copySelectedTextToolStripMenuItem_Click);
            // 
            // textBoxA
            // 
            this.textBoxA.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tableLayoutPanel1.SetColumnSpan(this.textBoxA, 2);
            this.textBoxA.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxA.Location = new System.Drawing.Point(0, 472);
            this.textBoxA.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.textBoxA.Multiline = true;
            this.textBoxA.Name = "textBoxA";
            this.textBoxA.ReadOnly = true;
            this.textBoxA.Size = new System.Drawing.Size(70, 90);
            this.textBoxA.TabIndex = 5;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel2, 3);
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.Controls.Add(this.checkBoxGrant5Sec, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.checkBoxExistence, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.checkBoxLastWrite, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.comboBoxWildCard, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.checkBoxLength, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.checkBoxTopLevel, 2, 2);
            this.tableLayoutPanel2.Controls.Add(this.checkBox1by1, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.checkBoxLiveView, 1, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(80, 472);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 4;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(180, 90);
            this.tableLayoutPanel2.TabIndex = 7;
            // 
            // checkBoxGrant5Sec
            // 
            this.checkBoxGrant5Sec.AutoSize = true;
            this.checkBoxGrant5Sec.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxGrant5Sec.Location = new System.Drawing.Point(59, 44);
            this.checkBoxGrant5Sec.Margin = new System.Windows.Forms.Padding(0);
            this.checkBoxGrant5Sec.Name = "checkBoxGrant5Sec";
            this.checkBoxGrant5Sec.Size = new System.Drawing.Size(59, 22);
            this.checkBoxGrant5Sec.TabIndex = 0;
            this.checkBoxGrant5Sec.Text = "+/- 5s";
            this.checkBoxGrant5Sec.UseVisualStyleBackColor = true;
            // 
            // checkBoxExistence
            // 
            this.checkBoxExistence.AutoSize = true;
            this.checkBoxExistence.Checked = true;
            this.checkBoxExistence.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxExistence.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxExistence.Location = new System.Drawing.Point(5, 66);
            this.checkBoxExistence.Margin = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.checkBoxExistence.Name = "checkBoxExistence";
            this.checkBoxExistence.Size = new System.Drawing.Size(54, 24);
            this.checkBoxExistence.TabIndex = 2;
            this.checkBoxExistence.Text = "exist";
            this.checkBoxExistence.UseVisualStyleBackColor = true;
            // 
            // checkBoxLastWrite
            // 
            this.checkBoxLastWrite.AutoSize = true;
            this.checkBoxLastWrite.Checked = true;
            this.checkBoxLastWrite.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLastWrite.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxLastWrite.Location = new System.Drawing.Point(5, 44);
            this.checkBoxLastWrite.Margin = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.checkBoxLastWrite.Name = "checkBoxLastWrite";
            this.checkBoxLastWrite.Size = new System.Drawing.Size(54, 22);
            this.checkBoxLastWrite.TabIndex = 1;
            this.checkBoxLastWrite.Text = "time";
            this.checkBoxLastWrite.UseVisualStyleBackColor = true;
            this.checkBoxLastWrite.CheckedChanged += new System.EventHandler(this.checkBoxLastWrite_CheckedChanged);
            // 
            // comboBoxWildCard
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.comboBoxWildCard, 2);
            this.comboBoxWildCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxWildCard.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.comboBoxWildCard.FormattingEnabled = true;
            this.comboBoxWildCard.Location = new System.Drawing.Point(59, 66);
            this.comboBoxWildCard.Margin = new System.Windows.Forms.Padding(0);
            this.comboBoxWildCard.Name = "comboBoxWildCard";
            this.comboBoxWildCard.Size = new System.Drawing.Size(121, 24);
            this.comboBoxWildCard.TabIndex = 3;
            this.comboBoxWildCard.KeyDown += new System.Windows.Forms.KeyEventHandler(this.comboBoxWildCard_KeyDown);
            this.comboBoxWildCard.Validated += new System.EventHandler(this.comboBoxWildCard_Validated);
            // 
            // checkBoxLength
            // 
            this.checkBoxLength.AutoSize = true;
            this.checkBoxLength.Checked = true;
            this.checkBoxLength.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLength.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxLength.Location = new System.Drawing.Point(5, 22);
            this.checkBoxLength.Margin = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.checkBoxLength.Name = "checkBoxLength";
            this.checkBoxLength.Size = new System.Drawing.Size(54, 22);
            this.checkBoxLength.TabIndex = 0;
            this.checkBoxLength.Text = "size";
            this.checkBoxLength.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.tableLayoutPanel2.SetColumnSpan(this.label2, 3);
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(0, 3);
            this.label2.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(177, 19);
            this.label2.TabIndex = 4;
            this.label2.Text = "Comparison Rules";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // checkBoxTopLevel
            // 
            this.checkBoxTopLevel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxTopLevel.Location = new System.Drawing.Point(118, 44);
            this.checkBoxTopLevel.Margin = new System.Windows.Forms.Padding(0);
            this.checkBoxTopLevel.Name = "checkBoxTopLevel";
            this.checkBoxTopLevel.Size = new System.Drawing.Size(62, 22);
            this.checkBoxTopLevel.TabIndex = 1;
            this.checkBoxTopLevel.Text = "just top";
            this.toolTip1.SetToolTip(this.checkBoxTopLevel, "only 1st level of a folder");
            this.checkBoxTopLevel.UseVisualStyleBackColor = true;
            // 
            // checkBox1by1
            // 
            this.checkBox1by1.AutoSize = true;
            this.checkBox1by1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBox1by1.Location = new System.Drawing.Point(118, 22);
            this.checkBox1by1.Margin = new System.Windows.Forms.Padding(0);
            this.checkBox1by1.Name = "checkBox1by1";
            this.checkBox1by1.Size = new System.Drawing.Size(62, 22);
            this.checkBox1by1.TabIndex = 1;
            this.checkBox1by1.Text = "1 : 1";
            this.checkBox1by1.UseVisualStyleBackColor = true;
            this.checkBox1by1.CheckedChanged += new System.EventHandler(this.checkBox1by1_CheckedChanged);
            // 
            // checkBoxLiveView
            // 
            this.checkBoxLiveView.AutoSize = true;
            this.checkBoxLiveView.Checked = true;
            this.checkBoxLiveView.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLiveView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxLiveView.Location = new System.Drawing.Point(59, 25);
            this.checkBoxLiveView.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.checkBoxLiveView.Name = "checkBoxLiveView";
            this.checkBoxLiveView.Size = new System.Drawing.Size(59, 19);
            this.checkBoxLiveView.TabIndex = 5;
            this.checkBoxLiveView.Text = "live view";
            this.checkBoxLiveView.UseVisualStyleBackColor = true;
            this.checkBoxLiveView.CheckedChanged += new System.EventHandler(this.checkBoxLiveView_CheckedChanged);
            // 
            // progressBar1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.progressBar1, 13);
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBar1.Location = new System.Drawing.Point(3, 465);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(778, 4);
            this.progressBar1.Step = 1;
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 8;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel4, 2);
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 57F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 43F));
            this.tableLayoutPanel4.Controls.Add(this.checkBoxToRight, 0, 2);
            this.tableLayoutPanel4.Controls.Add(this.checkBoxEoNA, 0, 3);
            this.tableLayoutPanel4.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.buttonCopy, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.checkBoxToLeft, 0, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(564, 472);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 4;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(140, 90);
            this.tableLayoutPanel4.TabIndex = 10;
            // 
            // checkBoxToRight
            // 
            this.checkBoxToRight.AutoSize = true;
            this.checkBoxToRight.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.checkBoxToRight.Location = new System.Drawing.Point(3, 47);
            this.checkBoxToRight.Name = "checkBoxToRight";
            this.checkBoxToRight.Size = new System.Drawing.Size(54, 16);
            this.checkBoxToRight.TabIndex = 6;
            this.checkBoxToRight.Text = "--->";
            this.toolTip1.SetToolTip(this.checkBoxToRight, "selectors are logical AND");
            this.checkBoxToRight.UseVisualStyleBackColor = true;
            this.checkBoxToRight.CheckedChanged += new System.EventHandler(this.checkBoxToRight_CheckedChanged);
            // 
            // checkBoxEoNA
            // 
            this.checkBoxEoNA.AutoSize = true;
            this.checkBoxEoNA.Location = new System.Drawing.Point(3, 69);
            this.checkBoxEoNA.Name = "checkBoxEoNA";
            this.checkBoxEoNA.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.checkBoxEoNA.Size = new System.Drawing.Size(43, 17);
            this.checkBoxEoNA.TabIndex = 2;
            this.checkBoxEoNA.Text = "n/a";
            this.toolTip1.SetToolTip(this.checkBoxEoNA, "selectors are logical AND");
            this.checkBoxEoNA.UseVisualStyleBackColor = true;
            this.checkBoxEoNA.CheckedChanged += new System.EventHandler(this.checkBoxEoNA_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(0, 3);
            this.label1.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 19);
            this.label1.TabIndex = 3;
            this.label1.Text = "Select Files";
            this.toolTip1.SetToolTip(this.label1, "selectors are logical AND");
            // 
            // buttonCopy
            // 
            this.buttonCopy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonCopy.Location = new System.Drawing.Point(82, 3);
            this.buttonCopy.Name = "buttonCopy";
            this.tableLayoutPanel4.SetRowSpan(this.buttonCopy, 4);
            this.buttonCopy.Size = new System.Drawing.Size(55, 84);
            this.buttonCopy.TabIndex = 4;
            this.buttonCopy.Text = "Copy";
            this.buttonCopy.UseVisualStyleBackColor = true;
            this.buttonCopy.Click += new System.EventHandler(this.buttonCopy_Click);
            // 
            // checkBoxToLeft
            // 
            this.checkBoxToLeft.AutoSize = true;
            this.checkBoxToLeft.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.checkBoxToLeft.Location = new System.Drawing.Point(3, 25);
            this.checkBoxToLeft.Name = "checkBoxToLeft";
            this.checkBoxToLeft.Size = new System.Drawing.Size(54, 16);
            this.checkBoxToLeft.TabIndex = 5;
            this.checkBoxToLeft.Text = "<---";
            this.toolTip1.SetToolTip(this.checkBoxToLeft, "selectors are logical AND");
            this.checkBoxToLeft.UseVisualStyleBackColor = true;
            this.checkBoxToLeft.CheckedChanged += new System.EventHandler(this.checkBoxToLeft_CheckedChanged);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.label3, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.buttonDelete, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.checkBoxXXX, 0, 2);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(454, 472);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(60, 90);
            this.tableLayoutPanel3.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(0, 3);
            this.label3.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 14);
            this.label3.TabIndex = 0;
            this.label3.Text = "XXX Files";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // buttonDelete
            // 
            this.buttonDelete.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonDelete.Location = new System.Drawing.Point(3, 20);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(54, 46);
            this.buttonDelete.TabIndex = 1;
            this.buttonDelete.Text = "Delete";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // checkBoxXXX
            // 
            this.checkBoxXXX.AutoSize = true;
            this.checkBoxXXX.Location = new System.Drawing.Point(3, 72);
            this.checkBoxXXX.Name = "checkBoxXXX";
            this.checkBoxXXX.Size = new System.Drawing.Size(47, 15);
            this.checkBoxXXX.TabIndex = 2;
            this.checkBoxXXX.Text = "XXX";
            this.checkBoxXXX.UseVisualStyleBackColor = true;
            this.checkBoxXXX.CheckedChanged += new System.EventHandler(this.checkBoxXXX_CheckedChanged);
            // 
            // splitter1
            // 
            this.splitter1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitter1.Location = new System.Drawing.Point(429, 472);
            this.splitter1.Margin = new System.Windows.Forms.Padding(25, 0, 3, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 90);
            this.splitter1.TabIndex = 12;
            this.splitter1.TabStop = false;
            // 
            // splitter2
            // 
            this.splitter2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitter2.Location = new System.Drawing.Point(539, 472);
            this.splitter2.Margin = new System.Windows.Forms.Padding(25, 0, 3, 0);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(3, 90);
            this.splitter2.TabIndex = 13;
            this.splitter2.TabStop = false;
            // 
            // timerFirstStart
            // 
            this.timerFirstStart.Interval = 1;
            this.timerFirstStart.Tick += new System.EventHandler(this.timerFirstStart_Tick);
            // 
            // FileComparer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "FileComparer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "File Comparer - \'show not matching files\'";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FileComparer_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FileComparer_FormClosed);
            this.Load += new System.EventHandler(this.FileComparer_Load);
            this.Resize += new System.EventHandler(this.FileComparer_Resize);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.contextMenuStrip.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Timer timerFirstStart;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.TextBox textBoxB;
        private System.Windows.Forms.TextBox textBoxA;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.CheckBox checkBoxExistence;
        private System.Windows.Forms.CheckBox checkBoxLastWrite;
        private System.Windows.Forms.CheckBox checkBoxLength;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ColumnHeader columnHeader0;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_winmerge;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_LeftEditor;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_RightFile;
        private System.Windows.Forms.ToolStripMenuItem bothFilesOpenEditorToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxGrant5Sec;
        private System.Windows.Forms.CheckBox checkBox1by1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.CheckBox checkBoxTopLevel;
        private System.Windows.Forms.ComboBox comboBoxWildCard;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem copySelectedTextToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxEoNA;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonCopy;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Splitter splitter2;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectNothingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem invertSelectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.CheckBox checkBoxToRight;
        private System.Windows.Forms.CheckBox checkBoxToLeft;
        private System.Windows.Forms.CheckBox checkBoxXXX;
        private System.Windows.Forms.ToolStripMenuItem changeStatusOfAllFilesToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxLiveView;
        private System.Windows.Forms.ToolStripMenuItem changeStatusOfSelectedFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCopyToLeft;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCopyToRight;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDelete;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemIdle;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem restoreOriginalStatusAfterLastComparisonToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lastSelectionToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}