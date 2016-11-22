using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace RLProcess
{
	[Serializable]
	public class GaussianPolicyModel
	{
		public float[] m_Mean;
		public float m_StandDev;
		public GaussianKernel[] m_GaussianKernel;

		private float[] m_State;
		private float m_Action;

		private string serializeDataPath;

		public GaussianPolicyModel()
		{
		}
	
		/*  */
		public GaussianPolicyModel(int num_of_kernel, int state_dimension)
		{
			//initialize Policy Model
			m_Mean = new float[num_of_kernel];
			m_StandDev = 1.0f;
			m_State = new float[state_dimension];

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
		public void SetState(float[] state) {
			m_State = (float[])state.Clone();
		}

		/*------------*/
		/* Set Action */
		/*------------*/
		public void SetAction(float action) {
			m_Action = action;
		}

		/*------------------------------------*/
		/* Calculate Gradient respect to Mean */
		/*------------------------------------*/
		float[] CalcGradientMean() {
			float[] basis_func_val = new float[m_Mean.Length];

			//calc basis function values
			for (int i = 0; i < basis_func_val.Length; i++) {
				basis_func_val[i] = m_GaussianKernel[i].Result(m_State);
			}

			//calc... meanT basis_func
			float mb = 0.0f;
			for (int i = 0; i < m_Mean.Length; i++) {
				mb += m_Mean[i] * basis_func_val[i];
			}

			//calc... ( a - mb ) / sigma^2
			float a_mb__sig = ( m_Action - mb ) / Mathf.Pow( m_StandDev, 2.0f);

			//calc Answer
			float[] ans = new float[m_Mean.Length];
			for (int i = 0; i < ans.Length; i++) {
				ans[i] = a_mb__sig * basis_func_val[i];
			}

			return (float[])ans.Clone();
		}

		/* Calculate Gradient respect to Standard Deviation */
		float CalcgradientStandDev() {
			float[] basis_func_val = new float[m_Mean.Length];

			//calc basis function values
			for (int i = 0; i < basis_func_val.Length; i++)
			{
				basis_func_val[i] = m_GaussianKernel[i].Result(m_State);
			}

			//calc... meanT basis_func
			float mb = 0.0f;
			for (int i = 0; i < m_Mean.Length; i++)
			{
				mb += m_Mean[i] * basis_func_val[i];
			}

			//calc... ( a - mb )^2
			float a_mb2 = Mathf.Pow( m_Action - mb, 2.0f);

			//calc Answer
			float ans = (a_mb2 - Mathf.Pow(m_StandDev, 2.0f)) / Mathf.Pow(m_StandDev, 3.0f);

			return ans;
		}

		public void OutputParamtersToXML()
		{
			Debug.LogFormat("ok");
			serializeDataPath = Application.dataPath + "/GPMData.xml";
			XmlUtil.Seialize<GaussianPolicyModel>(serializeDataPath, (GaussianPolicyModel)this.MemberwiseClone());
		}

		public void InputParametersFromXML()
		{
			serializeDataPath = Application.dataPath + "/GPMData.xml";
			GaussianPolicyModel buf = XmlUtil.Deserialize<GaussianPolicyModel>(serializeDataPath);
			int num_of_kernel = buf.m_GaussianKernel.Length;
			int state_dimension = buf.m_GaussianKernel[0].m_KernelCenter.Length;
			Debug.LogFormat("num: {0} / dim: {1}", num_of_kernel, state_dimension);
			m_Mean = new float[num_of_kernel];
			for (int i = 0; i < num_of_kernel; i++)
			{
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
			for (int i = 0; i < state_dimension; i++)
			{
				float kc = m_GaussianKernel[1].m_KernelCenter[i];
				Debug.LogFormat("kc: {0}", kc);
			}
		}
	}
}