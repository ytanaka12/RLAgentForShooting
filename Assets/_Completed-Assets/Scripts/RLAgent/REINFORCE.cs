using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RLProcess
{

	public class REINFORCE : MonoBehaviour
	{
		/* Must need to scecify these parameter */
		//private const int m_NumKernel = 3;
		private const int m_StateDim = 2;
		private float m_eps = 0.1f;

		public class OneFrameData : ICloneable
		{
			public float[] State = new float[m_StateDim];
			public float Action;
			public float Reward;
			public object Clone()
			{
				return MemberwiseClone();
			}
		}

		public List<List<OneFrameData>> m_Trajectories = new List<List<OneFrameData>>();

		private List<TankInfo> m_LogAITank = new List<TankInfo>();
		private List<TankInfo> m_LogHuTank = new List<TankInfo>();

		public GaussianPolicyModel m_GaussianPolicyModel = new GaussianPolicyModel();

		public string fNameGPM_XML = "/GPMData.xml";

		void Start() {
			//m_GaussianPolicyModel.InputParametersFromXML(fNameGPM_XML);
			//EigenFunc eigen = new EigenFunc();
			//float[,] bufMat = new float[3,3]{ { 1f, 2f, 1f },{ 2f, 1f, 0f },{ 1f, 1f, 2f } };
			//float[,] AnsMat = new float[3, 3];
			//Debug.LogFormat("bufMat: {0}", bufMat[0,0]);
			//eigen.InverseMatrix(bufMat, ref AnsMat);
			//Debug.LogFormat("AnsMat: {0}", AnsMat[0,0]);
		}

		/*--------------------*/
		/* set log of AI Tank */
		/*--------------------*/
		public void SetLogAITank(List<TankInfo> log_ai_tank) {
			for (int i = 0; i < log_ai_tank.Count; i++) {
				m_LogAITank.Add((TankInfo)log_ai_tank[i].Clone());
			}
		}

		/*-----------------------*/
		/* set log of Human Tank */
		/*-----------------------*/
		public void SetLogHuTank(List<TankInfo> log_hu_tank)
		{
			for (int i = 0; i < log_hu_tank.Count; i++)
			{
				m_LogHuTank.Add((TankInfo)log_hu_tank[i].Clone());
			}
		}

		/* set trajectories */
		public void SetTrajectories(List<List<OneFrameData>> trajectories) {
			m_Trajectories = new List<List<OneFrameData>>(trajectories);
		}

		/*------------------------------------------*/
		/* calculate trajectory for policy gradient */
		/*------------------------------------------*/
		public bool CalcTrajectory() {
			if (m_LogAITank.Count < 1 || m_LogHuTank.Count < 1 || m_LogAITank.Count != m_LogHuTank.Count) {
				return false;
			}
			//Debug.LogFormat("ok: {0}", m_LogAITank[5].m_State.m_Position);

			//calc relative position from AI to Human
			for (int i = 0; i < m_LogAITank.Count; i++) {
				Vector3 bufPos = m_LogHuTank[i].m_State.m_Position - m_LogAITank[i].m_State.m_Position;
				Vector3 bufEul = m_LogAITank[i].m_State.m_Euler;
				bufPos = Quaternion.Euler(-bufEul) * bufPos;

				TankInfo bufTankInfo = new TankInfo();
				bufTankInfo = (TankInfo)m_LogAITank[i].Clone();
				bufTankInfo.m_State.m_Position = bufPos;

				bufTankInfo.m_State.m_Euler = m_LogHuTank[i].m_State.m_Euler - m_LogAITank[i].m_State.m_Euler;

				//m_Trajectory.Add((TankInfo)bufTankInfo.Clone());
			}
			//WriteTrajectory(m_Trajectory);

			return true;
		}

		/*---------------*/
		/* Run REINFORCE */
		/*---------------*/
		public void RunREINFORCE() {
			//GradientAscent();
			NaturalGradientAscent();
		}

		/*------------*/
		/* add vector */
		/*------------*/
		float[] AddVectorB2VectorA(ref float[] A, ref float[] B)
		{
			float[] ans = new float[A.Length];

			//Debug.LogFormat("length: {0} / {1} / {2}", A.Length, B.Length, ans.Length);

			for (int i = 0; i < A.Length; i++)
			{
				ans[i] = 0.0f;
			}

			for (int i = 0; i < A.Length; i++)
			{
				ans[i] = A[i] + B[i];
			}
			return (float[])ans.Clone();
		}

		/*-------------------*/
		/* vector * vector T */
		/*-------------------*/
		float[,] VecVecT(float[] A, float[] BT) {
			float[,] mat = new float[A.Length, BT.Length];

			for (int c = 0; c < A.Length; c++)
			{
				for (int r = 0; r < BT.Length; r++)
				{
					mat[c, r] = A[c] * BT[r];
				}
			}
			return (float[,])mat.Clone();
		}

		/*-------------------*/
		/* Add Matrix Matrix */
		/*-------------------*/
		float[,] AddMatrixMatrix(float[,] A, float[,] B) {
			int dim = (int)Mathf.Sqrt(A.Length);
			float[,] mat = new float[dim, dim];

			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					mat[i, j] = A[i, j] + B[i, j];
				}
			}

			return (float[,])mat.Clone();
		}

		/*-------------------------------*/
		/* Multiple Coefficient * Matrix */
		/*-------------------------------*/
		float[,] MultipleMatrix(float Coef, float[,] A)
		{
			int dim = (int)Mathf.Sqrt(A.Length);
			float[,] mat = new float[dim, dim];

			for (int i = 0; i < dim; i++)
			{
				for (int j = 0; j < dim; j++)
				{
					mat[i, j] = Coef * A[i, j];
				}
			}

			return (float[,])mat.Clone();
		}

		float[] MultipleMatrixVector(float[,] mat, float[] vec) {
			int dim = vec.Length;
			float[] ans = new float[dim];

			for (int i = 0; i < dim; i++) {
				for (int j = 0; j < dim; j++) {
					ans[i] += mat[i, j] * vec[j];
				}
			}

			return (float[])ans.Clone();
		}

		/*---------------------------------*/
		/* Optimization by Gradient Ascent */
		/*---------------------------------*/
		public void GradientAscent() {
			float[] gAscentMean = new float[m_GaussianPolicyModel.m_Mean.Length];
			//initialize
			for (int i = 0; i < gAscentMean.Length; i++) {
				gAscentMean[i] = 0.0f;
			}
			float gAscentStandDev = 0.0f;

			float[] befMean = new float[m_GaussianPolicyModel.m_Mean.Length];
			befMean = (float[])m_GaussianPolicyModel.m_Mean.Clone();
			float befStandDev = new float();
			befStandDev = m_GaussianPolicyModel.m_StandDev;

			for (int n = 0; n < m_Trajectories.Count ; n++) {
				for (int t = 0; t < m_Trajectories[n].Count; t++) {
					//set state
					m_GaussianPolicyModel.SetState((float[])m_Trajectories[n][t].State.Clone());

					//set action
					m_GaussianPolicyModel.SetAction(m_Trajectories[n][t].Action);
					//m_GaussianPolicyModel.SetAction(1.6f);

					//calculate Gradient
					float[] bufMean = m_GaussianPolicyModel.CalcGradientMean();
                    gAscentMean = AddVectorB2VectorA( ref gAscentMean, ref bufMean );
					gAscentStandDev += m_GaussianPolicyModel.CalcgradientStandDev();
				}
			}

			//Ascent
			for (int i = 0; i < gAscentMean.Length; i++)
			{
				m_GaussianPolicyModel.m_Mean[i] += m_eps * gAscentMean[i];
			}
			m_GaussianPolicyModel.m_StandDev += m_eps * gAscentStandDev;

			/* limit */
			if (m_GaussianPolicyModel.m_StandDev < 1.0f) {
				m_GaussianPolicyModel.m_Mean = (float[])befMean.Clone();
				m_GaussianPolicyModel.m_StandDev = befStandDev;
			}

			m_GaussianPolicyModel.SetState((float[])m_Trajectories[0][10].State.Clone());
            float buf = m_GaussianPolicyModel.CalcActionMean();
			//Debug.LogFormat("ActionMean: {0}", buf);

			//terminate judge
			//if (gAscentMean[0] < 0.1f)
			//{
			//	return;
			//}
		}

		/*-----------------------------------------*/
		/* Optimization by Natural Gradient Ascent */
		/*-----------------------------------------*/
		public void NaturalGradientAscent()
		{
			//initialize
			float[] gAscentMean = new float[m_GaussianPolicyModel.m_Mean.Length];
			for (int i = 0; i < gAscentMean.Length; i++)
			{
				gAscentMean[i] = 0.0f;
			}
			float gAscentStandDev = 0.0f;

			//temporary memory
			float[] befMean = new float[m_GaussianPolicyModel.m_Mean.Length];
			befMean = (float[])m_GaussianPolicyModel.m_Mean.Clone();
			float befStandDev = new float();
			befStandDev = m_GaussianPolicyModel.m_StandDev;

			//vector for calculation of Fisher information matrix
			float[] vFisherParam = new float[m_GaussianPolicyModel.m_Mean.Length + 1];  // + StandDev
			float[,] FisherMat = new float[m_GaussianPolicyModel.m_Mean.Length + 1, m_GaussianPolicyModel.m_Mean.Length + 1];

			//calc
			for (int n = 0; n < m_Trajectories.Count; n++)
			{
				float[] bufFisherMean = new float[m_GaussianPolicyModel.m_Mean.Length];
				float bufFisherStandDev = 0.0f;
				//calc gradient like previous
				for (int t = 0; t < m_Trajectories[n].Count; t++)
				{
					//set state
					m_GaussianPolicyModel.SetState((float[])m_Trajectories[n][t].State.Clone());

					//set action
					m_GaussianPolicyModel.SetAction(m_Trajectories[n][t].Action);
					//m_GaussianPolicyModel.SetAction(1.6f);

					//calculate Gradient
					float[] bufMean = m_GaussianPolicyModel.CalcGradientMean();
					gAscentMean = AddVectorB2VectorA(ref gAscentMean, ref bufMean);
					bufFisherMean = AddVectorB2VectorA(ref bufFisherMean, ref bufMean);
					float bufStandDev = m_GaussianPolicyModel.CalcgradientStandDev();
					gAscentStandDev += bufStandDev;
					bufFisherStandDev += bufStandDev;

					//for calc Fisher Information Matrix
					//bufFisherMean = (float[])bufMean.Clone();
					//bufFisherStandDev = bufStandDev;
                }

				//calc Fisher Information Matrix
				for (int i = 0; i < m_GaussianPolicyModel.m_Mean.Length; i++) {
					vFisherParam[i] = bufFisherMean[i];
				}
				vFisherParam[m_GaussianPolicyModel.m_Mean.Length] = bufFisherStandDev;
				float[,] bufMat = new float[m_GaussianPolicyModel.m_Mean.Length + 1, m_GaussianPolicyModel.m_Mean.Length + 1];
				bufMat = VecVecT(vFisherParam, vFisherParam);
				FisherMat = AddMatrixMatrix(FisherMat, bufMat);
            }

			// Fisher Information Matrix !!
			FisherMat = MultipleMatrix(1.0f / (float)m_Trajectories.Count, FisherMat);
			//for (int i = 0; i < 100; i++)
			//{
			//	Debug.LogFormat("fisher mat[{0}]: {1}", i, FisherMat[i, i]);
			//}

			//Inverse Fisher Information Matrix
			EigenFunc eigen = new EigenFunc();
			float[,] InvFisherMat = eigen.InverseMatrix(FisherMat);

			for (int i = 0; i < 100; i++)
			{
				Debug.LogFormat("inv fisher mat[{0}]: {1}", i, InvFisherMat[i, i]);
			}

			//Calc Natural Gradient
			float[] provGradientVec = new float[m_GaussianPolicyModel.m_Mean.Length + 1];  // + StandDev
			for (int i = 0; i < m_GaussianPolicyModel.m_Mean.Length; i++)
			{
				provGradientVec[i] = gAscentMean[i];
			}
			provGradientVec[m_GaussianPolicyModel.m_Mean.Length] = gAscentStandDev;
			float[] NaturalGradientVec = new float[m_GaussianPolicyModel.m_Mean.Length + 1];    //NaturalGradient
			NaturalGradientVec = MultipleMatrixVector(InvFisherMat, provGradientVec);

			for (int i = 0; i < m_GaussianPolicyModel.m_Mean.Length; i++)
			{
				gAscentMean[i] = NaturalGradientVec[i];
			}
			gAscentStandDev = NaturalGradientVec[m_GaussianPolicyModel.m_Mean.Length];

			//Ascent
			for (int i = 0; i < gAscentMean.Length; i++)
			{
				m_GaussianPolicyModel.m_Mean[i] += m_eps * gAscentMean[i];
			}
			m_GaussianPolicyModel.m_StandDev += m_eps * gAscentStandDev;

			/* limit */
			if (m_GaussianPolicyModel.m_StandDev < 1.0f)
			{
				m_GaussianPolicyModel.m_Mean = (float[])befMean.Clone();
				m_GaussianPolicyModel.m_StandDev = befStandDev;
			}

			m_GaussianPolicyModel.SetState((float[])m_Trajectories[0][10].State.Clone());
			float buf = m_GaussianPolicyModel.CalcActionMean();
			//Debug.LogFormat("ActionMean: {0}", buf);

			//terminate judge
			//if (gAscentMean[0] < 0.1f)
			//{
			//	return;
			//}
		}

		/*----------------------------*/
		/* Get Action based on policy */
		/*----------------------------*/
		public float GetAction(float[] state) {
			m_GaussianPolicyModel.SetState((float[])state.Clone());
			float ans = m_GaussianPolicyModel.CalcActionMean();
			return ans;
		}

		/*----------------*/
		/* write log file */
		/*----------------*/
		void WriteTrajectory(List<TankInfo> trajectory)
		{
			StreamWriter writer = null;
			writer = new StreamWriter(@"Assets/LogFiles/trajectory.csv", false, System.Text.Encoding.Default);

			for (int i = 0; i < trajectory.Count; i++)
			{
				TankStateStruct bufState = trajectory[i].m_State;
				TankActionStruct bufAction = trajectory[i].m_Action;
				float reward = trajectory[i].m_Reward;
				writer.Write(i);
				writer.Write("," + bufState.m_Position.x);
				writer.Write("," + bufState.m_Position.y);
				writer.Write("," + bufState.m_Position.z);
				writer.Write("," + bufState.m_Euler.x);
				writer.Write("," + bufState.m_Euler.y);
				writer.Write("," + bufState.m_Euler.z);
				writer.Write("," + bufAction.m_MovementInput);
				writer.Write("," + bufAction.m_TurnInput);
				writer.Write("," + bufAction.m_Fired);
				writer.Write("," + reward);
				writer.Write("\n");
			}
			writer.Flush();
			writer.Close();
		}

	}
}
