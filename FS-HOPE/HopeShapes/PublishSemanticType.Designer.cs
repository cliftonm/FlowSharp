namespace HopeShapes
{
    partial class PublishSemanticType
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
            this.pgSemanticType = new System.Windows.Forms.PropertyGrid();
            this.btnPublish = new System.Windows.Forms.Button();
            this.ckUnload = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // pgSemanticType
            // 
            this.pgSemanticType.LineColor = System.Drawing.SystemColors.ControlDark;
            this.pgSemanticType.Location = new System.Drawing.Point(13, 13);
            this.pgSemanticType.Name = "pgSemanticType";
            this.pgSemanticType.Size = new System.Drawing.Size(475, 280);
            this.pgSemanticType.TabIndex = 0;
            this.pgSemanticType.ToolbarVisible = false;
            // 
            // btnPublish
            // 
            this.btnPublish.Location = new System.Drawing.Point(522, 13);
            this.btnPublish.Name = "btnPublish";
            this.btnPublish.Size = new System.Drawing.Size(75, 23);
            this.btnPublish.TabIndex = 1;
            this.btnPublish.Text = "Publish";
            this.btnPublish.UseVisualStyleBackColor = true;
            this.btnPublish.Click += new System.EventHandler(this.btnPublish_Click);
            // 
            // ckUnload
            // 
            this.ckUnload.AutoSize = true;
            this.ckUnload.Checked = true;
            this.ckUnload.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckUnload.Location = new System.Drawing.Point(504, 49);
            this.ckUnload.Name = "ckUnload";
            this.ckUnload.Size = new System.Drawing.Size(103, 17);
            this.ckUnload.TabIndex = 2;
            this.ckUnload.Text = "Unload on close";
            this.ckUnload.UseVisualStyleBackColor = true;
            // 
            // PublishSemanticType
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(609, 305);
            this.Controls.Add(this.ckUnload);
            this.Controls.Add(this.btnPublish);
            this.Controls.Add(this.pgSemanticType);
            this.Name = "PublishSemanticType";
            this.Text = "Publish Semantic Type";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PropertyGrid pgSemanticType;
        private System.Windows.Forms.Button btnPublish;
        public System.Windows.Forms.CheckBox ckUnload;
    }
}