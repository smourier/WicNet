namespace WicNetExplorer;

partial class AboutForm
{
    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
        tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
        buttonOk = new System.Windows.Forms.Button();
        pictureBoxIcon = new System.Windows.Forms.PictureBox();
        labelText = new System.Windows.Forms.Label();
        tableLayoutPanelMain.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)pictureBoxIcon).BeginInit();
        SuspendLayout();
        // 
        // tableLayoutPanelMain
        // 
        resources.ApplyResources(tableLayoutPanelMain, "tableLayoutPanelMain");
        tableLayoutPanelMain.Controls.Add(buttonOk, 1, 1);
        tableLayoutPanelMain.Controls.Add(pictureBoxIcon, 0, 0);
        tableLayoutPanelMain.Controls.Add(labelText, 1, 0);
        tableLayoutPanelMain.Name = "tableLayoutPanelMain";
        // 
        // buttonOk
        // 
        resources.ApplyResources(buttonOk, "buttonOk");
        buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
        buttonOk.Name = "buttonOk";
        buttonOk.UseVisualStyleBackColor = true;
        // 
        // pictureBoxIcon
        // 
        resources.ApplyResources(pictureBoxIcon, "pictureBoxIcon");
        pictureBoxIcon.Name = "pictureBoxIcon";
        pictureBoxIcon.TabStop = false;
        // 
        // labelText
        // 
        resources.ApplyResources(labelText, "labelText");
        labelText.Name = "labelText";
        // 
        // AboutForm
        // 
        AcceptButton = buttonOk;
        resources.ApplyResources(this, "$this");
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        CancelButton = buttonOk;
        Controls.Add(tableLayoutPanelMain);
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        KeyPreview = true;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "AboutForm";
        tableLayoutPanelMain.ResumeLayout(false);
        tableLayoutPanelMain.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)pictureBoxIcon).EndInit();
        ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
    private System.Windows.Forms.Button buttonOk;
    private System.Windows.Forms.PictureBox pictureBoxIcon;
    private System.Windows.Forms.Label labelText;
}