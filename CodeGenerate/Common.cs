using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _5sPowerCodeGenerate
{
    public class Common
    {
        #region 返回C#实体数据类型
        public static string findModelsType(string name)
        {
            if (name == "uniqueidentifier")
            {
                return "Guid";
            }
            if (name == "int")
            {
                return "int";
            }
            else if (name == "smallint")
            {
                return "short";
            }
            else if (name == "bigint")
            {
                return "long";
            }
            else if (name == "tinyint")
            {
                return "byte";
            }
            else if (name == "image")
            {
                return "byte[]";
            }
            else if (name == "numeric" || name == "real")
            {
                return "Single";
            }
            else if (name == "float")
            {
                return "float";
            }
            else if (name == "decimal")
            {
                return "decimal";
            }
            else if (name == "char" || name == "varchar" || name == "text" || name == "nchar" || name == "nvarchar" || name == "ntext")
            {
                return "string";
            }
            else if (name == "bit")
            {
                return "bool";
            }
            else if (name == "datetime" || name == "smalldatetime" || name == "date" || name == "datetime2")
            {
                return "DateTime";
            }
            else if (name == "money" || name == "smallmoney")
            {
                return "double";
            }
            else if (name == "datetimeoffset")
            {
                return "DateTimeOffset";
            }
            else if (name == "timestamp")
            {
                return "byte[]";
            }
            else
            {
                return "string";
            }
        }
        #endregion

        #region 返回数据库类型
        public static bool isString(string type)
        {
            bool b = false;
            if (type == "varchar" || type == "char" || type == "nvarchar" || type == "nchar")
            {
                b = true;
            }
            return b;
        }
        #endregion

        #region 返回C#数据类型
        public static string findType(string name)
        {
            if (name == "int" || name == "smallint")
            {
                return "Convert.ToInt32(";
            }
            else if (name == "tinyint")
            {
                return "Convert.ToByte(";
            }
            else if (name == "numeric" || name == "real" || name == "float")
            {
                return "Convert.ToSingle(";
            }
            else if (name == "decimal")
            {
                return "Convert.ToDecimal(";
            }
            else if (name == "char" || name == "varchar" || name == "text" || name == "nchar" || name == "nvarchar" || name == "ntext")
            {
                return ".ToString()";
            }
            else if (name == "bit")
            {
                return "Convert.ToBoolean(";
            }
            else if (name == "datetime" || name == "smalldatetime")
            {
                return "Convert.ToDateTime(";
            }
            else if (name == "money" || name == "smallmoney")
            {
                return "Convert.ToDouble(";
            }
            else
            {
                return ".ToString()";
            }
        }
        #endregion

        #region 转换空格
        public static string createplace(int a)
        {
            return new string(' ', a);
        }
        #endregion

        #region 首字母大写
        public static string fristToUpper(string name)
        {
            name = name.Substring(0, 1).ToUpper() + name.Substring(1);
            return name;
        }
        #endregion

        #region 首字母小写
        public static string fristToLower(string name)
        {
            name = name.Substring(0, 1).ToLower() + name.Substring(1);
            return name;
        }

        public static string GetIsNullable(string type, bool isNull)
        {
            if (type == "bool" || type == "int" || type == "Guid" || type == "short" || type == "long" ||
                type == "DateTime" || type == "double" || type == "decimal" || type == "DateTimeOffset"
                || type == "timestamp")
            {
                if (isNull)
                    return "?";
                else
                    return "";
            }
            else
                return "";
        }
        #endregion
    }
}
