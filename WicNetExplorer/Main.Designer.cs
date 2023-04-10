namespace WicNetExplorer
{
    partial class Main
    {
        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            menuStripMain = new System.Windows.Forms.MenuStrip();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItemOpen = new System.Windows.Forms.ToolStripMenuItem();
            openRecentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            clearRecentListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            imageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            infoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            metadataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            directXInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            openFileLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            honorOrientationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            honorColorContextsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            preferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            showDecodableFileExtensionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            showEncodableFileExtensionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            showWicComponentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            showSystemInformationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorDebug = new System.Windows.Forms.ToolStripSeparator();
            gCCollectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            cascadeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tileHorizontallyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tileVerticallyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            aboutWicNetExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            menuStripMain.SuspendLayout();
            SuspendLayout();
            // 
            // menuStripMain
            // 
            menuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem, imageToolStripMenuItem, optionsToolStripMenuItem, toolsToolStripMenuItem, windowToolStripMenuItem, helpToolStripMenuItem });
            resources.ApplyResources(menuStripMain, "menuStripMain");
            menuStripMain.MdiWindowListItem = windowToolStripMenuItem;
            menuStripMain.Name = "menuStripMain";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripMenuItemOpen, openRecentToolStripMenuItem, toolStripSeparator1, saveToolStripMenuItem, saveAsToolStripMenuItem, toolStripSeparator3, closeToolStripMenuItem, toolStripSeparator2, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(fileToolStripMenuItem, "fileToolStripMenuItem");
            fileToolStripMenuItem.DropDownOpening += FileToolStripMenuItem_DropDownOpening;
            // 
            // toolStripMenuItemOpen
            // 
            toolStripMenuItemOpen.Name = "toolStripMenuItemOpen";
            resources.ApplyResources(toolStripMenuItemOpen, "toolStripMenuItemOpen");
            toolStripMenuItemOpen.Click += ToolStripMenuItemOpen_Click;
            // 
            // openRecentToolStripMenuItem
            // 
            openRecentToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripSeparator4, clearRecentListToolStripMenuItem });
            openRecentToolStripMenuItem.Name = "openRecentToolStripMenuItem";
            resources.ApplyResources(openRecentToolStripMenuItem, "openRecentToolStripMenuItem");
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(toolStripSeparator4, "toolStripSeparator4");
            // 
            // clearRecentListToolStripMenuItem
            // 
            clearRecentListToolStripMenuItem.Name = "clearRecentListToolStripMenuItem";
            resources.ApplyResources(clearRecentListToolStripMenuItem, "clearRecentListToolStripMenuItem");
            clearRecentListToolStripMenuItem.Click += ClearRecentListToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(toolStripSeparator1, "toolStripSeparator1");
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            resources.ApplyResources(saveToolStripMenuItem, "saveToolStripMenuItem");
            saveToolStripMenuItem.Click += SaveToolStripMenuItem_Click;
            // 
            // saveAsToolStripMenuItem
            // 
            saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            resources.ApplyResources(saveAsToolStripMenuItem, "saveAsToolStripMenuItem");
            saveAsToolStripMenuItem.Click += SaveAsToolStripMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(toolStripSeparator3, "toolStripSeparator3");
            // 
            // closeToolStripMenuItem
            // 
            closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            resources.ApplyResources(closeToolStripMenuItem, "closeToolStripMenuItem");
            closeToolStripMenuItem.Click += CloseToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(toolStripSeparator2, "toolStripSeparator2");
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(exitToolStripMenuItem, "exitToolStripMenuItem");
            exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
            // 
            // imageToolStripMenuItem
            // 
            imageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { infoToolStripMenuItem, metadataToolStripMenuItem, directXInfoToolStripMenuItem, toolStripSeparator7, openFileLocationToolStripMenuItem });
            imageToolStripMenuItem.Name = "imageToolStripMenuItem";
            resources.ApplyResources(imageToolStripMenuItem, "imageToolStripMenuItem");
            imageToolStripMenuItem.DropDownOpening += ImageToolStripMenuItem_DropDownOpening;
            // 
            // infoToolStripMenuItem
            // 
            infoToolStripMenuItem.Name = "infoToolStripMenuItem";
            resources.ApplyResources(infoToolStripMenuItem, "infoToolStripMenuItem");
            infoToolStripMenuItem.Click += InfoToolStripMenuItem_Click;
            // 
            // metadataToolStripMenuItem
            // 
            metadataToolStripMenuItem.Name = "metadataToolStripMenuItem";
            resources.ApplyResources(metadataToolStripMenuItem, "metadataToolStripMenuItem");
            metadataToolStripMenuItem.Click += MetadataToolStripMenuItem_Click;
            // 
            // directXInfoToolStripMenuItem
            // 
            directXInfoToolStripMenuItem.Name = "directXInfoToolStripMenuItem";
            resources.ApplyResources(directXInfoToolStripMenuItem, "directXInfoToolStripMenuItem");
            directXInfoToolStripMenuItem.Click += DirectXInfoToolStripMenuItem_Click;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            resources.ApplyResources(toolStripSeparator7, "toolStripSeparator7");
            // 
            // openFileLocationToolStripMenuItem
            // 
            openFileLocationToolStripMenuItem.Name = "openFileLocationToolStripMenuItem";
            resources.ApplyResources(openFileLocationToolStripMenuItem, "openFileLocationToolStripMenuItem");
            openFileLocationToolStripMenuItem.Click += OpenFileLocationToolStripMenuItem_Click;
            // 
            // optionsToolStripMenuItem
            // 
            optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { honorOrientationToolStripMenuItem, honorColorContextsToolStripMenuItem, toolStripSeparator5, preferencesToolStripMenuItem });
            optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            resources.ApplyResources(optionsToolStripMenuItem, "optionsToolStripMenuItem");
            optionsToolStripMenuItem.DropDownOpening += OptionsToolStripMenuItem_DropDownOpening;
            // 
            // honorOrientationToolStripMenuItem
            // 
            honorOrientationToolStripMenuItem.Checked = true;
            honorOrientationToolStripMenuItem.CheckOnClick = true;
            honorOrientationToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            honorOrientationToolStripMenuItem.Name = "honorOrientationToolStripMenuItem";
            resources.ApplyResources(honorOrientationToolStripMenuItem, "honorOrientationToolStripMenuItem");
            honorOrientationToolStripMenuItem.Click += HonorOrientationToolStripMenuItem_Click;
            // 
            // honorColorContextsToolStripMenuItem
            // 
            honorColorContextsToolStripMenuItem.Checked = true;
            honorColorContextsToolStripMenuItem.CheckOnClick = true;
            honorColorContextsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            honorColorContextsToolStripMenuItem.Name = "honorColorContextsToolStripMenuItem";
            resources.ApplyResources(honorColorContextsToolStripMenuItem, "honorColorContextsToolStripMenuItem");
            honorColorContextsToolStripMenuItem.Click += HonorColorContextsToolStripMenuItem_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            resources.ApplyResources(toolStripSeparator5, "toolStripSeparator5");
            // 
            // preferencesToolStripMenuItem
            // 
            preferencesToolStripMenuItem.Name = "preferencesToolStripMenuItem";
            resources.ApplyResources(preferencesToolStripMenuItem, "preferencesToolStripMenuItem");
            preferencesToolStripMenuItem.Click += PreferencesToolStripMenuItem_Click;
            // 
            // toolsToolStripMenuItem
            // 
            toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { showDecodableFileExtensionsToolStripMenuItem, showEncodableFileExtensionsToolStripMenuItem, showWicComponentsToolStripMenuItem, toolStripSeparator6, showSystemInformationToolStripMenuItem, toolStripSeparatorDebug, gCCollectToolStripMenuItem });
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            resources.ApplyResources(toolsToolStripMenuItem, "toolsToolStripMenuItem");
            // 
            // showDecodableFileExtensionsToolStripMenuItem
            // 
            showDecodableFileExtensionsToolStripMenuItem.Name = "showDecodableFileExtensionsToolStripMenuItem";
            resources.ApplyResources(showDecodableFileExtensionsToolStripMenuItem, "showDecodableFileExtensionsToolStripMenuItem");
            showDecodableFileExtensionsToolStripMenuItem.Click += ShowDecodableFileExtensionsToolStripMenuItem_Click;
            // 
            // showEncodableFileExtensionsToolStripMenuItem
            // 
            showEncodableFileExtensionsToolStripMenuItem.Name = "showEncodableFileExtensionsToolStripMenuItem";
            resources.ApplyResources(showEncodableFileExtensionsToolStripMenuItem, "showEncodableFileExtensionsToolStripMenuItem");
            showEncodableFileExtensionsToolStripMenuItem.Click += ShowEncodableFileExtensionsToolStripMenuItem_Click;
            // 
            // showWicComponentsToolStripMenuItem
            // 
            showWicComponentsToolStripMenuItem.Name = "showWicComponentsToolStripMenuItem";
            resources.ApplyResources(showWicComponentsToolStripMenuItem, "showWicComponentsToolStripMenuItem");
            showWicComponentsToolStripMenuItem.Click += ShowWicComponentsToolStripMenuItem_Click;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            resources.ApplyResources(toolStripSeparator6, "toolStripSeparator6");
            // 
            // showSystemInformationToolStripMenuItem
            // 
            showSystemInformationToolStripMenuItem.Name = "showSystemInformationToolStripMenuItem";
            resources.ApplyResources(showSystemInformationToolStripMenuItem, "showSystemInformationToolStripMenuItem");
            showSystemInformationToolStripMenuItem.Click += ShowSystemInformationToolStripMenuItem_Click;
            // 
            // toolStripSeparatorDebug
            // 
            toolStripSeparatorDebug.Name = "toolStripSeparatorDebug";
            resources.ApplyResources(toolStripSeparatorDebug, "toolStripSeparatorDebug");
            // 
            // gCCollectToolStripMenuItem
            // 
            gCCollectToolStripMenuItem.Name = "gCCollectToolStripMenuItem";
            resources.ApplyResources(gCCollectToolStripMenuItem, "gCCollectToolStripMenuItem");
            gCCollectToolStripMenuItem.Click += GCCollectToolStripMenuItem_Click;
            // 
            // windowToolStripMenuItem
            // 
            windowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { cascadeToolStripMenuItem, tileHorizontallyToolStripMenuItem, tileVerticallyToolStripMenuItem });
            windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            resources.ApplyResources(windowToolStripMenuItem, "windowToolStripMenuItem");
            // 
            // cascadeToolStripMenuItem
            // 
            cascadeToolStripMenuItem.Name = "cascadeToolStripMenuItem";
            resources.ApplyResources(cascadeToolStripMenuItem, "cascadeToolStripMenuItem");
            cascadeToolStripMenuItem.Click += CascadeToolStripMenuItem_Click;
            // 
            // tileHorizontallyToolStripMenuItem
            // 
            tileHorizontallyToolStripMenuItem.Name = "tileHorizontallyToolStripMenuItem";
            resources.ApplyResources(tileHorizontallyToolStripMenuItem, "tileHorizontallyToolStripMenuItem");
            tileHorizontallyToolStripMenuItem.Click += TileHorizontallyToolStripMenuItem_Click;
            // 
            // tileVerticallyToolStripMenuItem
            // 
            tileVerticallyToolStripMenuItem.Name = "tileVerticallyToolStripMenuItem";
            resources.ApplyResources(tileVerticallyToolStripMenuItem, "tileVerticallyToolStripMenuItem");
            tileVerticallyToolStripMenuItem.Click += TileVerticallyToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { aboutWicNetExplorerToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            resources.ApplyResources(helpToolStripMenuItem, "helpToolStripMenuItem");
            // 
            // aboutWicNetExplorerToolStripMenuItem
            // 
            aboutWicNetExplorerToolStripMenuItem.Name = "aboutWicNetExplorerToolStripMenuItem";
            resources.ApplyResources(aboutWicNetExplorerToolStripMenuItem, "aboutWicNetExplorerToolStripMenuItem");
            aboutWicNetExplorerToolStripMenuItem.Click += AboutWicNetExplorerToolStripMenuItem_Click;
            // 
            // Main
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(menuStripMain);
            IsMdiContainer = true;
            KeyPreview = true;
            MainMenuStrip = menuStripMain;
            Name = "Main";
            menuStripMain.ResumeLayout(false);
            menuStripMain.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStripMain;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOpen;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gCCollectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openRecentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearRecentListToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem windowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cascadeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tileHorizontallyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tileVerticallyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem imageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem infoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem metadataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem honorOrientationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem honorColorContextsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem preferencesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showWicComponentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorDebug;
        private System.Windows.Forms.ToolStripMenuItem showDecodableFileExtensionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showEncodableFileExtensionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutWicNetExplorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem showSystemInformationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem directXInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem openFileLocationToolStripMenuItem;
    }
}