﻿namespace c_sharp;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        // this.components = new System.ComponentModel.Container();
        // this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        // this.ClientSize = new System.Drawing.Size(800, 450);
        // this.Text = "Form1";

        textBox1 = new TextBox();
        SuspendLayout();
        // 
        // textBox1
        // 
        textBox1.Location = new Point(160, 184);
        textBox1.Name = "textBox1";
        textBox1.Size = new Size(133, 23);
        textBox1.TabIndex = 0;
        textBox1.Text = "안녕하세요 레논입니다";
        textBox1.TextAlign = HorizontalAlignment.Center;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(726, 450);
        Controls.Add(textBox1);
        Name = "레논";
        Text = "레논";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private TextBox textBox1;
}
