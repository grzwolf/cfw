using System;
using System.IO;
using System.Windows.Forms;

namespace ChangeModifiedTime {
    public partial class AttributesEditor : Form {
        private string m_path;
        private readonly bool m_directory;

        public AttributesEditor(string filename, bool bDirectory) {
            this.InitializeComponent();
            this.m_path = filename;
            this.m_directory = bDirectory;
            this.ShowFileTimes(this.m_path);
        }

        void ShowFileTimes(string filename) {
            //Get path
            this.m_path = filename;

            //Display Path in the TextBox
            this.tbStatus.Text = this.m_path;
            this.tbStatus.Select(this.tbStatus.Text.Length, 0);

            //Get the Attributes of the file and assign them to the DateTimePicker Object
            if ( this.m_directory ) {
                this.dtpCreated.Value = Directory.GetCreationTime(this.m_path);
                this.dtpModified.Value = Directory.GetLastWriteTime(this.m_path);
                this.dtpAccessed.Value = Directory.GetLastAccessTime(this.m_path);
            } else {
                this.dtpCreated.Value = File.GetCreationTime(this.m_path);
                this.dtpModified.Value = File.GetLastWriteTime(this.m_path);
                this.dtpAccessed.Value = File.GetLastAccessTime(this.m_path);
            }

            //Enable the CheckBoxes
            this.cbCreated.Enabled = true;
            this.cbModified.Enabled = true;
            this.cbAccessed.Enabled = true;

            //Enable the Save button
            this.bSave.Enabled = true;
        }

        private void bSave_Click(object sender, EventArgs e) {
            try {
                if ( this.m_path != "" ) {
                    if ( this.checkBoxAllTheSame.Checked ) {
                        if ( this.m_directory ) {
                            Directory.SetCreationTime(this.m_path, this.dtpCreated.Value);
                            Directory.SetLastWriteTime(this.m_path, this.dtpCreated.Value);
                            Directory.SetLastAccessTime(this.m_path, this.dtpCreated.Value);
                        } else {
                            File.SetCreationTime(this.m_path, this.dtpCreated.Value);
                            File.SetLastWriteTime(this.m_path, this.dtpCreated.Value);
                            File.SetLastAccessTime(this.m_path, this.dtpCreated.Value);
                        }
                    } else {
                        //Modify the File Created time
                        if ( this.cbCreated.Checked ) {
                            if ( this.m_directory ) {
                                Directory.SetCreationTime(this.m_path, this.dtpCreated.Value);
                            } else {
                                File.SetCreationTime(this.m_path, this.dtpCreated.Value);
                            }
                        }

                        //Modify the File Modified time
                        if ( this.cbModified.Checked ) {
                            if ( this.m_directory ) {
                                Directory.SetLastWriteTime(this.m_path, this.dtpModified.Value);
                            } else {
                                File.SetLastWriteTime(this.m_path, this.dtpModified.Value);
                            }
                        }

                        //Modify the File Accessed time
                        if ( this.cbAccessed.Checked ) {
                            if ( this.m_directory ) {
                                Directory.SetLastAccessTime(this.m_path, this.dtpAccessed.Value);
                            } else {
                                File.SetLastAccessTime(this.m_path, this.dtpAccessed.Value);
                            }
                        }

                        //Display Message in the Text Box if any of the attributes has been modified
                        if ( this.cbCreated.Enabled || this.cbModified.Enabled || this.cbAccessed.Enabled ) {
                            this.tbStatus.Text = "Values set successfully.";
                        }
                    }
                }
            } catch ( Exception ) {
                MessageBox.Show("File time could not be set. Any further operation will be stopped.\n\nYou may retry as 'Administrator'.", "Error");
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                this.Close();
                return;
            }

            // closing this dlg with "Yes" means: all selected folders/files shall gett the same datetime
            if ( this.checkBoxApplyToSelection.Checked && !this.checkBoxRecursion.Checked ) {
                this.DialogResult = System.Windows.Forms.DialogResult.Yes;
            } else {
                if ( this.checkBoxRecursion.Checked ) {
                    // closing this dlg with "OK" means: only the first selected item will set datetime
                    this.DialogResult = System.Windows.Forms.DialogResult.Ignore;
                } else {
                    // closing this dlg with "OK" means: only the first selected item will set datetime
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                }
            }
            this.Close();
        }

        // checkbox status checked correlates with enable status of datetimepicker
        private void cbCreated_CheckedChanged(object sender, EventArgs e) {
            if ( this.checkBoxAllTheSame.Checked ) {
                return;
            }
            this.dtpCreated.Enabled = this.cbCreated.Checked;
        }
        private void cbModified_CheckedChanged(object sender, EventArgs e) {
            if ( this.checkBoxAllTheSame.Checked ) {
                return;
            }
            this.dtpModified.Enabled = this.cbModified.Checked;
        }
        private void cbAccessed_CheckedChanged(object sender, EventArgs e) {
            if ( this.checkBoxAllTheSame.Checked ) {
                return;
            }
            this.dtpAccessed.Enabled = this.cbAccessed.Checked;
        }

        private void checkBoxRecursion_CheckedChanged(object sender, EventArgs e) {
            if ( this.checkBoxRecursion.Checked ) {
                this.checkBoxApplyToSelection.Checked = true;
                this.checkBoxApplyToSelection.Enabled = false;
            } else {
                this.checkBoxApplyToSelection.Checked = false;
                this.checkBoxApplyToSelection.Enabled = true;
            }
        }

        // all the same works only well for files, not for directories
        private void checkBoxAllTheSame_CheckedChanged(object sender, EventArgs e) {
            if ( this.checkBoxAllTheSame.Checked ) {
                this.dtpCreated.Enabled = true;
                this.cbCreated.Checked = true;
                this.cbCreated.Enabled = false;
                this.dtpModified.Enabled = false;
                this.cbModified.Checked = true;
                this.cbModified.Enabled = false;
                this.labelModified.Enabled = false;
                this.dtpAccessed.Enabled = false;
                this.cbAccessed.Checked = true;
                this.cbAccessed.Enabled = false;
                this.labelAccessed.Enabled = false;
                this.dtpModified.Value = this.dtpCreated.Value;
                this.dtpAccessed.Value = this.dtpCreated.Value;
            } else {
                this.dtpCreated.Enabled = false;
                this.cbCreated.Checked = false;
                this.cbCreated.Enabled = true;
                this.dtpModified.Enabled = false;
                this.cbModified.Checked = false;
                this.cbModified.Enabled = true;
                this.labelModified.Enabled = true;
                this.dtpAccessed.Enabled = false;
                this.cbAccessed.Checked = false;
                this.cbAccessed.Enabled = true;
                this.labelAccessed.Enabled = true;
                if ( this.m_directory ) {
                    this.dtpModified.Value = Directory.GetLastWriteTime(this.m_path);
                    this.dtpAccessed.Value = Directory.GetLastAccessTime(this.m_path);
                } else {
                    this.dtpModified.Value = File.GetLastWriteTime(this.m_path);
                    this.dtpAccessed.Value = File.GetLastAccessTime(this.m_path);
                }
            }
        }

    }
}