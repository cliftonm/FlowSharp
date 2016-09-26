namespace FlowSharp
{
    partial class Form1
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
			this.pnlProperties.SuspendLayout();
			this.SuspendLayout();
			// 
			// pnlToolbox
			// 
			this.pnlToolbox.Dock = System.Windows.Forms.DockStyle.Left;
			this.pnlToolbox.Location = new System.Drawing.Point(0, 0);
			this.pnlToolbox.Name = "pnlToolbox";
			this.pnlToolbox.Size = new System.Drawing.Size(200, 584);
			this.pnlToolbox.TabIndex = 0;
			// 
			// pnlProperties
			// 
			this.pnlProperties.Controls.Add(this.pgElement);
			this.pnlProperties.Dock = System.Windows.Forms.DockStyle.Right;
			this.pnlProperties.Location = new System.Drawing.Point(710, 0);
			this.pnlProperties.Name = "pnlProperties";
			this.pnlProperties.Size = new System.Drawing.Size(200, 584);
			this.pnlProperties.TabIndex = 1;
			// 
			// pgElement
			// 
			this.pgElement.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pgElement.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.150944F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pgElement.Location = new System.Drawing.Point(0, 0);
			this.pgElement.Name = "pgElement";
			this.pgElement.PropertySort = System.Windows.Forms.PropertySort.Categorized;
			this.pgElement.Size = new System.Drawing.Size(200, 584);
			this.pgElement.TabIndex = 0;
			// 
			// pnlCanvas
			// 
			this.pnlCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlCanvas.Location = new System.Drawing.Point(200, 0);
			this.pnlCanvas.Name = "pnlCanvas";
			this.pnlCanvas.Size = new System.Drawing.Size(510, 584);
			this.pnlCanvas.TabIndex = 2;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(910, 584);
			this.Controls.Add(this.pnlCanvas);
			this.Controls.Add(this.pnlProperties);
			this.Controls.Add(this.pnlToolbox);
			this.Name = "Form1";
			this.Text = "Form1";
			this.pnlProperties.ResumeLayout(false);
			this.ResumeLayout(false);

        }

		#endregion

		private System.Windows.Forms.Panel pnlToolbox;
		private System.Windows.Forms.Panel pnlProperties;
		private System.Windows.Forms.PropertyGrid pgElement;
		private System.Windows.Forms.Panel pnlCanvas;
	}
}

