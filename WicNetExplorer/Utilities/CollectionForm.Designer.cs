namespace WicNetExplorer.Utilities
{
    partial class CollectionForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CollectionForm));
            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.CopyToClipboard = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.listViewMain = new System.Windows.Forms.ListView();
            this.columnHeaderType = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderName = new System.Windows.Forms.ColumnHeader();
            this.propertyGridObject = new System.Windows.Forms.PropertyGrid();
            this.contextMenuStripGrid = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.expandChildrenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseChildrenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.expandAllItemsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseAllItemsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanelMain.SuspendLayout();
            this.panelButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            this.contextMenuStripGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanelMain
            // 
            resources.ApplyResources(this.tableLayoutPanelMain, "tableLayoutPanelMain");
            this.tableLayoutPanelMain.Controls.Add(this.panelButtons, 0, 1);
            this.tableLayoutPanelMain.Controls.Add(this.splitContainerMain, 0, 0);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            // 
            // panelButtons
            // 
            resources.ApplyResources(this.panelButtons, "panelButtons");
            this.panelButtons.Controls.Add(this.CopyToClipboard);
            this.panelButtons.Controls.Add(this.buttonOk);
            this.panelButtons.Controls.Add(this.buttonCancel);
            this.panelButtons.Name = "panelButtons";
            // 
            // CopyToClipboard
            // 
            resources.ApplyResources(this.CopyToClipboard, "CopyToClipboard");
            this.CopyToClipboard.Name = "CopyToClipboard";
            this.CopyToClipboard.UseVisualStyleBackColor = true;
            this.CopyToClipboard.Click += new System.EventHandler(this.ButtonCopyToClipboard_Click);
            // 
            // buttonOk
            // 
            resources.ApplyResources(this.buttonOk, "buttonOk");
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.CausesValidation = false;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // splitContainerMain
            // 
            resources.ApplyResources(this.splitContainerMain, "splitContainerMain");
            this.splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.listViewMain);
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.propertyGridObject);
            // 
            // listViewMain
            // 
            this.listViewMain.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderType,
            this.columnHeaderName});
            resources.ApplyResources(this.listViewMain, "listViewMain");
            this.listViewMain.FullRowSelect = true;
            this.listViewMain.MultiSelect = false;
            this.listViewMain.Name = "listViewMain";
            this.listViewMain.UseCompatibleStateImageBehavior = false;
            this.listViewMain.View = System.Windows.Forms.View.Details;
            this.listViewMain.SelectedIndexChanged += new System.EventHandler(this.ListViewMain_SelectedIndexChanged);
            // 
            // columnHeaderType
            // 
            resources.ApplyResources(this.columnHeaderType, "columnHeaderType");
            // 
            // columnHeaderName
            // 
            resources.ApplyResources(this.columnHeaderName, "columnHeaderName");
            // 
            // propertyGridObject
            // 
            this.propertyGridObject.ContextMenuStrip = this.contextMenuStripGrid;
            resources.ApplyResources(this.propertyGridObject, "propertyGridObject");
            this.propertyGridObject.Name = "propertyGridObject";
            this.propertyGridObject.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            this.propertyGridObject.ToolbarVisible = false;
            // 
            // contextMenuStripGrid
            // 
            this.contextMenuStripGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.expandChildrenToolStripMenuItem,
            this.collapseChildrenToolStripMenuItem,
            this.toolStripSeparator1,
            this.expandAllItemsToolStripMenuItem,
            this.collapseAllItemsToolStripMenuItem});
            this.contextMenuStripGrid.Name = "contextMenuStripGrid";
            resources.ApplyResources(this.contextMenuStripGrid, "contextMenuStripGrid");
            this.contextMenuStripGrid.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStripGrid_Opening);
            // 
            // expandChildrenToolStripMenuItem
            // 
            this.expandChildrenToolStripMenuItem.Name = "expandChildrenToolStripMenuItem";
            resources.ApplyResources(this.expandChildrenToolStripMenuItem, "expandChildrenToolStripMenuItem");
            this.expandChildrenToolStripMenuItem.Click += new System.EventHandler(this.ExpandChildrenToolStripMenuItem_Click);
            // 
            // collapseChildrenToolStripMenuItem
            // 
            this.collapseChildrenToolStripMenuItem.Name = "collapseChildrenToolStripMenuItem";
            resources.ApplyResources(this.collapseChildrenToolStripMenuItem, "collapseChildrenToolStripMenuItem");
            this.collapseChildrenToolStripMenuItem.Click += new System.EventHandler(this.CollapseChildrenToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // expandAllItemsToolStripMenuItem
            // 
            this.expandAllItemsToolStripMenuItem.Name = "expandAllItemsToolStripMenuItem";
            resources.ApplyResources(this.expandAllItemsToolStripMenuItem, "expandAllItemsToolStripMenuItem");
            this.expandAllItemsToolStripMenuItem.Click += new System.EventHandler(this.ExpandAllItemsToolStripMenuItem_Click);
            // 
            // collapseAllItemsToolStripMenuItem
            // 
            this.collapseAllItemsToolStripMenuItem.Name = "collapseAllItemsToolStripMenuItem";
            resources.ApplyResources(this.collapseAllItemsToolStripMenuItem, "collapseAllItemsToolStripMenuItem");
            this.collapseAllItemsToolStripMenuItem.Click += new System.EventHandler(this.CollapseAllItemsToolStripMenuItem_Click);
            // 
            // CollectionForm
            // 
            this.AcceptButton = this.buttonOk;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.Controls.Add(this.tableLayoutPanelMain);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CollectionForm";
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.panelButtons.ResumeLayout(false);
            this.panelButtons.PerformLayout();
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.contextMenuStripGrid.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripGrid;
        private System.Windows.Forms.ToolStripMenuItem expandAllItemsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collapseAllItemsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem expandChildrenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collapseChildrenToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        public System.Windows.Forms.Button CopyToClipboard;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.PropertyGrid propertyGridObject;
        private System.Windows.Forms.ListView listViewMain;
        private System.Windows.Forms.ColumnHeader columnHeaderType;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
    }
}