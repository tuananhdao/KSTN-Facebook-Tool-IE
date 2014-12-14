using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KSTN_Facebook_Tool
{
    public partial class MainForm : Form
    {
        // Disable WebBrowser Sounds
        const int FEATURE_DISABLE_NAVIGATION_SOUNDS = 21;
        const int SET_FEATURE_ON_PROCESS = 0x00000002;

        [DllImport("urlmon.dll")]
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Error)]
        static extern int CoInternetSetFeatureEnabled(
            int FeatureEntry,
            [MarshalAs(UnmanagedType.U4)] int dwFlags,
            bool fEnable);

        static void DisableClickSounds()
        {
            CoInternetSetFeatureEnabled(
                FEATURE_DISABLE_NAVIGATION_SOUNDS,
                SET_FEATURE_ON_PROCESS,
                true);
        }

        public MainForm()
        {
            InitializeComponent();
            DisableClickSounds();
            
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            Program.wbf = new WebBrowserForm();
            //Program.wbf.Show();
            Program.loadingForm = new LoadingForm();
            this.Focus();
            cbMethods.SelectedIndex = 0;
            //dgGroupInvites.DataSource = Program.wbf.dt;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.wbf.Close();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            Program.wbf.Fb_Login(txtUser.Text, txtPass.Text);
        }

        private void btnBrowse1_Click(object sender, EventArgs e)
        {
            var fDialog = new System.Windows.Forms.OpenFileDialog();
            fDialog.Title = "Open Arial Bitmap File";
            fDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            fDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            DialogResult result = fDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                if (Path.GetDirectoryName(fDialog.FileName) != fDialog.InitialDirectory)
                {
                    MessageBox.Show("Chỉ được sử dụng file ảnh trên Desktop!");
                    return;
                }
                string file = fDialog.SafeFileName;
                txtBrowse1.Text = file;
            }
        }

        private void btnBrowse2_Click(object sender, EventArgs e)
        {
            var fDialog = new System.Windows.Forms.OpenFileDialog();
            fDialog.Title = "Open Arial Bitmap File";
            fDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            fDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            DialogResult result = fDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                if (Path.GetDirectoryName(fDialog.FileName) != fDialog.InitialDirectory)
                {
                    MessageBox.Show("Chỉ được sử dụng file ảnh trên Desktop!");
                    return;
                }
                string file = fDialog.SafeFileName;
                txtBrowse2.Text = file;
            }
        }

        private void btnBrowse3_Click(object sender, EventArgs e)
        {
            var fDialog = new System.Windows.Forms.OpenFileDialog();
            fDialog.Title = "Open Arial Bitmap File";
            fDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            fDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            DialogResult result = fDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                if (Path.GetDirectoryName(fDialog.FileName) != fDialog.InitialDirectory) {
                    MessageBox.Show("Chỉ được sử dụng file ảnh trên Desktop!");
                    return;
                }
                string file = fDialog.SafeFileName;
                txtBrowse3.Text = file;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void btnPost_Click(object sender, EventArgs e)
        {
            if (txtBrowse1.Text == "" && txtBrowse2.Text == "" && txtBrowse3.Text == "" && txtContent.Text == "")
            {
                MessageBox.Show("Điền nội dung trước khi post bài!");
                return;
            }
            btnPost.Enabled = false;
            txtContent.Enabled = false;
            txtDelay.Enabled = false;
            cbMethods.Enabled = false;
            txtBrowse1.Enabled = false;
            txtBrowse2.Enabled = false;
            txtBrowse3.Enabled = false;
            btnBrowse1.Enabled = false;
            btnBrowse2.Enabled = false;
            btnBrowse3.Enabled = false;
            Program.wbf.AutoPost();
        }

        private void lblViewProfile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(lblViewProfile.Text);
        }

        private void btnGroupExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.FileName = "GROUPS.txt";
            saveFile.ShowDialog();

            using (StreamWriter sw = new StreamWriter(saveFile.FileName, false))
            {
                if (Program.wbf.dt.Rows.Count > 0)
                {
                    foreach (DataRow row in Program.wbf.dt.Rows)
                    {
                        sw.WriteLine(row["group_link"] + "");
                    }
                }
                else
                {
                    sw.WriteLine("No group found.");
                }
                sw.Close();
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*
            if (tabControl1.SelectedTab == tabControl1.TabPages["tabPageInvite"])
            {
                if (dgGroupInvites.DataSource == null)
                {
                    dgGroupInvites.DataSource = Program.wbf.dt;
                }
            }
            */
        }

        private void btnInvite_Click(object sender, EventArgs e)
        {
            Program.wbf.AutoInvite();
        }
    }
}
