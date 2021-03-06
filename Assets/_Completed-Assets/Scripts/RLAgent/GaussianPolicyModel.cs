﻿using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MOL;

namespace RLProcess
{
	[Serializable]
	public class GaussianPolicyModel : ICloneable
	{
		public double[] m_Mean;
		public double m_StandDev;
		public GaussianKernel[] m_GaussianKernel;

		private double[] m_State;
		private double m_Action;

		private string serializeDataPath;

		/*-------*/
		/* Clone */
		/*-------*/
		public object Clone()
		{
			return MemberwiseClone();
		}

		public GaussianPolicyModel() {
		}
	
		/*  */
		public GaussianPolicyModel(int num_of_kernel, int state_dimension)
		{
			//initialize Policy Model
			m_Mean = new double[num_of_kernel];
			m_StandDev = 1.0f;
			m_State = new double[state_dimension];

			//initialize Gaussian Kernel
			m_GaussianKernel = new GaussianKernel[num_of_kernel];
			for (int i = 0; i < num_of_kernel; i++)
			{
				m_GaussianKernel[i] = new GaussianKernel(state_dimension);
			}
		}

		/*-----------*/
		/* Set State */
		/*-----------*/
		public void SetState(double[] state) {
			m_State = new double[state.Length];
			m_State = (double[])state.Clone();
		}

		/*------------*/
		/* Set Action */
		/*------------*/
		public void SetAction(double action) {
			m_Action = action;
		}

		/* calculate Mean of Action */
		public double CalcActionMean()
		{
			double[] basis_func_val = new double[m_Mean.Length];

			//calc basis function values
			for (int i = 0; i < basis_func_val.Length; i++)
			{
				basis_func_val[i] = m_GaussianKernel[i].Result(m_State);
			}

			//calc... meanT basis_func
			double mb = 0.0d;
			for (int i = 0; i < m_Mean.Length; i++)
			{
				mb += m_Mean[i] * basis_func_val[i];
			}
			//Debug.LogFormat("mb: {0}", mb);
			return mb;
		}

		/*------------------------------------*/
		/* Calculate Gradient respect to Mean */
		/*------------------------------------*/
		public double[] CalcGradientMean() {
			double[] basis_func_val = new double[m_Mean.Length];
			//Debug.LogFormat("mean.length: {0}", m_Mean.Length);

			//calc basis function values
			for (int i = 0; i < basis_func_val.Length; i++) {
				basis_func_val[i] = m_GaussianKernel[i].Result(m_State);
			}
			//Debug.LogFormat("bf value: {0}, {1}, {2}", basis_func_val[0], basis_func_val[1], basis_func_val[2]);

			//calc... meanT basis_func
			double mb = 0.0f;
			for (int i = 0; i < m_Mean.Length; i++) {
				mb += m_Mean[i] * basis_func_val[i];
			}

			//calc... ( a - mb ) / sigma^2
			double a_mb__sig = ( m_Action - mb ) / Math.Pow( m_StandDev, 2.0f);

			//calc Answer
			double[] ans = new double[m_Mean.Length];
			for (int i = 0; i < ans.Length; i++) {
				ans[i] = a_mb__sig * basis_func_val[i];
			}

			return (double[])ans.Clone();
		}

		/*--------------------------------------------------*/
		/* Calculate Gradient respect to Standard Deviation */
		/*--------------------------------------------------*/
		public double CalcgradientStandDev() {
			double[] basis_func_val = new double[m_Mean.Length];

			//calc basis function values
			for (int i = 0; i < basis_func_val.Length; i++)
			{
				basis_func_val[i] = m_GaussianKernel[i].Result(m_State);
			}

			//calc... meanT basis_func
			double mb = 0.0f;
			for (int i = 0; i < m_Mean.Length; i++)
			{
				mb += m_Mean[i] * basis_func_val[i];
			}

			//calc... ( a - mb )^2
			double a_mb2 = Math.Pow( m_Action - mb, 2.0f);

			//calc Answer
			double ans = (a_mb2 - Math.Pow(m_StandDev, 2.0f)) / Math.Pow(m_StandDev, 3.0f);

			return ans;
		}

		/*--------------------------------------------------------------*/
		/* Fisher Information Matrix (Under mild regularity conditions) */
		/*--------------------------------------------------------------*/
		public Matrix FisherInfoMatrix() {
			Matrix Ans = new Matrix(m_Mean.Length + 1, m_Mean.Length + 1);
			Matrix basis_func_val = new Matrix(m_Mean.Length);

			//calc basis function values
			for (int i = 0; i < m_Mean.Length; i++)
			{
				basis_func_val[i] = m_GaussianKernel[i].Result(m_State);
			}

			//calc... meanT basis_func
			double mb = 0.0f;
			for (int i = 0; i < m_Mean.Length; i++)
			{
				mb += m_Mean[i] * basis_func_val[i];
			}

			//calc... ( a - mb )^2
			double a_mb2 = Math.Pow(m_Action - mb, 2.0);

			double aa = - basis_func_val[0] * basis_func_val[0] / Math.Pow(m_StandDev, 2.0);
			double bb = -2.0 * (m_Action - mb) * basis_func_val[0] / Math.Pow(m_StandDev, 3.0);

			double cc = 2.0 * (m_Action - mb) * ( - basis_func_val[0] ) / Math.Pow(m_StandDev, 3.0);
			double dd = -3.0 * a_mb2 / Math.Pow(m_StandDev, 4.0) + 1.0 / Math.Pow(m_StandDev, 2.0);

			for (int r = 0; r < m_Mean.Length; r++) {
				for (int c = 0; c < m_Mean.Length; c++){
					Ans[r, c] = -basis_func_val[c] / Math.Pow(m_StandDev, 2.0) * basis_func_val[r];
				}
			}

			for (int i = 0; i < m_Mean.Length; i++) {
				Ans[m_Mean.Length, i] = -2.0 * (m_Action - mb) / Math.Pow(m_StandDev, 3.0) * basis_func_val[i];
				Ans[i, m_Mean.Length] = Ans[m_Mean.Length, i];
			}

			Ans[m_Mean.Length, m_Mean.Length] = -3.0 * a_mb2 / Math.Pow(m_StandDev, 4.0) + 1.0 / Math.Pow(m_StandDev, 2.0);

			return Ans;
		}

		public void OutputParamtersToXML(string file_path)
		{
			Debug.LogFormat("output parameter to xml file");
			serializeDataPath = Application.dataPath + file_path;
			XmlUtil.Seialize<GaussianPolicyModel>(serializeDataPath, (GaussianPolicyModel)this.MemberwiseClone());
		}

		public void InputParametersFromXML(string file_path)
		{
			serializeDataPath = Application.dataPath + file_path;
			GaussianPolicyModel buf = XmlUtil.Deserialize<GaussianPolicyModel>(serializeDataPath);
			int num_of_kernel = buf.m_GaussianKernel.Length;
			int state_dimension = buf.m_GaussianKernel[0].m_KernelCenter.Length;
			Debug.LogFormat("loarding xml info... num: {0} / dim: {1}", num_of_kernel, state_dimension);
			m_Mean = new double[num_of_kernel];
			for (int i = 0; i < num_of_kernel; i++)
			{
				//Debug.LogFormat("i = [{0}]", i);
				m_Mean[i] = buf.m_Mean[i];
			}
			m_StandDev = buf.m_StandDev;
			m_GaussianKernel = new GaussianKernel[num_of_kernel];
			for (int i = 0; i < num_of_kernel; i++)
			{
				m_GaussianKernel[i] = new GaussianKernel(state_dimension);
				m_GaussianKernel[i] = (GaussianKernel)buf.m_GaussianKernel[i];
			}
			//Debug.LogFormat("okok");
			//for (int i = 0; i < state_dimension; i++)
			//{
			//	double kc = m_GaussianKernel[1].m_KernelCenter[i];
			//	//Debug.LogFormat("kc: {0}", kc);
			//}
		}
	}
}