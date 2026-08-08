#nullable enable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace GwCopyPro.Forms
{
    partial class BatchInsertDialog
    {
        /// <summary>Required designer variable.</summary>
        private IContainer? components = null;

        /// <summary>Clean up any resources being used.</summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _blinkTimer.Stop();
                _blinkTimer.Dispose();
                _cts.Cancel();
                _cts.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        private Panel accent = null!;

        /// <summary>
        /// Required method for Designer support - do not modify the contents of this method with the code editor.
        /// Builds only the static chrome; the per-member rows and action buttons are laid out
        /// dynamically in <see cref="BuildDynamicContent"/> because their positions and the
        /// form's own size depend on the group's member count at construction time.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.accent = new Panel();
            this.SuspendLayout();
            //
            // accent
            //
            this.accent.Dock = DockStyle.Top;
            this.accent.Height = 4;
            this.accent.BackColor = Color.FromArgb(40, 160, 80);
            //
            // BatchInsertDialog
            //
            this.BackColor = Color.FromArgb(18, 22, 32);
            this.ForeColor = Color.FromArgb(180, 210, 255);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.ControlBox = false;
            this.Controls.Add(this.accent);
            this.ResumeLayout(false);
        }
    }
}
