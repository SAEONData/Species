using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace BLA
{
    public class SuperSQL
    {
        public SuperSQL(SqlConnection con, SqlTransaction trn, HttpResponse rsp)
        {
            this.con = con;
            this.trn = trn;
            this.rsp = rsp;
        }
        public void add(String name, object val)
        {
            vals[name] = val;
        }

        public int executeSQL(String sql)
        {
            int ret = 0;
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Transaction = trn;
                ret = cmd.ExecuteNonQuery();
            }
            return ret;
        }

        public int modify(String table, string where)
        {
            string fields = "";
            foreach (string name in vals.Keys)
            {
                if (fields != "")
                {
                    fields += ", ";
                }
                fields += name + "=" + "@" + name;
            }
            String sql = String.Format("UPDATE {0} SET {1} WHERE {2}", table, fields, where);
            int ret = 0;
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Transaction = trn;
                foreach (string name in vals.Keys)
                    cmd.Parameters.AddWithValue("@" + name, vals[name]);

                try
                {
                    ret = cmd.ExecuteNonQuery();
                }
                catch (Exception err)
                {
                    rsp.Write("sql: " + sql + "<br>");
                    rsp.Write("msg: " + err.Message + "<br>");
                }
            }
            return ret;
        }

        public int insert(String table, String field = "")
        {
            string fields = "";
            string values = "";
            foreach (string name in vals.Keys)
            {
                if (fields != "")
                {
                    fields += ", ";
                    values += ", ";
                }
                fields += name;
                values += "@" + name;
            }

            String sql = String.Format("INSERT INTO {0} ({1}) VALUES ({2})", table, fields, values);
            int ret = 0;
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Transaction = trn;
                foreach (string name in vals.Keys)
                    cmd.Parameters.AddWithValue("@" + name, vals[name]);

                try
                {
                    ret = cmd.ExecuteNonQuery();
                }
                catch (Exception err)
                {
                    rsp.Write("sql: " + sql + "<br>");
                    rsp.Write("msg: " + err.Message + "<br>");
                }
            }

            if (field != "")
                return getMaxID(table, field);
            else
                return ret;
        }

        public int getMaxID(String table, String field)
        {
            String sql = String.Format("SELECT MAX({0}) AS val FROM {1}", field, table);
            int id = 0;
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                using (SqlDataReader set = cmd.ExecuteReader())
                {
                    if (set.Read())
                        id = (int)set["val"];
                }
            }
            return id;
        }

        public object select(String table, String field)
        {
            string values = "";
            foreach (string name in vals.Keys)
            {
                if (values != "")
                    values += " AND ";
                values += name + " = " + "@" + name;
            }

            String sql = String.Format("SELECT {0} FROM {1} WHERE {2}", field, table, values);

            object ret = null;
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Transaction = trn;
                foreach (string name in vals.Keys)
                    cmd.Parameters.AddWithValue("@" + name, vals[name]);

                using (SqlDataReader set = cmd.ExecuteReader())
                {
                    if (set.Read())
                        ret = set[field];
                }
            }
            return ret;
        }

        public int selectInt(String table, String field)
        {
            object ret = select(table, field);
            return ret != null ? (int)ret : -1;
        }

        public string selectString(String table, String field)
        {
            object ret = select(table, field);
            return ret != null ? (string)ret : "";
        }

        protected SqlConnection con;
        protected SqlTransaction trn;
        protected HttpResponse rsp;
        protected Dictionary<string, object> vals = new Dictionary<string, object>();
    }
}