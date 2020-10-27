namespace experimenting
{
	using System;

	public class Program
	{
		public static bool IsPow2(long x) => x != 0 && (x & (x - 1)) == 0;
		public static bool IsPow2(int x) => x != 0 && (x & (x - 1)) == 0;

		public static void Main()
		{
			Console.WriteLine("result=" + IsPow2(-2147483648));
		}
	}
}
