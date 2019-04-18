namespace EXIFGeotagger
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EXIFGeoTagger));
            this.gMap = new GMap.NET.WindowsForms.GMapControl();
            this.txtConsole = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.listLayers = new System.Windows.Forms.ListView();
            this.ckBoxLayers = new System.Windows.Forms.CheckedListBox();
            this.lbPosition = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuNew = new System.Windows.Forms.ToolStripMenuItem();
            this.projectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSave = new System.Windows.Forms.ToolStripMenuItem();
            this.menuQuit = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.accessmdbToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.layerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.plotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.photosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.markersMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.accesDataMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.excelDataMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.geotagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuRunGeoTag = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bgWorker1 = new System.ComponentModel.BackgroundWorker();
            this.btnZoom = new System.Windows.Forms.Button();
            this.btnArrow = new System.Windows.Forms.Button();
            this.lbScale = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gMap
            // 
            this.gMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gMap.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gMap.Bearing = 0F;
            this.gMap.CanDragMap = true;
            this.gMap.EmptyTileColor = System.Drawing.Color.Navy;
            this.gMap.GrayScaleMode = false;
            this.gMap.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow;
            this.gMap.LevelsKeepInMemmory = 5;
            this.gMap.Location = new System.Drawing.Point(-3, 0);
            this.gMap.MarkersEnabled = true;
            this.gMap.MaxZoom = 100;
            this.gMap.MinZoom = 2;
            this.gMap.MouseWheelZoomEnabled = true;
            this.gMap.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter;
            this.gMap.Name = "gMap";
            this.gMap.NegativeMode = false;
            this.gMap.PolygonsEnabled = true;
            this.gMap.RetryLoadTile = 0;
            this.gMap.RoutesEnabled = true;
            this.gMap.ScaleMode = GMap.NET.WindowsForms.ScaleModes.Integer;
            this.gMap.SelectedAreaFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(65)))), ((int)(((byte)(105)))), ((int)(((byte)(225)))));
            this.gMap.ShowTileGridLines = false;
            this.gMap.Size = new System.Drawing.Size(665, 512);
            this.gMap.TabIndex = 5;
            this.gMap.Zoom = 10D;
            this.gMap.OnMarkerClick += new GMap.NET.WindowsForms.MarkerClick(this.gMap_OnMarkerClick);
            this.gMap.Load += new System.EventHandler(this.gMap_Load);
            this.gMap.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gMap_OnMouseMoved);
            // 
            // txtConsole
            // 
            this.txtConsole.Location = new System.Drawing.Point(203, 22);
            this.txtConsole.Multiline = true;
            this.txtConsole.Name = "txtConsole";
            this.txtConsole.Size = new System.Drawing.Size(299, 138);
            this.txtConsole.TabIndex = 4;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 21);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btnArrow);
            this.splitContainer1.Panel1.Controls.Add(this.btnZoom);
            this.splitContainer1.Panel1.Controls.Add(this.listLayers);
            this.splitContainer1.Panel1.Controls.Add(this.ckBoxLayers);
            this.splitContainer1.Panel1.Controls.Add(this.txtConsole);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lbScale);
            this.splitContainer1.Panel2.Controls.Add(this.lbPosition);
            this.splitContainer1.Panel2.Controls.Add(this.gMap);
            this.splitContainer1.Size = new System.Drawing.Size(1186, 531);
            this.splitContainer1.SplitterDistance = 517;
            this.splitContainer1.TabIndex = 6;
            // 
            // listLayers
            // 
            this.listLayers.CheckBoxes = true;
            this.listLayers.Location = new System.Drawing.Point(273, 227);
            this.listLayers.Name = "listLayers";
            this.listLayers.Size = new System.Drawing.Size(121, 97);
            this.listLayers.TabIndex = 6;
            this.listLayers.UseCompatibleStateImageBehavior = false;
            this.listLayers.View = System.Windows.Forms.View.List;
            this.listLayers.SelectedIndexChanged += new System.EventHandler(this.listLayers_SelectedIndexChanged);
            // 
            // ckBoxLayers
            // 
            this.ckBoxLayers.FormattingEnabled = true;
            this.ckBoxLayers.Location = new System.Drawing.Point(14, 190);
            this.ckBoxLayers.Name = "ckBoxLayers";
            this.ckBoxLayers.Size = new System.Drawing.Size(146, 139);
            this.ckBoxLayers.TabIndex = 5;
            this.ckBoxLayers.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ckBox_ItemCheck);
            this.ckBoxLayers.SelectedIndexChanged += new System.EventHandler(this.ckBoxLayers_SelectedIndexChanged);
            // 
            // lbPosition
            // 
            this.lbPosition.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbPosition.AutoSize = true;
            this.lbPosition.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbPosition.Location = new System.Drawing.Point(3, 515);
            this.lbPosition.Name = "lbPosition";
            this.lbPosition.Size = new System.Drawing.Size(47, 13);
            this.lbPosition.TabIndex = 6;
            this.lbPosition.Text = "lat: long:";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.layerToolStripMenuItem,
            this.plotToolStripMenuItem,
            this.dataToolStripMenuItem,
            this.geotagToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1210, 24);
            this.menuStrip1.TabIndex = 7;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuNew,
            this.menuOpen,
            this.menuSave,
            this.menuQuit,
            this.importToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // menuNew
            // 
            this.menuNew.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.projectToolStripMenuItem});
            this.menuNew.Name = "menuNew";
            this.menuNew.Size = new System.Drawing.Size(146, 22);
            this.menuNew.Text = "New ";
            // 
            // projectToolStripMenuItem
            // 
            this.projectToolStripMenuItem.Name = "projectToolStripMenuItem";
            this.projectToolStripMenuItem.Size = new System.Drawing.Size(111, 22);
            this.projectToolStripMenuItem.Text = "Project";
            // 
            // menuOpen
            // 
            this.menuOpen.Name = "menuOpen";
            this.menuOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.menuOpen.Size = new System.Drawing.Size(146, 22);
            this.menuOpen.Text = "&Open";
            this.menuOpen.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.fileMenuOpen_Click);
            // 
            // menuSave
            // 
            this.menuSave.Name = "menuSave";
            this.menuSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.menuSave.Size = new System.Drawing.Size(146, 22);
            this.menuSave.Text = "&Save";
            this.menuSave.Click += new System.EventHandler(this.menuSave_Click);
            // 
            // menuQuit
            // 
            this.menuQuit.Name = "menuQuit";
            this.menuQuit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.menuQuit.Size = new System.Drawing.Size(146, 22);
            this.menuQuit.Text = "&Quit";
            this.menuQuit.Click += new System.EventHandler(this.menuQuit_Click);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.accessmdbToolStripMenuItem});
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.importToolStripMenuItem.Text = "Import";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.connectAccess_Click);
            // 
            // accessmdbToolStripMenuItem
            // 
            this.accessmdbToolStripMenuItem.Name = "accessmdbToolStripMenuItem";
            this.accessmdbToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.accessmdbToolStripMenuItem.Text = "Access (.mdb)";
            // 
            // layerToolStripMenuItem
            // 
            this.layerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem});
            this.layerToolStripMenuItem.Name = "layerToolStripMenuItem";
            this.layerToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
            this.layerToolStripMenuItem.Text = "Layer";
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.saveToolStripMenuItem.Text = "Save";
            // 
            // plotToolStripMenuItem
            // 
            this.plotToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.photosToolStripMenuItem,
            this.markersMenuItem});
            this.plotToolStripMenuItem.Name = "plotToolStripMenuItem";
            this.plotToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.plotToolStripMenuItem.Text = "Plot";
            // 
            // photosToolStripMenuItem
            // 
            this.photosToolStripMenuItem.Name = "photosToolStripMenuItem";
            this.photosToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.photosToolStripMenuItem.Text = "Photos";
            this.photosToolStripMenuItem.Click += new System.EventHandler(this.photosToolStripMenuItem_Click);
            // 
            // markersMenuItem
            // 
            this.markersMenuItem.Name = "markersMenuItem";
            this.markersMenuItem.Size = new System.Drawing.Size(116, 22);
            this.markersMenuItem.Text = "Markers";
            this.markersMenuItem.Click += new System.EventHandler(this.markersMenuItem_Click);
            // 
            // dataToolStripMenuItem
            // 
            this.dataToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.accesDataMenu,
            this.excelDataMenu});
            this.dataToolStripMenuItem.Name = "dataToolStripMenuItem";
            this.dataToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.dataToolStripMenuItem.Text = "Data";
            // 
            // accesDataMenu
            // 
            this.accesDataMenu.Name = "accesDataMenu";
            this.accesDataMenu.Size = new System.Drawing.Size(220, 22);
            this.accesDataMenu.Text = "MS Access (*.accdb | *mdb)";
            this.accesDataMenu.Click += new System.EventHandler(this.connectAccess_Click);
            // 
            // excelDataMenu
            // 
            this.excelDataMenu.Name = "excelDataMenu";
            this.excelDataMenu.Size = new System.Drawing.Size(220, 22);
            this.excelDataMenu.Text = "MS Excel (*xlsx | *xls)";
            // 
            // geotagToolStripMenuItem
            // 
            this.geotagToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuRunGeoTag});
            this.geotagToolStripMenuItem.Name = "geotagToolStripMenuItem";
            this.geotagToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
            this.geotagToolStripMenuItem.Text = "Tools";
            // 
            // menuRunGeoTag
            // 
            this.menuRunGeoTag.Name = "menuRunGeoTag";
            this.menuRunGeoTag.Size = new System.Drawing.Size(112, 22);
            this.menuRunGeoTag.Text = "Geotag";
            this.menuRunGeoTag.Click += new System.EventHandler(this.menuRunGeoTag_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // bgWorker1
            // 
            this.bgWorker1.WorkerReportsProgress = true;
            this.bgWorker1.WorkerSupportsCancellation = true;
            this.bgWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgWorker1_DoWork);
            this.bgWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgWorker1_ProgressChanged);
            this.bgWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgWorker1_RunWorkerCompleted);
            // 
            // btnZoom
            // 
            this.btnZoom.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnZoom.Image = ((System.Drawing.Image)(resources.GetObject("btnZoom.Image")));
            this.btnZoom.Location = new System.Drawing.Point(14, 66);
            this.btnZoom.Name = "btnZoom";
            this.btnZoom.Size = new System.Drawing.Size(40, 40);
            this.btnZoom.TabIndex = 7;
            this.btnZoom.UseVisualStyleBackColor = true;
            this.btnZoom.Click += new System.EventHandler(this.btnZoom_Click);
            // 
            // btnArrow
            // 
            this.btnArrow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnArrow.Image = ((System.Drawing.Image)(resources.GetObject("btnArrow.Image")));
            this.btnArrow.Location = new System.Drawing.Point(14, 22);
            this.btnArrow.Name = "btnArrow";
            this.btnArrow.Size = new System.Drawing.Size(40, 40);
            this.btnArrow.TabIndex = 8;
            this.btnArrow.UseVisualStyleBackColor = true;
            this.btnArrow.Click += new System.EventHandler(this.btnArrow_Click);
            // 
            // lbScale
            // 
            this.lbScale.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lbScale.AutoSize = true;
            this.lbScale.Location = new System.Drawing.Point(243, 514);
            this.lbScale.Name = "lbScale";
            this.lbScale.Size = new System.Drawing.Size(46, 13);
            this.lbScale.TabIndex = 7;
            this.lbScale.Text = "Scale 1:";
            this.lbScale.Click += new System.EventHandler(this.lbScale_Click);
            // 
            // EXIFGeoTagger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1210, 564);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "EXIFGeoTagger";
            this.Text = "EXIFGeoTagger";
            this.Load += new System.EventHandler(this.EXIFGeoTagger_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private GMap.NET.WindowsForms.GMapControl gMap;
        private System.Windows.Forms.TextBox txtConsole;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label lbPosition;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem geotagToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuOpen;
        private System.Windows.Forms.ToolStripMenuItem menuRunGeoTag;
        private System.Windows.Forms.ToolStripMenuItem menuNew;
        private System.Windows.Forms.ToolStripMenuItem projectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuSave;
        private System.Windows.Forms.ToolStripMenuItem menuQuit;
        private System.ComponentModel.BackgroundWorker bgWorker1;
        private System.Windows.Forms.ToolStripMenuItem plotToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem photosToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem accessmdbToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem markersMenuItem;
        private System.Windows.Forms.CheckedListBox ckBoxLayers;
        private System.Windows.Forms.ToolStripMenuItem layerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem accesDataMenu;
        private System.Windows.Forms.ToolStripMenuItem excelDataMenu;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ListView listLayers;
        private System.Windows.Forms.Button btnZoom;
        private System.Windows.Forms.Button btnArrow;
        private System.Windows.Forms.Label lbScale;
    }
}

