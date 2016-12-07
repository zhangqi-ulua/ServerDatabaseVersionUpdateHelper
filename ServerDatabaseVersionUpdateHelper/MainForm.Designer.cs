namespace ServerDatabaseVersionUpdateHelper
{
    partial class MainForm
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
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.lblOldConnString = new System.Windows.Forms.Label();
            this.txtOldConnString = new System.Windows.Forms.TextBox();
            this.lblNewConnString = new System.Windows.Forms.Label();
            this.txtNewConnString = new System.Windows.Forms.TextBox();
            this.lblConfigPath = new System.Windows.Forms.Label();
            this.txtConfigPath = new System.Windows.Forms.TextBox();
            this.btnConfigPath = new System.Windows.Forms.Button();
            this.btnCompare = new System.Windows.Forms.Button();
            this.grpResult = new System.Windows.Forms.GroupBox();
            this.btnSaveToFile = new System.Windows.Forms.Button();
            this.btnCopyToClipboard = new System.Windows.Forms.Button();
            this.chkIgnoreComment = new System.Windows.Forms.CheckBox();
            this.rtxResult = new System.Windows.Forms.RichTextBox();
            this.grpResult.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblOldConnString
            // 
            this.lblOldConnString.AutoSize = true;
            this.lblOldConnString.Location = new System.Drawing.Point(22, 26);
            this.lblOldConnString.Name = "lblOldConnString";
            this.lblOldConnString.Size = new System.Drawing.Size(137, 12);
            this.lblOldConnString.TabIndex = 0;
            this.lblOldConnString.Text = "旧版数据库连接字符串：";
            // 
            // txtOldConnString
            // 
            this.txtOldConnString.Location = new System.Drawing.Point(165, 23);
            this.txtOldConnString.Name = "txtOldConnString";
            this.txtOldConnString.Size = new System.Drawing.Size(541, 21);
            this.txtOldConnString.TabIndex = 1;
            // 
            // lblNewConnString
            // 
            this.lblNewConnString.AutoSize = true;
            this.lblNewConnString.Location = new System.Drawing.Point(22, 56);
            this.lblNewConnString.Name = "lblNewConnString";
            this.lblNewConnString.Size = new System.Drawing.Size(137, 12);
            this.lblNewConnString.TabIndex = 2;
            this.lblNewConnString.Text = "新版数据库连接字符串：";
            // 
            // txtNewConnString
            // 
            this.txtNewConnString.Location = new System.Drawing.Point(165, 53);
            this.txtNewConnString.Name = "txtNewConnString";
            this.txtNewConnString.Size = new System.Drawing.Size(541, 21);
            this.txtNewConnString.TabIndex = 3;
            // 
            // lblConfigPath
            // 
            this.lblConfigPath.AutoSize = true;
            this.lblConfigPath.Location = new System.Drawing.Point(22, 86);
            this.lblConfigPath.Name = "lblConfigPath";
            this.lblConfigPath.Size = new System.Drawing.Size(89, 12);
            this.lblConfigPath.TabIndex = 4;
            this.lblConfigPath.Text = "配置文件路径：";
            // 
            // txtConfigPath
            // 
            this.txtConfigPath.Location = new System.Drawing.Point(165, 83);
            this.txtConfigPath.Name = "txtConfigPath";
            this.txtConfigPath.Size = new System.Drawing.Size(456, 21);
            this.txtConfigPath.TabIndex = 5;
            // 
            // btnConfigPath
            // 
            this.btnConfigPath.Location = new System.Drawing.Point(631, 81);
            this.btnConfigPath.Name = "btnConfigPath";
            this.btnConfigPath.Size = new System.Drawing.Size(75, 23);
            this.btnConfigPath.TabIndex = 6;
            this.btnConfigPath.Text = "浏览";
            this.btnConfigPath.UseVisualStyleBackColor = true;
            this.btnConfigPath.Click += new System.EventHandler(this.btnConfigPath_Click);
            // 
            // btnCompare
            // 
            this.btnCompare.Location = new System.Drawing.Point(728, 41);
            this.btnCompare.Name = "btnCompare";
            this.btnCompare.Size = new System.Drawing.Size(68, 43);
            this.btnCompare.TabIndex = 7;
            this.btnCompare.Text = "对比";
            this.btnCompare.UseVisualStyleBackColor = true;
            this.btnCompare.Click += new System.EventHandler(this.btnCompare_Click);
            // 
            // grpResult
            // 
            this.grpResult.Controls.Add(this.btnSaveToFile);
            this.grpResult.Controls.Add(this.btnCopyToClipboard);
            this.grpResult.Controls.Add(this.chkIgnoreComment);
            this.grpResult.Controls.Add(this.rtxResult);
            this.grpResult.Location = new System.Drawing.Point(24, 138);
            this.grpResult.Name = "grpResult";
            this.grpResult.Size = new System.Drawing.Size(772, 324);
            this.grpResult.TabIndex = 8;
            this.grpResult.TabStop = false;
            this.grpResult.Text = "对比结果及生成SQL";
            // 
            // btnSaveToFile
            // 
            this.btnSaveToFile.Location = new System.Drawing.Point(698, 200);
            this.btnSaveToFile.Name = "btnSaveToFile";
            this.btnSaveToFile.Size = new System.Drawing.Size(58, 44);
            this.btnSaveToFile.TabIndex = 3;
            this.btnSaveToFile.Text = "保存到文件";
            this.btnSaveToFile.UseVisualStyleBackColor = true;
            this.btnSaveToFile.Click += new System.EventHandler(this.btnSaveToFile_Click);
            // 
            // btnCopyToClipboard
            // 
            this.btnCopyToClipboard.Location = new System.Drawing.Point(698, 128);
            this.btnCopyToClipboard.Name = "btnCopyToClipboard";
            this.btnCopyToClipboard.Size = new System.Drawing.Size(58, 44);
            this.btnCopyToClipboard.TabIndex = 2;
            this.btnCopyToClipboard.Text = "复制到剪贴板";
            this.btnCopyToClipboard.UseVisualStyleBackColor = true;
            this.btnCopyToClipboard.Click += new System.EventHandler(this.btnCopyToClipboard_Click);
            // 
            // chkIgnoreComment
            // 
            this.chkIgnoreComment.AutoSize = true;
            this.chkIgnoreComment.Location = new System.Drawing.Point(698, 87);
            this.chkIgnoreComment.Name = "chkIgnoreComment";
            this.chkIgnoreComment.Size = new System.Drawing.Size(72, 16);
            this.chkIgnoreComment.TabIndex = 1;
            this.chkIgnoreComment.Text = "忽略注释";
            this.chkIgnoreComment.UseVisualStyleBackColor = true;
            // 
            // rtxResult
            // 
            this.rtxResult.Location = new System.Drawing.Point(19, 32);
            this.rtxResult.Name = "rtxResult";
            this.rtxResult.Size = new System.Drawing.Size(663, 268);
            this.rtxResult.TabIndex = 0;
            this.rtxResult.Text = "";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(819, 484);
            this.Controls.Add(this.grpResult);
            this.Controls.Add(this.btnCompare);
            this.Controls.Add(this.btnConfigPath);
            this.Controls.Add(this.txtConfigPath);
            this.Controls.Add(this.lblConfigPath);
            this.Controls.Add(this.txtNewConnString);
            this.Controls.Add(this.lblNewConnString);
            this.Controls.Add(this.txtOldConnString);
            this.Controls.Add(this.lblOldConnString);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "服务器数据库版本升级助手 1.0   by 张齐（https://github.com/zhangqi-ulua）";
            this.grpResult.ResumeLayout(false);
            this.grpResult.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblOldConnString;
        private System.Windows.Forms.TextBox txtOldConnString;
        private System.Windows.Forms.Label lblNewConnString;
        private System.Windows.Forms.TextBox txtNewConnString;
        private System.Windows.Forms.Label lblConfigPath;
        private System.Windows.Forms.TextBox txtConfigPath;
        private System.Windows.Forms.Button btnConfigPath;
        private System.Windows.Forms.Button btnCompare;
        private System.Windows.Forms.GroupBox grpResult;
        private System.Windows.Forms.Button btnSaveToFile;
        private System.Windows.Forms.Button btnCopyToClipboard;
        private System.Windows.Forms.CheckBox chkIgnoreComment;
        private System.Windows.Forms.RichTextBox rtxResult;
    }
}

