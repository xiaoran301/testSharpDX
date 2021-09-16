
using System;
using System.Diagnostics;
namespace WindowsFormsApp1
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnWater1 = new System.Windows.Forms.Button();
            this.btnWater1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnWater1
            // 
            this.btnWater1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnWater1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnWater1.Font = new System.Drawing.Font("Arial Narrow", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            //this.btnWater1.ForeColor = System.Drawing.Color.White;
            this.btnWater1.Location = new System.Drawing.Point(5, 5);
            this.btnWater1.Name = "btnWater1";
            this.btnWater1.Size = new System.Drawing.Size(250, 140);
            this.btnWater1.TabIndex = 0;
            this.btnWater1.Text = "Test Water";
            this.btnWater1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.btnWater1.UseVisualStyleBackColor = true;
            this.btnWater1.Click += new System.EventHandler(this.btnWater1_Click);
            // 
            // btnWater1
            // 
            this.btnWater1.Location = new System.Drawing.Point(12, 12);
            this.btnWater1.Name = "btnWater1";
            this.btnWater1.Size = new System.Drawing.Size(170, 61);
            this.btnWater1.TabIndex = 0;
            this.btnWater1.Text = "测试水";
            this.btnWater1.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnWater1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        private void btnWater1_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Send to debug output.");
        }


        #endregion

        private System.Windows.Forms.Button btnWater1;
    }
}

