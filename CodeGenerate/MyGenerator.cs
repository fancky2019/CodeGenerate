using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;
using System.Threading;
using CodeGenerator;

namespace CodeGenerator
{
    public partial class MyGeneratorFrm : Form
    {
        string connectstring;   //数据库连接字符串
        SqlConnection cn;       //数据库连接对象
        List<ProcedureInfo> list = new List<ProcedureInfo>();   //表对象
        public List<string> Allli = new List<string>();         //所有表
        private List<string> _selectedTableNames = new List<string>();            //所关联的表
        string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);  //默认桌面路径
        string userId = "";        //用户名
        string password = "";      //密码
        string database = "master";//数据库
        string table = "";         //当前表名
        public MyGeneratorFrm()
        {
            InitializeComponent();
        }

        #region 连接测试
        /// <summary>
        /// 连接测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            groupBox2.Enabled = true;

            database = textBox1.Text;
            userId = textBox2.Text;
            password = textBox3.Text;

            this.comboBox1.Items.Clear();


            string sql = string.Format("select name from sys.databases");
            if (checkBox1.Checked)
            {
                connectstring = string.Format(" data source={0}; initial catalog=master;integrated security=sspi", database);
            }
            else
            {
                connectstring = string.Format(" data source={0}; initial catalog=master;user id={1};password={2}", database, userId, password);
            }
            cn = new SqlConnection(connectstring);
            try
            {

                cn.Open();
                SqlCommand cm = new SqlCommand(sql, cn);
                using (SqlDataReader dr = cm.ExecuteReader(CommandBehavior.CloseConnection))
                {

                    while (dr.Read())
                    {
                        comboBox1.Items.Add(dr[0].ToString());
                        Allli.Add(dr[0].ToString());
                    }
                }
            }
            catch
            {
                groupBox2.Enabled = false;
                MessageBox.Show("用户登录失败（请检查用户名和密码）");
            }
            this.comboBox1.SelectedIndex = this.comboBox1.Items.IndexOf("master");
        }
        #endregion

        #region 集成验证
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox2.Enabled = textBox3.Enabled = !checkBox1.Checked;
        }
        #endregion

        #region 选择表
        private void btnSelectTable_Click(object sender, EventArgs e)
        {
            cn.Open();
            cn.ChangeDatabase(comboBox1.Text);
            SqlCommand cm = new SqlCommand("select name from sys.Tables", cn);
            SqlDataReader dr = cm.ExecuteReader();
            _selectedTableNames.Clear();
            while (dr.Read())
            {
                _selectedTableNames.Add(dr[0].ToString());

            }
            dr.Close();
            cn.Close();
            SelectTabelFrm selectTabelFrm = new SelectTabelFrm(_selectedTableNames);

            selectTabelFrm.FormClosing += (s,ee)=>
            {
                this._selectedTableNames = selectTabelFrm.SelectedTableNames;
            };

            selectTabelFrm.ShowDialog();

        }

    
        #endregion

        #region 选择数据库
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            database = comboBox1.Text;
            try
            {
                cn.Open();
                cn.ChangeDatabase(comboBox1.Text);
                _selectedTableNames.Clear();
                SqlCommand cm = new SqlCommand("select name from sys.Tables", cn);
                using (SqlDataReader dr = cm.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (dr.Read())
                    {
                        _selectedTableNames.Add(dr[0].ToString());
                    }
                }
            }
            catch { MessageBox.Show("该数据库不可操作 请尝试关闭重试"); cn.Close(); }

            finally
            {

            }
        }
        #endregion

        #region 选择路径
        /// <summary>
        /// 选择生成路径 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog open = new FolderBrowserDialog();

            if (open.ShowDialog() == DialogResult.OK)
            {
                path = open.SelectedPath;
                this.textBox4.Text = path;
            }
        }
        #endregion

        #region 生成
        /// <summary>
        /// 生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            this.chenggong.Visible = false;
            this.timer1.Enabled = false;
            GenerateCode("table");
            this.chenggong.Visible = true;
            this.timer1.Enabled = true;
        }
        /// <summary>
        /// 按sql生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            _selectedTableNames.Clear();
            if (string.IsNullOrEmpty(textBox6.Text))
            {
                MessageBox.Show("请输入SQL查询语句");
                return;
            }
            if (string.IsNullOrEmpty(textBox5.Text))
            {
                MessageBox.Show("请输入要生成的文件名");
                return;
            }

            this.chenggong.Visible = false;
            this.timer1.Enabled = false;
            GenerateCode("sql");

            this.timer1.Enabled = true;
        }
        #endregion

        #region 生成方法
        public void GenerateCode(string type)
        {
            try
            {
                int count = 0;
                if (checkBox2.Checked)
                {
                    count++;
                    if (!Directory.Exists(path + @"\ViewModel"))
                    {
                        Directory.CreateDirectory(path + @"\ViewModel");
                    }
                }
                if (checkBox3.Checked)
                {
                    count++;
                    if (!Directory.Exists(path + @"\ApiModel"))
                    {
                        Directory.CreateDirectory(path + @"\ApiModel");
                    }
                }
                if (checkBox4.Checked)
                {
                    count++;
                    if (!Directory.Exists(path + @"\DataModel"))
                    {
                        Directory.CreateDirectory(path + @"\DataModel");
                    }
                }

                progressBar1.Value = 0;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = _selectedTableNames.Count;

                if (type != "sql")
                {
                    foreach (string tablename in _selectedTableNames)
                    {
                        string sql = "SELECT col.name AS 列名, typ.name as 数据类型,col.max_length AS 占用字节数," +
                               "  col.is_nullable  AS 是否允许非空,col.is_identity  AS 是否自增," +
                               " case when exists  ( SELECT 1  FROM sys.indexes idx join sys.index_columns idxCol on (idx.object_id = idxCol.object_id)" +
                               "  WHERE idx.object_id = col.object_id AND idxCol.index_column_id = col.column_id AND idx.is_primary_key = 1" +
                               " ) THEN 1 ELSE 0 END  AS 是否是主键,isnull(g.[value],'') AS 说明 FROM sys.columns col left join sys.types typ on (col.system_type_id = typ.system_type_id AND col.user_type_id = typ.user_type_id)" +
                               " left join sys.extended_properties g on (col.object_id = g.major_id AND g.minor_id = col.column_id)" +
                               " WHERE col.object_id =(SELECT object_id FROM sys.tables WHERE name = '" + tablename + "')" +
                               "select t.name as 表名,c.Name as 列名,type1.name as 类型,c.length as 长度,col.is_nullable  AS 是否允许非空,col.is_identity  AS 是否自增 from sysColumns c,sys.tables t,sys.types type1,sys.columns col where t.name='singertab' and t.object_id = c.id and c.xusertype=type1.user_type_id and  col.object_id =(SELECT object_id FROM sys.tables WHERE name = '" + tablename + "')";

                        list = new List<ProcedureInfo>();

                        cn.Open();
                        cn.ChangeDatabase(comboBox1.Text);
                        SqlCommand cm = new SqlCommand(sql, cn);
                        table = tablename.Substring(0, 1).ToUpper() + tablename.Substring(1);
                        using (SqlDataReader dr = cm.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            while (dr.Read())
                            {
                                ProcedureInfo pro = new ProcedureInfo();
                                pro.Row = (string)dr[0];
                                pro.Type = (string)dr[1];
                                pro.Length = Convert.ToInt16(dr[2]);
                                pro.IsNull = Convert.ToBoolean(dr[3]);
                                pro.IsIdentity = Convert.ToBoolean(dr[4]);
                                pro.IsPrimary = Convert.ToBoolean(dr[5]);
                                pro.Comment = (string)dr[6];
                                list.Add(pro);
                            }
                        }
                        if (checkBox2.Checked)
                        {
                            GenerateViewModel(list);
                        }
                        if (checkBox3.Checked)
                        {
                            GenerateApiModel(list);
                        }
                        if (checkBox4.Checked)
                        {
                            GenerateDataModel(list);
                        }
                        progressBar1.Value++;
                    }
                }
                else
                {
                    progressBar1.Maximum = 1;
                    table = textBox5.Text;
                    var sql = textBox6.Text;

                    var validateResult = ValidateSQL(sql);
                    if (!string.IsNullOrEmpty(validateResult))
                    {
                        MessageBox.Show(validateResult);
                        return;
                    }
                    list = new List<ProcedureInfo>();
                    cn.Open();
                    cn.ChangeDatabase(comboBox1.Text);
                    SqlCommand cm = new SqlCommand(sql, cn);
                    using (SqlDataReader dr = cm.ExecuteReader(CommandBehavior.SchemaOnly))
                    {
                        var dtSchema = dr.GetSchemaTable();
                        for (int i = 0; i < dtSchema.Rows.Count; i++)
                        {
                            ProcedureInfo pro = new ProcedureInfo();
                            pro.Row = dtSchema.Rows[i][0].ToString();
                            pro.Type = dtSchema.Rows[i][24].ToString();
                            pro.Length = Convert.ToInt32(dtSchema.Rows[i][2]);
                            pro.IsNull = Convert.ToBoolean(dtSchema.Rows[i][13]);
                            pro.IsIdentity = Convert.ToBoolean(dtSchema.Rows[i][18]);
                            pro.IsPrimary = Convert.ToBoolean(dtSchema.Rows[i][17]);
                            pro.Comment = "";
                            list.Add(pro);
                        }
                    }
                    cn.Close();
                    if (checkBox2.Checked)
                    {
                        GenerateViewModel(list);
                    }
                    if (checkBox3.Checked)
                    {
                        GenerateApiModel(list);
                    }
                    if (checkBox4.Checked)
                    {
                        GenerateDataModel(list);
                    }
                    progressBar1.Value++;
                    this.chenggong.Visible = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        #region DataModel
        private void GenerateDataModel(List<ProcedureInfo> list)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("using System;");
            sb.Append(Environment.NewLine);
            sb.Append("using System.Collections.Generic;");
            sb.Append(Environment.NewLine);
            sb.Append("using System.Text;");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("namespace ");
            sb.Append(textBox10.Text);
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("    public class ");
            sb.Append(table);
            sb.Append(Environment.NewLine);
            sb.Append(Common.createplace(4));
            sb.Append("{");
            sb.Append(Environment.NewLine);

            foreach (ProcedureInfo pro in list)
            {
                string name = pro.Row;
                string comment = pro.Comment;
                var type = Common.findModelsType(pro.Type);

                sb.Append(Common.createplace(8));
                sb.Append("/// <summary>");
                sb.Append(Environment.NewLine);
                sb.Append(Common.createplace(8));
                sb.Append("/// ");
                sb.Append(comment);
                sb.Append(Environment.NewLine);
                sb.Append(Common.createplace(8));
                sb.Append("/// </summary>");
                sb.Append(Environment.NewLine);
                sb.Append(Common.createplace(8));
                sb.Append("public");
                sb.Append(" ");
                sb.Append(type);
                sb.Append(Common.GetIsNullable(type, pro.IsNull));
                sb.Append(" ");
                sb.Append(name.Substring(0, 1).ToUpper() + name.Substring(1));
                sb.Append(" { get; set; }");
                sb.Append(Environment.NewLine);
            }
            sb.Append(Common.createplace(4));
            sb.Append("}");
            sb.Append(Environment.NewLine);
            sb.Append("}");

            using (var fs = new StreamWriter(new FileStream(path + @"\DataModel\" + table + ".cs", FileMode.Create), Encoding.UTF8))
            {
                fs.AutoFlush = true;
                fs.Write(sb.ToString());
            }
        }

        #endregion

        #region APIModel
        private void GenerateApiModel(List<ProcedureInfo> list)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("using System;");
            sb.Append(Environment.NewLine);
            sb.Append("using System.ComponentModel.DataAnnotations;");
            sb.Append(Environment.NewLine);
            sb.Append("using _5SPower.Framework.Models;");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("namespace ");
            sb.Append(textBox10.Text);
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("    public partial class ");
            sb.Append(table);
            sb.Append("Request : HttpApiRequestObject");
            sb.Append(Environment.NewLine);
            sb.Append(Common.createplace(4));
            sb.Append("{");
            sb.Append(Environment.NewLine);

            foreach (ProcedureInfo pro in list)
            {
                string name = pro.Row;
                name = name.Substring(0, 1).ToUpper() + name.Substring(1);
                string comment = pro.Comment;
                var type = Common.findModelsType(pro.Type);

                if (!pro.IsNull)
                {
                    sb.Append(Common.createplace(8));
                    sb.Append(string.Format("[Required(ErrorMessage = \"{0}是必填项\")]", !string.IsNullOrEmpty(pro.Comment) ? pro.Comment : name));
                    sb.Append(Environment.NewLine);
                }
                sb.Append(Common.createplace(8));
                sb.Append(string.Format("[Display(Name = \"{0}\")]", !string.IsNullOrEmpty(pro.Comment) ? pro.Comment : name));
                sb.Append(Environment.NewLine);
                if (type == "string")
                {
                    if (pro.Length > 0)
                    {
                        if (pro.Type == "nvarchar" && pro.Length != 2147483647)
                        {
                            sb.Append(Common.createplace(8));
                            sb.Append(string.Format("[StringLength({0}, ErrorMessage = \"最多输入{0}个字符\")]", pro.Length / 2));
                            sb.Append(Environment.NewLine);
                        }
                        else if (pro.Type == "varchar")
                        {
                            sb.Append(Common.createplace(8));
                            sb.Append(string.Format("[StringLength({0}, ErrorMessage = \"最多输入{0}个字符\")]", pro.Length));
                            sb.Append(Environment.NewLine);
                        }
                        else { }
                    }
                }
                else if (type == "int")
                {
                    sb.Append(Common.createplace(8));
                    sb.Append("[RegularExpression(\"^[1-9][0-9]*$\", ErrorMessage = \"请输入正整数\")]");
                    sb.Append(Environment.NewLine);
                }
                sb.Append(Common.createplace(8));
                sb.Append("public");
                sb.Append(" ");
                sb.Append(type);
                sb.Append(Common.GetIsNullable(type, pro.IsNull));
                sb.Append(" ");
                sb.Append(name);
                sb.Append(" { get; set; }");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
            }
            sb.Append(Common.createplace(4));
            sb.Append("}");
            sb.Append(Environment.NewLine);
            sb.Append("}");

            using (var fs = new StreamWriter(new FileStream(path + @"\ApiModel\" + table + "Request.cs", FileMode.Create), Encoding.UTF8))
            {
                fs.AutoFlush = true;
                fs.Write(sb.ToString());
            }

            StringBuilder sb2 = new StringBuilder();
            sb2.Append("using System;");
            sb2.Append(Environment.NewLine);
            sb2.Append("using System.ComponentModel.DataAnnotations;");
            sb2.Append(Environment.NewLine);
            sb2.Append("using _5SPower.Framework.Models;");
            sb2.Append(Environment.NewLine);
            sb2.Append(Environment.NewLine);
            sb2.Append("namespace ");
            sb2.Append(textBox10.Text);
            sb2.Append(Environment.NewLine);
            sb2.Append("{");
            sb2.Append(Environment.NewLine);
            sb2.Append("    public partial class ");
            sb2.Append(table);
            sb2.Append("Response : HttpApiResponseObject");
            sb2.Append(Environment.NewLine);
            sb2.Append(Common.createplace(4));
            sb2.Append("{");
            sb2.Append(Environment.NewLine);
            sb2.Append(Common.createplace(4));
            sb2.Append("}");
            sb2.Append(Environment.NewLine);
            sb2.Append("}");

            using (var fs = new StreamWriter(new FileStream(path + @"\ApiModel\" + table + "Response.cs", FileMode.Create), Encoding.UTF8))
            {
                fs.AutoFlush = true;
                fs.Write(sb2.ToString());
            }



        }
        #endregion

        #region ViewModel
        private void GenerateViewModel(List<ProcedureInfo> list)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("using System;");
            sb.Append(Environment.NewLine);
            sb.Append("using System.Collections.Generic;");
            sb.Append(Environment.NewLine);
            sb.Append("using System.Text;");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("namespace ");
            sb.Append(textBox10.Text);
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("    public class ");
            sb.Append(table);
            sb.Append(Environment.NewLine);
            sb.Append(Common.createplace(4));
            sb.Append("{");
            sb.Append(Environment.NewLine);

            foreach (ProcedureInfo pro in list)
            {
                string name = pro.Row;
                string comment = pro.Comment;
                var type = Common.findModelsType(pro.Type);

                sb.Append(Common.createplace(8));
                sb.Append("/// <summary>");
                sb.Append(Environment.NewLine);
                sb.Append(Common.createplace(8));
                sb.Append("/// ");
                sb.Append(comment);
                sb.Append(Environment.NewLine);
                sb.Append(Common.createplace(8));
                sb.Append("/// </summary>");
                sb.Append(Environment.NewLine);
                sb.Append(Common.createplace(8));
                sb.Append("public");
                sb.Append(" ");
                sb.Append(type);
                sb.Append(Common.GetIsNullable(type, pro.IsNull));
                sb.Append(" ");
                sb.Append(name.Substring(0, 1).ToUpper() + name.Substring(1));
                sb.Append(" { get; set; }");
                sb.Append(Environment.NewLine);
            }
            sb.Append(Common.createplace(4));
            sb.Append("}");
            sb.Append(Environment.NewLine);
            sb.Append("}");

            using (var fs = new StreamWriter(new FileStream(path + @"\ViewModel\" + table + ".cs", FileMode.Create), Encoding.UTF8))
            {
                fs.AutoFlush = true;
                fs.Write(sb.ToString());
            }


        }
        #endregion

        #region sql语句检查

        public string ValidateSQL(string sql)
        {
            string bResult = string.Empty;
            if (sql.ToUpper().Contains("UPDATE") || sql.ToUpper().Contains("INSERT") || sql.ToUpper().Contains("CREATE") || sql.ToUpper().Contains("DELETE"))
                return "SQL语句中只能包含查询！";
            cn.Open();
            SqlCommand cmd = cn.CreateCommand();
            cmd.CommandText = "SET PARSEONLY ON";
            cmd.ExecuteNonQuery();
            try
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                bResult = "输入的SQL语句不正确！";
            }
            finally
            {
                cmd.CommandText = "SET PARSEONLY OFF";
                cmd.ExecuteNonQuery();
                cn.Close();
            }
            return bResult;
        }

        #endregion


        #endregion

        #region 窗口关闭和最小化事件
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        private void pictureBox2_MouseEnter(object sender, EventArgs e)
        {

        }

        private void pictureBox2_MouseLeave(object sender, EventArgs e)
        {

        }

        private void pictureBox3_MouseEnter(object sender, EventArgs e)
        {

        }

        private void pictureBox3_MouseLeave(object sender, EventArgs e)
        {

        }
        #endregion
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.chenggong.Visible = false;
            this.progressBar1.Value = 0;
            this.timer1.Enabled = false;
        }


    }
}
