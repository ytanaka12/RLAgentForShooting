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

		public float Result(ref float[] s)
		{
			//error
			if (s.Length != m_KernelCenter.Length)
			{
				Debug.LogFormat("Length of kernel center is invalid");
				return -1.0f;
			}

			//Debug.LogFormat("kernel center: {0}, {1}, {2}", m_KernelCenter[0], m_KernelCenter[1], m_KernelCenter[2]);

			float buf = 0;
			for (int i = 0; i < m_KernelCenter.Length; i++)
			{
				buf += Mathf.Pow(m_KernelCenter[i] - s[i], 2.0f);
				//Debug.LogFormat("kc - s = {0} - {1}", m_KernelCenter[i], s[i]);
			}
			float ans = Mathf.Exp(-buf / (2f * m_Sigma));
			//Debug.LogFormat("ans: {0}", ans);

			return ans;
		}
	}
}