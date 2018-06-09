// frmProcess.cs


#region Using Directives
using System;
using System.Diagnostics;
using System.Windows.Forms;
#endregion

namespace Launcher
{
    public partial class frmProcess : Form
    {
        string selectedProcess;

        #region Constructors
        public frmProcess()
        {
            selectedProcess = String.Empty;

            InitializeComponent();
        }
        #endregion

        #region Form Events
        private void frmProcess_Load(object sender, EventArgs e)
        {
            refresh();
        }

        private void btnSelectProcess_Click(object sender, EventArgs e)
        {
            processSelection();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            refresh();
        }

        private void lstProcesses_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            processSelection();
        }
        #endregion

        #region Private Methods
        private void processSelection()
        {

            if (lstProcesses.SelectedItems.Count > 0)
            {
                selectedProcess = lstProcesses.SelectedItems[0].ToString();

                frmMain parent = (frmMain)this.Owner;
                parent.ProcessName = selectedProcess;

                Close();
            }
        }

        private void refresh()
        {
            lstProcesses.Items.Clear();

            Process[] processList = Process.GetProcesses();

            foreach (Process process in processList)
                lstProcesses.Items.Add(process.ProcessName + " | " + process.Id);
        }
        #endregion
    }
}
