// login.cs


#region Using Directives
using System;
using System.Diagnostics;
using System.Windows.Forms;
#endregion

/**
 * NOTE: ALL of these should be customized for your needs
 **/
namespace Launcher
{
    public partial class frmLogin : Form
    {
        #region Locals
        private string GetSecurityToken = String.Empty;
        private static Properties.Settings Settings = Properties.Settings.Default;
        private string Auth = "";
        #endregion

        #region Constructors
        public frmLogin()
        {
            InitializeComponent();
        }
        #endregion

        #region Form Events
        private void login_Load(object sender, EventArgs e)
        {
            string license = Settings.License;

            if (license.Length > 0)
                txtLogin.Text = license;
        }

        private void forgotButton_Click(object sender, EventArgs e)
        {
            Process.Start(Settings.BaseURL + Settings.Login);
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            Auth = Settings.BaseURL + Settings.BaseAuth;

            loginButton.Enabled = false;

            try
            {
                // TODO: Move strings to string resource
                GetSecurityToken = Utility.Request(Auth + "?action=GetSecurityToken");

                string CheckLicense = Utility.Request(Auth + "?action=CheckLicense&license=" + txtLogin.Text + "&token=" + GetSecurityToken);

                switch (Utility.DecryptAuthResponse(CheckLicense))
                {
                    case "Successful response: License found in database.":
                        GetSecurityToken = Utility.Request(Auth + "?action=GetSecurityToken");

                        string CheckBanStatus = Utility.Request(Auth + "?action=CheckBanStatus&license=" + txtLogin.Text + "&token=" + GetSecurityToken);

                        switch (Utility.DecryptAuthResponse(CheckBanStatus))
                        {
                            case "Successful response: License is not banned.":
                                Settings.License = txtLogin.Text;
                                Settings.Save();

                                Hide();

                                frmMain MainForm = new frmMain();
                                MainForm.Show();
                                break;
                            case "An Error Occured: Required authentication token missing. ID: 0x1":
                                MessageBox.Show(this, "A required authentication security token was not sent to the server.", "Security ID: 0x1");
                                break;
                            case "An Error Occured: Required authentication token missing. ID: 0x2":
                                MessageBox.Show(this, "A required authentication security token was not sent to the server.", "Security ID: 0x2");
                                break;
                            case "An Error Occured: License was not found in database. ID: 0x3":
                                MessageBox.Show(this, "The license you entered, \"" + txtLogin.Text + "\", was not found in the database.", "Security ID: 0x3");
                                break;
                            case "An Error Occured: License is banned. ID: 0x4":
                                MessageBox.Show(this, "License is banned.", "Security ID: 0x4");
                                break;
                            default:
                                MessageBox.Show(this, "An unknown error occured.", "Error");
                                break;
                        }
                        break;
                    case "An Error Occured: License was not found in database. ID: 0x3":
                        MessageBox.Show(this, "The license entered, \"" + txtLogin.Text + "\", was not found in the database.", "Security ID: 0x3");
                        break;
                    case "An Error Occured: Required authentication token missing. ID: 0x2":
                        MessageBox.Show(this, "A required authentication security token was not sent to the server.", "Security ID: 0x2");
                        break;
                    case "An Error Occured: Required authentication token missing. ID: 0x1":
                        MessageBox.Show(this, "A required authentication security token was not sent to the server.", "Security ID: 0x1");
                        break;
                    default:
                        MessageBox.Show(this, "An unknown error occured.", "Error");
                        break;
                }
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            loginButton.Enabled = true;
        }
        #endregion
    }
}