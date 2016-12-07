using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class ConfigReader
{
    public static Dictionary<string, TableCompareRule> LoadConfig(string filePath, out string errorString)
    {
        if (!File.Exists(filePath))
        {
            errorString = string.Format("输入路径为{0}的配置文件不存在", filePath);
            return null;
        }

        Dictionary<string, TableCompareRule> allTableCompareRule = new Dictionary<string, TableCompareRule>();

        using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
        {
            string line = null;
            int lineNumber = 0;
            while ((line = reader.ReadLine()) != null)
            {
                ++lineNumber;

                // 忽略以#开头的注释行和空行
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;

                // 以@开头的为表格对比方式配置
                if (line.StartsWith("@"))
                {
                    // 配置以英文冒号分隔的键值对，冒号左边声明数据库表名，右边声明对比方式（0为忽略，不进行比较；1为仅比较表结构；2为比较结构及数据）
                    string[] keyAndVale = line.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                    if (keyAndVale.Length != 2)
                    {
                        errorString = string.Format("第{0}行的配置\"{1}\"不合法，以@开头的表格对比方式配置，需用英文冒号分隔数据库表名和对比方式", lineNumber, line);
                        return null;
                    }
                    string tableName = keyAndVale[0].Substring(1).Trim();
                    if (string.IsNullOrEmpty(tableName))
                    {
                        errorString = string.Format("第{0}行的配置\"{1}\"不合法，以@开头的表格对比方式配置中，未声明表格名", lineNumber, line);
                        return null;
                    }
                    int compareWayNumber;
                    if (int.TryParse(keyAndVale[1], out compareWayNumber) == false)
                    {
                        errorString = string.Format("第{0}行的配置\"{1}\"不合法，以@开头的表格对比方式配置中，键值\"{2}\"不是合法的对比方式数字，请按以下规则配置：0为忽略，不进行比较；1为仅比较表结构；2为比较结构及数据", lineNumber, line, keyAndVale[1]);
                        return null;
                    }
                    if (Enum.IsDefined(typeof(TableCompareWays), compareWayNumber) == false || compareWayNumber == (int)TableCompareWays.UnDefine)
                    {
                        errorString = string.Format("第{0}行的配置\"{1}\"不合法，以@开头的表格对比方式配置中，键值\"{2}\"不是合法的对比方式数字，请按以下规则配置：0为忽略，不进行比较；1为仅比较表结构；2为比较结构及数据", lineNumber, line, keyAndVale[1]);
                        return null;
                    }

                    if (!allTableCompareRule.ContainsKey(tableName))
                        allTableCompareRule.Add(tableName, new TableCompareRule());

                    TableCompareRule tableCompareRule = allTableCompareRule[tableName];
                    if (tableCompareRule.CompareWay != TableCompareWays.UnDefine)
                    {
                        errorString = string.Format("第{0}行的配置\"{1}\"重复对表格{2}进行对比方式配置，之前已经声明了对比方式为{3}", lineNumber, line, tableName, (int)tableCompareRule.CompareWay);
                        return null;
                    }

                    tableCompareRule.CompareWay = (TableCompareWays)compareWayNumber;
                }
                // 以!开头的为配置对某张表格进行数据对比时忽略的列名
                else if (line.StartsWith("!"))
                {
                    // 键名为表名，键值为要忽略的列名，不同列名之间用|分隔
                    string[] keyAndVale = line.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                    if (keyAndVale.Length != 2)
                    {
                        errorString = string.Format("第{0}行的配置\"{1}\"不合法，以!开头的对某张表格进行数据对比时忽略的列名的配置，需用英文冒号分隔数据库表名和列名", lineNumber, line);
                        return null;
                    }
                    string tableName = keyAndVale[0].Substring(1).Trim();
                    if (string.IsNullOrEmpty(tableName))
                    {
                        errorString = string.Format("第{0}行的配置\"{1}\"不合法，以!开头的对某张表格进行数据对比时忽略的列名的配置，未声明表格名", lineNumber, line);
                        return null;
                    }
                    string[] columnNames = keyAndVale[1].Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    if (columnNames.Length < 1)
                    {
                        errorString = string.Format("第{0}行的配置\"{1}\"不合法，以!开头的对某张表格进行数据对比时忽略的列名的配置，未声明忽略哪些列", lineNumber, line);
                        return null;
                    }

                    if (!allTableCompareRule.ContainsKey(tableName))
                        allTableCompareRule.Add(tableName, new TableCompareRule());

                    TableCompareRule tableCompareRule = allTableCompareRule[tableName];
                    if (tableCompareRule.CompareIgnoreColumn.Count > 0)
                    {
                        errorString = string.Format("第{0}行的配置\"{1}\"重复对表格{2}配置数据对比时忽略的列名，之前已进行过声明", lineNumber, line, tableName);
                        return null;
                    }

                    for (int i = 0; i < columnNames.Length; ++i)
                        tableCompareRule.CompareIgnoreColumn.Add(columnNames[i].Trim());
                }
                // 以$开头的为配置对某张表格进行数据对比时忽略对特定列中特定取值的行进行比较
                else if (line.StartsWith("$"))
                {
                    // 键名为表名，键值为列名后面在英文小括号中声明数据值，使用正则表达式进行模糊匹配，若需要同时满足多个列为特定值，规则之间用&&连接
                    // 比如“$server_config:key(^channelId$)&&lastUpdateUser(^管理员1$)”表示忽略对server_config中key列中值为“channelId”并且上次修改者为“管理员1”的行进行比较。注意若要精确匹配，需使用正则表达式中的^和$表示字符串开头和结尾
                    // 考虑到正则表达式中冒号为特殊符号，不使用split分隔键值对，而是查找最左边的冒号
                    int colonIndex = line.IndexOf(":");
                    if (colonIndex == -1)
                    {
                        errorString = string.Format("第{0}行的配置\"{1}\"不合法，以$开头的对某张表格进行指定行忽略的配置，需用英文冒号分隔数据库表名和特殊值声明", lineNumber, line);
                        return null;
                    }
                    string tableName = line.Substring(1, colonIndex - 1).Trim();
                    if (string.IsNullOrEmpty(tableName))
                    {
                        errorString = string.Format("第{0}行的配置\"{1}\"不合法，以$开头的对某张表格进行指定行忽略的配置，未声明表格名", lineNumber, line);
                        return null;
                    }
                    string configString = line.Substring(colonIndex + 1).Trim();
                    if (string.IsNullOrEmpty(configString))
                    {
                        errorString = string.Format("第{0}行的配置\"{1}\"不合法，以$开头的对某张表格进行指定行忽略的配置，需在冒号后声明的键值格式为列名后面在英文小括号中声明数据值，使用正则表达式进行模糊匹配，若需要同时满足多个列为特定值，规则之间用&&连接", lineNumber, line);
                        return null;
                    }

                    Dictionary<string, Regex> oneIgnoreLineInfo = new Dictionary<string, Regex>();
                    // 用&&分隔需同时满足的多个忽略特定值配置
                    string[] allIgnoreConfigString = configString.Split(new string[] { "&&" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < allIgnoreConfigString.Length; ++i)
                    {
                        string oneIgnoreConfigString = allIgnoreConfigString[i].Trim();
                        int leftBracketIndex = oneIgnoreConfigString.IndexOf("(");
                        int rightBracketIndex = oneIgnoreConfigString.LastIndexOf(")");
                        if (leftBracketIndex != -1 && rightBracketIndex > leftBracketIndex)
                        {
                            string columnName = oneIgnoreConfigString.Substring(0, leftBracketIndex).Trim();
                            if (string.IsNullOrEmpty(columnName))
                            {
                                errorString = string.Format("第{0}行的配置\"{1}\"不合法，以$开头的对某张表格进行指定行忽略的配置中，\"{2}\"未在括号前声明列名", lineNumber, line, oneIgnoreConfigString);
                                return null;
                            }
                            string ignoreValue = oneIgnoreConfigString.Substring(leftBracketIndex + 1, rightBracketIndex - leftBracketIndex - 1);
                            if (string.IsNullOrEmpty(ignoreValue))
                            {
                                errorString = string.Format("第{0}行的配置\"{1}\"不合法，以$开头的对某张表格进行指定行忽略的配置中，\"{2}\"未在列名后面在英文小括号中声明数据值", lineNumber, line, oneIgnoreConfigString);
                                return null;
                            }

                            Regex regex = new Regex(ignoreValue);
                            oneIgnoreLineInfo.Add(columnName, regex);
                        }
                        else
                        {
                            errorString = string.Format("第{0}行的配置\"{1}\"不合法，以$开头的对某张表格进行指定行忽略的配置中，\"{2}\"不符合键值格式为列名后面在英文小括号中声明数据值", lineNumber, line, oneIgnoreConfigString);
                            return null;
                        }
                    }

                    if (!allTableCompareRule.ContainsKey(tableName))
                        allTableCompareRule.Add(tableName, new TableCompareRule());

                    TableCompareRule tableCompareRule = allTableCompareRule[tableName];
                    tableCompareRule.CompareIgnoreData.Add(oneIgnoreLineInfo);
                }
                else
                {
                    errorString = string.Format("第{0}行的配置\"{1}\"不合法，以非法字符\"{2}\"开头，请根据说明文件进行配置", lineNumber, line, line[0]);
                    return null;
                }
            }
        }

        errorString = null;
        return allTableCompareRule;
    }
}
