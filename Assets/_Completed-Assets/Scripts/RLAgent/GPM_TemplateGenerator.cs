using UnityEngine;
using System.Collections;

namespace RLProcess
{
	public class GPM_TemplateGenerator : MonoBehaviour
	{
		void Start()
		{
			const int numOfSize = 10;
			const int numOfKernel = numOfSize * numOfSize;
			const int stateDim = 2;
			const float lengthX = 40.0f;	//length of area where the kernels are located
			const float lengthY = 40.0f;    //length of area where the kernels are located
			GaussianPolicyModel GPM = new GaussianPolicyModel(numOfKernel, stateDim);

			//Set GPM Parameters
			for (int i = 0; i < numOfKernel; i++)
			{
				GPM.m_Mean[i] = 0.0f;
			}
			GPM.m_StandDev = 500d;

			//Set Kernels Parameters
			int iKernel = 0;
			for (int i = 0; i < numOfSize; i++)
			{
				float x = lengthX / (float)numOfSize * (float)i - lengthX / 2f;
				for (int j = 0; j < numOfSize; j++)
				{
					float y = lengthY / (float)numOfSize * (float)j - lengthY / 2f;
					GPM.m_GaussianKernel[iKernel].m_KernelCenter[0] = x;
					GPM.m_GaussianKernel[iKernel].m_KernelCenter[1] = y;
					//GPM.m_GaussianKernel[iKernel].m_KernelCenter[2] = 0.0f;
					GPM.m_GaussianKernel[iKernel].m_Sigma = 2d;
					iKernel++;
				}
			}
			GPM.OutputParamtersToXML("/GPMs/GPM_k100_40x40.xml");
		}
	}
}