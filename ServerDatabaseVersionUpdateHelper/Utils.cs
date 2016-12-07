using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

public class Utils
{
    // 用于输出结果的富文本框
    public static RichTextBox RtxResult = null;
    // 上一次在富文本框中输出的文字颜色
    private static Color _lastTextColor = Color.Black;

    public static void AppendOutputText(string text, OutputType type)
    {
        Color color;
        if (type == OutputType.Comment)
            color = Color.DarkGray;
        else if (type == OutputType.Warning)
            color = Color.Orange;
        else if (type == OutputType.Error)
            color = Color.Red;
        else if (type == OutputType.Sql)
            color = Color.Black;
        else
            color = _lastTextColor;

        RtxResult.SelectionColor = color;
        _lastTextColor = color;

        if (type != OutputType.Sql && type != OutputType.None)
            RtxResult.AppendText("-- ");

        RtxResult.AppendText(text);
        RtxResult.Focus();
    }

    /// <summary>
    /// 将List中的所有数据用指定分隔符连接为一个新字符串
    /// </summary>
    public static string CombineString(IList<string> list, string separateString)
    {
        if (list == null || list.Count < 1)
            return null;
        else
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < list.Count; ++i)
                builder.Append(list[i]).Append(separateString);

            string result = builder.ToString();
            // 去掉最后多加的一次分隔符
            if (separateString != null)
                return result.Substring(0, result.Length - separateString.Length);
            else
                return result;
        }
    }
}

public enum OutputType
{
    None,
    Comment,
    Warning,
    Error,
    Sql,
}
