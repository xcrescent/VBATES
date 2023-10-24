using System;

namespace VBATES
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
            this.EnquiryTextBox = new System.Windows.Forms.TextBox();
            this.EnquiryButton = new System.Windows.Forms.Button();
            this.ResponseLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // EnquiryTextBox
            // 
            this.EnquiryTextBox.Location = new System.Drawing.Point(313, 147);
            this.EnquiryTextBox.Name = "EnquiryTextBox";
            this.EnquiryTextBox.Size = new System.Drawing.Size(100, 22);
            this.EnquiryTextBox.TabIndex = 0;
            // 
            // EnquiryButton
            // 
            this.EnquiryButton.Location = new System.Drawing.Point(313, 195);
            this.EnquiryButton.Name = "EnquiryButton";
            this.EnquiryButton.Size = new System.Drawing.Size(100, 23);
            this.EnquiryButton.TabIndex = 1;
            this.EnquiryButton.Text = "EnquiryButton";
            this.EnquiryButton.UseVisualStyleBackColor = true;
            this.EnquiryButton.Click += new System.EventHandler(this.EnquiryButton_Click);
            // 
            // ResponseLabel
            // 
            this.ResponseLabel.AutoSize = true;
            this.ResponseLabel.Location = new System.Drawing.Point(310, 255);
            this.ResponseLabel.Name = "ResponseLabel";
            this.ResponseLabel.Size = new System.Drawing.Size(104, 16);
            this.ResponseLabel.TabIndex = 2;
            this.ResponseLabel.Text = "ResponseLabel";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.ResponseLabel);
            this.Controls.Add(this.EnquiryButton);
            this.Controls.Add(this.EnquiryTextBox);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox EnquiryTextBox;
        private System.Windows.Forms.Button EnquiryButton;
        private System.Windows.Forms.Label ResponseLabel;

        

    }
}

