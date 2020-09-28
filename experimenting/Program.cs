using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace experimenting
{
    class Program
    {
        static void Main(string[] args)
        {

            Bar bar = new Bar();
            bar.foo.a = 1;
            bar.foo.b = 2;
            Console.WriteLine("bar=" + bar);

            FieldInfo fieldInfo_foo = typeof(Bar).GetField("foot");
            FieldInfo fieldInfo_a = typeof(Foo).GetField("a");
            fieldInfo_a.SetValue(bar.foo, 3);
            Console.WriteLine("bar2=" + bar);

        }
    }

    public struct Foo
    {
        public int a;
        public int b;
    }

    public class Bar
    {
        public Foo foo;
        public override string ToString() => $"foo.a={foo.a} foo.b={foo.b}";
    }


}
