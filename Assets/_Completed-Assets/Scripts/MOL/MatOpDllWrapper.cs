using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Runtime.InteropServices;   //Dll


public class MatOpDllWrapper
{

	[DllImport("MatOp")]
	private static extern void InverseMat_FullPivLUd(int dim, double[] a, double[] ans);

	[DllImport("MatOp")]
	private static extern void Determinant(int dim, double[] a, ref double ans);

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
		//Detect Invalid
		if (Mat.GetLength(0) != Mat.GetLength(1)) {
			Debug.LogFormat("The Matrix needs to be Square Matrix when using InverseMatrix()");
			return (double[,])Mat.Clone();
		}

		int dim = Mat.GetLength(0);					//Dimension of Matrix
		double[] arr = new double[Mat.Length];		//One dimension array to include elements of matrix
		double[,] AnsMat = new double[dim, dim];	//Matrix after applying Inverse process

		Matrix2Array(Mat, ref arr);					//Convert Matrix to Array
		double[] ansArr = new double[Mat.Length];	//One dimension array to include elements of matrix after applying Inverse process
		InverseMat_FullPivLUd(dim, arr, ansArr);    //Applying Inverse process, and get array applied the process
		Array2Matrix(ansArr, AnsMat);               //Convert array to Matrix

		return (double[,])AnsMat.Clone();
	}

	public double Determinant(double[,] Mat) {
		//Detect Invalid
		if (Mat.GetLength(0) != Mat.GetLength(1))
		{
			Debug.LogFormat("The Matrix needs to be Square Matrix when using InverseMatrix()");
			return 0.0;
		}

		int dim = Mat.GetLength(0);                 //Dimension of Matrix
		double[] arr = new double[Mat.Length];      //One dimension array to include elements of matrix

		Matrix2Array(Mat, ref arr);                 //Convert Matrix to Array
		double Ans = new double();   //One dimension array to include elements of matrix after applying Inverse process
		Determinant(dim, arr, ref Ans);    //Applying Inverse process, and get array applied the process

		return Ans;
	}
}
