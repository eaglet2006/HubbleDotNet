namespace TestSFQL
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
            this.textBoxSFQL = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.buttonLexical = new System.Windows.Forms.Button();
            this.buttonLexicalPerformance = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxSFQL
            // 
            this.textBoxSFQL.Location = new System.Drawing.Point(22, 34);
            this.textBoxSFQL.Multiline = true;
            this.textBoxSFQL.Name = "textBoxSFQL";
            this.textBoxSFQL.Size = new System.Drawing.Size(308, 370);
            this.textBoxSFQL.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "SFQL";
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Location = new System.Drawing.Point(426, 34);
            this.textBoxOutput.Multiline = true;
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.Size = new System.Drawing.Size(308, 370);
            this.textBoxOutput.TabIndex = 2;
            // 
            // buttonLexical
            // 
            this.buttonLexical.Location = new System.Drawing.Point(342, 87);
            this.buttonLexical.Name = "buttonLexical";
            this.buttonLexical.Size = new System.Drawing.Size(75, 23);
            this.buttonLexical.TabIndex = 3;
            this.buttonLexical.Text = "Lexical";
            this.buttonLexical.UseVisualStyleBackColor = true;
            this.buttonLexical.Click += new System.EventHandler(this.buttonLexical_Click);
            // 
            // buttonLexicalPerformance
            // 
            this.buttonLexicalPerformance.Location = new System.Drawing.Point(342, 133);
            this.buttonLexicalPerformance.Name = "buttonLexicalPerformance";
            this.buttonLexicalPerformance.Size = new System.Drawing.Size(75, 23);
            this.buttonLexicalPerformance.TabIndex = 4;
            this.buttonLexicalPerformance.Text = "Lexical P";
            this.buttonLexicalPerformance.UseVisualStyleBackColor = true;
            this.buttonLexicalPerformance.Click += new System.EventHandler(this.buttonLexicalPerformance_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(759, 444);
            this.Controls.Add(this.buttonLexicalPerformance);
            this.Controls.Add(this.buttonLexical);
            this.Controls.Add(this.textBoxOutput);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxSFQL);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxSFQL;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.Button buttonLexical;
        private System.Windows.Forms.Button buttonLexicalPerformance;
    }
}

