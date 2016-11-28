// EigenFuncs.cpp : DLL アプリケーション用にエクスポートされる関数を定義します。
//

#include "stdafx.h"
#include <stdio.h>
#include <string.h>
#include <vector>
#include <Eigen/Core>
#include <Eigen/Geometry>
#include <Eigen/LU>
#include "EigenFuncs.h"

Eigen::MatrixXf Vec2Mat(int nrow, int ncol, float a[]) {
	Eigen::MatrixXf ans = Eigen::MatrixXf::Zero(nrow, ncol);
	for (int r = 0; r < nrow; r++) {
		for (int c = 0; c < ncol; c++) {

		}
	}
	return ans;
}

extern "C" {

	// No argument
	EIGEN_FUNCS_API float TestFunc(int dim, float A[])
	{
		//Eigen::Vector2f buf;
		float buf = 0.0;
		for (int i = 0; i < dim; i++) {
			buf += A[i];
		}
		return buf;
	}

	EIGEN_FUNCS_API float Func1(std::vector<float> A) {
		float buf = 0.0;
		for (int i = 0; i < A.size(); i++) {
			buf += A[i];
		}
		A.data();
		return buf;
	}

	EIGEN_FUNCS_API void Addition_Vec(int dim, float a[], float b[], float ans[]) {
		Eigen::VectorXf av = Eigen::VectorXf::Zero(dim);
		Eigen::VectorXf bv = Eigen::VectorXf::Zero(dim);

		for (int i = 0; i < dim; i++) {
			av(i) = a[i];
			bv(i) = b[i];
		}

		Eigen::VectorXf ansv = av + bv;

		for (int i = 0; i < dim; i++) {
			ans[i] = ansv(i);
		}
	}

	EIGEN_FUNCS_API void InverseMat(int dim, float a[], float ans[]) {
		Eigen::MatrixXf mat = Eigen::MatrixXf::Zero(dim, dim);

		int count = 0;
		for (int c = 0; c < dim; c++) {
			for (int r = 0; r < dim; r++) {
				mat(c, r) = a[count];
				count++;
			}
		}

		mat = mat.inverse();

		count = 0;
		for (int c = 0; c < dim; c++) {
			for (int r = 0; r < dim; r++) {
				ans[count] = mat(c, r);
				count++;
			}
		}
	}

} // extern "C"


