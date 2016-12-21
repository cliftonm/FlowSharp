namespace FlowSharpMenuService
{
    partial class NavigateDlg
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
            this.lbShapes = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // lbShapes
            // 
            this.lbShapes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbShapes.FormattingEnabled = true;
            this.lbShapes.Location = new System.Drawing.Point(0, 0);
            this.lbShapes.Name = "lbShapes";
            this.lbShapes.Size = new System.Drawing.Size(471, 313);
            this.lbShapes.TabIndex = 0;
            this.lbShapes.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lbShapes_MouseClick);
            this.lbShapes.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.lbShapes_KeyPress);
            // 
            // NavigateDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(471, 313);
            this.Controls.Add(this.lbShapes);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "NavigateDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Navigate...";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lbShapes;
    }
}