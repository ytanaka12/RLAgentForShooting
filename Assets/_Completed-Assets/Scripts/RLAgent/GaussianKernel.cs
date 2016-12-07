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
		public double[] m_KernelCenter;
		public double m_Sigma;

		public GaussianKernel()
		{
		}

		public GaussianKernel(int state_dimension)
		{
			this.m_KernelCenter = new double[state_dimension];
		}

		public double Result(double[] s)
		{
			//error
			if (s.Length != m_KernelCenter.Length)
			{
				Debug.LogFormat("Length of kernel center is invalid");
				return -1.0d;
			}

			//Debug.LogFormat("kernel center: {0}, {1}, {2}", m_KernelCenter[0], m_KernelCenter[1], m_KernelCenter[2]);

			double buf = 0.0d;
			for (int i = 0; i < m_KernelCenter.Length; i++)
			{
				buf += Math.Pow(m_KernelCenter[i] - s[i], 2.0d);
				//Debug.LogFormat("kc - s = {0} - {1}", m_KernelCenter[i], s[i]);
			}
			double ans = Math.Exp(-buf / (2.0d * m_Sigma * m_Sigma));
			//Debug.LogFormat("ans: {0}", ans);

			return ans;
		}
	}
}