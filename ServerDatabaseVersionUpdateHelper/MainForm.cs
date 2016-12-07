using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ServerDatabaseVersionUpdateHelper
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Utils.RtxResult = rtxResult;
        }

        private void btnCompare_Click(object sender, EventArgs e)
        {
            rtxResult.Text = string.Empty;
            string errorString = null;

            // 检查新旧数据库连接字符串是否输入
            AppValues.ConnectStringForOldDatabase = txtOldConnString.Text.Trim();
            AppValues.ConnectStringForNewDatabase = txtNewConnString.Text.Trim();
            if (string.IsNullOrEmpty(AppValues.ConnectStringForOldDatabase))
            {
                MessageBox.Show("必须输入旧版数据库连接字符串", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(AppValues.ConnectStringForNewDatabase))
            {
                MessageBox.Show("必须输入新版数据库连接字符串", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (AppValues.ConnectStringForOldDatabase.Equals(AppValues.ConnectStringForNewDatabase))
            {
                MessageBox.Show("输入的新旧两数据库连接字符串重复", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // 检查配置文件
            AppValues.ConfigFilePath = txtConfigPath.Text.Trim();
            if (string.IsNullOrEmpty(AppValues.ConfigFilePath))
            {
                DialogResult dialogResult = MessageBox.Show("未指定配置文件，本工具将默认比较两数据库中所有表格的结构和数据，确定要这样做吗？\n\n点击“是”进行默认比较，点击“否”放弃", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.No)
                    return;
                else
                    AppValues.AllTableCompareRule = new Dictionary<string, TableCompareRule>();
            }
            else
            {
                Utils.AppendOutputText("读取配置文件：", OutputType.Comment);
                AppValues.AllTableCompareRule = ConfigReader.LoadConfig(AppValues.ConfigFilePath, out errorString);
                if (!string.IsNullOrEmpty(errorString))
                {
                    MessageBox.Show(string.Concat("配置文件中存在以下错误，请修正后重试：\n\n", errorString), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                    Utils.AppendOutputText("成功\n", OutputType.None);
            }
            // 连接两数据库
            Utils.AppendOutputText("连接旧版数据库：", OutputType.Comment);
            MySQLOperateHelper.ConnectToDatabase(AppValues.ConnectStringForOldDatabase, out AppValues.OldConn, out AppValues.OldSchemaName, out AppValues.OldExistTableNames, out errorString);
            if (!string.IsNullOrEmpty(errorString))
            {
                MessageBox.Show(string.Concat("连接旧版数据库失败，错误原因为：", errorString), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
                Utils.AppendOutputText("成功\n", OutputType.None);

            Utils.AppendOutputText("连接新版数据库：", OutputType.Comment);
            MySQLOperateHelper.ConnectToDatabase(AppValues.ConnectStringForNewDatabase, out AppValues.NewConn, out AppValues.NewSchemaName, out AppValues.NewExistTableNames, out errorString);
            if (!string.IsNullOrEmpty(errorString))
            {
                MessageBox.Show(string.Concat("连接新版数据库失败，错误原因为：", errorString), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
                Utils.AppendOutputText("成功\n", OutputType.None);

            // 整理数据库结构信息
            Utils.AppendOutputText("分析旧版数据库表结构：", OutputType.Comment);
            AppValues.OldTableInfo = new Dictionary<string, TableInfo>();
            foreach (string tableName in AppValues.OldExistTableNames)
            {
                TableInfo tableInfo = MySQLOperateHelper.GetTableInfo(AppValues.OldSchemaName, tableName, AppValues.OldConn);
                AppValues.OldTableInfo.Add(tableName, tableInfo);
            }
            Utils.AppendOutputText("成功\n", OutputType.None);

            Utils.AppendOutputText("分析新版数据库表结构：", OutputType.Comment);
            AppValues.NewTableInfo = new Dictionary<string, TableInfo>();
            foreach (string tableName in AppValues.NewExistTableNames)
            {
                TableInfo tableInfo = MySQLOperateHelper.GetTableInfo(AppValues.NewSchemaName, tableName, AppValues.NewConn);
                AppValues.NewTableInfo.Add(tableName, tableInfo);
            }
            Utils.AppendOutputText("成功\n", OutputType.None);

            Utils.AppendOutputText("\n", OutputType.None);
            // 进行对比，并展示结果
            MySQLOperateHelper.CompareAndShowResult(out errorString);
            if (!string.IsNullOrEmpty(errorString))
            {
                string tips = string.Concat("对比中发现以下问题，请修正后重新进行比较：\n\n", errorString);
                MessageBox.Show(tips, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                Utils.AppendOutputText("\n", OutputType.Comment);
                Utils.AppendOutputText("对比完毕", OutputType.Comment);
            }
        }

        private void btnConfigPath_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "请选择配置文件所在路径";
            dialog.Multiselect = false;
            dialog.Filter = "文本文件 (*.txt)|*.txt";
            if (dialog.ShowDialog() == DialogResult.OK)
                txtConfigPath.Text = dialog.FileName;
        }

        private string _GetTextIgnoreComment(string text)
        {
            StringBuilder stringBuilder = new StringBuilder();
            using (StringReader reader = new StringReader(rtxResult.Text))
            {
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!line.StartsWith("--"))
                        stringBuilder.AppendLine(line);
                }
            }

            return stringBuilder.ToString();
        }

        private void btnCopyToClipboard_Click(object sender, EventArgs e)
        {
            string text = null;
            if (chkIgnoreComment.Checked == true)
                text = _GetTextIgnoreComment(rtxResult.Text);
            else
                text = rtxResult.Text;

            Clipboard.SetData(DataFormats.Text, text);
            MessageBox.Show("已成功复制到剪贴板", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnSaveToFile_Click(object sender, EventArgs e)
        {
            string text = null;
            if (chkIgnoreComment.Checked == true)
                text = _GetTextIgnoreComment(rtxResult.Text);
            else
                text = rtxResult.Text;

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.ValidateNames = true;
            dialog.Title = "请选择导出文件的存储路径";
            dialog.Filter = "Sql files (*.sql)|*.sql|Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string savePath = dialog.FileName;
                try
                {
                    StreamWriter writer = new StreamWriter(savePath, false, new UTF8Encoding(false));
                    writer.Write(text);
                    writer.Flush();
                    writer.Close();

                    MessageBox.Show("保存成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(string.Concat("保存失败，错误原因为：", exception.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
