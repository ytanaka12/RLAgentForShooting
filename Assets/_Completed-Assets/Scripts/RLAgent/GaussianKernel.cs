using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RLProcess
{
	[Serializable]
	public class GaussianKernel
	{
		public float[] m_KernelCenter;
		public float m_Sigma;

		public GaussianKernel()
		{
		}

		public GaussianKernel(int state_dimension)
		{
			this.m_KernelCenter = new float[state_dimension];
		}

		public float Result(float[] s)
		{
			//error
			if (s.Length != m_KernelCenter.Length)
			{
				Debug.LogFormat("Length of kernel center is invalid");
				return -1.0f;
			}

			float buf = 0;
			for (int i = 0; i < m_KernelCenter.Length; i++)
			{
				buf += Mathf.Pow(m_KernelCenter[i] - s[i], 2f);
			}
			float ans = Mathf.Exp(-buf / (2f * m_Sigma));
			return ans;
		}
	}
}