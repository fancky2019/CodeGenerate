using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CodeGenerator
{
    public partial class SelectTabelFrm : Form
    {
        private List<string> _tableNameList;
        List<Panel> _listPanel = new List<Panel>();

        public List<string> SelectedTableNames { get; set; }

        public SelectTabelFrm(List<string> tableNameList)
        {
            InitializeComponent();
            this._tableNameList = tableNameList;
            SelectedTableNames = new List<string>();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            DisplayTables(this._tableNameList);
        }

        private void DisplayTables(List<string> tableNames)
        {
            _listPanel.Clear();
            this.panelTableNames.Controls.Clear();
            this.panel2.Controls.Clear();
            //List<string> list = ((Form1)Owner).list;
            int x = 2; int y = 0;
            int x1 = 2;
            int i = 0;
            int y1 = 12;
            List<List<string>> tableNamePages = GetChangePage(tableNames, 50);
            Panel tablesPanel = null;
            LinkLabel linkLabel = null;
            CheckBox checkBox = null;
            foreach (List<string> currentPageNames in tableNamePages)
            {
                i++;
                tablesPanel = new Panel();
                if (i == 1)
                {
                    this.panelTableNames.Controls.Add(tablesPanel);
                }
                tablesPanel.Name = "p-" + i;
                tablesPanel.Tag = i;
                tablesPanel.Dock = DockStyle.Fill;

                linkLabel = new LinkLabel();
                this.panel2.Controls.Add(linkLabel);
                linkLabel.Name = "llll" + i;
                linkLabel.Text = i.ToString();
                linkLabel.Left = x1;
                linkLabel.AutoSize = true;
                x1 += linkLabel.Width;
                linkLabel.Top = y1;
                linkLabel.ForeColor = Color.Black;
                linkLabel.Width = 7;


                linkLabel.Click += new EventHandler(pageIndexChange_Click);
                x1 += linkLabel.Width;
                x = 2;
                y = 0;
                if (i != 0 && i % 25 == 0)
                {
                    y1 += 15;
                    x1 = 2;
                }
                int currentColumnMaxWidth = 0;
                foreach (string tableName in currentPageNames)
                {
                    checkBox = new CheckBox();
                    checkBox.Left = x;
                    checkBox.Top = y;
                    checkBox.Text = tableName;
                    checkBox.Tag = tableName;
                    checkBox.Checked = this.cbSelectAll.Checked;
                    checkBox.AutoSize = true;
                    tablesPanel.Controls.Add(checkBox);
                    currentColumnMaxWidth = currentColumnMaxWidth > checkBox.Width ? currentColumnMaxWidth : checkBox.Width;
                    y += checkBox.Height;
                    //checkBox的高度16，剩余不够16,就换列排
                    if (y >= this.panelTableNames.Height - 16)
                    {
                        //x += checkBox.Width;
                        x += currentColumnMaxWidth;
                        y = 0;
                    }
                }
                _listPanel.Add(tablesPanel);
            }
        }

        void pageIndexChange_Click(object sender, EventArgs e)
        {
            panelTableNames.Controls.Clear();
            int i = Convert.ToInt32((sender as LinkLabel).Text);
            Panel pa = new Panel();
            if (_listPanel.Count > 0)
            {
                pa = _listPanel[i - 1];
            }
            foreach (var control in pa.Controls)
            {
                CheckBox checkBox = control as CheckBox;
                if (checkBox != null)
                {
                    checkBox.Checked = this.cbSelectAll.Checked;
                }
            }
            panelTableNames.Controls.Add(pa);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (this.cbSelectAll.Checked)
            {
                for (int i = 0; i < panelTableNames.Controls.Count; i++)
                {
                    Panel pa = (Panel)panelTableNames.Controls[i];
                    foreach (Control con in pa.Controls)
                    {
                        if (con is CheckBox)
                        {
                            CheckBox ch = con as CheckBox;
                            ch.Checked = true;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < panelTableNames.Controls.Count; i++)
                {
                    Panel pa = (Panel)panelTableNames.Controls[i];
                    foreach (Control con in pa.Controls)
                    {
                        if (con is CheckBox)
                        {
                            CheckBox ch = con as CheckBox;
                            ch.Checked = false;
                        }
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < panelTableNames.Controls.Count; i++)
            {

                Panel pa = (Panel)panelTableNames.Controls[i];
                foreach (Control con in pa.Controls)
                {
                    if (con is CheckBox)
                    {
                        CheckBox ch = con as CheckBox;
                        if (ch.Checked)
                            SelectedTableNames.Add(ch.Text);
                    }
                }
            }
            this.Close();
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="list"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<List<string>> GetChangePage(List<string> list, int count)
        {

            List<List<string>> listBig = new List<List<string>>();
            int count1 = list.Count / count;
            for (int i = 0; i < count1; i++)
            {
                List<string> list2 = new List<string>();
                for (int j = i * count; j < (i + 1) * count; j++)
                {
                    list2.Add(list[j]);
                }
                listBig.Add(list2);
            }
            if (list.Count % count != 0)
            {
                List<string> list3 = new List<string>();
                for (int i = count1 * count; i < list.Count; i++)
                {
                    list3.Add(list[i]);
                }
                listBig.Add(list3);
            }
            return listBig;
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            var result = _tableNameList;
            if (!string.IsNullOrEmpty(this.txtTableName.Text.Trim()))
            {
                result = this._tableNameList.Where(p => p.Contains(this.txtTableName.Text.Trim())).ToList();
            }
            DisplayTables(result);
        }
    }
}
