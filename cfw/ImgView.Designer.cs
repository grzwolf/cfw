namespace cfw
{
    partial class ImgView
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
            this.buttonPlus = new System.Windows.Forms.Button();
            this.buttonMinus = new System.Windows.Forms.Button();
            this.button1by1 = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.imageBox = new CyotecViewer.ImageBox();
            this.buttonLeftTurn = new System.Windows.Forms.Button();
            this.buttonRightTurn = new System.Windows.Forms.Button();
            this.buttonRotate = new System.Windows.Forms.Button();
            this.checkBoxSave = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonPlus
            // 
            this.buttonPlus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonPlus.Location = new System.Drawing.Point(3, 3);
            this.buttonPlus.Name = "buttonPlus";
            this.buttonPlus.Size = new System.Drawing.Size(34, 24);
            this.buttonPlus.TabIndex = 1;
            this.buttonPlus.Text = "+";
            this.buttonPlus.UseVisualStyleBackColor = true;
            this.buttonPlus.Click += new System.EventHandler(this.buttonPlus_Click);
            // 
            // buttonMinus
            // 
            this.buttonMinus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonMinus.Location = new System.Drawing.Point(83, 3);
            this.buttonMinus.Name = "buttonMinus";
            this.buttonMinus.Size = new System.Drawing.Size(34, 24);
            this.buttonMinus.TabIndex = 2;
            this.buttonMinus.Text = "-";
            this.buttonMinus.UseVisualStyleBackColor = true;
            this.buttonMinus.Click += new System.EventHandler(this.buttonMinus_Click);
            // 
            // button1by1
            // 
            this.button1by1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button1by1.Location = new System.Drawing.Point(43, 3);
            this.button1by1.Name = "button1by1";
            this.button1by1.Size = new System.Drawing.Size(34, 24);
            this.button1by1.TabIndex = 3;
            this.button1by1.Text = "fit";
            this.button1by1.UseVisualStyleBackColor = true;
            this.button1by1.Click += new System.EventHandler(this.button1by1_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 8;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.buttonPlus, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.imageBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.buttonMinus, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.button1by1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonLeftTurn, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonRightTurn, 6, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonRotate, 7, 0);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxSave, 4, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(492, 458);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // imageBox
            // 
            this.imageBox.AutoScroll = true;
            this.imageBox.AutoSize = false;
            this.imageBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tableLayoutPanel1.SetColumnSpan(this.imageBox, 8);
            this.imageBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageBox.Location = new System.Drawing.Point(0, 30);
            this.imageBox.Margin = new System.Windows.Forms.Padding(0);
            this.imageBox.Name = "imageBox";
            this.imageBox.Size = new System.Drawing.Size(492, 428);
            this.imageBox.TabIndex = 0;
            this.imageBox.Zoom = 75;
            this.imageBox.ZoomIncrement = 25;
            // 
            // buttonLeftTurn
            // 
            this.buttonLeftTurn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonLeftTurn.Location = new System.Drawing.Point(375, 3);
            this.buttonLeftTurn.Name = "buttonLeftTurn";
            this.buttonLeftTurn.Size = new System.Drawing.Size(34, 23);
            this.buttonLeftTurn.TabIndex = 4;
            this.buttonLeftTurn.Text = "l";
            this.buttonLeftTurn.UseVisualStyleBackColor = true;
            this.buttonLeftTurn.Click += new System.EventHandler(this.buttonLeftTurn_Click);
            // 
            // buttonRightTurn
            // 
            this.buttonRightTurn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonRightTurn.Location = new System.Drawing.Point(415, 3);
            this.buttonRightTurn.Name = "buttonRightTurn";
            this.buttonRightTurn.Size = new System.Drawing.Size(34, 23);
            this.buttonRightTurn.TabIndex = 5;
            this.buttonRightTurn.Text = "r";
            this.buttonRightTurn.UseVisualStyleBackColor = true;
            this.buttonRightTurn.Click += new System.EventHandler(this.buttonRightTurn_Click);
            // 
            // buttonRotate
            // 
            this.buttonRotate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonRotate.Location = new System.Drawing.Point(455, 3);
            this.buttonRotate.Name = "buttonRotate";
            this.buttonRotate.Size = new System.Drawing.Size(34, 23);
            this.buttonRotate.TabIndex = 6;
            this.buttonRotate.Text = "t";
            this.buttonRotate.UseVisualStyleBackColor = true;
            this.buttonRotate.Click += new System.EventHandler(this.buttonRotate_Click);
            // 
            // checkBoxSave
            // 
            this.checkBoxSave.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.checkBoxSave.AutoSize = true;
            this.checkBoxSave.Location = new System.Drawing.Point(320, 6);
            this.checkBoxSave.Name = "checkBoxSave";
            this.checkBoxSave.Size = new System.Drawing.Size(49, 17);
            this.checkBoxSave.TabIndex = 7;
            this.checkBoxSave.Text = "save";
            this.checkBoxSave.UseVisualStyleBackColor = true;
            // 
            // ImgView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "ImgView";
            this.Size = new System.Drawing.Size(492, 458);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private CyotecViewer.ImageBox imageBox;
        private System.Windows.Forms.Button buttonPlus;
        private System.Windows.Forms.Button buttonMinus;
        private System.Windows.Forms.Button button1by1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button buttonLeftTurn;
        private System.Windows.Forms.Button buttonRightTurn;
        private System.Windows.Forms.Button buttonRotate;
        private System.Windows.Forms.CheckBox checkBoxSave;
    }
}
