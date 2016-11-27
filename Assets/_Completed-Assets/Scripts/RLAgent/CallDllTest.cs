using System;
using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Runtime.InteropServices;	//Dll
//using System.Diagnostics;

namespace TestDLLCSharp
{
	public class CallDllTest : MonoBehaviour
	{
		[DllImport("DllTest")] public static extern int func1();

		[DllImport("DllTest.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int func2(int a, int b);

		[DllImport("DllTest.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int func3(int a, int b, ref int c);

		[DllImport("DllTest.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int func4(string str);

		[DllImport("DllTest.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int func5(StringBuilder sb, int len);

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct DATA
		{
			public int i1;
			public int i2;
			public int i3;
			public ushort u1;
			public ushort u2;
			public ushort u3;
			public byte c1;
			public byte c2;
			public byte c3;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string sz1;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string sz2;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
			public string sz3;
		}

		void Start() {
			Debug.LogFormat("hogehogehoge");
			submain();
		}

		[DllImport("DllTest.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int func6(ref DATA data);

		void submain()
		{
			int c = func1();
			Debug.LogFormat("func1----------------{0}", c);

			c = func2(12, 10);
			Debug.LogFormat("func2----------------{0}", c); ;

			int d = 0;
			c = func3(13, 11, ref d);
			Debug.LogFormat("func3 returned{0} ({1})", c, d);

			c = func4("Hello World");
			Debug.LogFormat("func4 returned{0}", c);

			StringBuilder sb = new StringBuilder(512);
			c = func5(sb, 512);
			Debug.LogFormat("func5 returned{0} ({1})", c, sb);

			DATA data = new DATA();
			data.i1 = 25;
			data.i2 = 27;
			data.i3 = 0;
			data.u1 = 15;
			data.u2 = 17;
			data.u3 = 0;
			data.c1 = 5;
			data.c2 = 7;
			data.c3 = 0;
			data.sz1 = "HELLO+";
			data.sz2 = "WORLD";
			data.sz3 = "";
			c = func6(ref data);
			Debug.LogFormat("func6 returned {0}", c);
			Debug.LogFormat("data.i3 = {0}", data.i3);
			Debug.LogFormat("data.u3 = {0}", data.u3);
			Debug.LogFormat("data.c3 = {0}", data.c3);
			Debug.LogFormat("data.sz3 = {0}", data.sz3);

			Debug.LogFormat("done!");
		}
	}
}

/* result
func1 returned 11
func2 returned 22
func3 returned 33 (24)
Hello World
func4 returned 44
func5 returned 55 (Oh my goodness!)
func6 returned 66
data.i3 = 52
data.u3 = 32
data.c3 = 12
data.sz3 = HELLO+WORLD
*/
