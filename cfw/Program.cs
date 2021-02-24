using System;
using System.Windows.Forms;

namespace cfw {
    //static class Program
    //{
    //    /// <summary>
    //    /// The main entry point for the application.
    //    /// </summary>
    //    [STAThread]
    //    static void Main()
    //    {
    //        Application.EnableVisualStyles();
    //        Application.SetCompatibleTextRenderingDefault(false);
    //        Application.Run(new MainForm());
    //    }
    //}

    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Add handler for UI thread exceptions
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(UIThreadException);

            // Force all WinForms errors to go through handler
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            // This handler is for catching non-UI thread exceptions
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            try {
                Application.Run(new MainForm());
            } catch {; }
        }

        // https://stackoverflow.com/questions/10552751/how-to-catch-an-unhandled-exception-in-c
        static void CurrentDomain_UnhandledException(Object sender, UnhandledExceptionEventArgs e) {
            try {
                Exception ex = (Exception)e.ExceptionObject;
                MessageBox.Show("Unhandled domain exception:\n\n" + ex.Message);
            } catch ( Exception exc ) {
                try {
                    MessageBox.Show("Fatal exception happend inside UnhadledExceptionHandler: \n\n" + exc.Message, "");
                } catch ( Exception ex ) {
                    MessageBox.Show("Unhadled domain exception while exception:\n\n" + ex.Message);
                }
            }
        }
        private static void UIThreadException(object sender, System.Threading.ThreadExceptionEventArgs t) {
            try {
                MessageBox.Show("Unhandled UIThreadException caught:\n\n" + t.Exception);
            } catch {
                try {
                    MessageBox.Show("Fatal UIThreadException happend", "Fatal Windows Forms Error");
                } catch {
                    MessageBox.Show("Unhandled UIThreadException while handling another exception.");
                }
            }
        }

    }
}
