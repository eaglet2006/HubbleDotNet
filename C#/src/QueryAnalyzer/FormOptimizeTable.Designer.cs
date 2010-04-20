namespace QueryAnalyzer
{
    partial class FormOptimizeTable
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
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.buttonFinished = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(39, 12);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(403, 42);
            this.progressBar.TabIndex = 0;
            // 
            // buttonFinished
            // 
            this.buttonFinished.Enabled = false;
            this.buttonFinished.Location = new System.Drawing.Point(39, 75);
            this.buttonFinished.Name = "buttonFinished";
            this.buttonFinished.Size = new System.Drawing.Size(75, 23);
            this.buttonFinished.TabIndex = 1;
            this.buttonFinished.Text = "Finished";
            this.buttonFinished.UseVisualStyleBackColor = true;
            this.buttonFinished.Click += new System.EventHandler(this.buttonFinished_Click);
            // 
            // FormOptimizeTable
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(490, 114);
            this.Controls.Add(this.buttonFinished);
            this.Controls.Add(this.progressBar);
            this.Name = "FormOptimizeTable";
            this.Text = "FormOptimizeTable";
            this.Load += new System.EventHandler(this.FormOptimizeTable_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormOptimizeTable_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button buttonFinished;
    }
}