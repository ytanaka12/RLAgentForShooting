using UnityEngine;
using System.Collections;

public class ComputeMatrix {

	/*------------*/
	/* add vector */
	/*------------*/
	public double[] AddVectorB2VectorA(ref double[] A, ref double[] B)
	{
		double[] ans = new double[A.Length];

		//Debug.LogFormat("length: {0} / {1} / {2}", A.Length, B.Length, ans.Length);

		for (int i = 0; i < A.Length; i++)
		{
			ans[i] = 0.0f;
		}

		for (int i = 0; i < A.Length; i++)
		{
			ans[i] = A[i] + B[i];
		}
		return (double[])ans.Clone();
	}

	/*-------------------*/
	/* vector * vector T */
	/*-------------------*/
	public double[,] VecVecT(double[] A, double[] BT)
	{
		double[,] mat = new double[A.Length, BT.Length];

		for (int c = 0; c < A.Length; c++)
		{
			for (int r = 0; r < BT.Length; r++)
			{
				mat[c, r] = A[c] * BT[r];
			}
		}
		return (double[,])mat.Clone();
	}

	/*-------------------*/
	/* Add Matrix Matrix */
	/*-------------------*/
	public double[,] AddMatrixMatrix(double[,] A, double[,] B)
	{
		int dim = (int)Mathf.Sqrt(A.Length);
		double[,] mat = new double[dim, dim];

		for (int i = 0; i < dim; i++)
		{
			for (int j = 0; j < dim; j++)
			{
				mat[i, j] = A[i, j] + B[i, j];
			}
		}

		return (double[,])mat.Clone();
	}

	/*-------------------------------*/
	/* Multiple Coefficient * Matrix */
	/*-------------------------------*/
	public double[,] MultipleMatrix(double Coef, double[,] A)
	{
		int dim = (int)Mathf.Sqrt(A.Length);
		double[,] mat = new double[dim, dim];

		for (int i = 0; i < dim; i++)
		{
			for (int j = 0; j < dim; j++)
			{
				mat[i, j] = Coef * A[i, j];
			}
		}

		return (double[,])mat.Clone();
	}

	public double[] MultipleMatrixVector(double[,] mat, double[] vec)
	{
		int dim = vec.Length;
		double[] ans = new double[dim];

		for (int i = 0; i < dim; i++)
		{
			for (int j = 0; j < dim; j++)
			{
				ans[i] += mat[i, j] * vec[j];
			}
		}

		return (double[])ans.Clone();
	}
}
