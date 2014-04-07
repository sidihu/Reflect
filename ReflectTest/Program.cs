using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Data;
using System.Data.Common;
using System.ComponentModel.DataAnnotations;

namespace ReflectTest
{
    public class User
    {
        public string Name { get; set; }
        public int? Age
        {
            get;
            set;
            //get { return age; }
            //set
            //{
            //    if (value.ToString()=="")
            //        value = 1;
            //    else
            //        value = 0;
            //}
        }
        public decimal? Money { get; set; }
        public DateTime Time { get; set; }
        public short? st { get; set; }
        

        public void InertCreate(User u)
        {
            PropertyInfo[] pis = u.GetType().GetProperties();
            foreach (PropertyInfo p in pis)
            {
                //DbType dbtype = GetDbType(p.PropertyType);
                if (p.GetValue(u) == null)
                    continue;
                Console.WriteLine(p.Name + "--" + p.PropertyType + "--" + p.GetValue(u));
                //Console.WriteLine(dbtype.ToString());
            }
            Console.ReadLine();
        }

        public static DbType GetDbType(Type type)
        {

            String name = type.Name;
            
            DbType val = DbType.String; // default value

            val = (DbType)Enum.Parse(typeof(DbType), name, true);
            //DbType.
            return val;

        }
    }

    

    class Program
    {
        static void Main(string[] args)
        {
            User u = new User();
            u.Name = "sidihu";
            //u.Age = 23;
            u.Money = 1233534534;
            u.Time =DateTime.Now;
            //u.st = 33;

            //PropertyInfo[] pis = u.GetType().GetProperties();
            //foreach(PropertyInfo p in pis)
            //{
            //    Console.WriteLine(p.Name + "--" + p.PropertyType + "--" + p.GetValue(u));
            //}
            //Console.ReadLine();

            u.InertCreate(u);
            //DbParameter dbParameter=

        }
    }
}
