namespace EXIFGeotagger
{
    partial class DelimitedText
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
            this.txtDataSource = new System.Windows.Forms.TextBox();
            this.lbDataSource = new System.Windows.Forms.Label();
            this.txtLayer = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnColour = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lbDelimiter = new System.Windows.Forms.Label();
            this.txtCustom = new System.Windows.Forms.TextBox();
            this.lbCustom = new System.Windows.Forms.Label();
            this.rdComma = new System.Windows.Forms.RadioButton();
            this.rdColon = new System.Windows.Forms.RadioButton();
            this.rdSemiColon = new System.Windows.Forms.RadioButton();
            this.rdTab = new System.Windows.Forms.RadioButton();
            this.rdSpace = new System.Windows.Forms.RadioButton();
            this.cbXField = new System.Windows.Forms.ComboBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.lbXField = new System.Windows.Forms.Label();
            this.cbYField = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbID = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.txtLayer2 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // txtDataSource
            // 
            this.txtDataSource.Location = new System.Drawing.Point(41, 43);
            this.txtDataSource.Name = "txtDataSource";
            this.txtDataSource.Size = new System.Drawing.Size(562, 20);
            this.txtDataSource.TabIndex = 0;
            this.txtDataSource.TextChanged += new System.EventHandler(this.TxtDataSource_TextChanged);
            // 
            // lbDataSource
            // 
            this.lbDataSource.AutoSize = true;
            this.lbDataSource.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDataSource.Location = new System.Drawing.Point(38, 24);
            this.lbDataSource.Name = "lbDataSource";
            this.lbDataSource.Size = new System.Drawing.Size(83, 16);
            this.lbDataSource.TabIndex = 4;
            this.lbDataSource.Text = "Data Source";
            // 
            // txtLayer
            // 
            this.txtLayer.Location = new System.Drawing.Point(41, 97);
            this.txtLayer.Name = "txtLayer";
            this.txtLayer.Size = new System.Drawing.Size(231, 20);
            this.txtLayer.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(39, 78);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 16);
            this.label2.TabIndex = 6;
            this.label2.Text = "Layer Name";
            // 
            // btnColour
            // 
            this.btnColour.Location = new System.Drawing.Point(287, 97);
            this.btnColour.Name = "btnColour";
            this.btnColour.Size = new System.Drawing.Size(24, 23);
            this.btnColour.TabIndex = 7;
            this.btnColour.UseVisualStyleBackColor = true;
            this.btnColour.Click += new System.EventHandler(this.btnColour_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(275, 78);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 16);
            this.label4.TabIndex = 9;
            this.label4.Text = "Colour";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(609, 40);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 10;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lbDelimiter
            // 
            this.lbDelimiter.AutoSize = true;
            this.lbDelimiter.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDelimiter.Location = new System.Drawing.Point(42, 132);
            this.lbDelimiter.Name = "lbDelimiter";
            this.lbDelimiter.Size = new System.Drawing.Size(61, 16);
            this.lbDelimiter.TabIndex = 11;
            this.lbDelimiter.Text = "Delimiter";
            // 
            // txtCustom
            // 
            this.txtCustom.Location = new System.Drawing.Point(426, 151);
            this.txtCustom.Name = "txtCustom";
            this.txtCustom.Size = new System.Drawing.Size(21, 20);
            this.txtCustom.TabIndex = 18;
            this.txtCustom.TextChanged += new System.EventHandler(this.TxtCustom_TextChanged);
            // 
            // lbCustom
            // 
            this.lbCustom.AutoSize = true;
            this.lbCustom.Location = new System.Drawing.Point(453, 155);
            this.lbCustom.Name = "lbCustom";
            this.lbCustom.Size = new System.Drawing.Size(42, 13);
            this.lbCustom.TabIndex = 19;
            this.lbCustom.Text = "Custom";
            // 
            // rdComma
            // 
            this.rdComma.AutoSize = true;
            this.rdComma.Location = new System.Drawing.Point(42, 151);
            this.rdComma.Name = "rdComma";
            this.rdComma.Size = new System.Drawing.Size(60, 17);
            this.rdComma.TabIndex = 20;
            this.rdComma.TabStop = true;
            this.rdComma.Text = "Comma";
            this.rdComma.UseVisualStyleBackColor = true;
            this.rdComma.Click += new System.EventHandler(this.rdComma_Click);
            // 
            // rdColon
            // 
            this.rdColon.AutoSize = true;
            this.rdColon.Location = new System.Drawing.Point(350, 151);
            this.rdColon.Name = "rdColon";
            this.rdColon.Size = new System.Drawing.Size(52, 17);
            this.rdColon.TabIndex = 21;
            this.rdColon.TabStop = true;
            this.rdColon.Text = "Colon";
            this.rdColon.UseVisualStyleBackColor = true;
            this.rdColon.Click += new System.EventHandler(this.rdColon_Click);
            // 
            // rdSemiColon
            // 
            this.rdSemiColon.AutoSize = true;
            this.rdSemiColon.Location = new System.Drawing.Point(247, 151);
            this.rdSemiColon.Name = "rdSemiColon";
            this.rdSemiColon.Size = new System.Drawing.Size(75, 17);
            this.rdSemiColon.TabIndex = 22;
            this.rdSemiColon.TabStop = true;
            this.rdSemiColon.Text = "SemiColon";
            this.rdSemiColon.UseVisualStyleBackColor = true;
            this.rdSemiColon.Click += new System.EventHandler(this.rdSemiColon_Click);
            // 
            // rdTab
            // 
            this.rdTab.AutoSize = true;
            this.rdTab.Location = new System.Drawing.Point(189, 151);
            this.rdTab.Name = "rdTab";
            this.rdTab.Size = new System.Drawing.Size(44, 17);
            this.rdTab.TabIndex = 23;
            this.rdTab.TabStop = true;
            this.rdTab.Text = "Tab";
            this.rdTab.UseVisualStyleBackColor = true;
            this.rdTab.Click += new System.EventHandler(this.rdTab_Click);
            // 
            // rdSpace
            // 
            this.rdSpace.AutoSize = true;
            this.rdSpace.Location = new System.Drawing.Point(118, 151);
            this.rdSpace.Name = "rdSpace";
            this.rdSpace.Size = new System.Drawing.Size(56, 17);
            this.rdSpace.TabIndex = 24;
            this.rdSpace.TabStop = true;
            this.rdSpace.Text = "Space";
            this.rdSpace.UseVisualStyleBackColor = true;
            this.rdSpace.Click += new System.EventHandler(this.rdSpace_Click);
            // 
            // cbXField
            // 
            this.cbXField.FormattingEnabled = true;
            this.cbXField.Location = new System.Drawing.Point(204, 184);
            this.cbXField.Name = "cbXField";
            this.cbXField.Size = new System.Drawing.Size(175, 21);
            this.cbXField.TabIndex = 25;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(41, 211);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.Size = new System.Drawing.Size(899, 313);
            this.dataGridView1.TabIndex = 26;
            // 
            // lbXField
            // 
            this.lbXField.AutoSize = true;
            this.lbXField.Location = new System.Drawing.Point(159, 187);
            this.lbXField.Name = "lbXField";
            this.lbXField.Size = new System.Drawing.Size(39, 13);
            this.lbXField.TabIndex = 27;
            this.lbXField.Text = "X Field";
            // 
            // cbYField
            // 
            this.cbYField.FormattingEnabled = true;
            this.cbYField.Location = new System.Drawing.Point(444, 184);
            this.cbYField.Name = "cbYField";
            this.cbYField.Size = new System.Drawing.Size(175, 21);
            this.cbYField.TabIndex = 28;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(399, 187);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 29;
            this.label1.Text = "Y Field";
            // 
            // cbID
            // 
            this.cbID.FormattingEnabled = true;
            this.cbID.Location = new System.Drawing.Point(66, 184);
            this.cbID.Name = "cbID";
            this.cbID.Size = new System.Drawing.Size(73, 21);
            this.cbID.TabIndex = 30;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(42, 187);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(18, 13);
            this.label3.TabIndex = 31;
            this.label3.Text = "ID";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(386, 101);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(87, 17);
            this.checkBox1.TabIndex = 32;
            this.checkBox1.Text = "Link to Layer";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.CheckBox1_CheckedChanged);
            // 
            // txtLayer2
            // 
            this.txtLayer2.Location = new System.Drawing.Point(496, 101);
            this.txtLayer2.Name = "txtLayer2";
            this.txtLayer2.Size = new System.Drawing.Size(231, 20);
            this.txtLayer2.TabIndex = 33;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(493, 78);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 16);
            this.label5.TabIndex = 34;
            this.label5.Text = "Layer Name";
            // 
            // DelimitedText
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(977, 536);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtLayer2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbID);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbYField);
            this.Controls.Add(this.lbXField);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.cbXField);
            this.Controls.Add(this.rdSpace);
            this.Controls.Add(this.rdTab);
            this.Controls.Add(this.rdSemiColon);
            this.Controls.Add(this.rdColon);
            this.Controls.Add(this.rdComma);
            this.Controls.Add(this.lbCustom);
            this.Controls.Add(this.txtCustom);
            this.Controls.Add(this.lbDelimiter);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnColour);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtLayer);
            this.Controls.Add(this.lbDataSource);
            this.Controls.Add(this.txtDataSource);
            this.Name = "DelimitedText";
            this.Text = "Import Delimited Text File";
            this.Load += new System.EventHandler(this.DelimitedText_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtDataSource;
        private System.Windows.Forms.Label lbDataSource;
        private System.Windows.Forms.TextBox txtLayer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnColour;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lbDelimiter;
        private System.Windows.Forms.TextBox txtCustom;
        private System.Windows.Forms.Label lbCustom;
        private System.Windows.Forms.RadioButton rdComma;
        private System.Windows.Forms.RadioButton rdColon;
        private System.Windows.Forms.RadioButton rdSemiColon;
        private System.Windows.Forms.RadioButton rdTab;
        private System.Windows.Forms.RadioButton rdSpace;
        private System.Windows.Forms.ComboBox cbXField;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Label lbXField;
        private System.Windows.Forms.ComboBox cbYField;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbID;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.TextBox txtLayer2;
        private System.Windows.Forms.Label label5;
    }
}