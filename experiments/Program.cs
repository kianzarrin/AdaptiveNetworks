using System;
using System.Text;
using System.IO;
using System.Diagnostics;

using Fbx;
namespace experiments {
    class Program {
        static void Main(string[] args)
        {
            Test4();
        }

        static void Test4() {
            Test3();
            string dir = @"C:\Users\dell\AppData\Local\Colossal Order\Cities_Skylines\Addons\Import\ARDumps\";
            string file1 = "RoadMediumNode._ascii.fbx"; // can open this
            string file2 = "TEST3_RoadMediumNode.binary.fbx"; // can open this
            string file3 = "TEST3_RoadMediumNode.ascii.fbx";
            string fileB = "TEST3B_RoadMediumNode.binary.fbx";

            Console.WriteLine("reading binary ...");
            var doc1 = FbxIO.ReadBinary(dir + file2);

            FbxIO.WriteAscii(doc1, dir + file3);
            var doc2 = FbxIO.ReadAscii(dir + file3);
            doc1.Diff(doc2);

            FbxIO.WriteBinary(doc2, dir + fileB);
        }



        static Process Execute(string dir, string exeFile, string args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo {
                WorkingDirectory = dir,
                FileName = exeFile,
                Arguments = args,
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            Process process = new Process { StartInfo = startInfo};
            process.Start();
            return process;
        }

        static void Test3()
        {
            string dir = @"C:\Users\dell\AppData\Local\Colossal Order\Cities_Skylines\Addons\Import\ARDumps";
            string fileIn = "RoadMediumNode._ascii.fbx";
            string fileOut = "TEST3_RoadMediumNode.binary.fbx";
            string converter = "FbxFormatConverter.exe";

            Execute(dir, converter, $"-c {fileIn} -o {fileOut} -binary").WaitForExit();
        }

        static void Test2()
        {
            string dir = @"C:\Users\dell\AppData\Local\Colossal Order\Cities_Skylines\Addons\Import\ARDumps\";
            string file1 = "RoadMediumNode.binary.fbx"; // can open this

            var doc = FbxIO.ReadBinary(dir + file1); 
            string fileA = "testA_" + file1;
            FbxIO.WriteBinary(doc, dir + fileA); // can open this
            
            doc = FbxIO.ReadBinary(dir + file1);
            string fileB = "testB_" + file1;
            FbxIO.WriteAscii(doc, dir + fileB); // can open this

            doc = FbxIO.ReadAscii(dir + fileB);
            FbxIO.WriteBinary(doc, dir + "testC_" + file1);  // can' open this
        }

        static void Test1()
        {
            string dir = @"C:\Users\dell\AppData\Local\Colossal Order\Cities_Skylines\Addons\Import\ARDumps\";
            string file1 = "RoadMediumNode._ascii.fbx";

            var doc = FbxIO.ReadAscii(dir + file1);
            string fileA = "testA_" + file1;
            FbxIO.WriteAscii(doc, dir + fileA);

            doc = FbxIO.ReadAscii(dir + fileA);
            string fileB = "testB_" + file1;
            FbxIO.WriteBinary(doc, dir + fileB); // i can't open this

            doc = FbxIO.ReadBinary(dir + fileB);
            FbxIO.WriteAscii(doc, dir + "testC_" + file1); // i can open this
        }
    }
}