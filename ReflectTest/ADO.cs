using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Reflection;
using System.Data.SqlClient;

namespace ReflectTest
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class FieldAttribute : Attribute
    {
        private string _Fields;
        /// <summary>
        /// 字段名称
        /// </summary>
        public string Fields
        {
            get { return _Fields; }

        }

        private DbType _Dbtype;
        /// <summary>
        /// 字段类型
        /// </summary>
        public DbType Dbtype
        {
            get { return _Dbtype; }

        }

        private int _ValueLength;
        /// <summary>
        /// 字段值长度
        /// </summary>
        public int ValueLength
        {
            get { return _ValueLength; }

        }

        private bool _PK_Primary;
        /// <summary>
        /// 是否是主键
        /// </summary>
        public bool PK_Primary
        {
            get { return _PK_Primary; }
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fields"> 字段名</param>
        /// <param name="types"> 字段类型</param>
        /// <param name="i"> 字段值长度</param>
        public FieldAttribute(string fields, DbType types, int i,bool PK=false)
        {

            _Fields = fields;
            _Dbtype = types;
            _PK_Primary = PK;
            _ValueLength = i;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TableAttribute : Attribute
    {
        private string _TableName;
        /// <summary>
        /// 映射的表名
        /// </summary>
        public string TableName
        {
            get { return _TableName; }
        }
        /// <summary>
        /// 定位函数映射表名；
        /// </summary>
        /// <param name="table"></param>
        public TableAttribute(string table)
        {
            _TableName = table;
        }

    }

    /// <summary>
    /// 处理反射的类
    /// </summary>
    /// <typeparam name="T"> 与数据库表建立联系的类</typeparam>
    public class AttributesContext<T>
    {
        /// <summary>
        ///存放AttributesContext 类中所有的当前错误信息
        /// </summary>
        private string ExceptionBug;

        /// <summary>
        /// 读取映射的表名
        /// </summary>
        /// <param name="Info">自定义类型</param>
        /// <returns>放回建立映射表名 没简历表面返回空</returns>
        public string xTable(T Info)
        {

            Type userAttu = Info.GetType();
            try
            {
                TableAttribute tables = (TableAttribute)userAttu.GetCustomAttributes(false)[0];
                //在TableAttribute中我设置的是不容许多个特性所取 【0】
                return tables.TableName;
            }
            catch (ArgumentNullException e)
            {
                ExceptionBug = e.Message;
                return null;
            }
            catch (NotSupportedException e1)
            {
                ExceptionBug = e1.Message;
                return null;
            }


        }

        /// <summary>
        /// 放回自定义类与表建立的映射的字段名
        /// </summary>
        /// <param name="Info">与表建立映射联系的类</param>
        /// <returns>表字段的数组Dictionary   FieldAttribute ,Object[]| key 字段名 FieldAttribute ,value需要自己转换;object [2]  其中第一个是字段名称  第2个是字段值 </returns>
        public Dictionary<FieldAttribute, Object[]> xField(T Info)
        {
            Dictionary<FieldAttribute, Object[]> xFields = new Dictionary<FieldAttribute, Object[]>();

            Type types = Info.GetType();
            PropertyInfo[] typesPro = types.GetProperties();

            foreach (PropertyInfo pro in typesPro)
            {
                object[] attu = pro.GetCustomAttributes(false);

                object objValue = pro.GetGetMethod().Invoke(Info, null);//取特性描述相应字段的值
                object objFieldName = (Object)pro.Name;//取特性对应类的字段名称
                object[] classInfo = new object[2];//把类中的字段名称与值存放；

                classInfo[0] = objFieldName;
                classInfo[1] = objValue;

                foreach (Attribute afield in attu)
                {
                    if (afield is FieldAttribute)
                    {
                        FieldAttribute column = afield as FieldAttribute;//把afield转换成FieldAttribute类型
                        xFields.Add(column, classInfo);//把字段存放到key 把特性描述的字段字存放到value

                    }

                }

            }

            return xFields;
        }

    }

////用AttributesContext中的  public Dictionary<FieldAttribute, Object[]> xField(T Info)
//   当我们知道 Dictionary就可以取出相应的信息让后我们在拼接一下 就可以获得命令语句;

//我写得是拼接的字符串下面是列子；

    /// <summary>
    /// 用来拼接操作数据库的字符串
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Install<T>
    {
        /// <summary>
        /// 拼接查询字符串(只能用在与表建立联系的实体类)
        /// </summary>
        /// <param name="types"> 与表建立映射的自定义类</param>
        /// <returns>查询字符串</returns>
        public string insertDate(T types)
        {
            string cmdtxt = "insert into ";
            string cmdparVar = null;
            Type userAttu = types.GetType();
            TableAttribute tables = (TableAttribute)userAttu.GetCustomAttributes(false)[0];
            cmdtxt += tables.TableName + "(";
            PropertyInfo[] info = userAttu.GetProperties();

            foreach (PropertyInfo prs in info)
            {
                object[] attu = prs.GetCustomAttributes(false);

                foreach (Attribute abute in attu)
                {
                    if (abute is FieldAttribute)
                    {
                        FieldAttribute midle = abute as FieldAttribute;
                        cmdtxt += midle.Fields + ",";
                        object obj = prs.GetGetMethod().Invoke(types, null);
                        if (midle.Dbtype == DbType.Int32)
                            cmdparVar += obj + ",";
                        else
                            cmdparVar += "'" + obj + "',";

                    }
                }

            }

            cmdparVar = cmdparVar.Substring(0, cmdparVar.Length - 1);
            cmdtxt = cmdtxt.Substring(0, cmdtxt.Length - 1) + ")";

            cmdtxt += "values(" + cmdparVar + ")";
            return cmdtxt;
        }

        /// <summary>
        /// 创建存储过程的参数  只用于 (SQL语法)
        /// </summary>
        /// <param name="parName">参数名（与存储过程参数名一样）</param>
        /// <param name="parType">参数的数据类型</param>
        /// <param name="parSize">参数字段长度 int 用0表示长度</param>
        /// <param name="parVal">输入参数的值</param>
        /// <param name="aspect">传入方式</param>
        /// <returns></returns>

        public static SqlParameter CreateProcParameters(string parName, SqlDbType parType, int parSize, object parVal, ParameterDirection aspect = ParameterDirection.Input)
        {
            SqlParameter p = new SqlParameter();
            p.ParameterName = parName;
            p.SqlDbType = parType;
            p.Direction = aspect;
            if (parSize != 0)
                p.Size = parSize;
            p.SqlValue = parVal;
            return p;
        }

        //循环添加参数到命令中

        //public static void AddParametersTocmd(SqlCommand cmd,params SqlParameter[] pList)
        //    {
        //        foreach (SqlParameter s in pList)
        //        {
        //            cmd.Parameters.Add(s);//添加参数
        //        }
        //    }


    }

    public class ADO
    {

    }
}
