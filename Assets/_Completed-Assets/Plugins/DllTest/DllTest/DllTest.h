#pragma once

extern "C" {

#ifdef DLLTEST_EXPORTS
#define CPPDLL_API __declspec(dllexport)
#else
#define CPPDLL_API __declspec(dllimport)
#endif

	CPPDLL_API int func1();
	CPPDLL_API int func2(int a, int b);
	CPPDLL_API int func3(int a, int b, int* c);
	CPPDLL_API int func4(const char* psz);
	CPPDLL_API int func5(char* psz, int len);

	typedef struct _Data {
		int i1;
		int i2;
		int i3;
		unsigned short u1;
		unsigned short u2;
		unsigned short u3;
		unsigned char c1;
		unsigned char c2;
		unsigned char c3;
		char sz1[32];
		char sz2[32];
		char sz3[64];
	} Data;

	CPPDLL_API int func6(Data* data);

} // extern "C"
