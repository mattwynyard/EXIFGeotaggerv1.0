namespace EXIFGeotaggerv0._1
{
    partial class EXIFGeoTagger
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.btnGeotag = new System.Windows.Forms.Button();
            this.txtConsole = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(420, 39);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(110, 33);
            this.btnBrowse.TabIndex = 0;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtFilePath
            // 
            this.txtFilePath.Location = new System.Drawing.Point(42, 46);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(372, 20);
            this.txtFilePath.TabIndex = 1;
            // 
            // btnGeotag
            // 
            this.btnGeotag.Location = new System.Drawing.Point(430, 97);
            this.btnGeotag.Name = "btnGeotag";
            this.btnGeotag.Size = new System.Drawing.Size(75, 56);
            this.btnGeotag.TabIndex = 2;
            this.btnGeotag.Text = "GeoTag";
            this.btnGeotag.UseVisualStyleBackColor = true;
            this.btnGeotag.Click += new System.EventHandler(this.btnGeotag_Click);
            // 
            // txtConsole
            // 
            this.txtConsole.Location = new System.Drawing.Point(42, 97);
            this.txtConsole.Multiline = true;
            this.txtConsole.Name = "txtConsole";
            this.txtConsole.Size = new System.Drawing.Size(372, 184);
            this.txtConsole.TabIndex = 4;
            // 
            // EXIFGeoTagger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1210, 564);
            this.Controls.Add(this.txtConsole);
            this.Controls.Add(this.btnGeotag);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.btnBrowse);
            this.KeyPreview = true;
            this.Name = "EXIFGeoTagger";
            this.Text = "EXIFGeoTagger";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Button btnGeotag;
        private System.Windows.Forms.TextBox txtConsole;
    }
}

