namespace WicNetExplorer.Utilities;

partial class ArrayForm
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ArrayForm));
        this.listViewArray = new System.Windows.Forms.ListView();
        this.columnIndex = new System.Windows.Forms.ColumnHeader();
        this.columnValue = new System.Windows.Forms.ColumnHeader();
        this.SuspendLayout();
        // 
        // listViewArray
        // 
        this.listViewArray.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
        this.columnIndex,
        this.columnValue});
        resources.ApplyResources(this.listViewArray, "listViewArray");
        this.listViewArray.FullRowSelect = true;
        this.listViewArray.Name = "listViewArray";
        this.listViewArray.UseCompatibleStateImageBehavior = false;
        this.listViewArray.View = System.Windows.Forms.View.Details;
        // 
        // columnIndex
        // 
        resources.ApplyResources(this.columnIndex, "columnIndex");
        // 
        // columnValue
        // 
        resources.ApplyResources(this.columnValue, "columnValue");
        // 
        // ArrayForm
        // 
        resources.ApplyResources(this, "$this");
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.listViewArray);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
        this.KeyPreview = true;
        this.Name = "ArrayForm";
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ListView listViewArray;
    private System.Windows.Forms.ColumnHeader columnIndex;
    private System.Windows.Forms.ColumnHeader columnValue;
}