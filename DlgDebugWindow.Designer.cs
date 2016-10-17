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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbTrace = new System.Windows.Forms.TextBox();
            this.btnClearTrace = new System.Windows.Forms.Button();
            this.ckTraceEnabled = new System.Windows.Forms.CheckBox();
            this.ckRoutingEvents = new System.Windows.Forms.CheckBox();
            this.ckShapeEvents = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnUpdate
            // 
            this.btnUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUpdate.Location = new System.Drawing.Point(561, 12);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(75, 23);
            this.btnUpdate.TabIndex = 2;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // tvShapes
            // 
            this.tvShapes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tvShapes.Location = new System.Drawing.Point(13, 42);
            this.tvShapes.Name = "tvShapes";
            this.tvShapes.Size = new System.Drawing.Size(624, 220);
            this.tvShapes.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 284);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "Trace:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "Shapes:";
            // 
            // tbTrace
            // 
            this.tbTrace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbTrace.Location = new System.Drawing.Point(16, 306);
            this.tbTrace.Multiline = true;
            this.tbTrace.Name = "tbTrace";
            this.tbTrace.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbTrace.Size = new System.Drawing.Size(621, 236);
            this.tbTrace.TabIndex = 6;
            // 
            // btnClearTrace
            // 
            this.btnClearTrace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearTrace.Location = new System.Drawing.Point(561, 277);
            this.btnClearTrace.Name = "btnClearTrace";
            this.btnClearTrace.Size = new System.Drawing.Size(75, 23);
            this.btnClearTrace.TabIndex = 7;
            this.btnClearTrace.Text = "Clear";
            this.btnClearTrace.UseVisualStyleBackColor = true;
            this.btnClearTrace.Click += new System.EventHandler(this.btnClearTrace_Click);
            // 
            // ckTraceEnabled
            // 
            this.ckTraceEnabled.AutoSize = true;
            this.ckTraceEnabled.Location = new System.Drawing.Point(71, 283);
            this.ckTraceEnabled.Name = "ckTraceEnabled";
            this.ckTraceEnabled.Size = new System.Drawing.Size(72, 19);
            this.ckTraceEnabled.TabIndex = 8;
            this.ckTraceEnabled.Text = "Enabled";
            this.ckTraceEnabled.UseVisualStyleBackColor = true;
            this.ckTraceEnabled.CheckedChanged += new System.EventHandler(this.ckTraceEnabled_CheckedChanged);
            // 
            // ckRoutingEvents
            // 
            this.ckRoutingEvents.AutoSize = true;
            this.ckRoutingEvents.Enabled = false;
            this.ckRoutingEvents.Location = new System.Drawing.Point(168, 283);
            this.ckRoutingEvents.Name = "ckRoutingEvents";
            this.ckRoutingEvents.Size = new System.Drawing.Size(108, 19);
            this.ckRoutingEvents.TabIndex = 9;
            this.ckRoutingEvents.Text = "Routing Events";
            this.ckRoutingEvents.UseVisualStyleBackColor = true;
            // 
            // ckShapeEvents
            // 
            this.ckShapeEvents.AutoSize = true;
            this.ckShapeEvents.Enabled = false;
            this.ckShapeEvents.Location = new System.Drawing.Point(297, 284);
            this.ckShapeEvents.Name = "ckShapeEvents";
            this.ckShapeEvents.Size = new System.Drawing.Size(101, 19);
            this.ckShapeEvents.TabIndex = 10;
            this.ckShapeEvents.Text = "Shape Events";
            this.ckShapeEvents.UseVisualStyleBackColor = true;
            // 
            // DlgDebugWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(649, 554);
            this.Controls.Add(this.ckShapeEvents);
            this.Controls.Add(this.ckRoutingEvents);
            this.Controls.Add(this.ckTraceEnabled);
            this.Controls.Add(this.btnClearTrace);
            this.Controls.Add(this.tbTrace);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tvShapes);
            this.Controls.Add(this.btnUpdate);
            this.Name = "DlgDebugWindow";
            this.Text = "FlowSharp Debug Window";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.TreeView tvShapes;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox tbTrace;
        private System.Windows.Forms.Button btnClearTrace;
        private System.Windows.Forms.CheckBox ckTraceEnabled;
        private System.Windows.Forms.CheckBox ckRoutingEvents;
        private System.Windows.Forms.CheckBox ckShapeEvents;
    }
}