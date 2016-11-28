#pragma once

extern "C" {

#ifdef EIGENFUNCS_EXPORTS
#define EIGEN_FUNCS_API __declspec(dllexport)
#else
#define EIGEN_FUNCS_API __declspec(dllimport)
#endif

	EIGEN_FUNCS_API float TestFunc(int dim, float A[]);
	EIGEN_FUNCS_API float Func1(std::vector<float> A);

	EIGEN_FUNCS_API void Addition_Vec(int dim, float a[], float b[], float ans[]);
	EIGEN_FUNCS_API void InverseMat(int dim, float a[], float ans[]);
} // extern "C"
