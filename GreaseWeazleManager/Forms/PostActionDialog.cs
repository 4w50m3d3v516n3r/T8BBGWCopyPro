using System;
using System.Windows.Forms;
using GwCopyPro.Models;

namespace GwCopyPro.Forms
{
    /// <summary>
    /// Compact modal dialog for creating or editing a single <see cref="PostAction"/>.
    /// Presents fields for name, action type, executable path, arguments, and enabled state.
    /// Changes are written back to the supplied <see cref="PostAction"/> on OK.
    /// </summary>
    public partial class PostActionDialog : Form
    {
        private readonly PostAction _action = new();

        /// <summary>Design-time-only constructor. Do not use at runtime.</summary>
        public PostActionDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialises the dialog, pre-populating all fields from <paramref name="action"/>.
        /// </summary>
        /// <param name="action">The post-action to edit; modified in-place on OK.</param>
        public PostActionDialog(PostAction action)
        {
            _action = action;
            InitializeComponent();
            PopulateContent(action);
        }

        /// <summary>Fills in the per-instance field values that InitializeComponent cannot know statically.</summary>
        private void PopulateContent(PostAction action)
        {
            txtName.Text = action.Name;
            cmbType.SelectedIndex = (int)action.ActionType;
            txtExe.Text = action.ExecutablePath;
            txtArgs.Text = action.Arguments;
            chkEnabled.Checked = action.IsEnabled;
        }

        /// <summary>Opens a file picker for the executable/script path.</summary>
        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
                { Filter = "Executables (*.exe;*.bat;*.ps1)|*.exe;*.bat;*.ps1|All (*.*)|*.*" };
            if (ofd.ShowDialog(this) == DialogResult.OK) txtExe.Text = ofd.FileName;
        }

        /// <summary>Writes the edited field values back to the underlying <see cref="PostAction"/>.</summary>
        private void BtnOk_Click(object? sender, EventArgs e)
        {
            _action.Name           = txtName.Text;
            _action.ActionType     = (PostActionType)cmbType.SelectedIndex;
            _action.ExecutablePath = txtExe.Text;
            _action.Arguments      = txtArgs.Text;
            _action.IsEnabled      = chkEnabled.Checked;
        }
    }
}
