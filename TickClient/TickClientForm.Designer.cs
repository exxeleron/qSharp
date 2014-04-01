//
//   Copyright (c) 2011-2014 Exxeleron GmbH
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//

namespace qSharp.Sample
{
    partial class TickClientForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.qhostTB = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.qtableTB = new System.Windows.Forms.TextBox();
            this.subscribeBtn = new System.Windows.Forms.Button();
            this.unsubscribeBtn = new System.Windows.Forms.Button();
            this.data = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "host:";
            // 
            // qhostTB
            // 
            this.qhostTB.Location = new System.Drawing.Point(48, 12);
            this.qhostTB.Name = "qhostTB";
            this.qhostTB.Size = new System.Drawing.Size(100, 20);
            this.qhostTB.TabIndex = 1;
            this.qhostTB.Text = "localhost:5010";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(173, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "table:";
            // 
            // qtableTB
            // 
            this.qtableTB.Location = new System.Drawing.Point(212, 12);
            this.qtableTB.Name = "qtableTB";
            this.qtableTB.Size = new System.Drawing.Size(100, 20);
            this.qtableTB.TabIndex = 3;
            this.qtableTB.Text = "trade";
            // 
            // subscribeBtn
            // 
            this.subscribeBtn.Location = new System.Drawing.Point(440, 9);
            this.subscribeBtn.Name = "subscribeBtn";
            this.subscribeBtn.Size = new System.Drawing.Size(84, 25);
            this.subscribeBtn.TabIndex = 4;
            this.subscribeBtn.Text = "Subscribe";
            this.subscribeBtn.UseVisualStyleBackColor = true;
            this.subscribeBtn.Click += new System.EventHandler(this.subscribeBtn_Click);
            // 
            // unsubscribeBtn
            // 
            this.unsubscribeBtn.Location = new System.Drawing.Point(538, 9);
            this.unsubscribeBtn.Name = "unsubscribeBtn";
            this.unsubscribeBtn.Size = new System.Drawing.Size(84, 25);
            this.unsubscribeBtn.TabIndex = 5;
            this.unsubscribeBtn.Text = "Unsubscribe";
            this.unsubscribeBtn.UseVisualStyleBackColor = true;
            this.unsubscribeBtn.Click += new System.EventHandler(this.unsubscribeBtn_Click);
            // 
            // data
            // 
            this.data.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.data.CausesValidation = false;
            this.data.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.data.FullRowSelect = true;
            this.data.GridLines = true;
            this.data.Location = new System.Drawing.Point(12, 40);
            this.data.Name = "data";
            this.data.Size = new System.Drawing.Size(610, 245);
            this.data.TabIndex = 6;
            this.data.UseCompatibleStateImageBehavior = false;
            this.data.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 149;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Width = 167;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Width = 139;
            // 
            // TickClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(639, 295);
            this.Controls.Add(this.data);
            this.Controls.Add(this.unsubscribeBtn);
            this.Controls.Add(this.subscribeBtn);
            this.Controls.Add(this.qtableTB);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.qhostTB);
            this.Controls.Add(this.label1);
            this.Name = "TickClientForm";
            this.Text = "TickClient";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox qhostTB;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox qtableTB;
        private System.Windows.Forms.Button subscribeBtn;
        private System.Windows.Forms.Button unsubscribeBtn;
        private System.Windows.Forms.ListView data;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
    }
}

