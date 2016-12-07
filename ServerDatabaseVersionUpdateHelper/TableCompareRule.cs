using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// 对数据库中一张表格进行对比的规则
/// </summary>
public class TableCompareRule
{
    // 表名
    public string TableName { get; set; }
    // 比较方式
    public TableCompareWays CompareWay { get; set; }
    // 如果进行数据对比，忽略对哪些列下的数据比较（存储列名）
    public List<string> CompareIgnoreColumn { get; set; }
    // 如果进行数据对比，忽略对特定列中特定取值的行进行比较（每个Dictionary配置一条忽略项，key为列名，value为需满足的正则表达式，每个Dictionary中所有条件要求同时满足）
    public List<Dictionary<string, Regex>> CompareIgnoreData { get; set; }

    public TableCompareRule()
    {
        // 注意在C#中未对枚举显式赋值时默认值为0
        CompareWay = TableCompareWays.UnDefine;
        CompareIgnoreColumn = new List<string>();
        CompareIgnoreData = new List<Dictionary<string, Regex>>();
    }

    // 返回该表格对比规则的说明
    public string GetCompareRuleComment()
    {
        StringBuilder stringBuilder = new StringBuilder();
        string compareWayString = null;
        if (CompareWay == TableCompareWays.Ignore)
            compareWayString = "忽略比较";
        else if (CompareWay == TableCompareWays.OnlyColumnInfo)
            compareWayString = "仅比较表结构";
        else if (CompareWay == TableCompareWays.ColumnInfoAndData)
            compareWayString = "比较表结构和数据";

        stringBuilder.Append("该表格比较方式为：").Append(compareWayString).AppendLine();

        // 比较数据时，配置需要忽略的列和行
        if (CompareWay == TableCompareWays.ColumnInfoAndData)
        {
            // 忽略的列
            if (CompareIgnoreColumn.Count > 0)
                stringBuilder.AppendFormat("比较数据时忽略以下列：{0}\n", Utils.CombineString(CompareIgnoreColumn, ","));

            // 忽略的特定行
            if (CompareIgnoreData.Count > 0)
            {
                stringBuilder.AppendLine("忽略满足下列条件的特定行：");
                const string SPLIT_STRING = " && ";
                for (int i = 0; i < CompareIgnoreData.Count; ++i)
                {
                    Dictionary<string, Regex> oneIgnoreData = CompareIgnoreData[i];
                    stringBuilder.Append(i + 1).Append(".");
                    foreach (var pair in oneIgnoreData)
                    {
                        stringBuilder.AppendFormat("列\"{0}\"的值满足\"{1}\"", pair.Key, pair.Value.ToString());
                        stringBuilder.Append(SPLIT_STRING);
                    }
                    // 去掉最后多余的连接字符串
                    stringBuilder.Remove(stringBuilder.Length - SPLIT_STRING.Length, SPLIT_STRING.Length);
                    stringBuilder.AppendLine();
                }
            }
        }

        return stringBuilder.ToString();
    }
}

/// <summary>
/// 对数据库中一张表格进行对比的方式
/// </summary>
public enum TableCompareWays
{
    // 未声明
    UnDefine = -1,
    // 忽略，不进行比较
    Ignore,
    // 仅比较表结构
    OnlyColumnInfo,
    // 比较结构及数据
    ColumnInfoAndData,
}
