using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MyLogLib;
using MySql.Data.MySqlClient;

#if false
    count表：检测数量，不用，丢弃
    module表：模块表，不用，丢弃
    module_style表：模块类型表，主站能检测的5种模块类型，基本无需修改
    operator表：操作人员表，可增、删、改、查
    style_ref_item表：模块类型对应的检测项，不能修改
    task表：任务表，表示某一次检测，第几个工位检测什么类型的模块的什么检测项
    test_result表：检测结果表，供查阅
    test_scheme表：检测方案表，与任务表一一对应
    test_term表：检测项表，5种类型模块的所有检测项
    workplace表：工位表，不用，丢弃
    workplace_module表：工位类型表，不用，丢弃
#endif

namespace mkjcApp
{
    internal class MySQLHelper
    {
        //MySQL相关全局变量
        public static string connString = $"server=localhost;port=3306;database=plc_scheme;user=root;password={System.Configuration.ConfigurationManager.AppSettings["sqlpwd"]};";
        public static MySqlConnection connect;

        public static DataTable SQLSelect(string sql)
        {
            DataSet dataset = new DataSet();
            using (MySqlConnection con = new MySqlConnection(connString))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                {

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    adapter.Fill(dataset);
                }
            }
            return dataset.Tables[0];
        }
        /// <summary>
        /// 数据库查询记录
        /// </summary>
        /// <param name="sqlstring"></param>
        public static List<String> SQLselect(string sqlstring, int num)
        {
            List<String> return_list = new List<string> { };

            //打开数据库连接
            connect = new MySqlConnection(connString);
            try
            {
                connect.Open();//建立连接，打开数据库
            }
            catch (Exception ex)
            {
                MyLog.WriteLog(ex);
                MessageBox.Show(ex.Message, "数据库错误");
                return null;
            }
            //数据库查询操作
            try
            {
                //string sqlstr = "select * from module_style";
                MySqlCommand cmd = new MySqlCommand(sqlstring, connect);
                MySqlDataReader reader = cmd.ExecuteReader();//相当于数据读出流
                while (reader.Read())//遍历表中数据
                {
                    for (int i = 0; i < num; i++)
                    {
                        return_list.Add(reader[i].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MyLog.WriteLog(ex);
                MessageBox.Show(ex.Message, "未知错误");
                return null;
            }
            connect.Close();
            return return_list;
        }

        /// <summary>
        /// 数据库插入操作
        /// </summary>
        /// <param name="sqlstring"></param>
        public static void SQLinsert(string sqlstring)
        {
            //打开数据库连接
            connect = new MySqlConnection(connString);
            try
            {
                connect.Open();//建立连接，打开数据库
            }
            catch (Exception ex)
            {
                MyLog.WriteLog(ex);
                MessageBox.Show(ex.Message, "数据库错误");
                return;
            }
            //数据库插入操作
            try
            {
                //检测完成，结果存数据库中，注意string格式，要加上双引号

                MySqlCommand cmd = new MySqlCommand(sqlstring, connect);
                int result = cmd.ExecuteNonQuery();//返回值为执行后数据库中受影响的数据行数
            }
            catch (Exception ex)
            {
                MyLog.WriteLog(ex);
                MessageBox.Show(ex.Message, "ERR");
            }
            finally
            {
                try { connect.Close(); } catch { }
            }
        }

        public static int SQLdelete(string sqlstring)
        {
            int result;
            //打开数据库连接
            connect = new MySqlConnection(connString);
            try
            {
                connect.Open();//建立连接，打开数据库
            }
            catch (Exception ex)
            {
                MyLog.WriteLog(ex);
                MessageBox.Show(ex.Message, "数据库错误");
                return -1;
            }
            //数据库删除操作
            try
            {
                //string sqlstr = "";
                MySqlCommand cmd = new MySqlCommand(sqlstring, connect);
                result = cmd.ExecuteNonQuery();//返回值为执行后数据库中受影响的数据行数                
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERR");                
                return -1;
            }
            finally
            {
                try { connect.Close(); } catch { }
            }
        }
    }
}
