namespace cfw
{
    partial class AltF7Form
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AltF7Form));
            this.buttonRun = new System.Windows.Forms.Button();
            this.textBoxProcessFile = new System.Windows.Forms.TextBox();
            this.buttonGetLocation = new System.Windows.Forms.Button();
            this.labelHHlist = new System.Windows.Forms.Label();
            this.textBoxFilePattern = new System.Windows.Forms.TextBox();
            this.checkBoxSkipSearchingBinaryFiles = new System.Windows.Forms.CheckBox();
            this.progressBarTotal = new System.Windows.Forms.ProgressBar();
            this.checkBoxOutputAddLine = new System.Windows.Forms.CheckBox();
            this.buttonDown = new System.Windows.Forms.Button();
            this.buttonUp = new System.Windows.Forms.Button();
            this.textBoxOutput = new System.Windows.Forms.RichTextBox();
            this.contextMenuStripOutput = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemOpenFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemJumpToFile = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItemCloseJumpToFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorizeFindTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorizeBuzzwordsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetColorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemClearWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.appendToOutputToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveOutputToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.textBoxFind = new System.Windows.Forms.TextBox();
            this.buttonX = new System.Windows.Forms.Button();
            this.listViewOutput = new System.Windows.Forms.ListView();
            this.columnHeaderDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderExt = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStripListView = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.jumpToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeAndJumpToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.winmergePairOfFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.copyListItemsToClipBoardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copySelectedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copySelectedFilesToClipBoardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteSelectedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textBoxSeparator = new System.Windows.Forms.TextBox();
            this.labelSeparator = new System.Windows.Forms.Label();
            this.textBoxFolderPattern = new System.Windows.Forms.TextBox();
            this.labelFolderExcludes = new System.Windows.Forms.Label();
            this.textBoxFolderExcludes = new System.Windows.Forms.TextBox();
            this.progressBarCurrent = new System.Windows.Forms.ProgressBar();
            this.checkBoxMatchCase = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.checkBoxComputer = new System.Windows.Forms.CheckBox();
            this.checkBoxLiveRefresh = new System.Windows.Forms.CheckBox();
            this.checkBoxSkip = new System.Windows.Forms.CheckBox();
            this.buttonBigFileSkip = new System.Windows.Forms.Button();
            this.timerBigFile = new System.Windows.Forms.Timer(this.components);
            this.checkBoxAdvancedOptions = new System.Windows.Forms.CheckBox();
            this.radioButtonToday = new System.Windows.Forms.RadioButton();
            this.radioButtonLastWeek = new System.Windows.Forms.RadioButton();
            this.panelDate = new System.Windows.Forms.Panel();
            this.checkBoxLen248 = new System.Windows.Forms.CheckBox();
            this.radioButtonDateAll = new System.Windows.Forms.RadioButton();
            this.dateTimePickerStop = new System.Windows.Forms.DateTimePicker();
            this.dateTimePickerStart = new System.Windows.Forms.DateTimePicker();
            this.radioButtonRange = new System.Windows.Forms.RadioButton();
            this.radioButtonLastMonth = new System.Windows.Forms.RadioButton();
            this.panelSize = new System.Windows.Forms.Panel();
            this.radioButtonSizeAll = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.radioButtonSmaller = new System.Windows.Forms.RadioButton();
            this.radioButtonEqual = new System.Windows.Forms.RadioButton();
            this.radioButtonLarger = new System.Windows.Forms.RadioButton();
            this.numericUpDownSize = new System.Windows.Forms.NumericUpDown();
            this.checkBoxFilesOnly = new System.Windows.Forms.CheckBox();
            this.comboBoxBuzzword = new System.Windows.Forms.ComboBox();
            this.checkBoxSearchText = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.checkBoxPathMustContain = new System.Windows.Forms.CheckBox();
            this.checkBoxPathMustNotContain = new System.Windows.Forms.CheckBox();
            this.checkBoxAlwaysSkipLargeFiles = new System.Windows.Forms.CheckBox();
            this.labelLinesText = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.panelTextOptions = new System.Windows.Forms.Panel();
            this.numericUpDownHugeFileLineCountLimit = new System.Windows.Forms.NumericUpDown();
            this.checkBoxKeepOutput = new System.Windows.Forms.CheckBox();
            this.panelSeparator = new System.Windows.Forms.Panel();
            this.previewCtl = new cfw.PreviewCtl();
            this.checkBoxPreview = new System.Windows.Forms.CheckBox();
            this.checkBoxExact = new System.Windows.Forms.CheckBox();
            this.timerUpdate = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStripOutput.SuspendLayout();
            this.contextMenuStripListView.SuspendLayout();
            this.panelDate.SuspendLayout();
            this.panelSize.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSize)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.panelTextOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHugeFileLineCountLimit)).BeginInit();
            this.panelSeparator.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonRun
            // 
            resources.ApplyResources(this.buttonRun, "buttonRun");
            this.buttonRun.Name = "buttonRun";
            this.tableLayoutPanel2.SetRowSpan(this.buttonRun, 3);
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.OnButtonRunClick);
            // 
            // textBoxProcessFile
            // 
            resources.ApplyResources(this.textBoxProcessFile, "textBoxProcessFile");
            this.tableLayoutPanel2.SetColumnSpan(this.textBoxProcessFile, 4);
            this.textBoxProcessFile.Name = "textBoxProcessFile";
            this.toolTip1.SetToolTip(this.textBoxProcessFile, resources.GetString("textBoxProcessFile.ToolTip"));
            // 
            // buttonGetLocation
            // 
            resources.ApplyResources(this.buttonGetLocation, "buttonGetLocation");
            this.buttonGetLocation.Name = "buttonGetLocation";
            this.buttonGetLocation.UseVisualStyleBackColor = true;
            this.buttonGetLocation.Click += new System.EventHandler(this.OnButtonClickGetLocation);
            // 
            // labelHHlist
            // 
            resources.ApplyResources(this.labelHHlist, "labelHHlist");
            this.tableLayoutPanel2.SetColumnSpan(this.labelHHlist, 9);
            this.labelHHlist.Name = "labelHHlist";
            // 
            // textBoxFilePattern
            // 
            resources.ApplyResources(this.textBoxFilePattern, "textBoxFilePattern");
            this.tableLayoutPanel2.SetColumnSpan(this.textBoxFilePattern, 3);
            this.textBoxFilePattern.Name = "textBoxFilePattern";
            this.toolTip1.SetToolTip(this.textBoxFilePattern, resources.GetString("textBoxFilePattern.ToolTip"));
            this.textBoxFilePattern.TextChanged += new System.EventHandler(this.textBoxFilePattern_TextChanged);
            this.textBoxFilePattern.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxFilePattern_KeyDown);
            // 
            // checkBoxSkipSearchingBinaryFiles
            // 
            resources.ApplyResources(this.checkBoxSkipSearchingBinaryFiles, "checkBoxSkipSearchingBinaryFiles");
            this.checkBoxSkipSearchingBinaryFiles.Name = "checkBoxSkipSearchingBinaryFiles";
            this.toolTip1.SetToolTip(this.checkBoxSkipSearchingBinaryFiles, resources.GetString("checkBoxSkipSearchingBinaryFiles.ToolTip"));
            this.checkBoxSkipSearchingBinaryFiles.UseVisualStyleBackColor = true;
            this.checkBoxSkipSearchingBinaryFiles.CheckedChanged += new System.EventHandler(this.checkBoxSkipSearchingBinaryFiles_CheckedChanged);
            // 
            // progressBarTotal
            // 
            resources.ApplyResources(this.progressBarTotal, "progressBarTotal");
            this.tableLayoutPanel2.SetColumnSpan(this.progressBarTotal, 9);
            this.progressBarTotal.Name = "progressBarTotal";
            this.progressBarTotal.Step = 1;
            // 
            // checkBoxOutputAddLine
            // 
            resources.ApplyResources(this.checkBoxOutputAddLine, "checkBoxOutputAddLine");
            this.checkBoxOutputAddLine.Name = "checkBoxOutputAddLine";
            this.checkBoxOutputAddLine.UseVisualStyleBackColor = true;
            // 
            // buttonDown
            // 
            resources.ApplyResources(this.buttonDown, "buttonDown");
            this.buttonDown.Name = "buttonDown";
            this.buttonDown.UseVisualStyleBackColor = true;
            this.buttonDown.Click += new System.EventHandler(this.ButtonDown_Click);
            // 
            // buttonUp
            // 
            resources.ApplyResources(this.buttonUp, "buttonUp");
            this.buttonUp.Name = "buttonUp";
            this.buttonUp.UseVisualStyleBackColor = true;
            this.buttonUp.Click += new System.EventHandler(this.ButtonUp_Click);
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.ContextMenuStrip = this.contextMenuStripOutput;
            this.textBoxOutput.DetectUrls = false;
            resources.ApplyResources(this.textBoxOutput, "textBoxOutput");
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TextBoxOutput_MouseDoubleClick);
            this.textBoxOutput.MouseDown += new System.Windows.Forms.MouseEventHandler(this.textBoxOutput_MouseDown);
            // 
            // contextMenuStripOutput
            // 
            this.contextMenuStripOutput.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemOpenFile,
            this.toolStripMenuItemJumpToFile,
            this.ToolStripMenuItemCloseJumpToFile,
            this.toolStripSeparator1,
            this.findToolStripMenuItem,
            this.colorizeFindTextToolStripMenuItem,
            this.colorizeBuzzwordsToolStripMenuItem,
            this.resetColorsToolStripMenuItem,
            this.toolStripSeparator3,
            this.toolStripMenuItemSelectAll,
            this.toolStripMenuItemCopy,
            this.toolStripMenuItemPaste,
            this.toolStripMenuItemDelete,
            this.toolStripSeparator2,
            this.toolStripMenuItemClearWindow,
            this.appendToOutputToolStripMenuItem,
            this.saveOutputToolStripMenuItem,
            this.toolStripSeparator4});
            this.contextMenuStripOutput.Name = "contextMenuStripOutput";
            resources.ApplyResources(this.contextMenuStripOutput, "contextMenuStripOutput");
            this.contextMenuStripOutput.Opening += new System.ComponentModel.CancelEventHandler(this.OnContextMenuOpen);
            // 
            // toolStripMenuItemOpenFile
            // 
            this.toolStripMenuItemOpenFile.Name = "toolStripMenuItemOpenFile";
            resources.ApplyResources(this.toolStripMenuItemOpenFile, "toolStripMenuItemOpenFile");
            this.toolStripMenuItemOpenFile.Click += new System.EventHandler(this.ToolStripMenuItemOpenFile_Click);
            // 
            // toolStripMenuItemJumpToFile
            // 
            this.toolStripMenuItemJumpToFile.Name = "toolStripMenuItemJumpToFile";
            resources.ApplyResources(this.toolStripMenuItemJumpToFile, "toolStripMenuItemJumpToFile");
            this.toolStripMenuItemJumpToFile.Click += new System.EventHandler(this.ToolStripMenuItemJumpToFile_Click);
            // 
            // ToolStripMenuItemCloseJumpToFile
            // 
            this.ToolStripMenuItemCloseJumpToFile.Name = "ToolStripMenuItemCloseJumpToFile";
            resources.ApplyResources(this.ToolStripMenuItemCloseJumpToFile, "ToolStripMenuItemCloseJumpToFile");
            this.ToolStripMenuItemCloseJumpToFile.Click += new System.EventHandler(this.ToolStripMenuItemCloseAndJumpToFile_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            resources.ApplyResources(this.findToolStripMenuItem, "findToolStripMenuItem");
            this.findToolStripMenuItem.Click += new System.EventHandler(this.findToolStripMenuItem_Click);
            // 
            // colorizeFindTextToolStripMenuItem
            // 
            resources.ApplyResources(this.colorizeFindTextToolStripMenuItem, "colorizeFindTextToolStripMenuItem");
            this.colorizeFindTextToolStripMenuItem.Name = "colorizeFindTextToolStripMenuItem";
            this.colorizeFindTextToolStripMenuItem.Click += new System.EventHandler(this.colorizeFindTextToolStripMenuItem_Click);
            // 
            // colorizeBuzzwordsToolStripMenuItem
            // 
            resources.ApplyResources(this.colorizeBuzzwordsToolStripMenuItem, "colorizeBuzzwordsToolStripMenuItem");
            this.colorizeBuzzwordsToolStripMenuItem.Name = "colorizeBuzzwordsToolStripMenuItem";
            this.colorizeBuzzwordsToolStripMenuItem.Click += new System.EventHandler(this.ColorizeBuzzwordsToolStripMenuItem_Click);
            // 
            // resetColorsToolStripMenuItem
            // 
            this.resetColorsToolStripMenuItem.Name = "resetColorsToolStripMenuItem";
            resources.ApplyResources(this.resetColorsToolStripMenuItem, "resetColorsToolStripMenuItem");
            this.resetColorsToolStripMenuItem.Click += new System.EventHandler(this.ResetColorsToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // toolStripMenuItemSelectAll
            // 
            this.toolStripMenuItemSelectAll.Name = "toolStripMenuItemSelectAll";
            resources.ApplyResources(this.toolStripMenuItemSelectAll, "toolStripMenuItemSelectAll");
            this.toolStripMenuItemSelectAll.Click += new System.EventHandler(this.ToolStripMenuItemSelectAll_Click);
            // 
            // toolStripMenuItemCopy
            // 
            this.toolStripMenuItemCopy.Name = "toolStripMenuItemCopy";
            resources.ApplyResources(this.toolStripMenuItemCopy, "toolStripMenuItemCopy");
            this.toolStripMenuItemCopy.Click += new System.EventHandler(this.ToolStripMenuItemCopy_Click);
            // 
            // toolStripMenuItemPaste
            // 
            this.toolStripMenuItemPaste.Name = "toolStripMenuItemPaste";
            resources.ApplyResources(this.toolStripMenuItemPaste, "toolStripMenuItemPaste");
            this.toolStripMenuItemPaste.Click += new System.EventHandler(this.ToolStripMenuItemPaste_Click);
            // 
            // toolStripMenuItemDelete
            // 
            this.toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
            resources.ApplyResources(this.toolStripMenuItemDelete, "toolStripMenuItemDelete");
            this.toolStripMenuItemDelete.Click += new System.EventHandler(this.ToolStripMenuItemDelete_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // toolStripMenuItemClearWindow
            // 
            this.toolStripMenuItemClearWindow.Name = "toolStripMenuItemClearWindow";
            resources.ApplyResources(this.toolStripMenuItemClearWindow, "toolStripMenuItemClearWindow");
            this.toolStripMenuItemClearWindow.Click += new System.EventHandler(this.ToolStripMenuItemClearWindow_Click);
            // 
            // appendToOutputToolStripMenuItem
            // 
            this.appendToOutputToolStripMenuItem.Name = "appendToOutputToolStripMenuItem";
            resources.ApplyResources(this.appendToOutputToolStripMenuItem, "appendToOutputToolStripMenuItem");
            this.appendToOutputToolStripMenuItem.Click += new System.EventHandler(this.appendToOutputToolStripMenuItem_Click);
            // 
            // saveOutputToolStripMenuItem
            // 
            this.saveOutputToolStripMenuItem.Name = "saveOutputToolStripMenuItem";
            resources.ApplyResources(this.saveOutputToolStripMenuItem, "saveOutputToolStripMenuItem");
            this.saveOutputToolStripMenuItem.Click += new System.EventHandler(this.saveOutputToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            // 
            // textBoxFind
            // 
            resources.ApplyResources(this.textBoxFind, "textBoxFind");
            this.tableLayoutPanel2.SetColumnSpan(this.textBoxFind, 4);
            this.textBoxFind.Name = "textBoxFind";
            this.textBoxFind.TextChanged += new System.EventHandler(this.textBoxFind_TextChanged);
            this.textBoxFind.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxFind_KeyDown);
            // 
            // buttonX
            // 
            resources.ApplyResources(this.buttonX, "buttonX");
            this.buttonX.Name = "buttonX";
            this.buttonX.UseVisualStyleBackColor = true;
            this.buttonX.Click += new System.EventHandler(this.buttonX_Click);
            // 
            // listViewOutput
            // 
            this.listViewOutput.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderDate,
            this.columnHeaderSize,
            this.columnHeaderFile,
            this.columnHeaderPath,
            this.columnHeaderExt});
            this.listViewOutput.ContextMenuStrip = this.contextMenuStripListView;
            resources.ApplyResources(this.listViewOutput, "listViewOutput");
            this.listViewOutput.FullRowSelect = true;
            this.listViewOutput.HideSelection = false;
            this.listViewOutput.Name = "listViewOutput";
            this.listViewOutput.UseCompatibleStateImageBehavior = false;
            this.listViewOutput.View = System.Windows.Forms.View.Details;
            this.listViewOutput.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewOutput_ColumnClick);
            this.listViewOutput.SelectedIndexChanged += new System.EventHandler(this.listViewOutput_SelectedIndexChanged);
            this.listViewOutput.DoubleClick += new System.EventHandler(this.listViewOutput_DoubleClick);
            // 
            // columnHeaderDate
            // 
            this.columnHeaderDate.Tag = "Date";
            resources.ApplyResources(this.columnHeaderDate, "columnHeaderDate");
            // 
            // columnHeaderSize
            // 
            this.columnHeaderSize.Tag = "Size";
            resources.ApplyResources(this.columnHeaderSize, "columnHeaderSize");
            // 
            // columnHeaderFile
            // 
            this.columnHeaderFile.Tag = "File";
            resources.ApplyResources(this.columnHeaderFile, "columnHeaderFile");
            // 
            // columnHeaderPath
            // 
            this.columnHeaderPath.Tag = "Path";
            resources.ApplyResources(this.columnHeaderPath, "columnHeaderPath");
            // 
            // columnHeaderExt
            // 
            this.columnHeaderExt.Tag = "Extension";
            resources.ApplyResources(this.columnHeaderExt, "columnHeaderExt");
            // 
            // contextMenuStripListView
            // 
            this.contextMenuStripListView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFileToolStripMenuItem,
            this.jumpToFileToolStripMenuItem,
            this.closeAndJumpToFileToolStripMenuItem,
            this.toolStripSeparator6,
            this.winmergePairOfFilesToolStripMenuItem,
            this.toolStripSeparator5,
            this.copyListItemsToClipBoardToolStripMenuItem,
            this.copySelectedFilesToolStripMenuItem,
            this.copySelectedFilesToClipBoardToolStripMenuItem,
            this.deleteSelectedFilesToolStripMenuItem});
            this.contextMenuStripListView.Name = "contextMenuStripListView";
            resources.ApplyResources(this.contextMenuStripListView, "contextMenuStripListView");
            this.contextMenuStripListView.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripListView_Opening);
            // 
            // openFileToolStripMenuItem
            // 
            this.openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
            resources.ApplyResources(this.openFileToolStripMenuItem, "openFileToolStripMenuItem");
            this.openFileToolStripMenuItem.Click += new System.EventHandler(this.openFileToolStripMenuItem_Click);
            // 
            // jumpToFileToolStripMenuItem
            // 
            this.jumpToFileToolStripMenuItem.Name = "jumpToFileToolStripMenuItem";
            resources.ApplyResources(this.jumpToFileToolStripMenuItem, "jumpToFileToolStripMenuItem");
            this.jumpToFileToolStripMenuItem.Click += new System.EventHandler(this.jumpToFileToolStripMenuItem_Click);
            // 
            // closeAndJumpToFileToolStripMenuItem
            // 
            this.closeAndJumpToFileToolStripMenuItem.Name = "closeAndJumpToFileToolStripMenuItem";
            resources.ApplyResources(this.closeAndJumpToFileToolStripMenuItem, "closeAndJumpToFileToolStripMenuItem");
            this.closeAndJumpToFileToolStripMenuItem.Click += new System.EventHandler(this.closeAndJumpToFileToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            resources.ApplyResources(this.toolStripSeparator6, "toolStripSeparator6");
            // 
            // winmergePairOfFilesToolStripMenuItem
            // 
            this.winmergePairOfFilesToolStripMenuItem.Name = "winmergePairOfFilesToolStripMenuItem";
            resources.ApplyResources(this.winmergePairOfFilesToolStripMenuItem, "winmergePairOfFilesToolStripMenuItem");
            this.winmergePairOfFilesToolStripMenuItem.Click += new System.EventHandler(this.winmergePairOfFilesToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
            // 
            // copyListItemsToClipBoardToolStripMenuItem
            // 
            this.copyListItemsToClipBoardToolStripMenuItem.Name = "copyListItemsToClipBoardToolStripMenuItem";
            resources.ApplyResources(this.copyListItemsToClipBoardToolStripMenuItem, "copyListItemsToClipBoardToolStripMenuItem");
            this.copyListItemsToClipBoardToolStripMenuItem.Click += new System.EventHandler(this.copyListItemsToClipBoardToolStripMenuItem_Click);
            // 
            // copySelectedFilesToolStripMenuItem
            // 
            this.copySelectedFilesToolStripMenuItem.Name = "copySelectedFilesToolStripMenuItem";
            resources.ApplyResources(this.copySelectedFilesToolStripMenuItem, "copySelectedFilesToolStripMenuItem");
            this.copySelectedFilesToolStripMenuItem.Click += new System.EventHandler(this.copySelectedFilesToolStripMenuItem_Click);
            // 
            // copySelectedFilesToClipBoardToolStripMenuItem
            // 
            this.copySelectedFilesToClipBoardToolStripMenuItem.Name = "copySelectedFilesToClipBoardToolStripMenuItem";
            resources.ApplyResources(this.copySelectedFilesToClipBoardToolStripMenuItem, "copySelectedFilesToClipBoardToolStripMenuItem");
            this.copySelectedFilesToClipBoardToolStripMenuItem.Click += new System.EventHandler(this.copySelectedFilesToClipBoardToolStripMenuItem_Click);
            // 
            // deleteSelectedFilesToolStripMenuItem
            // 
            this.deleteSelectedFilesToolStripMenuItem.Image = global::cfw.Properties.Resources.trash;
            this.deleteSelectedFilesToolStripMenuItem.Name = "deleteSelectedFilesToolStripMenuItem";
            resources.ApplyResources(this.deleteSelectedFilesToolStripMenuItem, "deleteSelectedFilesToolStripMenuItem");
            this.deleteSelectedFilesToolStripMenuItem.Click += new System.EventHandler(this.deleteSelectedFilesToolStripMenuItem_Click);
            // 
            // textBoxSeparator
            // 
            this.textBoxSeparator.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.textBoxSeparator, "textBoxSeparator");
            this.textBoxSeparator.Name = "textBoxSeparator";
            // 
            // labelSeparator
            // 
            resources.ApplyResources(this.labelSeparator, "labelSeparator");
            this.labelSeparator.Name = "labelSeparator";
            // 
            // textBoxFolderPattern
            // 
            resources.ApplyResources(this.textBoxFolderPattern, "textBoxFolderPattern");
            this.tableLayoutPanel2.SetColumnSpan(this.textBoxFolderPattern, 3);
            this.textBoxFolderPattern.Name = "textBoxFolderPattern";
            // 
            // labelFolderExcludes
            // 
            resources.ApplyResources(this.labelFolderExcludes, "labelFolderExcludes");
            this.labelFolderExcludes.Name = "labelFolderExcludes";
            // 
            // textBoxFolderExcludes
            // 
            resources.ApplyResources(this.textBoxFolderExcludes, "textBoxFolderExcludes");
            this.tableLayoutPanel2.SetColumnSpan(this.textBoxFolderExcludes, 2);
            this.textBoxFolderExcludes.Name = "textBoxFolderExcludes";
            // 
            // progressBarCurrent
            // 
            resources.ApplyResources(this.progressBarCurrent, "progressBarCurrent");
            this.tableLayoutPanel2.SetColumnSpan(this.progressBarCurrent, 9);
            this.progressBarCurrent.Name = "progressBarCurrent";
            this.progressBarCurrent.Step = 1;
            // 
            // checkBoxMatchCase
            // 
            resources.ApplyResources(this.checkBoxMatchCase, "checkBoxMatchCase");
            this.tableLayoutPanel2.SetColumnSpan(this.checkBoxMatchCase, 2);
            this.checkBoxMatchCase.Name = "checkBoxMatchCase";
            this.toolTip1.SetToolTip(this.checkBoxMatchCase, resources.GetString("checkBoxMatchCase.ToolTip"));
            this.checkBoxMatchCase.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            this.toolTip1.SetToolTip(this.label3, resources.GetString("label3.ToolTip"));
            // 
            // checkBoxComputer
            // 
            resources.ApplyResources(this.checkBoxComputer, "checkBoxComputer");
            this.checkBoxComputer.Name = "checkBoxComputer";
            this.checkBoxComputer.UseVisualStyleBackColor = true;
            this.checkBoxComputer.CheckedChanged += new System.EventHandler(this.checkBoxComputer_CheckedChanged);
            // 
            // checkBoxLiveRefresh
            // 
            resources.ApplyResources(this.checkBoxLiveRefresh, "checkBoxLiveRefresh");
            this.checkBoxLiveRefresh.Name = "checkBoxLiveRefresh";
            this.checkBoxLiveRefresh.UseVisualStyleBackColor = true;
            this.checkBoxLiveRefresh.CheckedChanged += new System.EventHandler(this.checkBoxLiveRefresh_CheckedChanged);
            // 
            // checkBoxSkip
            // 
            resources.ApplyResources(this.checkBoxSkip, "checkBoxSkip");
            this.checkBoxSkip.Name = "checkBoxSkip";
            this.checkBoxSkip.UseVisualStyleBackColor = true;
            this.checkBoxSkip.CheckedChanged += new System.EventHandler(this.checkBoxSkip_CheckedChanged);
            // 
            // buttonBigFileSkip
            // 
            this.buttonBigFileSkip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.tableLayoutPanel2.SetColumnSpan(this.buttonBigFileSkip, 2);
            resources.ApplyResources(this.buttonBigFileSkip, "buttonBigFileSkip");
            this.buttonBigFileSkip.Name = "buttonBigFileSkip";
            this.tableLayoutPanel2.SetRowSpan(this.buttonBigFileSkip, 2);
            this.buttonBigFileSkip.UseVisualStyleBackColor = false;
            this.buttonBigFileSkip.Click += new System.EventHandler(this.buttonBigFileSkip_Click);
            // 
            // timerBigFile
            // 
            this.timerBigFile.Interval = 5000;
            this.timerBigFile.Tick += new System.EventHandler(this.timerBigFile_Tick);
            // 
            // checkBoxAdvancedOptions
            // 
            resources.ApplyResources(this.checkBoxAdvancedOptions, "checkBoxAdvancedOptions");
            this.tableLayoutPanel2.SetColumnSpan(this.checkBoxAdvancedOptions, 2);
            this.checkBoxAdvancedOptions.Name = "checkBoxAdvancedOptions";
            this.checkBoxAdvancedOptions.UseVisualStyleBackColor = true;
            this.checkBoxAdvancedOptions.CheckedChanged += new System.EventHandler(this.checkBoxAdvancedOptions_CheckedChanged);
            // 
            // radioButtonToday
            // 
            resources.ApplyResources(this.radioButtonToday, "radioButtonToday");
            this.radioButtonToday.Name = "radioButtonToday";
            this.radioButtonToday.UseVisualStyleBackColor = true;
            this.radioButtonToday.CheckedChanged += new System.EventHandler(this.radioButtonToday_CheckedChanged);
            // 
            // radioButtonLastWeek
            // 
            resources.ApplyResources(this.radioButtonLastWeek, "radioButtonLastWeek");
            this.radioButtonLastWeek.Name = "radioButtonLastWeek";
            this.radioButtonLastWeek.UseVisualStyleBackColor = true;
            this.radioButtonLastWeek.CheckedChanged += new System.EventHandler(this.radioButtonLastWeek_CheckedChanged);
            // 
            // panelDate
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.panelDate, 9);
            this.panelDate.Controls.Add(this.checkBoxLen248);
            this.panelDate.Controls.Add(this.radioButtonDateAll);
            this.panelDate.Controls.Add(this.dateTimePickerStop);
            this.panelDate.Controls.Add(this.dateTimePickerStart);
            this.panelDate.Controls.Add(this.radioButtonRange);
            this.panelDate.Controls.Add(this.radioButtonLastMonth);
            this.panelDate.Controls.Add(this.radioButtonToday);
            this.panelDate.Controls.Add(this.radioButtonLastWeek);
            resources.ApplyResources(this.panelDate, "panelDate");
            this.panelDate.Name = "panelDate";
            // 
            // checkBoxLen248
            // 
            resources.ApplyResources(this.checkBoxLen248, "checkBoxLen248");
            this.checkBoxLen248.Name = "checkBoxLen248";
            this.toolTip1.SetToolTip(this.checkBoxLen248, resources.GetString("checkBoxLen248.ToolTip"));
            this.checkBoxLen248.UseVisualStyleBackColor = true;
            // 
            // radioButtonDateAll
            // 
            resources.ApplyResources(this.radioButtonDateAll, "radioButtonDateAll");
            this.radioButtonDateAll.Checked = true;
            this.radioButtonDateAll.Name = "radioButtonDateAll";
            this.radioButtonDateAll.TabStop = true;
            this.radioButtonDateAll.UseVisualStyleBackColor = true;
            this.radioButtonDateAll.CheckedChanged += new System.EventHandler(this.radioButtonDateAll_CheckedChanged);
            // 
            // dateTimePickerStop
            // 
            this.dateTimePickerStop.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            resources.ApplyResources(this.dateTimePickerStop, "dateTimePickerStop");
            this.dateTimePickerStop.Name = "dateTimePickerStop";
            // 
            // dateTimePickerStart
            // 
            this.dateTimePickerStart.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            resources.ApplyResources(this.dateTimePickerStart, "dateTimePickerStart");
            this.dateTimePickerStart.Name = "dateTimePickerStart";
            // 
            // radioButtonRange
            // 
            resources.ApplyResources(this.radioButtonRange, "radioButtonRange");
            this.radioButtonRange.Name = "radioButtonRange";
            this.radioButtonRange.UseVisualStyleBackColor = true;
            this.radioButtonRange.CheckedChanged += new System.EventHandler(this.radioButtonRange_CheckedChanged);
            // 
            // radioButtonLastMonth
            // 
            resources.ApplyResources(this.radioButtonLastMonth, "radioButtonLastMonth");
            this.radioButtonLastMonth.Name = "radioButtonLastMonth";
            this.radioButtonLastMonth.UseVisualStyleBackColor = true;
            this.radioButtonLastMonth.CheckedChanged += new System.EventHandler(this.radioButtonLastMonth_CheckedChanged);
            // 
            // panelSize
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.panelSize, 9);
            this.panelSize.Controls.Add(this.radioButtonSizeAll);
            this.panelSize.Controls.Add(this.label1);
            this.panelSize.Controls.Add(this.radioButtonSmaller);
            this.panelSize.Controls.Add(this.radioButtonEqual);
            this.panelSize.Controls.Add(this.radioButtonLarger);
            this.panelSize.Controls.Add(this.checkBoxOutputAddLine);
            this.panelSize.Controls.Add(this.numericUpDownSize);
            resources.ApplyResources(this.panelSize, "panelSize");
            this.panelSize.Name = "panelSize";
            // 
            // radioButtonSizeAll
            // 
            resources.ApplyResources(this.radioButtonSizeAll, "radioButtonSizeAll");
            this.radioButtonSizeAll.Checked = true;
            this.radioButtonSizeAll.Name = "radioButtonSizeAll";
            this.radioButtonSizeAll.TabStop = true;
            this.radioButtonSizeAll.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // radioButtonSmaller
            // 
            resources.ApplyResources(this.radioButtonSmaller, "radioButtonSmaller");
            this.radioButtonSmaller.Name = "radioButtonSmaller";
            this.radioButtonSmaller.UseVisualStyleBackColor = true;
            // 
            // radioButtonEqual
            // 
            resources.ApplyResources(this.radioButtonEqual, "radioButtonEqual");
            this.radioButtonEqual.Name = "radioButtonEqual";
            this.radioButtonEqual.UseVisualStyleBackColor = true;
            // 
            // radioButtonLarger
            // 
            resources.ApplyResources(this.radioButtonLarger, "radioButtonLarger");
            this.radioButtonLarger.Name = "radioButtonLarger";
            this.radioButtonLarger.UseVisualStyleBackColor = true;
            // 
            // numericUpDownSize
            // 
            this.numericUpDownSize.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.numericUpDownSize.Increment = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            resources.ApplyResources(this.numericUpDownSize, "numericUpDownSize");
            this.numericUpDownSize.Maximum = new decimal(new int[] {
            -1593835521,
            466537709,
            54210,
            0});
            this.numericUpDownSize.Name = "numericUpDownSize";
            this.numericUpDownSize.Value = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            // 
            // checkBoxFilesOnly
            // 
            resources.ApplyResources(this.checkBoxFilesOnly, "checkBoxFilesOnly");
            this.checkBoxFilesOnly.Name = "checkBoxFilesOnly";
            this.checkBoxFilesOnly.UseVisualStyleBackColor = true;
            this.checkBoxFilesOnly.CheckedChanged += new System.EventHandler(this.checkBoxFilesOnly_CheckedChanged);
            // 
            // comboBoxBuzzword
            // 
            resources.ApplyResources(this.comboBoxBuzzword, "comboBoxBuzzword");
            this.tableLayoutPanel2.SetColumnSpan(this.comboBoxBuzzword, 5);
            this.comboBoxBuzzword.FormattingEnabled = true;
            this.comboBoxBuzzword.Name = "comboBoxBuzzword";
            this.toolTip1.SetToolTip(this.comboBoxBuzzword, resources.GetString("comboBoxBuzzword.ToolTip"));
            this.comboBoxBuzzword.TextChanged += new System.EventHandler(this.comboBoxBuzzword_TextChanged);
            this.comboBoxBuzzword.Validated += new System.EventHandler(this.comboBoxBuzzword_Validated);
            // 
            // checkBoxSearchText
            // 
            resources.ApplyResources(this.checkBoxSearchText, "checkBoxSearchText");
            this.tableLayoutPanel2.SetColumnSpan(this.checkBoxSearchText, 2);
            this.checkBoxSearchText.Name = "checkBoxSearchText";
            this.toolTip1.SetToolTip(this.checkBoxSearchText, resources.GetString("checkBoxSearchText.ToolTip"));
            this.checkBoxSearchText.UseVisualStyleBackColor = true;
            this.checkBoxSearchText.Click += new System.EventHandler(this.checkBoxSearchText_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 15000;
            this.toolTip1.InitialDelay = 500;
            this.toolTip1.ReshowDelay = 100;
            // 
            // checkBoxPathMustContain
            // 
            resources.ApplyResources(this.checkBoxPathMustContain, "checkBoxPathMustContain");
            this.checkBoxPathMustContain.Name = "checkBoxPathMustContain";
            this.checkBoxPathMustContain.UseVisualStyleBackColor = true;
            // 
            // checkBoxPathMustNotContain
            // 
            resources.ApplyResources(this.checkBoxPathMustNotContain, "checkBoxPathMustNotContain");
            this.checkBoxPathMustNotContain.Name = "checkBoxPathMustNotContain";
            this.checkBoxPathMustNotContain.UseVisualStyleBackColor = true;
            // 
            // checkBoxAlwaysSkipLargeFiles
            // 
            resources.ApplyResources(this.checkBoxAlwaysSkipLargeFiles, "checkBoxAlwaysSkipLargeFiles");
            this.checkBoxAlwaysSkipLargeFiles.BackColor = System.Drawing.SystemColors.Control;
            this.checkBoxAlwaysSkipLargeFiles.Name = "checkBoxAlwaysSkipLargeFiles";
            this.checkBoxAlwaysSkipLargeFiles.UseVisualStyleBackColor = false;
            // 
            // labelLinesText
            // 
            resources.ApplyResources(this.labelLinesText, "labelLinesText");
            this.labelLinesText.Name = "labelLinesText";
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this.textBoxOutput, 0, 11);
            this.tableLayoutPanel2.Controls.Add(this.buttonDown, 5, 12);
            this.tableLayoutPanel2.Controls.Add(this.label3, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonUp, 4, 12);
            this.tableLayoutPanel2.Controls.Add(this.textBoxFilePattern, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.textBoxFind, 0, 12);
            this.tableLayoutPanel2.Controls.Add(this.buttonX, 9, 12);
            this.tableLayoutPanel2.Controls.Add(this.comboBoxBuzzword, 2, 3);
            this.tableLayoutPanel2.Controls.Add(this.checkBoxSearchText, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.checkBoxAdvancedOptions, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.buttonBigFileSkip, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.progressBarCurrent, 0, 7);
            this.tableLayoutPanel2.Controls.Add(this.progressBarTotal, 0, 6);
            this.tableLayoutPanel2.Controls.Add(this.buttonRun, 9, 2);
            this.tableLayoutPanel2.Controls.Add(this.labelHHlist, 0, 5);
            this.tableLayoutPanel2.Controls.Add(this.checkBoxComputer, 9, 1);
            this.tableLayoutPanel2.Controls.Add(this.buttonGetLocation, 9, 0);
            this.tableLayoutPanel2.Controls.Add(this.textBoxProcessFile, 5, 0);
            this.tableLayoutPanel2.Controls.Add(this.textBoxFolderPattern, 6, 1);
            this.tableLayoutPanel2.Controls.Add(this.checkBoxSkip, 8, 2);
            this.tableLayoutPanel2.Controls.Add(this.textBoxFolderExcludes, 6, 2);
            this.tableLayoutPanel2.Controls.Add(this.checkBoxPathMustContain, 5, 1);
            this.tableLayoutPanel2.Controls.Add(this.checkBoxPathMustNotContain, 5, 2);
            this.tableLayoutPanel2.Controls.Add(this.labelFolderExcludes, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonCancel, 5, 4);
            this.tableLayoutPanel2.Controls.Add(this.panelTextOptions, 0, 8);
            this.tableLayoutPanel2.Controls.Add(this.panelSize, 0, 9);
            this.tableLayoutPanel2.Controls.Add(this.panelDate, 0, 10);
            this.tableLayoutPanel2.Controls.Add(this.listViewOutput, 9, 11);
            this.tableLayoutPanel2.Controls.Add(this.checkBoxLiveRefresh, 9, 10);
            this.tableLayoutPanel2.Controls.Add(this.checkBoxKeepOutput, 9, 9);
            this.tableLayoutPanel2.Controls.Add(this.panelSeparator, 9, 8);
            this.tableLayoutPanel2.Controls.Add(this.checkBoxMatchCase, 7, 3);
            this.tableLayoutPanel2.Controls.Add(this.previewCtl, 6, 11);
            this.tableLayoutPanel2.Controls.Add(this.checkBoxPreview, 9, 5);
            this.tableLayoutPanel2.Controls.Add(this.checkBoxExact, 0, 1);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // panelTextOptions
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.panelTextOptions, 9);
            this.panelTextOptions.Controls.Add(this.numericUpDownHugeFileLineCountLimit);
            this.panelTextOptions.Controls.Add(this.checkBoxSkipSearchingBinaryFiles);
            this.panelTextOptions.Controls.Add(this.checkBoxAlwaysSkipLargeFiles);
            this.panelTextOptions.Controls.Add(this.labelLinesText);
            this.panelTextOptions.Controls.Add(this.checkBoxFilesOnly);
            resources.ApplyResources(this.panelTextOptions, "panelTextOptions");
            this.panelTextOptions.Name = "panelTextOptions";
            // 
            // numericUpDownHugeFileLineCountLimit
            // 
            this.numericUpDownHugeFileLineCountLimit.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.numericUpDownHugeFileLineCountLimit.Increment = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            resources.ApplyResources(this.numericUpDownHugeFileLineCountLimit, "numericUpDownHugeFileLineCountLimit");
            this.numericUpDownHugeFileLineCountLimit.Maximum = new decimal(new int[] {
            -1530494977,
            232830,
            0,
            0});
            this.numericUpDownHugeFileLineCountLimit.Name = "numericUpDownHugeFileLineCountLimit";
            this.numericUpDownHugeFileLineCountLimit.Value = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            // 
            // checkBoxKeepOutput
            // 
            this.checkBoxKeepOutput.AutoCheck = false;
            resources.ApplyResources(this.checkBoxKeepOutput, "checkBoxKeepOutput");
            this.checkBoxKeepOutput.Name = "checkBoxKeepOutput";
            this.checkBoxKeepOutput.UseVisualStyleBackColor = true;
            this.checkBoxKeepOutput.MouseDown += new System.Windows.Forms.MouseEventHandler(this.checkBoxKeepOutput_MouseDown);
            // 
            // panelSeparator
            // 
            this.panelSeparator.Controls.Add(this.textBoxSeparator);
            this.panelSeparator.Controls.Add(this.labelSeparator);
            resources.ApplyResources(this.panelSeparator, "panelSeparator");
            this.panelSeparator.Name = "panelSeparator";
            // 
            // previewCtl
            // 
            this.previewCtl.BackColor = System.Drawing.SystemColors.ControlLight;
            resources.ApplyResources(this.previewCtl, "previewCtl");
            this.previewCtl.Name = "previewCtl";
            this.previewCtl.Enter += new System.EventHandler(this.previewCtl_Enter);
            this.previewCtl.Leave += new System.EventHandler(this.previewCtl_Leave);
            // 
            // checkBoxPreview
            // 
            resources.ApplyResources(this.checkBoxPreview, "checkBoxPreview");
            this.checkBoxPreview.Name = "checkBoxPreview";
            this.checkBoxPreview.UseVisualStyleBackColor = true;
            this.checkBoxPreview.CheckedChanged += new System.EventHandler(this.checkBoxPreview_CheckedChanged);
            // 
            // checkBoxExact
            // 
            resources.ApplyResources(this.checkBoxExact, "checkBoxExact");
            this.checkBoxExact.Name = "checkBoxExact";
            this.checkBoxExact.UseVisualStyleBackColor = true;
            this.checkBoxExact.CheckedChanged += new System.EventHandler(this.checkBoxExact_CheckedChanged);
            // 
            // timerUpdate
            // 
            this.timerUpdate.Interval = 200;
            this.timerUpdate.Tick += new System.EventHandler(this.timerUpdate_Tick);
            // 
            // AltF7Form
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.Controls.Add(this.tableLayoutPanel2);
            this.KeyPreview = true;
            this.Name = "AltF7Form";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AltF7Form_FormClosing);
            this.Load += new System.EventHandler(this.AltF7Form_Load);
            this.ResizeEnd += new System.EventHandler(this.AltF7Form_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AltF7Form_KeyDown);
            this.contextMenuStripOutput.ResumeLayout(false);
            this.contextMenuStripListView.ResumeLayout(false);
            this.panelDate.ResumeLayout(false);
            this.panelDate.PerformLayout();
            this.panelSize.ResumeLayout(false);
            this.panelSize.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSize)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.panelTextOptions.ResumeLayout(false);
            this.panelTextOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownHugeFileLineCountLimit)).EndInit();
            this.panelSeparator.ResumeLayout(false);
            this.panelSeparator.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.TextBox textBoxProcessFile;
        private System.Windows.Forms.Button buttonGetLocation;
        private System.Windows.Forms.Label labelHHlist;
        private System.Windows.Forms.TextBox textBoxFilePattern;
        private System.Windows.Forms.CheckBox checkBoxSkipSearchingBinaryFiles;
        private System.Windows.Forms.ProgressBar progressBarTotal;
        private System.Windows.Forms.CheckBox checkBoxOutputAddLine;
        //private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.RichTextBox textBoxOutput; 
        private System.Windows.Forms.ContextMenuStrip contextMenuStripOutput;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOpenFile;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemJumpToFile;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSelectAll;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCopy;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPaste;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemClearWindow;
        private System.Windows.Forms.TextBox textBoxSeparator;
        private System.Windows.Forms.Label labelSeparator;
        private System.Windows.Forms.TextBox textBoxFolderPattern;
        private System.Windows.Forms.Label labelFolderExcludes;
        private System.Windows.Forms.TextBox textBoxFolderExcludes;
        private System.Windows.Forms.ProgressBar progressBarCurrent;
        private System.Windows.Forms.CheckBox checkBoxMatchCase;
        private System.Windows.Forms.TextBox textBoxFind;
        private System.Windows.Forms.Button buttonUp;
        private System.Windows.Forms.Button buttonDown;
        private System.Windows.Forms.Button buttonX;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem resetColorsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem colorizeFindTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem colorizeBuzzwordsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem appendToOutputToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveOutputToolStripMenuItem;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBoxComputer;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemCloseJumpToFile;
        private System.Windows.Forms.CheckBox checkBoxLiveRefresh;
        private System.Windows.Forms.CheckBox checkBoxSkip;
        private System.Windows.Forms.Button buttonBigFileSkip;
        private System.Windows.Forms.Timer timerBigFile;
        private System.Windows.Forms.CheckBox checkBoxAdvancedOptions;
        private System.Windows.Forms.RadioButton radioButtonToday;
        private System.Windows.Forms.RadioButton radioButtonLastWeek;
        private System.Windows.Forms.Panel panelDate;
        private System.Windows.Forms.RadioButton radioButtonRange;
        private System.Windows.Forms.RadioButton radioButtonLastMonth;
        private System.Windows.Forms.Panel panelSize;
        private System.Windows.Forms.RadioButton radioButtonSmaller;
        private System.Windows.Forms.RadioButton radioButtonEqual;
        private System.Windows.Forms.RadioButton radioButtonLarger;
        private System.Windows.Forms.DateTimePicker dateTimePickerStop;
        private System.Windows.Forms.DateTimePicker dateTimePickerStart;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxFilesOnly;
        private System.Windows.Forms.ColumnHeader columnHeaderDate;
        private System.Windows.Forms.ColumnHeader columnHeaderSize;
        private System.Windows.Forms.ColumnHeader columnHeaderFile;
        private System.Windows.Forms.ListView listViewOutput;
        private System.Windows.Forms.ColumnHeader columnHeaderPath;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripListView;
        private System.Windows.Forms.ToolStripMenuItem openFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem winmergePairOfFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem copySelectedFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteSelectedFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem jumpToFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeAndJumpToFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ComboBox comboBoxBuzzword;
        private System.Windows.Forms.CheckBox checkBoxSearchText;
        private System.Windows.Forms.ToolStripMenuItem copySelectedFilesToClipBoardToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox checkBoxPathMustContain;
        private System.Windows.Forms.CheckBox checkBoxPathMustNotContain;
        private System.Windows.Forms.CheckBox checkBoxAlwaysSkipLargeFiles;
        private System.Windows.Forms.Label labelLinesText;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Panel panelSeparator;
        private System.Windows.Forms.Panel panelTextOptions;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ColumnHeader columnHeaderExt;
        private System.Windows.Forms.RadioButton radioButtonDateAll;
        private System.Windows.Forms.RadioButton radioButtonSizeAll;
        private System.Windows.Forms.Timer timerUpdate;
        private System.Windows.Forms.ToolStripMenuItem copyListItemsToClipBoardToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxKeepOutput;
        private System.Windows.Forms.NumericUpDown numericUpDownSize;
        private System.Windows.Forms.NumericUpDown numericUpDownHugeFileLineCountLimit;
        private System.Windows.Forms.CheckBox checkBoxLen248;
        private PreviewCtl previewCtl;
        private System.Windows.Forms.CheckBox checkBoxPreview;
        private System.Windows.Forms.CheckBox checkBoxExact;
    }
}

