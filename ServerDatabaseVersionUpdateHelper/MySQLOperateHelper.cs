using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class MySQLOperateHelper
{
    // MySQL支持的用于定义Schema名的参数名
    private static string[] _DEFINE_SCHEMA_NAME_PARAM = { "Database", "Initial Catalog" };

    private const string _SELECT_DATA_SQL = "SELECT {0} FROM {1};";
    private const string _SELECT_COLUMN_INFO_SQL = "SELECT * FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' ORDER BY ORDINAL_POSITION ASC;";
    private const string _SELECT_TABLE_INFO_SQL = "SELECT {0} FROM information_schema.TABLES WHERE TABLE_SCHEMA = '{1}' AND TABLE_NAME = '{2}';";
    private const string _SHOW_CREATE_TABLE_SQL = "SHOW CREATE TABLE {0}";
    private const string _SHOW_INDEX_SQL = "SHOW INDEX FROM {0} WHERE Key_name != 'PRIMARY' AND Index_type = 'BTREE';";
    private const string _INSERT_DATA_SQL = "INSERT INTO {0} ({1}) VALUES {2};";
    private const string _DROP_TABLE_SQL = "DROP TABLE {0};";
    private const string _ALTER_TABLE_SQL = "ALTER TABLE {0}";
    private const string _DROP_COLUMN_SQL = "DROP COLUMN `{0}`";
    private const string _ADD_COLUMN_SQL = "ADD COLUMN `{0}` {1} {2}{3} COMMENT '{4}'";
    private const string _CHANGE_COLUMN_SQL = "CHANGE COLUMN `{0}` `{0}` {1} {2}{3} COMMENT '{4}'";
    private const string _DROP_PRIMARY_KEY_SQL = "DROP PRIMARY KEY";
    private const string _ADD_PRIMARY_KEY_SQL = "ADD PRIMARY KEY ({0})";
    private const string _DROP_INDEX_SQL = "DROP INDEX `{0}`";
    private const string _ADD_UNIQUE_INDEX_SQL = "ADD UNIQUE INDEX `{0}` ({1})";
    private const string _ALTER_TABLE_COMMENT_SQL = "COMMENT = '{0}'";
    private const string _ALTER_TABLE_COLLATION_SQL = "COLLATE = {0}";
    private const string _DROP_DATA_SQL = "DELETE FROM {0} WHERE {1};";
    private const string _UPDATE_DATA_SQL = "UPDATE {0} SET {1} WHERE {2};";

    /// <summary>
    /// 连接数据库，并返回连接对象、Schema名、所有表名
    /// </summary>
    public static bool ConnectToDatabase(string connectString, out MySqlConnection conn, out string schemaName, out List<string> existTableNames, out string errorString)
    {
        conn = null;
        schemaName = null;
        existTableNames = new List<string>();

        // 提取MySQL连接字符串中的Schema名
        foreach (string legalSchemaNameParam in _DEFINE_SCHEMA_NAME_PARAM)
        {
            int defineStartIndex = connectString.IndexOf(legalSchemaNameParam, StringComparison.CurrentCultureIgnoreCase);
            if (defineStartIndex != -1)
            {
                // 查找后面的等号
                int equalSignIndex = -1;
                for (int i = defineStartIndex + legalSchemaNameParam.Length; i < connectString.Length; ++i)
                {
                    if (connectString[i] == '=')
                    {
                        equalSignIndex = i;
                        break;
                    }
                }
                if (equalSignIndex == -1 || equalSignIndex + 1 == connectString.Length)
                {
                    errorString = string.Format("MySQL数据库连接字符串（\"{0}\"）中\"{1}\"后需要跟\"=\"进行Schema名声明", connectString, legalSchemaNameParam);
                    return false;
                }
                else
                {
                    // 查找定义的Schema名，在参数声明的=后面截止到下一个分号或字符串结束
                    int semicolonIndex = -1;
                    for (int i = equalSignIndex + 1; i < connectString.Length; ++i)
                    {
                        if (connectString[i] == ';')
                        {
                            semicolonIndex = i;
                            break;
                        }
                    }
                    if (semicolonIndex == -1)
                        schemaName = connectString.Substring(equalSignIndex + 1).Trim();
                    else
                        schemaName = connectString.Substring(equalSignIndex + 1, semicolonIndex - equalSignIndex - 1).Trim();
                }

                break;
            }
        }
        if (schemaName == null)
        {
            errorString = string.Format("MySQL数据库连接字符串（\"{0}\"）中不包含Schema名的声明，请在{1}中任选一个参数名进行声明", connectString, Utils.CombineString(_DEFINE_SCHEMA_NAME_PARAM, ","));
            return false;
        }

        try
        {
            conn = new MySqlConnection(connectString);
            conn.Open();
            if (conn.State == System.Data.ConnectionState.Open)
            {
                // 获取已存在的数据表名
                DataTable schemaInfo = conn.GetSchema(System.Data.SqlClient.SqlClientMetaDataCollectionNames.Tables);
                foreach (DataRow info in schemaInfo.Rows)
                    existTableNames.Add(info.ItemArray[2].ToString());

                errorString = null;
                return true;
            }
            else
            {
                errorString = "未知错误";
                return true;
            }
        }
        catch (Exception exception)
        {
            errorString = exception.Message;
            return false;
        }
    }

    /// <summary>
    /// 获取某张表的某个属性
    /// </summary>
    private static string _GetTableProperty(string schemaName, string tableName, string propertyName, MySqlConnection conn)
    {
        MySqlCommand cmd = new MySqlCommand(string.Format(_SELECT_TABLE_INFO_SQL, propertyName, schemaName, tableName), conn);
        DataTable dt = _ExecuteSqlCommand(cmd);
        return dt.Rows.Count > 0 ? dt.Rows[0][0].ToString() : string.Empty;
    }

    /// <summary>
    /// 获取某张表的建表SQL
    /// </summary>
    private static string _GetCreateTableSql(string schemaName, string tableName, string targetSchemaName, MySqlConnection conn)
    {
        MySqlCommand cmd = new MySqlCommand(string.Format(_SHOW_CREATE_TABLE_SQL, _CombineDatabaseTableFullName(schemaName, tableName)), conn);
        DataTable dt = _ExecuteSqlCommand(cmd);
        string createTableSql = dt.Rows[0]["Create Table"].ToString();
        // MySQL提供功能返回的建表SQL不含Schema，这里自己加上
        int firstBracketIndex = createTableSql.IndexOf("(");
        return string.Format("CREATE TABLE {0} {1};", _CombineDatabaseTableFullName(targetSchemaName, tableName), createTableSql.Substring(firstBracketIndex));
    }

    /// <summary>
    /// 获取填充某表格所有数据的SQL
    /// </summary>
    private static string _GetFillDataSql(TableInfo tableInfo, DataTable data, string targetSchemaName)
    {
        // 处理空表的情况
        if (data.Rows.Count == 0)
            return null;

        // 生成所有列名组成的定义字符串
        List<string> columnDefine = new List<string>();
        foreach (string columnName in tableInfo.AllColumnInfo.Keys)
            columnDefine.Add(string.Format("`{0}`", columnName));

        string columnDefineString = Utils.CombineString(columnDefine, ", ");

        // 逐行生成插入数据的SQL语句中的value定义部分
        StringBuilder valueDefineStringBuilder = new StringBuilder();
        int rowCount = data.Rows.Count;
        const string SPLIT_STRING = ",\n";
        for (int row = 0; row < rowCount; ++row)
        {
            List<string> values = new List<string>();
            foreach (string columnName in tableInfo.AllColumnInfo.Keys)
            {
                object value = data.Rows[row][columnName];
                values.Add(_GetDatabaseValueString(value));
            }

            valueDefineStringBuilder.AppendFormat("({0}){1}", Utils.CombineString(values, ","), SPLIT_STRING);
        }
        // 去掉末尾多余的逗号和换行
        string valueDefineString = valueDefineStringBuilder.ToString();
        valueDefineString = valueDefineString.Substring(0, valueDefineString.Length - SPLIT_STRING.Length);

        return string.Format(_INSERT_DATA_SQL, _CombineDatabaseTableFullName(targetSchemaName, tableInfo.TableName), columnDefineString, string.Concat("\n", valueDefineString));
    }

    /// <summary>
    /// 获取某表格的索引设置
    /// </summary>
    private static Dictionary<string, List<string>> _GetIndexInfo(string schemaName, string tableName, MySqlConnection conn)
    {
        Dictionary<string, List<string>> indexInfo = new Dictionary<string, List<string>>();

        // MySQL的SHOW INDEX语句中无法使用ORDER BY，而List中没有前面的元素就无法在后面指定下标处插入数据，故用下面的数据结构进行整理，其中内层Dictionary的key为序号，value为列名
        Dictionary<string, Dictionary<int, string>> tempIndexInfo = new Dictionary<string, Dictionary<int, string>>();

        MySqlCommand cmd = new MySqlCommand(string.Format(_SHOW_INDEX_SQL, _CombineDatabaseTableFullName(schemaName, tableName)), conn);
        DataTable dt = _ExecuteSqlCommand(cmd);

        int count = dt.Rows.Count;
        for (int i = 0; i < count; ++i)
        {
            string name = dt.Rows[i]["Key_name"].ToString();
            string columnName = dt.Rows[i]["Column_name"].ToString();
            int seq = int.Parse(dt.Rows[i]["Seq_in_index"].ToString());
            if (!tempIndexInfo.ContainsKey(name))
                tempIndexInfo.Add(name, new Dictionary<int, string>());

            Dictionary<int, string> tempColumnNames = tempIndexInfo[name];
            tempColumnNames.Add(seq, columnName);
        }

        // 转为Dictionary<string, List<string>>数据结构
        foreach (var pair in tempIndexInfo)
        {
            string name = pair.Key;
            indexInfo.Add(name, new List<string>());
            List<string> columnNames = indexInfo[name];
            int columnCount = pair.Value.Count;
            for (int seq = 1; seq <= columnCount; ++seq)
                columnNames.Add(pair.Value[seq]);
        }

        return indexInfo;
    }

    /// <summary>
    /// 获取删除某张表的SQL
    /// </summary>
    private static string _GetDropTableSql(string schemaName, string tableName)
    {
        return string.Format(_DROP_TABLE_SQL, _CombineDatabaseTableFullName(schemaName, tableName));
    }

    /// <summary>
    /// 根据Select语句返回查询结果
    /// </summary>
    private static DataTable _SelectData(string schemaName, string tableName, string selectColumns, MySqlConnection conn)
    {
        MySqlCommand cmd = new MySqlCommand(string.Format(_SELECT_DATA_SQL, selectColumns, _CombineDatabaseTableFullName(schemaName, tableName)), conn);
        return _ExecuteSqlCommand(cmd);
    }

    /// <summary>
    /// 返回某张表格所有列属性
    /// </summary>
    private static DataTable _GetAllColumnInfo(string schemaName, string tableName, MySqlConnection conn)
    {
        MySqlCommand cmd = new MySqlCommand(string.Format(_SELECT_COLUMN_INFO_SQL, schemaName, tableName), conn);
        return _ExecuteSqlCommand(cmd);
    }

    /// <summary>
    /// 将某张表格的属性作为TableInfo类返回
    /// </summary>
    public static TableInfo GetTableInfo(string schemaName, string tableName, MySqlConnection conn)
    {
        TableInfo tableInfo = new TableInfo();
        // Schema名
        tableInfo.SchemaName = schemaName;
        // 表名
        tableInfo.TableName = tableName;
        // 表注释（注意转义注释中的换行）
        tableInfo.Comment = _GetTableProperty(schemaName, tableName, "TABLE_COMMENT", conn).Replace(System.Environment.NewLine, "\\n").Replace("\n", "\\n");
        // 表校对集
        tableInfo.Collation = _GetTableProperty(schemaName, tableName, "TABLE_COLLATION", conn);
        // 索引设置
        tableInfo.IndexInfo = _GetIndexInfo(schemaName, tableName, conn);
        // 列信息
        DataTable dtColumnInfo = _GetAllColumnInfo(schemaName, tableName, conn);
        if (dtColumnInfo != null)
        {
            int columnCount = dtColumnInfo.Rows.Count;
            for (int i = 0; i < columnCount; ++i)
            {
                ColumnInfo columnInfo = new ColumnInfo();
                // 表名
                columnInfo.TableName = tableName;
                // 列名
                columnInfo.ColumnName = dtColumnInfo.Rows[i]["COLUMN_NAME"].ToString();
                // 注释（注意转义注释中的换行）
                columnInfo.Comment = dtColumnInfo.Rows[i]["COLUMN_COMMENT"].ToString().Replace(System.Environment.NewLine, "\\n").Replace("\n", "\\n");
                // 数据类型（包含长度）
                columnInfo.DataType = dtColumnInfo.Rows[i]["COLUMN_TYPE"].ToString();
                // 属性
                string columnKey = dtColumnInfo.Rows[i]["COLUMN_KEY"].ToString();
                if (!string.IsNullOrEmpty(columnKey))
                {
                    if (columnKey.IndexOf("PRI", StringComparison.CurrentCultureIgnoreCase) != -1)
                    {
                        columnInfo.IsPrimaryKey = true;
                        tableInfo.PrimaryKeyColumnNames.Add(columnInfo.ColumnName);
                    }
                    if (columnKey.IndexOf("UNI", StringComparison.CurrentCultureIgnoreCase) != -1)
                        columnInfo.IsUnique = true;
                    if (columnKey.IndexOf("MUL", StringComparison.CurrentCultureIgnoreCase) != -1)
                        columnInfo.IsMultiple = true;
                }
                // 额外属性
                string extra = dtColumnInfo.Rows[i]["EXTRA"].ToString();
                if (!string.IsNullOrEmpty(extra))
                {
                    if (columnKey.IndexOf("auto_increment", StringComparison.CurrentCultureIgnoreCase) != -1)
                        columnInfo.IsAutoIncrement = true;
                }
                // 是否非空
                columnInfo.IsNotEmpty = dtColumnInfo.Rows[i]["IS_NULLABLE"].ToString().Equals("NO", StringComparison.CurrentCultureIgnoreCase);
                // 默认值
                object defaultValue = dtColumnInfo.Rows[i]["COLUMN_DEFAULT"];
                string defaultValueString = _GetDatabaseValueString(defaultValue);
                columnInfo.DefaultValue = defaultValueString;

                tableInfo.AllColumnInfo.Add(columnInfo.ColumnName, columnInfo);
            }
        }

        return tableInfo;
    }

    /// <summary>
    /// 执行指定SQL语句，返回执行结果
    /// </summary>
    private static DataTable _ExecuteSqlCommand(MySqlCommand cmd)
    {
        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
        DataTable dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    /// <summary>
    /// 对新旧两版本数据库进行对比，并展示结果以及生成的SQL
    /// </summary>
    public static void CompareAndShowResult(out string errorString)
    {
        StringBuilder errorStringBuilder = new StringBuilder();

        // 找出新版本中删除的表
        List<string> dropTableNames = new List<string>();
        foreach (string tableName in AppValues.OldExistTableNames)
        {
            if (!AppValues.NewExistTableNames.Contains(tableName))
                dropTableNames.Add(tableName);
        }
        if (dropTableNames.Count > 0)
        {
            Utils.AppendOutputText(string.Format("新版本数据库中删除以下表格：{0}\n", Utils.CombineString(dropTableNames, ",")), OutputType.Comment);
            foreach (string tableName in dropTableNames)
            {
                Utils.AppendOutputText(string.Format("生成删除{0}表的SQL\n", tableName), OutputType.Comment);
                if (AppValues.AllTableCompareRule.ContainsKey(tableName) && AppValues.AllTableCompareRule[tableName].CompareWay == TableCompareWays.Ignore)
                {
                    Utils.AppendOutputText("该表格配置为忽略比较，故不进行删除\n", OutputType.Warning);
                    continue;
                }
                string dropTableSql = _GetDropTableSql(AppValues.OldSchemaName, tableName);
                Utils.AppendOutputText(dropTableSql, OutputType.Sql);
                Utils.AppendOutputText("\n", OutputType.None);
            }
        }
        // 找出新版本中新增的表
        List<string> addTableNames = new List<string>();
        foreach (string tableName in AppValues.NewExistTableNames)
        {
            if (!AppValues.OldExistTableNames.Contains(tableName))
                addTableNames.Add(tableName);
        }
        if (addTableNames.Count > 0)
        {
            Utils.AppendOutputText(string.Format("新版本数据库中新增以下表格：{0}\n", Utils.CombineString(addTableNames, ",")), OutputType.Comment);
            foreach (string tableName in addTableNames)
            {
                Utils.AppendOutputText(string.Format("生成创建{0}表及填充数据的SQL\n", tableName), OutputType.Comment);
                if (AppValues.AllTableCompareRule.ContainsKey(tableName) && AppValues.AllTableCompareRule[tableName].CompareWay == TableCompareWays.Ignore)
                {
                    Utils.AppendOutputText("该表格配置为忽略比较，故不进行新建\n", OutputType.Warning);
                    continue;
                }
                // 通过MySQL提供的功能得到建表SQL
                string createTableSql = _GetCreateTableSql(AppValues.NewSchemaName, tableName, AppValues.OldSchemaName, AppValues.NewConn);
                Utils.AppendOutputText(createTableSql, OutputType.Sql);
                Utils.AppendOutputText("\n", OutputType.None);
                // 得到填充数据的SQL
                DataTable data = _SelectData(AppValues.NewSchemaName, tableName, "*", AppValues.NewConn);
                string fillDataSql = _GetFillDataSql(AppValues.NewTableInfo[tableName], data, AppValues.OldSchemaName);
                if (!string.IsNullOrEmpty(fillDataSql))
                {
                    Utils.AppendOutputText(fillDataSql, OutputType.Sql);
                    Utils.AppendOutputText("\n", OutputType.None);
                }
            }
        }
        // 对两版本中均存在的表格进行对比
        foreach (string tableName in AppValues.NewExistTableNames)
        {
            if (AppValues.OldExistTableNames.Contains(tableName))
            {
                Utils.AppendOutputText(string.Format("开始对比{0}表\n", tableName), OutputType.Comment);
                TableInfo newTableInfo = AppValues.NewTableInfo[tableName];
                TableInfo oldTableInfo = AppValues.OldTableInfo[tableName];

                TableCompareRule compareRule = null;
                if (AppValues.AllTableCompareRule.ContainsKey(tableName))
                {
                    compareRule = AppValues.AllTableCompareRule[tableName];
                    using (StringReader reader = new StringReader(compareRule.GetCompareRuleComment()))
                    {
                        string line = null;
                        while ((line = reader.ReadLine()) != null)
                        {
                            Utils.AppendOutputText(line, OutputType.Comment);
                            Utils.AppendOutputText("\n", OutputType.None);
                        }
                    }
                }
                else
                {
                    compareRule = new TableCompareRule();
                    compareRule.CompareWay = TableCompareWays.ColumnInfoAndData;
                    Utils.AppendOutputText("未设置对该表格进行对比的方式，将默认对比表结构及数据\n", OutputType.Warning);
                }

                // 进行表结构比较
                const string SPLIT_STRING = ",\n";
                bool isPrimaryKeySame = true;
                if (compareRule.CompareWay != TableCompareWays.Ignore)
                {
                    Utils.AppendOutputText("开始进行结构对比\n", OutputType.Comment);
                    // 修改表结构的SQL开头
                    string alterTableSqlPrefix = string.Format(_ALTER_TABLE_SQL, _CombineDatabaseTableFullName(AppValues.OldSchemaName, tableName));
                    // 标识是否输出过修改表结构的SQL开头
                    bool hasOutputPrefix = false;
                    // 标识是否输出过该对比部分中的第一条SQL，非第一条输出前先加逗号并换行
                    bool hasOutputPartFirstSql = false;

                    // 找出删除列
                    List<string> dropColumnNames = new List<string>();
                    foreach (string columnName in oldTableInfo.AllColumnInfo.Keys)
                    {
                        if (!newTableInfo.AllColumnInfo.ContainsKey(columnName))
                            dropColumnNames.Add(columnName);
                    }
                    if (dropColumnNames.Count > 0)
                    {
                        if (hasOutputPrefix == false)
                        {
                            Utils.AppendOutputText(alterTableSqlPrefix, OutputType.Sql);
                            Utils.AppendOutputText("\n", OutputType.None);
                            hasOutputPrefix = true;
                        }
                        // 如果之前对比出差异并进行了输出，需要先为上一条语句添加逗号结尾
                        if (hasOutputPartFirstSql == true)
                        {
                            Utils.AppendOutputText(SPLIT_STRING, OutputType.Sql);
                            hasOutputPartFirstSql = false;
                        }
                        Utils.AppendOutputText(string.Format("新版本中删除以下列：{0}\n", Utils.CombineString(dropColumnNames, ",")), OutputType.Comment);
                        foreach (string columnName in dropColumnNames)
                        {
                            if (hasOutputPartFirstSql == false)
                                hasOutputPartFirstSql = true;
                            else
                                Utils.AppendOutputText(SPLIT_STRING, OutputType.Sql);

                            string dropColumnSql = string.Format(_DROP_COLUMN_SQL, columnName);
                            Utils.AppendOutputText(dropColumnSql, OutputType.Sql);
                        }
                    }

                    // 找出新增列
                    List<string> addColumnNames = new List<string>();
                    foreach (string columnName in newTableInfo.AllColumnInfo.Keys)
                    {
                        if (!oldTableInfo.AllColumnInfo.ContainsKey(columnName))
                            addColumnNames.Add(columnName);
                    }
                    string addColumnNameString = Utils.CombineString(addColumnNames, ",");
                    if (addColumnNames.Count > 0)
                    {
                        if (hasOutputPrefix == false)
                        {
                            Utils.AppendOutputText(alterTableSqlPrefix, OutputType.Sql);
                            Utils.AppendOutputText("\n", OutputType.None);
                            hasOutputPrefix = true;
                        }
                        if (hasOutputPartFirstSql == true)
                        {
                            Utils.AppendOutputText(SPLIT_STRING, OutputType.Sql);
                            hasOutputPartFirstSql = false;
                        }
                        Utils.AppendOutputText(string.Format("新版本中新增以下列：{0}\n", Utils.CombineString(addColumnNames, ",")), OutputType.Comment);
                        foreach (string columnName in addColumnNames)
                        {
                            if (hasOutputPartFirstSql == false)
                                hasOutputPartFirstSql = true;
                            else
                                Utils.AppendOutputText(SPLIT_STRING, OutputType.Sql);

                            // 根据新增列属性生成添加新列的SQL
                            ColumnInfo columnInfo = newTableInfo.AllColumnInfo[columnName];
                            string notEmptyString = columnInfo.IsNotEmpty == true ? "NOT NULL" : "NULL";
                            // 注意如果列设为NOT NULL，就不允许设置默认值为NULL
                            string defaultValue = columnInfo.DefaultValue.Equals("NULL") ? string.Empty : string.Concat(" DEFAULT ", columnInfo.DefaultValue);
                            string addColumnSql = string.Format(_ADD_COLUMN_SQL, columnName, columnInfo.DataType, notEmptyString, defaultValue, columnInfo.Comment);
                            Utils.AppendOutputText(addColumnSql, OutputType.Sql);
                        }
                    }

                    // 在改变列属性前需先同步索引设置，因为自增属性仅可用于设置了索引的列
                    // 找出主键修改
                    isPrimaryKeySame = true;
                    if (newTableInfo.PrimaryKeyColumnNames.Count != oldTableInfo.PrimaryKeyColumnNames.Count)
                        isPrimaryKeySame = false;
                    else
                    {
                        foreach (string primaryKey in newTableInfo.PrimaryKeyColumnNames)
                        {
                            if (!oldTableInfo.PrimaryKeyColumnNames.Contains(primaryKey))
                            {
                                isPrimaryKeySame = false;
                                break;
                            }
                        }
                    }
                    if (isPrimaryKeySame == false)
                    {
                        if (hasOutputPrefix == false)
                        {
                            Utils.AppendOutputText(alterTableSqlPrefix, OutputType.Sql);
                            Utils.AppendOutputText("\n", OutputType.None);
                            hasOutputPrefix = true;
                        }
                        if (hasOutputPartFirstSql == true)
                        {
                            Utils.AppendOutputText(SPLIT_STRING, OutputType.Sql);
                            hasOutputPartFirstSql = false;
                        }
                        string newPrimaryKeyString = newTableInfo.PrimaryKeyColumnNames.Count > 0 ? Utils.CombineString(newTableInfo.PrimaryKeyColumnNames, ",") : "无";
                        string oldPrimaryKeyString = oldTableInfo.PrimaryKeyColumnNames.Count > 0 ? Utils.CombineString(oldTableInfo.PrimaryKeyColumnNames, ",") : "无";
                        Utils.AppendOutputText(string.Format("新版本中主键为：{0}，而旧版本中为：{1}\n", newPrimaryKeyString, oldPrimaryKeyString), OutputType.Comment);
                        // 先删除原有的主键设置
                        Utils.AppendOutputText(_DROP_PRIMARY_KEY_SQL, OutputType.Sql);
                        Utils.AppendOutputText(SPLIT_STRING, OutputType.Sql);
                        // 再重新设置
                        List<string> primaryKeyDefine = new List<string>();
                        foreach (string primaryKey in newTableInfo.PrimaryKeyColumnNames)
                            primaryKeyDefine.Add(string.Format("`{0}`", primaryKey));

                        string addPrimaryKeySql = string.Format(_ADD_PRIMARY_KEY_SQL, Utils.CombineString(primaryKeyDefine, ","));
                        Utils.AppendOutputText(addPrimaryKeySql, OutputType.Sql);
                        hasOutputPartFirstSql = true;
                    }

                    // 找出唯一索引修改
                    // 找出新版本中删除的索引
                    List<string> dropIndexNames = new List<string>();
                    foreach (string name in oldTableInfo.IndexInfo.Keys)
                    {
                        if (!newTableInfo.IndexInfo.ContainsKey(name))
                            dropIndexNames.Add(name);
                    }
                    if (dropIndexNames.Count > 0)
                    {
                        if (hasOutputPrefix == false)
                        {
                            Utils.AppendOutputText(alterTableSqlPrefix, OutputType.Sql);
                            Utils.AppendOutputText("\n", OutputType.None);
                            hasOutputPrefix = true;
                        }
                        if (hasOutputPartFirstSql == true)
                        {
                            Utils.AppendOutputText(SPLIT_STRING, OutputType.Sql);
                            hasOutputPartFirstSql = false;
                        }
                        Utils.AppendOutputText(string.Format("新版本中删除以下索引：{0}\n", Utils.CombineString(dropIndexNames, ",")), OutputType.Comment);
                        foreach (string name in dropIndexNames)
                        {
                            if (hasOutputPartFirstSql == false)
                                hasOutputPartFirstSql = true;
                            else
                                Utils.AppendOutputText(SPLIT_STRING, OutputType.Sql);

                            string dropIndexSql = string.Format(_DROP_INDEX_SQL, name);
                            Utils.AppendOutputText(dropIndexSql, OutputType.Sql);
                        }
                    }
                    // 找出新版本中新增索引
                    List<string> addIndexNames = new List<string>();
                    foreach (string name in newTableInfo.IndexInfo.Keys)
                    {
                        if (!oldTableInfo.IndexInfo.ContainsKey(name))
                            addIndexNames.Add(name);
                    }
                    if (addIndexNames.Count > 0)
                    {
                        if (hasOutputPrefix == false)
                        {
                            Utils.AppendOutputText(alterTableSqlPrefix, OutputType.Sql);
                            Utils.AppendOutputText("\n", OutputType.None);
                            hasOutputPrefix = true;
                        }
                        if (hasOutputPartFirstSql == true)
                        {
                            Utils.AppendOutputText(SPLIT_STRING, OutputType.Sql);
                            hasOutputPartFirstSql = false;
                        }
                        Utils.AppendOutputText(string.Format("新版本中新增以下索引：{0}\n", Utils.CombineString(addIndexNames, ",")), OutputType.Comment);
                        foreach (string name in addIndexNames)
                        {
                            if (hasOutputPartFirstSql == false)
                                hasOutputPartFirstSql = true;
                            else
                                Utils.AppendOutputText(SPLIT_STRING, OutputType.Sql);

                            // 根据新增索引属性生成添加新索引的SQL
                            // 注意列名后必须声明排序方式，MySQL只支持索引的升序排列
                            List<string> columnDefine = new List<string>();
                            foreach (string columnName in newTableInfo.IndexInfo[name])
                                columnDefine.Add(string.Format("`{0}` ASC", columnName));

                            string addIndexSql = string.Format(_ADD_UNIQUE_INDEX_SQL, name, Utils.CombineString(columnDefine, ","));
                            Utils.AppendOutputText(addIndexSql, OutputType.Sql);
                        }
                    }
                    // 找出同名索引的变动
                    foreach (var pair in newTableInfo.IndexInfo)
                    {
                        string name = pair.Key;
                        if (oldTableInfo.IndexInfo.ContainsKey(name))
                        {
                            List<string> newIndexColumnInfo = pair.Value;
                            List<string> oldIndexColumnInfo = oldTableInfo.IndexInfo[name];
                            bool isIndexColumnSame = true;
                            if (newIndexColumnInfo.Count != oldIndexColumnInfo.Count)
                                isIndexColumnSame = false;
                            else
                            {
                                int count = newIndexColumnInfo.Count;
                                for (int i = 0; i < count; ++i)
                                {
                                    if (!newIndexColumnInfo[i].Equals(oldIndexColumnInfo[i]))
                                    {
                                        isIndexColumnSame = false;
                                        break;
                                    }
                                }
                            }

                            if (isIndexColumnSame == false)
                            {
                                if (hasOutputPrefix == false)
                                {
                                    Utils.AppendOutputText(alterTableSqlPrefix, OutputType.Sql);
                                    Utils.AppendOutputText("\n", OutputType.None);
                                    hasOutputPrefix = true;
                                }
                                if (hasOutputPartFirstSql == true)
                                {
                                    Utils.AppendOutputText(SPLIT_STRING, OutputType.Sql);
                                    hasOutputPartFirstSql = false;
                                }
                                Utils.AppendOutputText(string.Format("新版本中名为{0}的索引，涉及的列名为{1}，而旧版本中为{2}\n", name, Utils.CombineString(newIndexColumnInfo, ","), Utils.CombineString(oldIndexColumnInfo, ",")), OutputType.Comment);
                                // 先删除
                                string dropIndexSql = string.Format(_DROP_INDEX_SQL, name);
                                Utils.AppendOutputText(dropIndexSql, OutputType.Sql);
                                Utils.AppendOutputText(SPLIT_STRING, OutputType.Sql);
                                // 再重新创建
                                List<string> columnDefine = new List<string>();
                                foreach (string columnName in newIndexColumnInfo)
                                    columnDefine.Add(string.Format("`{0}` ASC", columnName));

                                string addIndexSql = string.Format(_ADD_UNIQUE_INDEX_SQL, name, Utils.CombineString(columnDefine, ","));
                                Utils.AppendOutputText(addIndexSql, OutputType.Sql);
                                hasOutputPartFirstSql = true;
                            }
                        }
                    }

                    // 找出列属性修改
                    foreach (string columnName in newTableInfo.AllColumnInfo.Keys)
                    {
                        if (oldTableInfo.AllColumnInfo.ContainsKey(columnName))
                        {
                            ColumnInfo newColumnInfo = newTableInfo.AllColumnInfo[columnName];
                            ColumnInfo oldColumnInfo = oldTableInfo.AllColumnInfo[columnName];
                            // 比较各个属性
                            bool isDataTypeSame = newColumnInfo.DataType.Equals(oldColumnInfo.DataType);
                            bool isCommentSame = newColumnInfo.Comment.Equals(oldColumnInfo.Comment);
                            bool isNotEmptySame = newColumnInfo.IsNotEmpty == oldColumnInfo.IsNotEmpty;
                            bool isAutoIncrementSame = newColumnInfo.IsAutoIncrement == oldColumnInfo.IsAutoIncrement;
                            bool isDefaultValueSame = newColumnInfo.DefaultValue.Equals(oldColumnInfo.DefaultValue);
                            if (isDataTypeSame == false || isCommentSame == false || isNotEmptySame == false || isAutoIncrementSame == false || isDefaultValueSame == false)
                            {
                                if (hasOutputPrefix == false)
                                {
                                    Utils.AppendOutputText(alterTableSqlPrefix, OutputType.Sql);
                                    Utils.AppendOutputText("\n", OutputType.None);
                                    hasOutputPrefix = true;
                                }
                                if (hasOutputPartFirstSql == true)
                                {
                                    Utils.AppendOutputText(SPLIT_STRING, OutputType.Sql);
                                    hasOutputPartFirstSql = false;
                                }
                                Utils.AppendOutputText(string.Format("列{0}中存在以下属性不同：\n", columnName), OutputType.Comment);
                                if (isDataTypeSame == false)
                                    Utils.AppendOutputText(string.Format("新版本中数据类型为{0}，而旧版本中为{1}\n", newColumnInfo.DataType, oldColumnInfo.DataType), OutputType.Comment);
                                if (isCommentSame == false)
                                    Utils.AppendOutputText(string.Format("新版本中列注释为\"{0}\"，而旧版本中为\"{1}\"\n", newColumnInfo.Comment, oldColumnInfo.Comment), OutputType.Comment);
                                if (isNotEmptySame == false)
                                    Utils.AppendOutputText(string.Format("新版本中数据{0}为空，而旧版本中{1}\n", newColumnInfo.IsNotEmpty == true ? "不允许" : "允许", oldColumnInfo.IsNotEmpty == true ? "不允许" : "允许"), OutputType.Comment);
                                if (isAutoIncrementSame == false)
                                    Utils.AppendOutputText(string.Format("新版本中列设为{0}，而旧版本中为{1}\n", newColumnInfo.IsAutoIncrement == true ? "自增" : "不自增", oldColumnInfo.IsAutoIncrement == true ? "自增" : "不自增"), OutputType.Comment);
                                if (isDefaultValueSame == false)
                                    Utils.AppendOutputText(string.Format("新版本中默认值为{0}，而旧版本中为{1}\n", newColumnInfo.DefaultValue, oldColumnInfo.DefaultValue), OutputType.Comment);

                                // 根据新的列属性进行修改
                                string notEmptyString = newColumnInfo.IsNotEmpty == true ? "NOT NULL" : "NULL";
                                string defaultValue = newColumnInfo.DefaultValue.Equals("NULL") ? string.Empty : string.Concat(" DEFAULT ", newColumnInfo.DefaultValue);
                                string changeColumnSql = string.Format(_CHANGE_COLUMN_SQL, columnName, newColumnInfo.DataType, notEmptyString, defaultValue, newColumnInfo.Comment);
                                Utils.AppendOutputText(changeColumnSql, OutputType.Sql);
                                hasOutputPartFirstSql = true;
                            }
                        }
                    }

                    // 对比表校对集
                    if (!newTableInfo.Collation.Equals(oldTableInfo.Collation))
                    {
                        if (hasOutputPrefix == false)
                        {
                            Utils.AppendOutputText(alterTableSqlPrefix, OutputType.Sql);
                            Utils.AppendOutputText("\n", OutputType.None);
                            hasOutputPrefix = true;
                        }
                        if (hasOutputPartFirstSql == true)
                        {
                            Utils.AppendOutputText(SPLIT_STRING, OutputType.Sql);
                            hasOutputPartFirstSql = false;
                        }
                        Utils.AppendOutputText(string.Format("新版本中该表格校对集为：\"{0}\"，而旧版本中为\"{1}\"\n", newTableInfo.Collation, oldTableInfo.Collation), OutputType.Comment);
                        string alterTableComment = string.Format(_ALTER_TABLE_COLLATION_SQL, newTableInfo.Collation);
                        Utils.AppendOutputText(alterTableComment, OutputType.Sql);
                        hasOutputPartFirstSql = true;
                    }

                    // 对比表注释
                    if (!newTableInfo.Comment.Equals(oldTableInfo.Comment))
                    {
                        if (hasOutputPrefix == false)
                        {
                            Utils.AppendOutputText(alterTableSqlPrefix, OutputType.Sql);
                            Utils.AppendOutputText("\n", OutputType.None);
                            hasOutputPrefix = true;
                        }
                        if (hasOutputPartFirstSql == true)
                        {
                            Utils.AppendOutputText(SPLIT_STRING, OutputType.Sql);
                            hasOutputPartFirstSql = false;
                        }
                        Utils.AppendOutputText(string.Format("新版本中该表格注释为：\"{0}\"，而旧版本中为\"{1}\"\n", newTableInfo.Comment, oldTableInfo.Comment), OutputType.Comment);
                        string alterTableComment = string.Format(_ALTER_TABLE_COMMENT_SQL, newTableInfo.Comment);
                        Utils.AppendOutputText(alterTableComment, OutputType.Sql);
                        hasOutputPartFirstSql = true;
                    }

                    // 最后添加分号结束
                    if (hasOutputPartFirstSql == true)
                    {
                        Utils.AppendOutputText(";\n", OutputType.Sql);
                        hasOutputPrefix = false;
                        hasOutputPartFirstSql = false;
                    }

                    // 进行表数据比较
                    if (compareRule.CompareWay == TableCompareWays.ColumnInfoAndData)
                    {
                        Utils.AppendOutputText("开始进行数据对比\n", OutputType.Comment);

                        // 检查表格是否设置了主键，本工具生成的同步数据的SQL需要通过主键确定数据行
                        if (newTableInfo.PrimaryKeyColumnNames.Count == 0)
                        {
                            string tips = string.Format("错误：表格\"{0}\"未设置主键，本工具无法通过主键生成定位并更新数据的SQL，请设置主键后重试\n本次操作被迫中止\n", tableName);
                            Utils.AppendOutputText(tips, OutputType.Error);
                            errorStringBuilder.Append(tips);
                            errorString = errorStringBuilder.ToString();
                            return;
                        }
                        // 检查用户设置的对比配置，不允许将主键列设为数据比较时忽略的列
                        if (compareRule.CompareIgnoreColumn.Count > 0)
                        {
                            foreach (string primaryKeyColumnName in newTableInfo.PrimaryKeyColumnNames)
                            {
                                if (compareRule.CompareIgnoreColumn.Contains(primaryKeyColumnName))
                                {
                                    string tips = string.Format("\n错误：对比数据时不允许将表格主键列设为忽略，而您的配置声明对表格\"{0}\"的主键列\"{1}\"进行忽略，请修正配置后重试\n本次操作被迫中止\n", tableName, primaryKeyColumnName);
                                    Utils.AppendOutputText(tips, OutputType.Error);
                                    errorStringBuilder.Append(tips);
                                    errorString = errorStringBuilder.ToString();
                                    return;
                                }
                            }
                        }
                        // 如果新旧版本中的主键设置不同，无法进行数据对比
                        if (isPrimaryKeySame == false)
                        {
                            string tips = string.Format("新旧两版本表格\"{0}\"的主键设置不同，本工具目前无法在此情况下进行数据比较并生成同步SQL，请先通过执行上面生成的同步数据库表结构SQL，使得旧版表格和新版为相同的主键设置后，再次运行本工具进行数据比较及同步\n", tableName);
                            Utils.AppendOutputText(tips, OutputType.Error);
                            errorStringBuilder.Append(tips);
                            continue;
                        }

                        DataTable newData = _SelectData(AppValues.NewSchemaName, tableName, "*", AppValues.NewConn);
                        DataTable oldData = _SelectData(AppValues.OldSchemaName, tableName, "*", AppValues.OldConn);
                        Dictionary<string, int> newDataInfo = _GetDataInfoByPrimaryKey(newData, newTableInfo.PrimaryKeyColumnNames);
                        Dictionary<string, int> oldDataInfo = _GetDataInfoByPrimaryKey(oldData, newTableInfo.PrimaryKeyColumnNames);

                        // 找出删除的数据
                        foreach (var pair in oldDataInfo)
                        {
                            string primaryKeyValueString = pair.Key;
                            int index = pair.Value;
                            if (!newDataInfo.ContainsKey(primaryKeyValueString))
                            {
                                DataRow dataRow = oldData.Rows[index];
                                string primaryKeyColumnNameAndValueString = _GetColumnNameAndValueString(dataRow, newTableInfo.PrimaryKeyColumnNames, " AND ");
                                Utils.AppendOutputText(string.Concat("新版本中删除了主键列为以下值的一行：", primaryKeyColumnNameAndValueString, "\n"), OutputType.Comment);
                                // 判断该行数据是否被设为忽略
                                //if (_IsIgnoreData(compareRule.CompareIgnoreData, dataRow) == true)
                                //    Utils.AppendOutputText("该行符合配置的需忽略的数据行，故不进行删除\n", OutputType.Warning);
                                //else
                                //{
                                string dropDataSql = string.Format(_DROP_DATA_SQL, _CombineDatabaseTableFullName(AppValues.OldSchemaName, tableName), _GetColumnNameAndValueString(dataRow, newTableInfo.PrimaryKeyColumnNames, " AND "));
                                Utils.AppendOutputText(dropDataSql, OutputType.Sql);
                                Utils.AppendOutputText("\n", OutputType.None);
                                //}
                            }
                        }

                        // 找出需要对比数据的列（以新版表中所有列为基准，排除用户设置的忽略列以及旧版表中不存在的列，因为主键列的值在找新旧两表对应行时已经对比，也无需比较）
                        List<string> compareColumnNames = new List<string>();
                        foreach (string columnName in newTableInfo.AllColumnInfo.Keys)
                        {
                            if (oldTableInfo.AllColumnInfo.ContainsKey(columnName) && !newTableInfo.PrimaryKeyColumnNames.Contains(columnName) && !compareRule.CompareIgnoreColumn.Contains(columnName))
                                compareColumnNames.Add(columnName);
                        }

                        // 生成新增数据时所有列名组成的定义字符串
                        List<string> columnDefine = new List<string>();
                        foreach (string columnName in newTableInfo.AllColumnInfo.Keys)
                            columnDefine.Add(string.Format("`{0}`", columnName));

                        string columnDefineString = Utils.CombineString(columnDefine, ", ");

                        foreach (var pair in newDataInfo)
                        {
                            string primaryKeyValueString = pair.Key;
                            int newTableIndex = pair.Value;
                            // 新增数据
                            if (!oldDataInfo.ContainsKey(primaryKeyValueString))
                            {
                                DataRow dataRow = newData.Rows[newTableIndex];
                                string primaryKeyColumnNameAndValueString = _GetColumnNameAndValueString(dataRow, newTableInfo.PrimaryKeyColumnNames, " AND ");
                                Utils.AppendOutputText(string.Concat("新版本中新增主键列为以下值的一行：", primaryKeyColumnNameAndValueString, "\n"), OutputType.Comment);
                                //// 判断该行数据是否被设为忽略
                                //if (_IsIgnoreData(compareRule.CompareIgnoreData, dataRow) == true)
                                //    Utils.AppendOutputText("该行符合配置的需忽略的数据行，故不进行新增\n", OutputType.Warning);
                                //else
                                //{
                                List<string> values = new List<string>();
                                foreach (string columnName in newTableInfo.AllColumnInfo.Keys)
                                {
                                    object value = dataRow[columnName];
                                    values.Add(_GetDatabaseValueString(value));
                                }

                                string valueString = string.Format("({0})", Utils.CombineString(values, ","));
                                string insertDataSql = string.Format(_INSERT_DATA_SQL, _CombineDatabaseTableFullName(AppValues.OldSchemaName, tableName), columnDefineString, valueString);
                                Utils.AppendOutputText(insertDataSql, OutputType.Sql);
                                Utils.AppendOutputText("\n", OutputType.None);
                                //}
                            }
                            // 判断未被忽略的列中的数据是否相同
                            else
                            {
                                int oldTableIndex = oldDataInfo[primaryKeyValueString];
                                DataRow newDataRow = newData.Rows[newTableIndex];
                                DataRow oldDataRow = oldData.Rows[oldTableIndex];

                                List<string> dataDiffColumnNames = new List<string>();
                                foreach (string columnName in compareColumnNames)
                                {
                                    string newDataValue = _GetDatabaseValueString(newDataRow[columnName]);
                                    string oldDataValue = _GetDatabaseValueString(oldDataRow[columnName]);
                                    if (!newDataValue.Equals(oldDataValue))
                                        dataDiffColumnNames.Add(columnName);
                                }
                                string primaryKeyColumnNameAndValueString = _GetColumnNameAndValueString(newDataRow, newTableInfo.PrimaryKeyColumnNames, " AND ");
                                if (dataDiffColumnNames.Count > 0)
                                {
                                    string newColumnNameAndValueString = _GetColumnNameAndValueString(newDataRow, dataDiffColumnNames, ", ");
                                    string oldColumnNameAndValueString = _GetColumnNameAndValueString(oldDataRow, dataDiffColumnNames, ", ");
                                    Utils.AppendOutputText(string.Format("主键为{0}的行中，新版本中以下数据为{1}，而旧版本中为{2}\n", primaryKeyColumnNameAndValueString, newColumnNameAndValueString, oldColumnNameAndValueString), OutputType.Comment);
                                    // 判断该行数据是否被设为忽略
                                    if (_IsIgnoreData(compareRule.CompareIgnoreData, newDataRow) == true)
                                        Utils.AppendOutputText("该行符合配置的需忽略的数据行，故不进行修改\n", OutputType.Warning);
                                    else
                                    {
                                        List<string> values = new List<string>();
                                        foreach (string columnName in newTableInfo.AllColumnInfo.Keys)
                                        {
                                            object value = newDataRow[columnName];
                                            values.Add(_GetDatabaseValueString(value));
                                        }
                                        string valueString = string.Format("({0})", Utils.CombineString(values, ","));
                                        string updateDataSql = string.Format(_UPDATE_DATA_SQL, _CombineDatabaseTableFullName(AppValues.OldSchemaName, tableName), newColumnNameAndValueString, primaryKeyColumnNameAndValueString);
                                        Utils.AppendOutputText(updateDataSql, OutputType.Sql);
                                        Utils.AppendOutputText("\n", OutputType.None);
                                    }
                                }

                                // 新版表格中新增列的值需要同步到旧表，无视用户是否设置为忽略列
                                if (addColumnNames.Count > 0)
                                {
                                    string addColumnNameAndValueString = _GetColumnNameAndValueString(newDataRow, addColumnNames, ", ");
                                    Utils.AppendOutputText(string.Format("为新版本中新增的{0}列填充数据\n", addColumnNameString), OutputType.Comment);
                                    string updateDataSql = string.Format(_UPDATE_DATA_SQL, _CombineDatabaseTableFullName(AppValues.OldSchemaName, tableName), addColumnNameAndValueString, primaryKeyColumnNameAndValueString);
                                    Utils.AppendOutputText(updateDataSql, OutputType.Sql);
                                    Utils.AppendOutputText("\n", OutputType.None);
                                }
                            }
                        }
                    }
                }
            }
        }

        errorString = errorStringBuilder.ToString();
    }

    /// <summary>
    /// 将数据整理为以各个主键值拼成key的Dictionary，value为数据在DataTable中的下标
    /// </summary>
    private static Dictionary<string, int> _GetDataInfoByPrimaryKey(DataTable data, List<string> primaryKeyColumnNames)
    {
        const string SPLIT_STRING = "_";
        Dictionary<string, int> info = new Dictionary<string, int>();
        List<string> tempColumnKeys = new List<string>();
        int count = data.Rows.Count;
        for (int i = 0; i < count; ++i)
        {
            string primaryKeyValueString = _GetPrimaryKeyValueString(data.Rows[i], primaryKeyColumnNames, SPLIT_STRING);
            info.Add(primaryKeyValueString, i);
        }

        return info;
    }

    /// <summary>
    /// 将某表格一行数据中的主键列值通过指定字符连接
    /// </summary>
    private static string _GetPrimaryKeyValueString(DataRow dataRow, List<string> primaryKeyColumnNames, string splitString)
    {
        List<string> tempValues = new List<string>();
        foreach (string columnName in primaryKeyColumnNames)
            tempValues.Add(dataRow[columnName].ToString());

        return Utils.CombineString(tempValues, splitString);
    }

    /// <summary>
    /// 返回某表格一行数据中的指定列名及取值，符合WHERE语句要求
    /// </summary>
    private static string _GetColumnNameAndValueString(DataRow dataRow, List<string> columnNames, string splitString)
    {
        List<string> tempValues = new List<string>();
        foreach (string columnName in columnNames)
        {
            object value = dataRow[columnName];
            string valueString = _GetDatabaseValueString(value);

            tempValues.Add(string.Format("`{0}`={1}", columnName, valueString));
        }

        return Utils.CombineString(tempValues, splitString);
    }

    /// <summary>
    /// 判断某表格中的一行是否为配置的需要忽略比较的数据
    /// </summary>
    private static bool _IsIgnoreData(List<Dictionary<string, Regex>> compareIgnoreData, DataRow dataRow)
    {
        DataColumnCollection dataColumnCollection = dataRow.Table.Columns;
        int count = compareIgnoreData.Count;
        for (int i = 0; i < count; ++i)
        {
            Dictionary<string, Regex> oneIgnoreData = compareIgnoreData[i];
            bool isMatch = true;
            foreach (string columnName in oneIgnoreData.Keys)
            {
                if (!dataColumnCollection.Contains(columnName))
                {
                    isMatch = false;
                    break;
                }

                Regex regex = oneIgnoreData[columnName];
                object value = dataRow[columnName];
                string valueString = null;
                if (value.GetType() == typeof(System.Boolean))
                {
                    if ((bool)value == true)
                        valueString = "1";
                    else
                        valueString = "0";
                }
                else
                    valueString = value.ToString();

                if (!regex.IsMatch(valueString))
                {
                    isMatch = false;
                    break;
                }
            }

            if (isMatch == true)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 获取数据库中一个数据在SQL语句中的表示形式
    /// </summary>
    private static string _GetDatabaseValueString(object value)
    {
        if (value.GetType() == typeof(System.DBNull))
            return "NULL";
        else if (value.GetType() == typeof(System.Boolean))
        {
            if ((bool)value == true)
                return "\"1\"";
            else
                return "\"0\"";
        }
        // MySQL中string类型的空字符串，若用单引号包裹则认为是NULL，用双引号包裹才认为是空字符串。还要注意转义数据中的引号
        else
            return string.Concat("\"", value.ToString().Replace("\"", "\\\""), "\"");
    }

    /// <summary>
    /// 将数据库的表名连同Schema名组成形如'schemaName'.'tableName'的字符串
    /// </summary>
    private static string _CombineDatabaseTableFullName(string schemaName, string tableName)
    {
        return string.Format("`{0}`.`{1}`", schemaName, tableName);
    }
}
