﻿using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RLProcess
{

	public struct TankStateStruct {
		public Vector3 m_Position;
		public Vector3 m_Euler;
	}

	public struct TankActionStruct {
		public float m_MovementInput;
		public float m_TurnInput;
		public bool m_Fired;
	}

	/* A Tank's Information is included in this class */
	public class TankInfo : ICloneable
	{
		public TankStateStruct m_State;
		public TankActionStruct m_Action;
		public float m_Reward;
		public float m_Damage;

		public object Clone()
		{
			return MemberwiseClone();
		}
	}

	public class RLAgent : MonoBehaviour
	{
		public class StateClass{
			float[] State = new float[3];
		}
		public class OneFrameData {
			StateClass States;
			float Action;
			float Reward;
		}
		List<OneFrameData> m_TrajectoryMovement = new List<OneFrameData>();
		List<OneFrameData> m_TrajectoryTurn = new List<OneFrameData>();
		List<OneFrameData> m_TrajectoryFired = new List<OneFrameData>();

		public GameObject m_GameManagerObj;
		private Complete.GameManager m_GameManager;
		private TankInfo m_AITankInfo = new TankInfo();	//AI's tank information
		private TankInfo m_HuTankInfo = new TankInfo(); //Humna's tank information
		private List<TankInfo> m_LogAITank = new List<TankInfo>();
		private List<TankInfo> m_LogHuTank = new List<TankInfo>();

		private bool m_IsRecording = false;
		private bool m_IsPlayBack = false;
		private List<TankInfo> m_Trajectory = new List<TankInfo>();

		private REINFORCE PGLearn = new REINFORCE();

		/*-----------------------------*/
		/* Use this for initialization */
		/*-----------------------------*/
		void Start()
		{
			m_GameManager = m_GameManagerObj.GetComponent<Complete.GameManager>();
			if (m_GameManager == null) {
				//Debug.LogFormat("akan");
			}
		}

		/*---------*/
		/* ClickOn */
		/*---------*/
		public void ClickOnRecStart() {
			m_IsRecording = true;
		}

		/*---------*/
		/* ClickOn */
		/*---------*/
		public void ClickOnRecStop()
		{
			m_IsRecording = false;
			WriteLogFile();

			//
			PGLearn.SetLogAITank(m_LogAITank);
			PGLearn.SetLogHuTank(m_LogHuTank);
			PGLearn.CalcTrajectory();
		}

		/*---------*/
		/* ClickOn */
		/*---------*/
		public void ClickOnPlayBack() {
			m_IsPlayBack = true;
		}

		/*---------*/
		/* ClickOn */
		/*---------*/
		public void ClickOnGPM_Load() {
			PGLearn.m_GaussianPolicyModel.InputParametersFromXML("/GPMData.xml");
		}

		/*---------*/
		/* ClickOn */
		/*---------*/
		public void ClickOnGPM_Update() {
			for (int i = 0; i < 10; i++)
			{
				PGLearn.RunREINFORCE();
			}
		}

		/*---------*/
		/* ClickOn */
		/*---------*/
		public void ClickOnMakeGaussianPolicyModelTemplate() {
			GaussianPolicyModel GPM = new GaussianPolicyModel(3, 3);
			GPM.OutputParamtersToXML("xmltemplate.xml");
		}

		/* ----------------- */
		/* Set random action */
		/* ----------------- */
		void SetRandomAction() {
			float a = 5.0f;
			m_GameManager.m_Tanks[1].m_Movement.m_MovementInputValue = UnityEngine.Random.Range(-1.0f, 1.0f) * Time.deltaTime * a;
			m_GameManager.m_Tanks[1].m_Movement.m_TurnInputValue = UnityEngine.Random.Range(-1.1f, 1.1f) * Time.deltaTime * a;
			float bufRandom = UnityEngine.Random.Range(0.0f, 1.0f);
			if (0.99f < bufRandom)
			{
				m_GameManager.m_Tanks[1].m_Shooting.m_FireForAI = true;
			}
			else {
				m_GameManager.m_Tanks[1].m_Shooting.m_FireForAI = false;
			}
		}

		/*------------*/
		/* Set Action */
		/*------------*/
		void SetAction(float movement, float turn, bool fired) {
			m_GameManager.m_Tanks[1].m_Movement.m_MovementInputValue = movement;
			m_GameManager.m_Tanks[1].m_Movement.m_TurnInputValue = turn;
			m_GameManager.m_Tanks[1].m_Shooting.m_FireForAI = fired;
        }

		/*----------*/
		/*----------*/
		/*----------*/
		void SetAITankInfo() {
			//Get State from GameManager
			m_AITankInfo.m_State.m_Position = m_GameManager.m_Tanks[1].m_Instance.transform.position;
			m_AITankInfo.m_State.m_Euler = m_GameManager.m_Tanks[1].m_Instance.transform.eulerAngles;

			//Get Action
			m_AITankInfo.m_Action.m_Fired = m_GameManager.m_Tanks[1].m_Shooting.m_FireForAI;
			m_AITankInfo.m_Action.m_MovementInput = m_GameManager.m_Tanks[1].m_Movement.m_MovementInputValue;
			m_AITankInfo.m_Action.m_TurnInput = m_GameManager.m_Tanks[1].m_Movement.m_TurnInputValue;
		}

		/*----------*/
		/*----------*/
		/*----------*/
		void SetHuTankInfo()
		{
			//Get State from GameManager
			m_HuTankInfo.m_State.m_Position = m_GameManager.m_Tanks[0].m_Instance.transform.position;
			m_HuTankInfo.m_State.m_Euler = m_GameManager.m_Tanks[0].m_Instance.transform.eulerAngles;

			//Get Action
			m_HuTankInfo.m_Action.m_Fired = m_GameManager.m_Tanks[0].m_Shooting.m_FireForAI;
			m_HuTankInfo.m_Action.m_MovementInput = m_GameManager.m_Tanks[0].m_Movement.m_MovementInputValue;
			m_HuTankInfo.m_Action.m_TurnInput = m_GameManager.m_Tanks[0].m_Movement.m_TurnInputValue;
		}

		private int iFrame = 0;
		/*-------------*/
		/* Switch Mode */
		/*-------------*/
		void SwitchMode() {
			//input Action
			//default mode
			if (m_IsRecording == true || m_IsPlayBack == false || m_LogAITank.Count < 1)
			{
				SetRandomAction();
				iFrame = 0;
			}
			//play back mode
			else if (m_IsPlayBack == true && iFrame < m_LogAITank.Count)
			{
				TankActionStruct buf = m_LogAITank[iFrame].m_Action;
				//TankActionStruct buf = m_LogHuTank[iFrame].m_Action;
				SetAction(buf.m_MovementInput, buf.m_TurnInput, buf.m_Fired);
				iFrame++;
			}
			//play back mode to default mode
			else if (m_IsPlayBack == true && m_LogAITank.Count <= iFrame)
			{
				m_IsPlayBack = false;
				iFrame = 0;
			}
		}

		/*----------------------------*/
		/* Main Loop for Control Tank */
		/*----------------------------*/
		void FixedUpdate()
		{
			SwitchMode();

			//set states
			SetAITankInfo();
			SetHuTankInfo();

			if (m_IsRecording == true)
			{
				//get information for logging
				m_LogAITank.Add((TankInfo)m_AITankInfo.Clone());
				m_LogHuTank.Add((TankInfo)m_HuTankInfo.Clone());
			}
		}

		/*-------------------------*/
		/* write log file function */
		/*-------------------------*/
		void WriteLogFile()
		{
			StreamWriter writer = null;
			writer = new StreamWriter(@"Assets/LogFiles/log.csv", false, System.Text.Encoding.Default);

			for (int i = 0; i < m_Trajectory.Count; i++)
			{
				TankStateStruct bufState = m_Trajectory[i].m_State;
				TankActionStruct bufAction = m_Trajectory[i].m_Action;
				float reward = m_Trajectory[i].m_Reward;
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