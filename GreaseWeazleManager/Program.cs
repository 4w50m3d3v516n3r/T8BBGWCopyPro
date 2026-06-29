using System;
using System.Windows.Forms;
using GwCopyPro.Forms;

namespace GwCopyPro
{
    /// <summary>
    /// Application entry point. Bootstraps WinForms and launches <see cref="MainForm"/>.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Application entry point. Configures visual styles, high-DPI mode, a
        /// global thread-exception handler, then starts the main form.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            Application.ThreadException += (s, e) =>
            {
                MessageBox.Show($"Unhandled error:\n{e.Exception.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            Application.Run(new MainForm());
        }
    }
}
