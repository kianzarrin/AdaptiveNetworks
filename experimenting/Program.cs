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
            var b = new A.B();
            var t = b.GetType();
            
            Console.WriteLine($"b: {t.Name} {t.FullName} {t.Namespace}" );



        }
    }

    public static class A
    {
        public static int Version;

        public class B
        {
            public int a;
            public int b;
        }
    }


}
