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

		private string serializeDataPath;

		public GaussianPolicyModel()
		{
		}

		public GaussianPolicyModel(int num_of_kernel, int state_dimension)
		{
			//initialize Policy Model
			m_Mean = new float[num_of_kernel];
			m_StandDev = 1.0f;

			//initialize Gaussian Kernel
			m_GaussianKernel = new GaussianKernel[num_of_kernel];
			for (int i = 0; i < num_of_kernel; i++)
			{
				m_GaussianKernel[i] = new GaussianKernel(state_dimension);
			}
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