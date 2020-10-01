using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;

namespace experimenting
{
    class Program
    {
        static void Main(string[] args)
        {
            V2 v2 = new V2 { x = 1, y = 2 };
            //V3 v3 = v2 as V3;
        }
    }

    class V2
    {
        public int x;
        public int y;

        public static implicit operator V3(V2 v2)
        {
            return new V3 { x = v2.x, y = v2.y, z = 0 };
        }
    }

    class V3
    {
        public int x;
        public int y;
        public int z;
    }

}
