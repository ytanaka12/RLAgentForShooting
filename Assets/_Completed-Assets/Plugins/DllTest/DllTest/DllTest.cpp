// DllTest.cpp : DLL アプリケーション用にエクスポートされる関数を定義します。
//

#include "stdafx.h"
#include <stdio.h>
#include <string.h>
#include "DllTest.h"
#include <stdexcept>

extern "C" {

	// No argument
	CPPDLL_API int func1()
	{
		return 11;
	}

	// Passes values
	CPPDLL_API int func2(int a, int b)
	{
		return a + b;
	}

	// Passes values and receives a value
	CPPDLL_API int func3(int a, int b, int* c)
	{
		*c = a + b;
		return 33;
	}

	// Passes a fixed-length string
	CPPDLL_API int func4(const char* psz)
	{
		printf("%s\n", psz);
		return 44;
	}

	// Receives a string
	CPPDLL_API int func5(char* psz, int len)
	{
		strcpy_s(psz, len, "Oh my goodness!");
		return 55;
	}

	// Passes a structure
	CPPDLL_API int func6(Data* data)
	{
		data->i3 = data->i1 + data->i2;
		data->u3 = data->u1 + data->u2;
		data->c3 = data->c1 + data->c2;

		strcpy_s(data->sz3, 64, data->sz1);
		strcat_s(data->sz3, 64, data->sz2);

		return 66;
	}

} // extern "C"


