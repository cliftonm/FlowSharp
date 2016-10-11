namespace FlowSharp
{
    partial class FlowSharpUI
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
            this.pnlToolbox = new System.Windows.Forms.Panel();
            this.pnlProperties = new System.Windows.Forms.Panel();
            this.pgElement = new System.Windows.Forms.PropertyGrid();
            this.pnlCanvas = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuNew = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuImport = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuSave = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.orderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuTopmost = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuBottommost = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMoveUp = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMoveDown = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlProperties.SuspendLayout();
            this.panel1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlToolbox
            // 
            this.pnlToolbox.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlToolbox.Location = new System.Drawing.Point(0, 25);
            this.pnlToolbox.Name = "pnlToolbox";
            this.pnlToolbox.Size = new System.Drawing.Size(169, 570);
            this.pnlToolbox.TabIndex = 0;
            // 
            // pnlProperties
            // 
            this.pnlProperties.Controls.Add(this.pgElement);
            this.pnlProperties.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlProperties.Location = new System.Drawing.Point(743, 25);
            this.pnlProperties.Name = "pnlProperties";
            this.pnlProperties.Size = new System.Drawing.Size(200, 570);
            this.pnlProperties.TabIndex = 1;
            // 
            // pgElement
            // 
            this.pgElement.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgElement.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.150944F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pgElement.Location = new System.Drawing.Point(0, 0);
            this.pgElement.Name = "pgElement";
            this.pgElement.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.pgElement.Size = new System.Drawing.Size(200, 570);
            this.pgElement.TabIndex = 0;
            // 
            // pnlCanvas
            // 
            this.pnlCanvas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlCanvas.Location = new System.Drawing.Point(175, 27);
            this.pnlCanvas.Name = "pnlCanvas";
            this.pnlCanvas.Size = new System.Drawing.Size(562, 565);
            this.pnlCanvas.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.pnlToolbox);
            this.panel1.Controls.Add(this.pnlCanvas);
            this.panel1.Controls.Add(this.pnlProperties);
            this.panel1.Controls.Add(this.menuStrip1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(943, 595);
            this.panel1.TabIndex = 3;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(18, 18);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.orderToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(943, 25);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuNew,
            this.toolStripMenuItem3,
            this.mnuOpen,
            this.mnuImport,
            this.toolStripMenuItem2,
            this.mnuSave,
            this.mnuSaveAs,
            this.toolStripMenuItem1,
            this.mnuExit});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 21);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // mnuNew
            // 
            this.mnuNew.Name = "mnuNew";
            this.mnuNew.Size = new System.Drawing.Size(165, 24);
            this.mnuNew.Text = "&New";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(162, 6);
            // 
            // mnuOpen
            // 
            this.mnuOpen.Name = "mnuOpen";
            this.mnuOpen.Size = new System.Drawing.Size(165, 24);
            this.mnuOpen.Text = "&Open...";
            // 
            // mnuImport
            // 
            this.mnuImport.Name = "mnuImport";
            this.mnuImport.Size = new System.Drawing.Size(165, 24);
            this.mnuImport.Text = "&Import...";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(162, 6);
            // 
            // mnuSave
            // 
            this.mnuSave.Name = "mnuSave";
            this.mnuSave.Size = new System.Drawing.Size(165, 24);
            this.mnuSave.Text = "&Save";
            // 
            // mnuSaveAs
            // 
            this.mnuSaveAs.Name = "mnuSaveAs";
            this.mnuSaveAs.Size = new System.Drawing.Size(165, 24);
            this.mnuSaveAs.Text = "Save &as";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(162, 6);
            // 
            // mnuExit
            // 
            this.mnuExit.Name = "mnuExit";
            this.mnuExit.Size = new System.Drawing.Size(165, 24);
            this.mnuExit.Text = "E&xit";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuCopy,
            this.mnuPaste,
            this.mnuDelete});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 21);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // mnuCopy
            // 
            this.mnuCopy.Name = "mnuCopy";
            this.mnuCopy.Size = new System.Drawing.Size(110, 24);
            this.mnuCopy.Text = "&Copy";
            // 
            // mnuPaste
            // 
            this.mnuPaste.Name = "mnuPaste";
            this.mnuPaste.Size = new System.Drawing.Size(110, 24);
            this.mnuPaste.Text = "&Paste";
            // 
            // mnuDelete
            // 
            this.mnuDelete.Name = "mnuDelete";
            this.mnuDelete.Size = new System.Drawing.Size(110, 24);
            this.mnuDelete.Text = "&Delete";
            // 
            // orderToolStripMenuItem
            // 
            this.orderToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuTopmost,
            this.mnuBottommost,
            this.mnuMoveUp,
            this.mnuMoveDown});
            this.orderToolStripMenuItem.Name = "orderToolStripMenuItem";
            this.orderToolStripMenuItem.Size = new System.Drawing.Size(49, 21);
            this.orderToolStripMenuItem.Text = "&Order";
            // 
            // mnuTopmost
            // 
            this.mnuTopmost.Name = "mnuTopmost";
            this.mnuTopmost.Size = new System.Drawing.Size(141, 24);
            this.mnuTopmost.Text = "To &Top";
            // 
            // mnuBottommost
            // 
            this.mnuBottommost.Name = "mnuBottommost";
            this.mnuBottommost.Size = new System.Drawing.Size(141, 24);
            this.mnuBottommost.Text = "To &Bottom";
            // 
            // mnuMoveUp
            // 
            this.mnuMoveUp.Name = "mnuMoveUp";
            this.mnuMoveUp.Size = new System.Drawing.Size(141, 24);
            this.mnuMoveUp.Text = "Move &Up";
            // 
            // mnuMoveDown
            // 
            this.mnuMoveDown.Name = "mnuMoveDown";
            this.mnuMoveDown.Size = new System.Drawing.Size(141, 24);
            this.mnuMoveDown.Text = "Move &Down";
            // 
            // FlowSharpUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(943, 595);
            this.Controls.Add(this.panel1);
            this.Name = "FlowSharpUI";
            this.Text = "FlowSharp";
            this.pnlProperties.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

		#endregion

		private System.Windows.Forms.Panel pnlToolbox;
		private System.Windows.Forms.Panel pnlProperties;
		private System.Windows.Forms.PropertyGrid pgElement;
		private System.Windows.Forms.Panel pnlCanvas;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem mnuNew;
		private System.Windows.Forms.ToolStripMenuItem mnuOpen;
		private System.Windows.Forms.ToolStripMenuItem mnuSave;
		private System.Windows.Forms.ToolStripMenuItem mnuSaveAs;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem mnuExit;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem mnuCopy;
		private System.Windows.Forms.ToolStripMenuItem mnuPaste;
		private System.Windows.Forms.ToolStripMenuItem mnuDelete;
		private System.Windows.Forms.ToolStripMenuItem orderToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem mnuTopmost;
		private System.Windows.Forms.ToolStripMenuItem mnuBottommost;
		private System.Windows.Forms.ToolStripMenuItem mnuMoveUp;
		private System.Windows.Forms.ToolStripMenuItem mnuMoveDown;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem mnuImport;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
    }
}

