using MySql.Data.MySqlClient;
using System.Collections.Generic;

public class AppValues
{
    /// <summary>
    /// 新版本数据库的连接字符串
    /// </summary>
    public static string ConnectStringForNewDatabase = null;

    /// <summary>
    /// 老版本数据库的连接字符串
    /// </summary>
    public static string ConnectStringForOldDatabase = null;

    /// <summary>
    /// 配置文件所在路径
    /// </summary>
    public static string ConfigFilePath = null;

    /// <summary>
    /// 从配置文件中读取的数据库对比规则（key：表名， value：对比规则）
    /// </summary>
    public static Dictionary<string, TableCompareRule> AllTableCompareRule = null;

    // 以下依次为新旧数据库连接对象、Schema名、已存在的表名
    public static MySqlConnection OldConn = null;
    public static MySqlConnection NewConn = null;
    public static string OldSchemaName = null;
    public static string NewSchemaName = null;
    public static List<string> OldExistTableNames = null;
    public static List<string> NewExistTableNames = null;
    // 新旧数据库中所有表格结构信息（key：表名， value：TableInfo类）
    public static Dictionary<string, TableInfo> OldTableInfo = null;
    public static Dictionary<string, TableInfo> NewTableInfo = null;
}
