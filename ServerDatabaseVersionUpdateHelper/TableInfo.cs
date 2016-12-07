using System.Collections.Generic;

/// <summary>
/// 一张数据库表格的信息
/// </summary>
public class TableInfo
{
    // Schema名
    public string SchemaName { get; set; }
    // 表名
    public string TableName { get; set; }
    // 注释
    public string Comment { get; set; }
    // 校对集
    public string Collation { get; set; }
    // 所有列信息（key：列名， value：列信息）
    public Dictionary<string, ColumnInfo> AllColumnInfo { get; set; }
    // 主键列的列名
    public List<string> PrimaryKeyColumnNames { get; set; }
    // 索引设置（key：索引名， value：按顺序排列的列名）
    public Dictionary<string, List<string>> IndexInfo { get; set; }

    public TableInfo()
    {
        AllColumnInfo = new Dictionary<string, ColumnInfo>();
        PrimaryKeyColumnNames = new List<string>();
        IndexInfo = new Dictionary<string, List<string>>();
    }
}

/// <summary>
/// 数据库表格中一列的信息
/// </summary>
public class ColumnInfo
{
    // 表名
    public string TableName { get; set; }
    // 列名
    public string ColumnName { get; set; }
    // 数据类型（包含长度）
    public string DataType { get; set; }
    // 注释
    public string Comment { get; set; }
    // 是否是主键
    public bool IsPrimaryKey { get; set; }
    // 是否唯一
    public bool IsUnique { get; set; }
    // 是否为索引列但允许重复值
    public bool IsMultiple { get; set; }
    // 是否非空
    public bool IsNotEmpty { get; set; }
    // 是否自增
    public bool IsAutoIncrement { get; set; }
    // 默认值
    public string DefaultValue { get; set; }
}
