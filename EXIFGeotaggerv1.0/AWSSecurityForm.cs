using Amazon;
using Amazon.Runtime.CredentialManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EXIFGeotagger
{
    public partial class AWSSecurityForm : Form
    {
        public AWSSecurityForm()
        {
            InitializeComponent();
            TopMost = true;
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            string title;
            string message;
            DialogResult result;
            MessageBoxButtons buttons;
            try
            {
                var options = new CredentialProfileOptions
                {
                    AccessKey = txtAccessKey.Text,
                    SecretKey = txtSecretKey.Text
                };
                var profile = new CredentialProfile("shared_profile", options);
                profile.Region = RegionEndpoint.APSoutheast2;
                var sharedFile = new SharedCredentialsFile();
                sharedFile.RegisterProfile(profile);

            }
            catch (Exception ex) {
                title = "Ërror creating configuration file";
                message = ex.Message;
                buttons = MessageBoxButtons.OK;

                result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Error);
                if (result == DialogResult.Yes)
                {
                    Close();
                   
                }
            }
            title = "";
            message = "Succesfully created configuration file";
            buttons = MessageBoxButtons.OK;

            result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                Close();
                
            }
            Close();
        }
    }
}
