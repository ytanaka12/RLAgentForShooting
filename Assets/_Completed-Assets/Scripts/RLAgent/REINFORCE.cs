using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MOL;

namespace RLProcess
{

	public class REINFORCE : MonoBehaviour
	{
		/* Must need to scecify these parameter */
		//private const int m_NumKernel = 3;
		private const int m_StateDim = 2;
		private double m_eps = Math.Pow(10.0d, -50.0d);
		//private double m_eps = 0.00001f;

		public class OneFrameData : ICloneable
		{
			public double[] State = new double[m_StateDim];
			public double Action;
			public double Reward;
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

		/*---------------------------------*/
		/* Optimization by Gradient Ascent */
		/*---------------------------------*/
		public void GradientAscent() {
			//double[] gAscentMean = new double[m_GaussianPolicyModel.m_Mean.Length];
			Matrix gAscentMean = new Matrix(m_GaussianPolicyModel.m_Mean.Length);
			//initialize
			//for (int i = 0; i < gAscentMean.Length; i++) {
			//	gAscentMean[i] = 0.0f;
			//}
			double gAscentStandDev = 0.0f;

			//double[] befMean = new double[m_GaussianPolicyModel.m_Mean.Length];
			//Matrix befMean = new Matrix(m_GaussianPolicyModel.m_Mean.Length);
			//befMean = (double[])m_GaussianPolicyModel.m_Mean.Clone();
			Matrix befMean = new Matrix((double[])m_GaussianPolicyModel.m_Mean.Clone());
            double befStandDev = new double();
			befStandDev = m_GaussianPolicyModel.m_StandDev;

			for (int n = 0; n < m_Trajectories.Count ; n++) {
				for (int t = 0; t < m_Trajectories[n].Count; t++) {
					//set state
					m_GaussianPolicyModel.SetState((double[])m_Trajectories[n][t].State.Clone());

					//set action
					m_GaussianPolicyModel.SetAction(m_Trajectories[n][t].Action);
					//m_GaussianPolicyModel.SetAction(1.6f);

					//calculate Gradient
					//double[] bufMean = m_GaussianPolicyModel.CalcGradientMean();
					Matrix bufMean = new Matrix(m_GaussianPolicyModel.CalcGradientMean());
					//gAscentMean = compMat.AdditionVecVec(gAscentMean, bufMean );
					gAscentMean = gAscentMean + bufMean;
					gAscentStandDev += m_GaussianPolicyModel.CalcgradientStandDev();
				}
			}

			//Ascent
			for (int i = 0; i < gAscentMean.GetNumOfRow(); i++)
			{
				m_GaussianPolicyModel.m_Mean[i] += m_eps * gAscentMean.Element[i,0];
			}
			m_GaussianPolicyModel.m_StandDev += m_eps * gAscentStandDev;

			/* limit */
			if (m_GaussianPolicyModel.m_StandDev < 1.0f) {
				m_GaussianPolicyModel.m_Mean = (double[])befMean.GetVector();
				m_GaussianPolicyModel.m_StandDev = befStandDev;
			}

			m_GaussianPolicyModel.SetState((double[])m_Trajectories[0][10].State.Clone());
            double buf = m_GaussianPolicyModel.CalcActionMean();
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
			//double[] gAscentMean = new double[m_GaussianPolicyModel.m_Mean.Length];
			Matrix gAscentMean = new Matrix(m_GaussianPolicyModel.m_Mean.Length, 1);
			double gAscentStandDev = 0.0f;

			//temporary memory
			//double[] befMean = new double[m_GaussianPolicyModel.m_Mean.Length];
			Matrix befMean = new Matrix((double[])m_GaussianPolicyModel.m_Mean.Clone());
			//befMean = (double[])m_GaussianPolicyModel.m_Mean.Clone();
			double befStandDev = new double();
			befStandDev = m_GaussianPolicyModel.m_StandDev;

			//vector for calculation of Fisher information matrix
			//double[] vFisherParam = new double[m_GaussianPolicyModel.m_Mean.Length + 1];  // + StandDev
			Matrix vFisherParam = new Matrix(m_GaussianPolicyModel.m_Mean.Length + 1);
			//double[,] FisherMat = new double[m_GaussianPolicyModel.m_Mean.Length + 1, m_GaussianPolicyModel.m_Mean.Length + 1];
			Matrix FisherMat = new Matrix(m_GaussianPolicyModel.m_Mean.Length + 1, m_GaussianPolicyModel.m_Mean.Length + 1);

			//calc
			for (int n = 0; n < m_Trajectories.Count; n++)
			{
				//double[] bufFisherMean = new double[m_GaussianPolicyModel.m_Mean.Length];
				Matrix bufFisherMean = new Matrix(m_GaussianPolicyModel.m_Mean.Length);
				double bufFisherStandDev = 0.0f;
				//calc gradient like previous
				for (int t = 0; t < m_Trajectories[n].Count; t++)
				{
					//set state
					m_GaussianPolicyModel.SetState((double[])m_Trajectories[n][t].State.Clone());

					//set action
					m_GaussianPolicyModel.SetAction(m_Trajectories[n][t].Action);
					//m_GaussianPolicyModel.SetAction(1.6f);

					//calculate Gradient
					//double[] bufMean = m_GaussianPolicyModel.CalcGradientMean();
					Matrix bufMean = new Matrix(m_GaussianPolicyModel.CalcGradientMean());
					gAscentMean = gAscentMean + bufMean;
					bufFisherMean = bufFisherMean + bufMean;
					double bufStandDev = m_GaussianPolicyModel.CalcgradientStandDev();
					gAscentStandDev += bufStandDev;
					bufFisherStandDev += bufStandDev;

					//for calc Fisher Information Matrix
					//bufFisherMean = (double[])bufMean.Clone();
					//bufFisherStandDev = bufStandDev;
                }

				//calc Fisher Information Matrix
				for (int i = 0; i < m_GaussianPolicyModel.m_Mean.Length; i++)
				{
					vFisherParam.Element[i, 0] = bufFisherMean.Element[i, 0];
				}
				vFisherParam.Element[m_GaussianPolicyModel.m_Mean.Length, 0] = bufFisherStandDev;
				//double[,] bufMat = new double[m_GaussianPolicyModel.m_Mean.Length + 1, m_GaussianPolicyModel.m_Mean.Length + 1];
				//bufMat = compMat.MultiplicationVecVecT(vFisherParam, vFisherParam);
				Matrix bufMat = vFisherParam * vFisherParam.Transposition();
				//FisherMat = compMat.AdditionMatMat(FisherMat, bufMat);
				FisherMat = FisherMat + bufMat;
            }

			// Fisher Information Matrix !!
			//FisherMat = compMat.MultiplicationCoefMat(1.0f / (double)m_Trajectories.Count, FisherMat);
			FisherMat = (1.0 / (double)m_Trajectories.Count) * FisherMat;

			//Inverse Fisher Information Matrix
			//EigenFunc eigen = new EigenFunc();
			//double[,] InvFisherMat = compMat.InverseMatrix(FisherMat);
			Matrix InvFisherMat = FisherMat.Inverse();

			//Calc Natural Gradient
			//double[] provGradientVec = new double[m_GaussianPolicyModel.m_Mean.Length + 1];  // + StandDev
			Matrix provGradientVec = new Matrix(m_GaussianPolicyModel.m_Mean.Length + 1, 1);
            for (int i = 0; i < m_GaussianPolicyModel.m_Mean.Length; i++)
			{
				provGradientVec.Element[i, 0] = gAscentMean.Element[i, 0];
			}
			provGradientVec.Element[m_GaussianPolicyModel.m_Mean.Length, 0] = gAscentStandDev;
			//double[] NaturalGradientVec = new double[m_GaussianPolicyModel.m_Mean.Length + 1];    //NaturalGradient
			Matrix NaturalGradientVec = new Matrix(m_GaussianPolicyModel.m_Mean.Length + 1, 1);
			//NaturalGradientVec = compMat.MultiplicationMatVec(InvFisherMat, provGradientVec);
			NaturalGradientVec = InvFisherMat * provGradientVec;

			//for (int i = 0; i < m_GaussianPolicyModel.m_Mean.Length; i++)
			//{
			//	Debug.LogFormat("Ascent Mean[{0}]: {1}", i, gAscentMean[i]);
			//}

			for (int i = 0; i < m_GaussianPolicyModel.m_Mean.Length; i++)
			{
				gAscentMean.Element[i, 0] = NaturalGradientVec.Element[i, 0];
			}
			gAscentStandDev = NaturalGradientVec.Element[m_GaussianPolicyModel.m_Mean.Length, 0];

			//for (int i = 0; i < m_GaussianPolicyModel.m_Mean.Length; i++)
			//{
			//	Debug.LogFormat("Ascent Mean[{0}]: {1}", i, gAscentMean[i]);
			//}

			//Ascent
			for (int i = 0; i < gAscentMean.GetNumOfRow(); i++)
			{
				m_GaussianPolicyModel.m_Mean[i] += m_eps * gAscentMean.Element[i, 0];
			}
			m_GaussianPolicyModel.m_StandDev += m_eps * gAscentStandDev;

			/* limit */
			if (m_GaussianPolicyModel.m_StandDev < 0.0001f)
			{
				m_GaussianPolicyModel.m_Mean = (double[])befMean.Clone();
				m_GaussianPolicyModel.m_StandDev = befStandDev;
			}

			m_GaussianPolicyModel.SetState((double[])m_Trajectories[0][10].State.Clone());
			double buf = m_GaussianPolicyModel.CalcActionMean();
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
		public float GetAction(double[] state) {
			m_GaussianPolicyModel.SetState((double[])state.Clone());
			double ans = m_GaussianPolicyModel.CalcActionMean();
			return (float)ans;
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
				double reward = trajectory[i].m_Reward;
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
