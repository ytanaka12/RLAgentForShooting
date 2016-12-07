using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Runtime.InteropServices;   //Dll


public class EigenFunc /*: MonoBehaviour*/
{

	[DllImport("EigenFuncs")]
	private static extern void InverseMat_FullPivLUd(int dim, double[] a, double[] ans);

	private void Matrix2Array(double[,] Mat, ref double[] Arr)
	{
		int count = 0;
		int dim = (int)Mathf.Sqrt(Mat.Length);
		for (int c = 0; c < dim; c++)
		{
			for (int r = 0; r < dim; r++)
			{
				Arr[count] = Mat[c, r];
				count++;
			}
		}
	}

	private void Array2Matrix(double[] Arr, double[,] Mat)
	{
		int count = 0;
		int dim = (int)Mathf.Sqrt(Mat.Length);
		for (int c = 0; c < dim; c++)
		{
			for (int r = 0; r < dim; r++)
			{
				Mat[c, r] = Arr[count];
				count++;
			}
		}
	}

	public double[,] InverseMatrix(double[,] Mat) {
		double[] arr = new double[Mat.Length];
		int dim = (int)Mathf.Sqrt(Mat.Length);
		double[,] AnsMat = new double[dim, dim];

		Matrix2Array(Mat, ref arr);
		double[] ansArr = new double[Mat.Length];
		//InverseMat(dim, arr, ansArr);
		//InverseMat_FullPivLU(dim, arr, ansArr);
		InverseMat_FullPivLUd(dim, arr, ansArr);
		Array2Matrix(ansArr, AnsMat);

		return (double[,])AnsMat.Clone();
	}

	void sampleSource() {
		/* EigenFunc.dll Sample */
		EigenFunc eigen = new EigenFunc();
		double[,] bufMat = new double[3, 3] { { 1f, 2f, 1f }, { 2f, 1f, 0f }, { 1f, 1f, 2f } };
		double[,] AnsMat = new double[3, 3];
		Debug.LogFormat("bufMat: {0}", bufMat[0, 0]);
		AnsMat = eigen.InverseMatrix(bufMat);
		Debug.LogFormat("AnsMat: {0}", AnsMat[2, 2]);
	}

	//void __Start()
	//{
	//	double[] A = new double[3] {1f, 2f, 10f };
	//	double c = TestFunc(3, A);
	//	Debug.LogFormat("TestFunc: {0}", c);

	//	double[] B = new double[3] {3f, 8f, 12f };

	//	double[] ans = new double[3];
	//	Addition_Vec(3, A, B, ans);

	//	double[] C = new double[9] {1f, 2f, 1f, 2f, 1f, 0f, 1f, 1f, 2f };
	//	//for (int i = 0; i < 9; i++) {
	//	//	C[i] = UnityEngine.Random.Range(0f, 10f);
	//	//}
	//	Debug.LogFormat("C: {0},{1},{2},{3},{4},{5},{6},{7},{8}", C[0], C[1], C[2], C[3], C[4], C[5], C[6], C[7], C[8]);

	//	double[] Mat = new double[9];
	//	dllInverseMat(3, C, Mat);

	//	Debug.LogFormat("Mat: {0},{1},{2},{3},{4},{5},{6},{7},{8}", Mat[0], Mat[1], Mat[2], Mat[3], Mat[4], Mat[5], Mat[6], Mat[7], Mat[8]);

	//	double[,] bufMat = new double[3, 3];
	//	Debug.LogFormat("mat length: {0}", Mathf.Sqrt( bufMat.Length));

	//	//Debug.LogFormat("ans: {0}, {1}, {2}", ans[0], ans[1], ans[2]);
	//}
}
