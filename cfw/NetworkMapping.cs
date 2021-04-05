using Microsoft.Win32;                                // registry
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;                                      // DriveInfo   
using System.Management;                              // ManagementObjectCollection --> needs reference to System.Management too
using System.Threading.Tasks;                         // task
using System.Windows.Forms;

namespace cfw {
    public partial class NetworkMapping : Form {
        public NetworkMapping(string sFolderToMap) {
            this.InitializeComponent();

            // render drives
            this.RenderDrives("C");

            // INI: read
            GrzTools.IniFile ini = new GrzTools.IniFile(System.Windows.Forms.Application.ExecutablePath + ".ini");
            for ( int i = 0; i < 10; i++ ) {
                string tmp = ini.IniReadValue("cfw", "mapping" + i.ToString(), "");
                if ( tmp.Length > 0 ) {
                    int ndx = this.comboBoxNetworkFolder.FindStringExact(tmp);
                    if ( ndx == -1 ) {
                        this.comboBoxNetworkFolder.Items.Add(tmp);
                    }
                }
            }

            // in case the network folder to map is already known
            int index = -1;
            if ( sFolderToMap.Length > 0 ) {
                string value = "";
                for ( int i = 0; i < this.comboBoxDrive.Items.Count; i++ ) {
                    value = this.comboBoxDrive.GetItemText(this.comboBoxDrive.Items[i]);
                    if ( value.StartsWith(sFolderToMap) ) {
                        index = i;
                    }
                }
            }

            // select 1st combobox item
            if ( this.comboBoxDrive.Items.Count > 0 ) {
                this.comboBoxDrive.SelectedIndex = 0;
                if ( index != -1 ) {
                    this.comboBoxDrive.SelectedIndex = index;
                }
            }
        }

        // get all drives
        void RenderDrives(string selectDrive) {
            List<string> drives = new List<string>();
            for ( int i = 67; i < 91; i++ ) {
                drives.Add((char)i + ":");
            }
            int ndx = 0;
            int selectIndex = 0;
            foreach ( string drive in drives ) {
                if ( drive.StartsWith(selectDrive) ) {
                    selectIndex = ndx;
                }
                // is it a permanently mapped network drive
                string remapAtLogon = "";
                string keyName = @"HKEY_CURRENT_USER\Network\" + drive.Substring(0, 1);
                string valueName = "RemotePath";
                string value = (string)Registry.GetValue(keyName, valueName, null);
                if ( value != null ) {
                    remapAtLogon = String.Format("<remap@logon to '{0}'>", value);
                }
                // skip non pingable drives, otherwise ResolveToUNC(..) will hang
                DriveInfo di = new DriveInfo(drive);
                if ( di.DriveType == DriveType.Network ) {
                    // network drive reachable?
                    if ( !GrzTools.Network.PingNetDriveOk(drive/*.Substring(0, 2)*/) ) {
                        this.comboBoxDrive.Items.Add(drive + " assigned network path is not accessible " + remapAtLogon);
                        ndx++;
                        continue;
                    }
                }
                // resolve network drives
                string mapping = MappedDriveResolver.ResolveToUNC(drive);
                if ( mapping == drive + "\\" ) {
                    mapping = "assigned local drive";
                } else {
                    if ( mapping == drive ) {
                        mapping = "";
                    } else {
                        this.comboBoxNetworkFolder.Items.Add(mapping);
                    }
                }
                // add info to combo
                this.comboBoxDrive.Items.Add(drive + " " + mapping + " " + remapAtLogon);
                ndx++;
            }

            this.comboBoxDrive.SelectedIndex = selectIndex;
        }
        // select network folder
        private void buttonSelectNetworkFolder_Click(object sender, EventArgs e) {
            // select network folder
            SelectFolderOrFile sff = new SelectFolderOrFile();
            sff.Text = "Select Network Folder";
            sff.DefaultPath = "Network";
            DialogResult dlr = sff.ShowDialog(this);
            if ( dlr != System.Windows.Forms.DialogResult.OK ) {
                sff.Dispose();
                return;
            }
            string DriveToMap = sff.ReturnPath;
            sff.Dispose();

            // add selection to combobox
            bool bFound = false;
            for ( int i = 0; i < this.comboBoxNetworkFolder.Items.Count; i++ ) {
                string value = this.comboBoxNetworkFolder.GetItemText(this.comboBoxNetworkFolder.Items[i]);
                if ( value == DriveToMap ) {
                    this.comboBoxNetworkFolder.SelectedIndex = i;
                    bFound = true;
                    break;
                }
            }
            if ( !bFound ) {
                this.comboBoxNetworkFolder.SelectedIndex = this.comboBoxNetworkFolder.Items.Add(DriveToMap);
            }
        }

        // enable/disable disconnect 
        private void comboBoxDrive_SelectedIndexChanged(object sender, EventArgs e) {
            if ( this.comboBoxDrive.Items.Count == 0 ) {
                return;
            }
            if ( this.comboBoxDrive.SelectedItem == null ) {
                return;
            }

            this.checkBoxRemapAtLogon.Checked = false;
            string selection = this.comboBoxDrive.SelectedItem.ToString();
            if ( selection.Contains("assigned local drive") ) {
                this.buttonConnect.Enabled = false;
                this.checkBoxRemapAtLogon.Text = "Restore Drive Mapping at logon";
                this.comboBoxNetworkFolder.ResetText();
                this.comboBoxNetworkFolder.SelectedIndex = -1;
            } else {
                this.buttonConnect.Enabled = true;
                // if a drive mapping was previously chosen persistent, we indicate this by checking the "remap" option
                string keyName = @"HKEY_CURRENT_USER\Network\" + selection.Substring(0, 1);
                string valueName = "RemotePath";
                string value = (string)Registry.GetValue(keyName, valueName, null);
                if ( value != null ) {
                    this.checkBoxRemapAtLogon.Checked = true;
                } else {
                    this.checkBoxRemapAtLogon.Checked = false;
                }
                // Connect or Disconnect: selection[4] == '<' means, we have a not connected network drive, which is supposed to be mapped at logon
                if ( (selection.Length > 5) && !(selection[4] == '<') ) {
                    this.buttonConnect.Text = "Disconnect";
                    this.checkBoxRemapAtLogon.Text = "Keep Drive Mapping at logon";
                } else {
                    this.buttonConnect.Text = "Connect";
                    this.checkBoxRemapAtLogon.Text = "Restore Drive Mapping at logon";
                }
                // preset the network path in case the drive is already mapped OR it is permanently mapped
                if ( selection.Contains("\\") ) {
                    string networkPath = selection.Substring(selection.IndexOf('\\'));
                    int length = networkPath.IndexOf(' ');
                    if ( length == -1 ) {
                        length = networkPath.LastIndexOf('\'');
                    }
                    networkPath = networkPath.Substring(0, length);
                    if ( length != -1 ) {
                        int index = this.comboBoxNetworkFolder.FindString(networkPath);
                        if ( index != -1 ) {
                            this.comboBoxNetworkFolder.SelectedIndex = index;
                        }
                    }
                }
            }
        }

        // connect & disconnect
        bool m_waitForTaskExit = true;
        private void buttonConnect_Click(object sender, EventArgs e) {
            // organize async break via "Cancel Window" --> event is sent to this method
            this.m_waitForTaskExit = true;
            if ( sender is Button ) {
                if ( ((Button)(sender)).Text == "cancel" ) {
                    this.m_waitForTaskExit = false;
                    return;
                }
            }

            // connect
            if ( this.buttonConnect.Text == "Connect" ) {
                if ( this.comboBoxDrive.SelectedItem.ToString().Contains("assigned local drive") ) {
                    MessageBox.Show("Please select another drive letter.");
                    return;
                }
                if ( this.comboBoxNetworkFolder.SelectedItem == null ) {
                    MessageBox.Show("Please select a network folder to connect to.", "Error");
                    return;
                }

                // get network folder & drive to map
                string DriveToMap = this.comboBoxNetworkFolder.SelectedItem.ToString();
                string driveletter = this.comboBoxDrive.SelectedItem.ToString().Substring(0, 1);
                this.buttonConnect.Text = "mapping ...";

                // get user & pwd
                string sUser = null;
                string sPwd = null;
                if ( this.checkBoxOption.Checked ) {
                    sUser = this.textBoxUser.Text;
                    sPwd = this.textBoxPwd.Text;
                }

                // allow to cancel the current operation
                CancelDialog dlg = new CancelDialog();
                dlg.WantClose += new EventHandler<EventArgs>(this.buttonConnect_Click);
                dlg.Location = new Point(MousePosition.X - 50, MousePosition.Y + 25);
                dlg.Show();

                // execute mapping in a separate thread to be able to cancle it (might take >60s until the network mapping returns with a failure)
                Task<int> task = new Task<int>(() => { return GrzTools.DriveSettings.MapNetworkDrive(driveletter, DriveToMap, sUser, sPwd, this.checkBoxRemapAtLogon.Checked); });
                task.Start();
                // cooperative wait for end of mapping OR user break
                do {
                    Application.DoEvents();
                } while ( !task.IsCompleted && this.m_waitForTaskExit );
                int ret = -1;
                if ( task.IsCompleted ) {
                    ret = task.Result;
                }

                // close cancel dialog
                this.buttonConnect.Text = "Connect";
                dlg.WantClose -= this.buttonConnect_Click;
                dlg.Close();

                // show result
                if ( ret != 0 ) {
                    if ( ret == -1 ) {
                        //                        MessageBox.Show("User interrupted network mapping service.", "User break");
                    } else {
                        MessageBox.Show("Something went wrong", "Error");
                    }
                } else {
                    GrzTools.AutoMessageBox.Show("Network folder '" + DriveToMap + "' was successfully connected to drive " + this.comboBoxDrive.SelectedItem.ToString(), "Success", 2000);
                    this.comboBoxDrive.Items.Clear();
                    this.RenderDrives(driveletter);
                }

                // done, get out
                return;
            }

            // disconnect
            if ( this.buttonConnect.Text == "Disconnect" ) {
                if ( this.comboBoxDrive.SelectedItem == null ) {
                    MessageBox.Show("Please select a drive to disconnect from network.", "Error");
                    return;
                }
                if ( this.comboBoxDrive.SelectedItem.ToString().Contains("assigned local drive") ) {
                    MessageBox.Show("Please select another drive letter.", "Error");
                    return;
                }

                string DriveToUnMap = this.comboBoxDrive.SelectedItem.ToString().Substring(0, 1);

                // execute disconnect
                Cursor.Current = Cursors.WaitCursor;
                int ret = GrzTools.DriveSettings.DisconnectNetworkDrive(DriveToUnMap, true, !this.checkBoxRemapAtLogon.Checked);
                Cursor.Current = Cursors.Default;
                if ( ret != 0 ) {
                    MessageBox.Show("Something went wrong", "Error");
                } else {
                    GrzTools.AutoMessageBox.Show(this.comboBoxDrive.SelectedItem.ToString() + " was successfully disconnected.", "Success", 2000);
                    this.comboBoxDrive.Items.Clear();
                    this.RenderDrives(DriveToUnMap);
                }
            }
        }

        // save ini
        private void NetworkMapping_FormClosing(object sender, FormClosingEventArgs e) {
            GrzTools.IniFile ini = new GrzTools.IniFile(System.Windows.Forms.Application.ExecutablePath + ".ini");
            for ( int i = 0; i < 10; i++ ) {
                if ( i < this.comboBoxNetworkFolder.Items.Count ) {
                    ini.IniWriteValue("cfw", "mapping" + i.ToString(), this.comboBoxNetworkFolder.GetItemText(this.comboBoxNetworkFolder.Items[i]));
                } else {
                    ini.IniWriteValue("cfw", "mapping" + i.ToString(), null);
                }
            }
        }

        // handle ENTER + DEL key pressed in ComboBox: insert new item / delete item 
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if ( this.ActiveControl == this.comboBoxNetworkFolder ) {
                if ( keyData == Keys.Return ) {
                    // insert item  
                    this.comboBoxNetworkFolder_Validated(null, null);
                    return true;
                }
                if ( keyData == Keys.Delete ) {
                    // delete item  
                    this.comboBoxNetworkFolder.Items.RemoveAt(this.comboBoxNetworkFolder.SelectedIndex);
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        // indicator that combobox had lost focus, ie. editing is over either thru ET or combo lost focus due to other reasons
        private void comboBoxNetworkFolder_Validated(object sender, EventArgs e) {
            // insert the just edited buzzword to the combobox list
            string str = this.comboBoxNetworkFolder.Text;
            int index = this.comboBoxNetworkFolder.FindStringExact(str);
            if ( index != -1 ) {
                this.comboBoxNetworkFolder.SelectedIndex = index;
            } else {
                if ( str.Length > 0 ) {
                    this.comboBoxNetworkFolder.Items.Insert(0, str);
                    this.comboBoxNetworkFolder.SelectedIndex = 0;
                }
            }
        }


        /// <summary>
        /// A static class to help with resolving a mapped drive path to a UNC network path.
        /// If a local drive path or a UNC network path are passed in, they will just be returned.
        /// </summary>
        /// <example>
        /// using System;
        /// using System.IO;
        /// using System.Management;    // Reference System.Management.dll
        /// 
        /// // Example/Test paths, these will need to be adjusted to match your environment. 
        /// string[] paths = new string[] {
        ///     @"Z:\ShareName\Sub-Folder",
        ///     @"\\ACME-FILE\ShareName\Sub-Folder",
        ///     @"\\ACME.COM\ShareName\Sub-Folder", // DFS
        ///     @"C:\Temp",
        ///     @"\\localhost\c$\temp",
        ///     @"\\workstation\Temp",
        ///     @"Z:", // Mapped drive pointing to \\workstation\Temp
        ///     @"C:\",
        ///     @"Temp",
        ///     @".\Temp",
        ///     @"..\Temp",
        ///     "",
        ///     "    ",
        ///     null
        /// };
        /// 
        /// foreach (var curPath in paths) {
        ///     try {
        ///         Console.WriteLine(string.Format("{0} = {1}",
        ///             curPath,
        ///             MappedDriveResolver.ResolveToUNC(curPath))
        ///         );
        ///     }
        ///     catch (Exception ex) {
        ///         Console.WriteLine(string.Format("{0} = {1}",
        ///             curPath,
        ///             ex.Message)
        ///         );
        ///     }
        /// }
        /// </example>
        public static class MappedDriveResolver {
            /// <summary>
            /// Resolves the given path to a full UNC path if the path is a mapped drive.
            /// Otherwise, just returns the given path.
            /// </summary>
            /// <param name="path">The path to resolve.</param>
            /// <returns></returns>
            public static string ResolveToUNC(string path) {
                if ( String.IsNullOrWhiteSpace(path) ) {
                    throw new ArgumentNullException("The path argument was null or whitespace.");
                }

                if ( !Path.IsPathRooted(path) ) {
                    throw new ArgumentException(
                        string.Format("The path '{0}' was not a rooted path and ResolveToUNC does not support relative paths.",
                            path)
                    );
                }

                // Is the path already in the UNC format?
                if ( path.StartsWith(@"\\") ) {
                    return path;
                }

                string rootPath = ResolveToRootUNC(path);

                if ( path.StartsWith(rootPath) ) {
                    return path; // Local drive, no resolving occurred
                } else {
                    return path.Replace(GetDriveLetter(path), rootPath);
                }
            }

            /// <summary>
            /// Resolves the given path to a root UNC path if the path is a mapped drive.
            /// Otherwise, just returns the given path.
            /// </summary>
            /// <param name="path">The path to resolve.</param>
            /// <returns></returns>
            public static string ResolveToRootUNC(string path) {
                if ( String.IsNullOrWhiteSpace(path) ) {
                    throw new ArgumentNullException("The path argument was null or whitespace.");
                }

                if ( !Path.IsPathRooted(path) ) {
                    throw new ArgumentException(
                        string.Format("The path '{0}' was not a rooted path and ResolveToRootUNC does not support relative paths.",
                        path)
                    );
                }

                if ( path.StartsWith(@"\\") ) {
                    return Directory.GetDirectoryRoot(path);
                }

                // Get just the drive letter for WMI call
                string driveletter = GetDriveLetter(path);

                // Query WMI if the drive letter is a network drive, and if so the UNC path for it
                try {
                    using ( ManagementObject mo = new ManagementObject() ) {
                        mo.Path = new ManagementPath(string.Format("Win32_LogicalDisk='{0}'", driveletter));

                        DriveType driveType = (DriveType)((uint)mo["DriveType"]);
                        string networkRoot = Convert.ToString(mo["ProviderName"]);

                        if ( driveType == DriveType.Network ) {
                            return networkRoot;
                        } else {
                            return driveletter + Path.DirectorySeparatorChar;
                        }
                    }
                } catch ( Exception ) {
                    return path;
                }
            }

            /// <summary>
            /// Checks if the given path is a network drive.
            /// </summary>
            /// <param name="path">The path to check.</param>
            /// <returns></returns>
            public static bool isNetworkDrive(string path) {
                if ( String.IsNullOrWhiteSpace(path) ) {
                    return false;
                    //                    throw new ArgumentNullException("The path argument was null or whitespace.");
                }

                if ( !Path.IsPathRooted(path) ) {
                    throw new ArgumentException(
                        string.Format("The path '{0}' was not a rooted path and ResolveToRootUNC does not support relative paths.",
                        path)
                    );
                }

                if ( path.StartsWith(@"\\") ) {
                    return true;
                }

                // Get just the drive letter for WMI call
                string driveletter = GetDriveLetter(path);

                // Query WMI if the drive letter is a network drive
                using ( ManagementObject mo = new ManagementObject() ) {
                    mo.Path = new ManagementPath(string.Format("Win32_LogicalDisk='{0}'", driveletter));
                    DriveType driveType = (DriveType)((uint)mo["DriveType"]);
                    return driveType == DriveType.Network;
                }
            }

            /// <summary>
            /// Given a path will extract just the drive letter with volume separator.
            /// </summary>
            /// <param name="path"></param>
            /// <returns>C:</returns>
            public static string GetDriveLetter(string path) {
                if ( String.IsNullOrWhiteSpace(path) ) {
                    throw new ArgumentNullException("The path argument was null or whitespace.");
                }

                if ( !Path.IsPathRooted(path) ) {
                    throw new ArgumentException(
                        string.Format("The path '{0}' was not a rooted path and GetDriveLetter does not support relative paths.",
                        path)
                    );
                }

                if ( path.StartsWith(@"\\") ) {
                    throw new ArgumentException("A UNC path was passed to GetDriveLetter");
                }

                return Directory.GetDirectoryRoot(path).Replace(Path.DirectorySeparatorChar.ToString(), "");
            }
        }

        private void checkBoxOption_CheckedChanged(object sender, EventArgs e) {
            if ( this.checkBoxOption.Checked ) {
                this.textBoxUser.Enabled = true;
                this.textBoxPwd.Enabled = true;
                this.labelUser.Enabled = true;
                this.labelPwd.Enabled = true;
            } else {
                this.textBoxUser.Enabled = false;
                this.textBoxPwd.Enabled = false;
                this.labelUser.Enabled = false;
                this.labelPwd.Enabled = false;
            }
        }

    }
}
