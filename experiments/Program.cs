using System;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace experiments {
    class Program {
        static void Main(string[] args)
        {
            Test1();
        }


        static SimpleDataSerializer GetReader(byte [] data) => SimpleDataSerializer.Reader(data);
        static SimpleDataSerializer GetWriter() => SimpleDataSerializer.Writer(version, 100);
        static Version version =  new Version(1,2);
        static void Log(object o) => Console.WriteLine(o.ToString());


        static void Test1()
        {
            var writer = GetWriter();
            writer.WriteUInt32(0);
            writer.WriteUInt32(0);
            var data = writer.GetBytes();
            var reader = GetReader(data);
            Log(reader.ReadUInt32());
            Log(reader.ReadUInt32_O());
        }
    }
}