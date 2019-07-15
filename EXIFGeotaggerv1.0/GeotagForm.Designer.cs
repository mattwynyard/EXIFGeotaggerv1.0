namespace EXIFGeotagger //v0._1
{
    partial class GeotagForm
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
            this.txtInputPath = new System.Windows.Forms.TextBox();
            this.txtOutputPath = new System.Windows.Forms.TextBox();
            this.btnBrowse1 = new System.Windows.Forms.Button();
            this.btnBrowse2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnGeotag = new System.Windows.Forms.Button();
            this.txtDataSource = new System.Windows.Forms.TextBox();
            this.btnBrowse0 = new System.Windows.Forms.Button();
            this.lbDataSource = new System.Windows.Forms.Label();
            this.txtLayer = new System.Windows.Forms.TextBox();
            this.lbLayer = new System.Windows.Forms.Label();
            this.btnColor = new System.Windows.Forms.Button();
            this.lbColor = new System.Windows.Forms.Label();
            this.ckBoxGeoMark = new System.Windows.Forms.CheckBox();
            this.comboFileType = new System.Windows.Forms.ComboBox();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.SuspendLayout();
            // 
            // txtInputPath
            // 
            this.txtInputPath.Location = new System.Drawing.Point(12, 111);
            this.txtInputPath.Name = "txtInputPath";
            this.txtInputPath.Size = new System.Drawing.Size(447, 20);
            this.txtInputPath.TabIndex = 0;
            // 
            // txtOutputPath
            // 
            this.txtOutputPath.Location = new System.Drawing.Point(12, 153);
            this.txtOutputPath.Name = "txtOutputPath";
            this.txtOutputPath.Size = new System.Drawing.Size(447, 20);
            this.txtOutputPath.TabIndex = 1;
            // 
            // btnBrowse1
            // 
            this.btnBrowse1.Location = new System.Drawing.Point(465, 111);
            this.btnBrowse1.Name = "btnBrowse1";
            this.btnBrowse1.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse1.TabIndex = 2;
            this.btnBrowse1.Text = "Browse";
            this.btnBrowse1.UseVisualStyleBackColor = true;
            this.btnBrowse1.Click += new System.EventHandler(this.btnBrowse1_Click);
            // 
            // btnBrowse2
            // 
            this.btnBrowse2.Location = new System.Drawing.Point(465, 153);
            this.btnBrowse2.Name = "btnBrowse2";
            this.btnBrowse2.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse2.TabIndex = 3;
            this.btnBrowse2.Text = "Browse";
            this.btnBrowse2.UseVisualStyleBackColor = true;
            this.btnBrowse2.Click += new System.EventHandler(this.btnBrowse2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 92);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "Input Path";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 134);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 16);
            this.label2.TabIndex = 5;
            this.label2.Text = "Output Path";
            // 
            // btnGeotag
            // 
            this.btnGeotag.Location = new System.Drawing.Point(589, 208);
            this.btnGeotag.Name = "btnGeotag";
            this.btnGeotag.Size = new System.Drawing.Size(96, 41);
            this.btnGeotag.TabIndex = 6;
            this.btnGeotag.Text = "Geotag";
            this.btnGeotag.UseVisualStyleBackColor = true;
            this.btnGeotag.Click += new System.EventHandler(this.btnGeotag_Click);
            // 
            // txtDataSource
            // 
            this.txtDataSource.Location = new System.Drawing.Point(12, 38);
            this.txtDataSource.Name = "txtDataSource";
            this.txtDataSource.Size = new System.Drawing.Size(447, 20);
            this.txtDataSource.TabIndex = 7;
            this.txtDataSource.TextChanged += new System.EventHandler(this.txtDataSource_TextChanged);
            // 
            // btnBrowse0
            // 
            this.btnBrowse0.Location = new System.Drawing.Point(465, 38);
            this.btnBrowse0.Name = "btnBrowse0";
            this.btnBrowse0.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse0.TabIndex = 8;
            this.btnBrowse0.Text = "Browse";
            this.btnBrowse0.UseVisualStyleBackColor = true;
            this.btnBrowse0.Click += new System.EventHandler(this.btnBrowse0_Click);
            // 
            // lbDataSource
            // 
            this.lbDataSource.AutoSize = true;
            this.lbDataSource.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDataSource.Location = new System.Drawing.Point(9, 19);
            this.lbDataSource.Name = "lbDataSource";
            this.lbDataSource.Size = new System.Drawing.Size(83, 16);
            this.lbDataSource.TabIndex = 9;
            this.lbDataSource.Text = "Data Source";
            // 
            // txtLayer
            // 
            this.txtLayer.Location = new System.Drawing.Point(12, 219);
            this.txtLayer.Name = "txtLayer";
            this.txtLayer.Size = new System.Drawing.Size(156, 20);
            this.txtLayer.TabIndex = 10;
            this.txtLayer.Text = "default";
            this.txtLayer.TextChanged += new System.EventHandler(this.TxtLayer_TextChanged);
            // 
            // lbLayer
            // 
            this.lbLayer.AutoSize = true;
            this.lbLayer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbLayer.Location = new System.Drawing.Point(12, 200);
            this.lbLayer.Name = "lbLayer";
            this.lbLayer.Size = new System.Drawing.Size(42, 16);
            this.lbLayer.TabIndex = 11;
            this.lbLayer.Text = "Layer";
            // 
            // btnColor
            // 
            this.btnColor.Location = new System.Drawing.Point(174, 213);
            this.btnColor.Name = "btnColor";
            this.btnColor.Size = new System.Drawing.Size(32, 30);
            this.btnColor.TabIndex = 12;
            this.btnColor.UseVisualStyleBackColor = true;
            this.btnColor.Click += new System.EventHandler(this.btnColour_Click);
            // 
            // lbColor
            // 
            this.lbColor.AutoSize = true;
            this.lbColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbColor.Location = new System.Drawing.Point(171, 194);
            this.lbColor.Name = "lbColor";
            this.lbColor.Size = new System.Drawing.Size(47, 16);
            this.lbColor.TabIndex = 13;
            this.lbColor.Text = "Colour";
            // 
            // ckBoxGeoMark
            // 
            this.ckBoxGeoMark.AutoSize = true;
            this.ckBoxGeoMark.Location = new System.Drawing.Point(577, 67);
            this.ckBoxGeoMark.Name = "ckBoxGeoMark";
            this.ckBoxGeoMark.Size = new System.Drawing.Size(176, 17);
            this.ckBoxGeoMark.TabIndex = 14;
            this.ckBoxGeoMark.Text = "Include non-geomarked records";
            this.ckBoxGeoMark.UseVisualStyleBackColor = true;
            this.ckBoxGeoMark.CheckedChanged += new System.EventHandler(this.chkGeoMark_CheckedChanged);
            // 
            // comboFileType
            // 
            this.comboFileType.FormattingEnabled = true;
            this.comboFileType.Location = new System.Drawing.Point(600, 40);
            this.comboFileType.Name = "comboFileType";
            this.comboFileType.Size = new System.Drawing.Size(121, 21);
            this.comboFileType.TabIndex = 15;
            // 
            // colorDialog1
            // 
            this.colorDialog1.AllowFullOpen = false;
            this.colorDialog1.Color = System.Drawing.Color.LightCoral;
            // 
            // GeotagForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(795, 326);
            this.Controls.Add(this.comboFileType);
            this.Controls.Add(this.ckBoxGeoMark);
            this.Controls.Add(this.lbColor);
            this.Controls.Add(this.btnColor);
            this.Controls.Add(this.lbLayer);
            this.Controls.Add(this.txtLayer);
            this.Controls.Add(this.lbDataSource);
            this.Controls.Add(this.btnBrowse0);
            this.Controls.Add(this.txtDataSource);
            this.Controls.Add(this.btnGeotag);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnBrowse2);
            this.Controls.Add(this.btnBrowse1);
            this.Controls.Add(this.txtOutputPath);
            this.Controls.Add(this.txtInputPath);
            this.Name = "GeotagForm";
            this.Text = "GeotagForm";
            this.Load += new System.EventHandler(this.GeotagForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtInputPath;
        private System.Windows.Forms.TextBox txtOutputPath;
        private System.Windows.Forms.Button btnBrowse1;
        private System.Windows.Forms.Button btnBrowse2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnGeotag;
        private System.Windows.Forms.TextBox txtDataSource;
        private System.Windows.Forms.Button btnBrowse0;
        private System.Windows.Forms.Label lbDataSource;
        private System.Windows.Forms.TextBox txtLayer;
        private System.Windows.Forms.Label lbLayer;
        private System.Windows.Forms.Button btnColor;
        private System.Windows.Forms.Label lbColor;
        private System.Windows.Forms.CheckBox ckBoxGeoMark;
        private System.Windows.Forms.ComboBox comboFileType;
        private System.Windows.Forms.ColorDialog colorDialog1;
    }
}