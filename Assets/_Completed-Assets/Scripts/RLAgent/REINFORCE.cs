using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace RLProcess
{
	public class REINFORCE : MonoBehaviour
	{
		public class StateClass
		{
			float[] State = new float[3];
		}
		public class OneFrameData
		{
			StateClass States;
			float Action;
			float Reward;
		}
		private List<List<OneFrameData>> m_TrajectoryMovement = new List<List<OneFrameData>>();
		private List<List<OneFrameData>> m_TrajectoryTurn = new List<List<OneFrameData>>();
		private List<List<OneFrameData>> m_TrajectoryFired = new List<List<OneFrameData>>();

		private List<TankInfo> m_LogAITank = new List<TankInfo>();
		private List<TankInfo> m_LogHuTank = new List<TankInfo>();
		private List<TankInfo> m_Trajectory = new List<TankInfo>();

		public GaussianPolicyModel m_GaussianPolicyModel = new GaussianPolicyModel();

		public GaussianPolicyModel m_GPM_MovementInput = new GaussianPolicyModel();
		public GaussianPolicyModel m_GPM_TurnInput = new GaussianPolicyModel();
		public GaussianPolicyModel m_GPM_Fired = new GaussianPolicyModel();
		//private const int m_NumKernel = 3;
		//private const int m_StateDim = 3;
		//public GaussianPolicyModel m_GaussianPolicyModel
		//	= new GaussianPolicyModel(m_NumKernel, m_StateDim);

		void Start() {
			m_GPM_MovementInput.InputParametersFromXML("/GPMData.xml");
			m_GPM_TurnInput.InputParametersFromXML("/GPMData.xml");
			m_GPM_Fired.InputParametersFromXML("/GPMData.xml");
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

				m_Trajectory.Add((TankInfo)bufTankInfo.Clone());
			}
			WriteTrajectory(m_Trajectory);

			Debug.LogFormat("output trajectory to xml file");
			string serializeDataPath = Application.dataPath + "/trajectory.xml";
			XmlUtil.Seialize<List<TankInfo>>(serializeDataPath, (List<TankInfo>)m_Trajectory);

			return true;
		}

		/*---------------*/
		/* Run REINFORCE */
		/*---------------*/
		public void RunREINFORCE() {
			GradientAscent(m_GaussianPolicyModel);
			m_GaussianPolicyModel.OutputParamtersToXML("/GPMData.xml");
		}

		/*---------------------------------------*/
		/* Prepare state vector, class to vector */
		/*---------------------------------------*/
		float[] PrepareStateVector(int n, int t) {
			List<float> state = new List<float>();
			state.Add(m_Trajectory[t].m_State.m_Position.x);
			//state.Add(m_Trajectory[t].m_State.m_Position.y);
			state.Add(m_Trajectory[t].m_State.m_Position.z);
			//state.Add(m_Trajectory[t].m_State.m_Euler.x);
			state.Add(m_Trajectory[t].m_State.m_Euler.y);
			//state.Add(m_Trajectory[t].m_State.m_Euler.z);

			float[] stateArray = state.ToArray();

			return (float[])stateArray.Clone();
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
		public void GradientAscent(GaussianPolicyModel gpm) {
			float eps = 0.05f;
			float[] gAscentMean = new float[3] { 0.0f, 0.0f, 0.0f};
			float gAscentStandDev = 0.0f;

			for (int n = 0; n < 1; n++) {
				for (int t = 0; t < m_Trajectory.Count; t++) {
					//set state
					gpm.SetState(PrepareStateVector(n, t));

					//set action
					gpm.SetAction(m_Trajectory[t].m_Action.m_MovementInput);
					//gpm.SetAction(5.6f);

					//calculate Gradient
					float[] bufMean = gpm.CalcGradientMean();
                    gAscentMean = AddVectorB2VectorA( ref gAscentMean, ref bufMean );
					gAscentStandDev += gpm.CalcgradientStandDev();
				}
			}

			//Ascent
			for (int i = 0; i < gAscentMean.Length; i++)
			{
				gpm.m_Mean[i] += eps * gAscentMean[i];
			}
			gpm.m_StandDev += eps * gAscentStandDev;

			Debug.LogFormat("ActionMean: {0}", gpm.CalcActionMean());

			//terminate judge
			//if (gAscentMean[0] < 0.1f)
			//{
			//	return;
			//}
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
