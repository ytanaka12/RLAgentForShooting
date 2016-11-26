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
			GradientAscent();
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

		/*---------------------------------*/
		/* Optimization by Gradient Ascent */
		/*---------------------------------*/
		public void GradientAscent() {
			float eps = 0.1f;
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
				m_GaussianPolicyModel.m_Mean[i] += eps * gAscentMean[i];
			}
			m_GaussianPolicyModel.m_StandDev += eps * gAscentStandDev;

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
