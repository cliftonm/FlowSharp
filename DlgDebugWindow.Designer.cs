namespace FlowSharp
{
    partial class DlgDebugWindow
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
            this.btnUpdate = new System.Windows.Forms.Button();
            this.tvShapes = new System.Windows.Forms.TreeView();
            this.label2 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tbUndoEvents = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ckShapeEvents = new System.Windows.Forms.CheckBox();
            this.ckRoutingEvents = new System.Windows.Forms.CheckBox();
            this.ckTraceEnabled = new System.Windows.Forms.CheckBox();
            this.btnClearTrace = new System.Windows.Forms.Button();
            this.tbTrace = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnUpdate
            // 
            this.btnUpdate.Location = new System.Drawing.Point(515, 10);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(75, 23);
            this.btnUpdate.TabIndex = 2;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // tvShapes
            // 
            this.tvShapes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tvShapes.Location = new System.Drawing.Point(7, 34);
            this.tvShapes.Name = "tvShapes";
            this.tvShapes.Size = new System.Drawing.Size(584, 208);
            this.tvShapes.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "Shapes:";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tbUndoEvents);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.tvShapes);
            this.splitContainer1.Panel1.Controls.Add(this.btnUpdate);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.ckShapeEvents);
            this.splitContainer1.Panel2.Controls.Add(this.ckRoutingEvents);
            this.splitContainer1.Panel2.Controls.Add(this.ckTraceEnabled);
            this.splitContainer1.Panel2.Controls.Add(this.btnClearTrace);
            this.splitContainer1.Panel2.Controls.Add(this.tbTrace);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Size = new System.Drawing.Size(1002, 724);
            this.splitContainer1.SplitterDistance = 241;
            this.splitContainer1.TabIndex = 11;
            // 
            // tbUndoEvents
            // 
            this.tbUndoEvents.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbUndoEvents.Location = new System.Drawing.Point(598, 34);
            this.tbUndoEvents.Multiline = true;
            this.tbUndoEvents.Name = "tbUndoEvents";
            this.tbUndoEvents.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbUndoEvents.Size = new System.Drawing.Size(392, 208);
            this.tbUndoEvents.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(594, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 15);
            this.label3.TabIndex = 7;
            this.label3.Text = "Undo Stack:";
            // 
            // ckShapeEvents
            // 
            this.ckShapeEvents.AutoSize = true;
            this.ckShapeEvents.Enabled = false;
            this.ckShapeEvents.Location = new System.Drawing.Point(301, 11);
            this.ckShapeEvents.Name = "ckShapeEvents";
            this.ckShapeEvents.Size = new System.Drawing.Size(101, 19);
            this.ckShapeEvents.TabIndex = 16;
            this.ckShapeEvents.Text = "Shape Events";
            this.ckShapeEvents.UseVisualStyleBackColor = true;
            // 
            // ckRoutingEvents
            // 
            this.ckRoutingEvents.AutoSize = true;
            this.ckRoutingEvents.Enabled = false;
            this.ckRoutingEvents.Location = new System.Drawing.Point(172, 10);
            this.ckRoutingEvents.Name = "ckRoutingEvents";
            this.ckRoutingEvents.Size = new System.Drawing.Size(103, 19);
            this.ckRoutingEvents.TabIndex = 15;
            this.ckRoutingEvents.Text = "Mouse Events";
            this.ckRoutingEvents.UseVisualStyleBackColor = true;
            // 
            // ckTraceEnabled
            // 
            this.ckTraceEnabled.AutoSize = true;
            this.ckTraceEnabled.Location = new System.Drawing.Point(75, 10);
            this.ckTraceEnabled.Name = "ckTraceEnabled";
            this.ckTraceEnabled.Size = new System.Drawing.Size(72, 19);
            this.ckTraceEnabled.TabIndex = 14;
            this.ckTraceEnabled.Text = "Enabled";
            this.ckTraceEnabled.UseVisualStyleBackColor = true;
            // 
            // btnClearTrace
            // 
            this.btnClearTrace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearTrace.Location = new System.Drawing.Point(919, 8);
            this.btnClearTrace.Name = "btnClearTrace";
            this.btnClearTrace.Size = new System.Drawing.Size(75, 23);
            this.btnClearTrace.TabIndex = 13;
            this.btnClearTrace.Text = "Clear";
            this.btnClearTrace.UseVisualStyleBackColor = true;
            this.btnClearTrace.Click += new System.EventHandler(this.btnClearTrace_Click);
            // 
            // tbTrace
            // 
            this.tbTrace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbTrace.Location = new System.Drawing.Point(7, 36);
            this.tbTrace.Multiline = true;
            this.tbTrace.Name = "tbTrace";
            this.tbTrace.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbTrace.Size = new System.Drawing.Size(991, 447);
            this.tbTrace.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 15);
            this.label1.TabIndex = 11;
            this.label1.Text = "Trace:";
            // 
            // DlgDebugWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1002, 724);
            this.Controls.Add(this.splitContainer1);
            this.Name = "DlgDebugWindow";
            this.Text = "FlowSharp Debug Window";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.TreeView tvShapes;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.CheckBox ckShapeEvents;
        private System.Windows.Forms.CheckBox ckRoutingEvents;
        private System.Windows.Forms.CheckBox ckTraceEnabled;
        private System.Windows.Forms.Button btnClearTrace;
        public System.Windows.Forms.TextBox tbTrace;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbUndoEvents;
        private System.Windows.Forms.Label label3;
    }
}