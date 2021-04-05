using System;
using System.Diagnostics;        // FileVersionInfo
using System.IO;                 // File 
using System.Reflection;         // Assembly
using System.Windows.Forms;

namespace cfw {
    public partial class About : Form {
        public About() {
            this.InitializeComponent();

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            string fn = Application.ExecutablePath;
            DateTime dt = File.GetLastWriteTime(fn);
            this.Text = "About CFW - " + version + " - Build Date: " + dt.ToShortDateString() + " " + dt.ToLongTimeString();
        }

        private void OnButtonOk(object sender, EventArgs e) {
            this.Close();
        }

        private void About_Load(object sender, EventArgs e) {

        }
    }
}
