using System;
using System.Collections;

namespace MOL {

	public class Matrix : ICloneable {
		private int m_NumOfRow;
		private int m_NumOfColumn;
		public double[,] Element;

		MatOpDllWrapper IMWrapper = new MatOpDllWrapper();

		/*-------*/
		/* Clone */
		/*-------*/
		public object Clone()
		{
			return MemberwiseClone();
		}

		/*-------------------*/
		/* Get number of row */
		/*-------------------*/
		public int GetNumOfRow() {
			return m_NumOfRow;
		}

		/*----------------------*/
		/* Get number of column */
		/*----------------------*/
		public int GetNumOfColumn()
		{
			return m_NumOfColumn;
		}

		/*---------------------*/
		/* Get Array as Vector */
		/*---------------------*/
		public double[] GetVector() {
			if (m_NumOfColumn != 1) {
				return new double[1];
			}

			double[] Ans = new double[m_NumOfRow];
			for (int i = 0; i < m_NumOfRow; i++) {
				Ans[i] = Element[i, 0];
			}
			return (double[])Ans.Clone();
		}

		/*---------------------------*/
		/* Initialization of Members */
		/*---------------------------*/
		private void InitMembers(int number_of_row = 1, int number_of_column = 1) {
			m_NumOfRow = number_of_row;
			m_NumOfColumn = number_of_column;
			Element = new double[m_NumOfRow, m_NumOfColumn];
		}

		/*-------------*/
		/* Constractor */
		/*-------------*/
		public Matrix(int number_of_row = 1, int number_of_column = 1) {
			InitMembers(number_of_row, number_of_column);
			Zero();
        }

		/*-------------*/
		/* Constractor */
		/* Vector      */
		/*-------------*/
		//public Matrix(int number_of_row = 1)
		//{
		//	InitMembers(number_of_row, 1);
		//	Zero();
		//}

		/*-------------*/
		/* Constractor */
		/*-------------*/
		public Matrix(double[] vector) {
			InitMembers(vector.Length, 1);
			for (int i = 0; i < vector.Length; i++) {
				Element[i, 0] = vector[i];
			}
		}

		/*-------------*/
		/* Constractor */
		/*-------------*/
		public Matrix(double[,] matrix)
		{
			InitMembers(matrix.GetLength(0), matrix.GetLength(1));
			for (int r = 0; r < m_NumOfRow; r++)
			{
				for (int c = 0; c < m_NumOfColumn; c++)
				{
					Element[r, c] = matrix[r, c];
				}
			}
		}

		/*-------------*/
		/* Constractor */
		/*-------------*/
		public Matrix(Matrix mat)
		{
			this.Element = (double[,])mat.Element.Clone();
			this.m_NumOfRow = mat.m_NumOfRow;
			this.m_NumOfColumn = mat.m_NumOfColumn;
		}

		/*---------------------*/
		/* Set Matrix All Zero */
		/*---------------------*/
		public void Zero() {
			for (int r = 0; r < m_NumOfRow; r++) {
				for (int c = 0; c < m_NumOfColumn; c++){
					Element[r, c] = 0.0;
				}
			}
		}

		/*--------------------*/
		/* Set Matrix All One */
		/*--------------------*/
		public void One() {
			for (int r = 0; r < m_NumOfRow; r++)
			{
				for (int c = 0; c < m_NumOfColumn; c++)
				{
					Element[r, c] = 1.0;
				}
			}
		}

		/*-------------------*/
		/* Set Matrix Random */
		/* value is 0.0~1.0  */
		/*-------------------*/
		public void Random() {
			Random random = new System.Random();
			for (int r = 0; r < m_NumOfRow; r++)
			{
				for (int c = 0; c < m_NumOfColumn; c++)
				{
					Element[r, c] = random.NextDouble();
				}
			}
		}

		/*-----------------*/
		/* Identity Matrix */
		/*-----------------*/
		public void Identity()
		{
			for (int r = 0; r < m_NumOfRow; r++)
			{
				for (int c = 0; c < m_NumOfColumn; c++)
				{
					if (r == c) {
						Element[r, c] = 1.0;
					}
					else {
						Element[r, c] = 0.0;
					}
				}
			}
		}

		/*----------*/
		/* ToString */
		/*----------*/
		public override string ToString()
		{
			string str = "";
			for (int r = 0; r < m_NumOfRow; r++)
			{
				for (int c = 0; c < m_NumOfColumn; c++)
				{
					str += "\t";
					str += Element[r, c];
				}
				str += "\n";
			}

			return str;
		}

		/*------------*/
		/* Addition + */
		/*------------*/
		public static Matrix operator +(Matrix a, Matrix b)
		{
			int aDim = a.Element.Length;
			int bDim = b.Element.Length;
			if (aDim != bDim) {
				Matrix err = new Matrix();
				return err;
			}

			Matrix Ans = new Matrix(a.m_NumOfRow, a.m_NumOfColumn);

			for (int r = 0; r < a.m_NumOfRow; r++)
			{
				for (int c = 0; c < a.m_NumOfColumn; c++)
				{
					Ans.Element[r,c] = a.Element[r,c] + b.Element[r,c];
				}
			}

			return Ans;
		}

		/*-------------*/
		/* Subtraction */
		/*-------------*/
		public static Matrix operator -(Matrix a, Matrix b)
		{
			int aDim = a.Element.Length;
			int bDim = b.Element.Length;
			if (aDim != bDim)
			{
				Matrix err = new Matrix();
				return err;
			}

			Matrix Ans = new Matrix(a.m_NumOfRow, a.m_NumOfColumn);

			for (int r = 0; r < a.m_NumOfRow; r++)
			{
				for (int c = 0; c < a.m_NumOfColumn; c++)
				{
					Ans.Element[r, c] = a.Element[r, c] - b.Element[r, c];
				}
			}

			return Ans;
		}

		/*----------------*/
		/* Multiplication */
		/*----------------*/
		public static Matrix operator *(Matrix a, Matrix b)
		{
			if (a.m_NumOfColumn != b.m_NumOfRow) {
				Matrix err = new Matrix();
				return err;
			}

			Matrix Ans = new Matrix(a.m_NumOfRow, b.m_NumOfColumn);
			Ans.Zero();

			for (int r = 0; r < Ans.m_NumOfRow; r++)
			{
				for (int c = 0; c < Ans.m_NumOfColumn; c++)
				{
					for (int i = 0; i < a.m_NumOfColumn; i++) {
						Ans.Element[r, c] += a.Element[r, i] * b.Element[i, c];
					}
				}
			}

			return Ans;
		}

		/*----------------*/
		/* Multiplication */
		/*----------------*/
		public static Matrix operator *(double a, Matrix b)
		{
			Matrix Ans = (Matrix)b.Clone();

			for (int r = 0; r < Ans.m_NumOfRow; r++)
			{
				for (int c = 0; c < Ans.m_NumOfColumn; c++)
				{
					Ans.Element[r, c] = a * Ans.Element[r, c];
				}
			}

			return Ans;
		}

		/*---------------*/
		/* Transposition */
		/*---------------*/
		public Matrix Transposition()
		{
			Matrix Ans = new Matrix(m_NumOfColumn, m_NumOfRow);

			for (int r = 0; r < m_NumOfRow; r++)
			{
				for (int c = 0; c < m_NumOfColumn; c++)
				{
					Ans.Element[c, r] = Element[r, c];
				}
			}

			return Ans;
		}

		/*---------*/
		/* Inverse */
		/*---------*/
		public Matrix Inverse() {
			Matrix Ans = new Matrix(m_NumOfRow, m_NumOfColumn);
			Ans.Element = IMWrapper.InverseMatrix(Element);
			return Ans;
		}

		/*-------------*/
		/* Determinant */
		/*-------------*/
		public double Determinant()
		{
			double Ans = IMWrapper.Determinant(Element);
			return Ans;
		}
	}
}
