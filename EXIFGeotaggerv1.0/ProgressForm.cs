﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EXIFGeotagger //v0._1
{
    public partial class ProgressForm : Form
    {
        public event cancelDelegate cancel;
        public delegate void cancelDelegate(object sender, EventArgs e);
        private GeotagReport report;
        public event FinishDelegate Finish;
        public delegate void FinishDelegate();
        public ProgressForm(string label)
        {
            InitializeComponent();
            lbMessage.Text = label;
            btnOK.Enabled = false;
        }

        public Boolean callFinish { get; set; } 

        public void setReport(GeotagReport report)
        {
            this.report = report;
        }

        public string Message
        {
            set { lbMessage.Text = value; }
        }

        public void enableOK()
        {
            btnOK.Enabled = true; ;
        }

        public void disableCancel()
        {
            btnCancel.Enabled = false;
        }

        public int ProgressValue
        {
            set { progressBar1.Value = value; }
        }

        public event EventHandler<EventArgs> Canceled;

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            // Create a copy of the event to work with
            EventHandler<EventArgs> ea = Canceled;
            /* If there are no subscribers, eh will be null so we need to check
             * to avoid a NullReferenceException. */
            cancel(sender, e);
        }

        private void ProgressForm_Load(object sender, EventArgs e)
        {
            this.BringToFront();
            this.TopMost = true;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (callFinish)
            {
                Finish();
            }
            Close(); ;
        }
    }
}
