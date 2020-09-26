using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace experimenting
{
    class Program
    {
        static void Main(string[] args)
        {
            var t = typeof(A);
            var fields = t.GetFields();
            foreach( var f in fields)
            {
                var att = f.GetCustomAttributes(typeof(KianAttribute), true);
                string att2; ;
                if (att == null)
                {
                    att2 = "null";
                } else if (att.Length == 0)
                {
                    att2 = "EMPTY";
                } else 
                {
                    att2 = (att[0] as KianAttribute).name;
                    if (att.Length > 1)
                    {
                        att2 = $"[0/{att.Length}]={att2}";
                    }
                }
                Console.WriteLine($"field={f.Name} attribute={att2}");
            }

        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class KianAttribute : Attribute
    {
        public string name;
        public KianAttribute(string _name) => name = _name;
    }

    public class A
    {
        [Kian("a and b")]
        public int a, b;
    }


}
