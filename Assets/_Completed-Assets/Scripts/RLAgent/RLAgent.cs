using System;
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

		public GameObject m_GameManagerObj;
		private Complete.GameManager m_GameManager;
		private TankInfo m_AITankInfo = new TankInfo(); //AI's tank information
		private TankInfo m_HuTankInfo = new TankInfo(); //Humna's tank information

		public class Episode : ICloneable {
			public List<TankInfo> m_LogAITank = new List<TankInfo>();
			public List<TankInfo> m_LogHuTank = new List<TankInfo>();
			public object Clone()
			{
				return MemberwiseClone();
			}
		}
		private Episode m_Episode = new Episode();
		private List<Episode> m_Episodes = new List<Episode>();

		private bool m_IsRecording = false;
		private bool m_IsPlayBack = false;


		private REINFORCE m_REINFORCE_Move = new REINFORCE();
		private REINFORCE m_REINFORCE_Turn = new REINFORCE();
		private REINFORCE m_REINFORCE_Fired = new REINFORCE();

		private string fNameLoadEpisode = "/LogFiles/log_play01.xml";
		private string fNameSaveEpisode = "/LogFiles/log.xml";

		private string[] fNamesOfLoadEpisodes = { "/LogFiles/log_play01.xml"};

		/*-----------------------------*/
		/* Use this for initialization */
		/*-----------------------------*/
		void Start()
		{
			m_GameManager = m_GameManagerObj.GetComponent<Complete.GameManager>();
			if (m_GameManager == null) {
				Debug.LogFormat("error: can not get component of GameManager");
			}
			ClickOnGPM_Load();
			//LoadEpisode(fNameLoadEpisode);

			/* EigenFunc.dll Sample */
			//EigenFunc eigen = new EigenFunc();
			//float[,] bufMat = new float[3, 3] { { 1f, 2f, 1f }, { 2f, 1f, 0f }, { 1f, 1f, 2f } };
			//float[,] AnsMat = new float[3, 3];
			//Debug.LogFormat("bufMat: {0}", bufMat[0,0]);
			//AnsMat = eigen.InverseMatrix(bufMat);
			//Debug.LogFormat("AnsMat: {0}", AnsMat[2, 2]);
		}

		/*-------------------------*/
		/* ClickOn Recording Start */
		/*-------------------------*/
		public void ClickOnRecStart() {
			m_IsRecording = true;
		}

		/*------------------------*/
		/* ClickOn Recording Stop */
		/*------------------------*/
		public void ClickOnRecStop()
		{
			//stop recording
			m_IsRecording = false;
			//write log file
			SaveEpisode(fNameSaveEpisode);
		}

		/*--------------------*/
		/* Loading an Episode */
		/*--------------------*/
		public void ClickOnLoadEpisode() {
			m_Episode = LoadEpisode(fNameLoadEpisode);
			Debug.LogFormat("An Episode was Loaded.");
			ClickOnLoadSomeEpisodes();
        }

		public void ClickOnLoadSomeEpisodes() {
			for (int i = 0; i < fNamesOfLoadEpisodes.Length; i++)
			{
				Episode buf = LoadEpisode(fNamesOfLoadEpisodes[i]);
				m_Episodes.Add((Episode)buf.Clone());
			}
			Debug.LogFormat("{0} Episodes were Loaded.", fNamesOfLoadEpisodes.Length);
			//Generate Trajectories
			//GenerateTrajectoriesForREINFORCE_ForExploration();
			GenerateTrajectoriesForREINFORCE_ForMimic();
        }

		public void ClickOnAdd2Episodes() {
			m_Episodes.Add((Episode)m_Episode.Clone());
			Debug.LogFormat("An Episode was added to Episodes data set.");
		}

		/*-------------------*/
		/* Saving an Episode */
		/*-------------------*/
		public void ClickOnSaveEpisode() {
			SaveEpisode(fNameSaveEpisode);
			Debug.LogFormat("An Episode was Saved.");
		}

		/*---------------------*/
		/* Clearing an Episode */
		/*---------------------*/
		public void ClickOnClearEpisode() {
			m_Episode.m_LogAITank.Clear();
			m_Episode.m_LogHuTank.Clear();

			m_Episodes.Clear();
			Debug.LogFormat("An Episode was Cleared.");
		}

		/*------------------------*/
		/* ClickOn Play Back Mode */
		/*------------------------*/
		public void ClickOnPlayBack() {
			m_IsPlayBack = true;
		}

		/*---------------------------------------*/
		/* ClickOn Loading Gaussian Policy Model */
		/*---------------------------------------*/
		public void ClickOnGPM_Load() {
			//PGLearn.m_GaussianPolicyModel.InputParametersFromXML("/GPMData.xml");
			m_REINFORCE_Move.m_GaussianPolicyModel.InputParametersFromXML("/GPMs/GPM_k100_40x40_Move.xml");
			m_REINFORCE_Turn.m_GaussianPolicyModel.InputParametersFromXML("/GPMs/GPM_k100_40x40_Turn.xml");
			m_REINFORCE_Fired.m_GaussianPolicyModel.InputParametersFromXML("/GPMs/GPM_k100_40x40_Fired.xml");
		}

		/*-----------------------*/
		/* ClickOn Policy Update */
		/*-----------------------*/
		public void ClickOnGPM_Update() {
			for (int i = 0; i < 1; i++)
			{
				m_REINFORCE_Move.RunREINFORCE();
				m_REINFORCE_Turn.RunREINFORCE();
				m_REINFORCE_Fired.RunREINFORCE();
			}
			Debug.LogFormat("num of kernels: {0}", m_REINFORCE_Move.m_GaussianPolicyModel.m_Mean.Length);
			//m_REINFORCE_Move.m_GaussianPolicyModel.OutputParamtersToXML("/GPMs/GPM_k100_40x40_Move.xml");
			//m_REINFORCE_Turn.m_GaussianPolicyModel.OutputParamtersToXML("/GPMs/GPM_k100_40x40_Turn.xml");
			//m_REINFORCE_Fired.m_GaussianPolicyModel.OutputParamtersToXML("/GPMs/GPM_k100_40x40_Fired.xml");
		}

		/*----------------------------------------------------------*/
		/* ClickOn Save Parameter Template of Gaussian Policy Model */
		/*----------------------------------------------------------*/
		public void ClickOnMakeGaussianPolicyModelTemplate() {
			GaussianPolicyModel GPM = new GaussianPolicyModel(3, 3);
			GPM.OutputParamtersToXML("xmltemplate.xml");
		}
		
		/*----------*/
		/*----------*/
		/*----------*/
		void SetAITankInfo() {
			//Get State from GameManager
			m_AITankInfo.m_State.m_Position = m_GameManager.m_Tanks[1].m_Instance.transform.position;
			m_AITankInfo.m_State.m_Euler = m_GameManager.m_Tanks[1].m_Instance.transform.eulerAngles * Mathf.PI / 180f;

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
			m_HuTankInfo.m_State.m_Euler = m_GameManager.m_Tanks[0].m_Instance.transform.eulerAngles * Mathf.PI / 180f;

			//Get Action
			m_HuTankInfo.m_Action.m_Fired = m_GameManager.m_Tanks[0].m_Shooting.m_FireForAI;
			m_HuTankInfo.m_Action.m_MovementInput = m_GameManager.m_Tanks[0].m_Movement.m_MovementInputValue;
			m_HuTankInfo.m_Action.m_TurnInput = m_GameManager.m_Tanks[0].m_Movement.m_TurnInputValue;
		}

		/*-----------------------------------------------------------------------*/
		/* Generate trajectories for each action policy model (For Exploration)  */
		/* if the agent explorates his policy by being applied epsilon-greedy... */
		/*-----------------------------------------------------------------------*/
		void GenerateTrajectoriesForREINFORCE_ForExploration() {
			//[Move] Action
            for (int n = 0; n < m_Episodes.Count; n++){
				List<REINFORCE.OneFrameData> buf_traj = new List<REINFORCE.OneFrameData>();
				for (int t = 0; t < m_Episodes[n].m_LogAITank.Count; t++) {
					REINFORCE.OneFrameData buf_ofd = new REINFORCE.OneFrameData();
					//state
					Vector3 relPos = m_Episodes[n].m_LogHuTank[t].m_State.m_Position - m_Episodes[n].m_LogAITank[t].m_State.m_Position;
					Vector3 gloEul = m_Episodes[n].m_LogAITank[t].m_State.m_Euler;
					relPos = Quaternion.Euler(-gloEul * 180f / Mathf.PI) * relPos;
					buf_ofd.State[0] = relPos.x;
					buf_ofd.State[1] = relPos.z;
					//buf_ofd.State[2] = relEul.y;
					//action
					buf_ofd.Action = m_Episodes[n].m_LogAITank[t].m_Action.m_MovementInput; //Move
					//reward
					buf_ofd.Reward = m_Episodes[n].m_LogAITank[t].m_Reward;
					//add
					buf_traj.Add((REINFORCE.OneFrameData)buf_ofd.Clone());
					//Debug.LogFormat("count: {0}", buf_traj.Count);
				}
				m_REINFORCE_Move.m_Trajectories.Add(new List<REINFORCE.OneFrameData>(buf_traj));
				//Debug.LogFormat("length: {0}", m_REINFORCE_Move.m_Trajectories[0].Count);
            }
			//[Turn] Action
			for (int n = 0; n < m_Episodes.Count; n++)
			{
				List<REINFORCE.OneFrameData> buf_traj = new List<REINFORCE.OneFrameData>();
				for (int t = 0; t < m_Episodes[n].m_LogAITank.Count; t++)
				{
					REINFORCE.OneFrameData buf_ofd = new REINFORCE.OneFrameData();
					//state
					Vector3 relPos = m_Episodes[n].m_LogHuTank[t].m_State.m_Position - m_Episodes[n].m_LogAITank[t].m_State.m_Position;
					Vector3 gloEul = m_Episodes[n].m_LogAITank[t].m_State.m_Euler;
					relPos = Quaternion.Euler(-gloEul * 180f / Mathf.PI) * relPos;
					buf_ofd.State[0] = relPos.x;
					buf_ofd.State[1] = relPos.z;
					//buf_ofd.State[2] = relEul.y;
					//action
					buf_ofd.Action = m_Episodes[n].m_LogAITank[t].m_Action.m_TurnInput; //Turn
					//reward
					buf_ofd.Reward = m_Episodes[n].m_LogAITank[t].m_Reward;
					//add
					buf_traj.Add((REINFORCE.OneFrameData)buf_ofd.Clone());
				}
				m_REINFORCE_Turn.m_Trajectories.Add(new List<REINFORCE.OneFrameData>(buf_traj));
			}
			//[Fired] Action
			for (int n = 0; n < m_Episodes.Count; n++)
			{
				List<REINFORCE.OneFrameData> buf_traj = new List<REINFORCE.OneFrameData>();
				for (int t = 0; t < m_Episodes[n].m_LogAITank.Count; t++)
				{
					REINFORCE.OneFrameData buf_ofd = new REINFORCE.OneFrameData();
					//state
					Vector3 relPos = m_Episodes[n].m_LogHuTank[t].m_State.m_Position - m_Episodes[n].m_LogAITank[t].m_State.m_Position;
					Vector3 gloEul = m_Episodes[n].m_LogAITank[t].m_State.m_Euler;
					relPos = Quaternion.Euler(-gloEul * 180f / Mathf.PI) * relPos;
					buf_ofd.State[0] = relPos.x;
					buf_ofd.State[1] = relPos.z;
					//buf_ofd.State[2] = relEul.y;
					//action
					buf_ofd.Action = Convert.ToSingle(m_Episodes[n].m_LogAITank[t].m_Action.m_Fired); //Fired
					//reward
					buf_ofd.Reward = m_Episodes[n].m_LogAITank[t].m_Reward;
					//add
					buf_traj.Add((REINFORCE.OneFrameData)buf_ofd.Clone());
				}
				m_REINFORCE_Fired.m_Trajectories.Add(new List<REINFORCE.OneFrameData>(buf_traj));
			}
		}

		/*------------------------------------------------------------------------*/
		/* Generate trajectories for each action policy model (For mimic strategy)*/
		/* if the agent mimics human play                                         */
		/*------------------------------------------------------------------------*/
		void GenerateTrajectoriesForREINFORCE_ForMimic()
		{
			//[Move] Action
			for (int n = 0; n < m_Episodes.Count; n++)
			{
				List<REINFORCE.OneFrameData> buf_traj = new List<REINFORCE.OneFrameData>();
				for (int t = 0; t < m_Episodes[n].m_LogAITank.Count; t++)
				{
					REINFORCE.OneFrameData buf_ofd = new REINFORCE.OneFrameData();
					//state
					Vector3 relPos = m_Episodes[n].m_LogAITank[t].m_State.m_Position - m_Episodes[n].m_LogHuTank[t].m_State.m_Position;
					Vector3 gloEul = m_Episodes[n].m_LogHuTank[t].m_State.m_Euler;
					relPos = Quaternion.Euler(-gloEul * 180f / Mathf.PI) * relPos;
					buf_ofd.State[0] = relPos.x;
					buf_ofd.State[1] = relPos.z;
					//buf_ofd.State[2] = relEul.y;
					//action
					buf_ofd.Action = m_Episodes[n].m_LogHuTank[t].m_Action.m_MovementInput; //Move
																							//reward
					buf_ofd.Reward = m_Episodes[n].m_LogHuTank[t].m_Reward;
					//add
					buf_traj.Add((REINFORCE.OneFrameData)buf_ofd.Clone());
				}
				m_REINFORCE_Move.m_Trajectories.Add(new List<REINFORCE.OneFrameData>(buf_traj));
			}
			//[Turn] Action
			for (int n = 0; n < m_Episodes.Count; n++)
			{
				List<REINFORCE.OneFrameData> buf_traj = new List<REINFORCE.OneFrameData>();
				for (int t = 0; t < m_Episodes[n].m_LogAITank.Count; t++)
				{
					REINFORCE.OneFrameData buf_ofd = new REINFORCE.OneFrameData();
					//state
					Vector3 relPos = m_Episodes[n].m_LogAITank[t].m_State.m_Position - m_Episodes[n].m_LogHuTank[t].m_State.m_Position;
					Vector3 gloEul = m_Episodes[n].m_LogHuTank[t].m_State.m_Euler;
					relPos = Quaternion.Euler(-gloEul * 180f / Mathf.PI) * relPos;
					buf_ofd.State[0] = relPos.x;
					buf_ofd.State[1] = relPos.z;
					//buf_ofd.State[2] = relEul.y;
					//action
					buf_ofd.Action = m_Episodes[n].m_LogHuTank[t].m_Action.m_TurnInput; //Turn
																						//reward
					buf_ofd.Reward = m_Episodes[n].m_LogHuTank[t].m_Reward;
					//add
					buf_traj.Add((REINFORCE.OneFrameData)buf_ofd.Clone());
				}
				m_REINFORCE_Turn.m_Trajectories.Add(new List<REINFORCE.OneFrameData>(buf_traj));
			}
			//[Fired] Action
			for (int n = 0; n < m_Episodes.Count; n++)
			{
				List<REINFORCE.OneFrameData> buf_traj = new List<REINFORCE.OneFrameData>();
				for (int t = 0; t < m_Episodes[n].m_LogAITank.Count; t++)
				{
					REINFORCE.OneFrameData buf_ofd = new REINFORCE.OneFrameData();
					//state
					Vector3 relPos = m_Episodes[n].m_LogAITank[t].m_State.m_Position - m_Episodes[n].m_LogHuTank[t].m_State.m_Position;
					Vector3 gloEul = m_Episodes[n].m_LogHuTank[t].m_State.m_Euler;
					relPos = Quaternion.Euler(-gloEul * 180f / Mathf.PI) * relPos;
					buf_ofd.State[0] = relPos.x;
					buf_ofd.State[1] = relPos.z;
					//buf_ofd.State[2] = relEul.y;
					//action
					buf_ofd.Action = Convert.ToSingle(m_Episodes[n].m_LogHuTank[t].m_Action.m_Fired); //Fired
					//reward
					buf_ofd.Reward = m_Episodes[n].m_LogHuTank[t].m_Reward;
					//add
					buf_traj.Add((REINFORCE.OneFrameData)buf_ofd.Clone());
				}
				m_REINFORCE_Fired.m_Trajectories.Add(new List<REINFORCE.OneFrameData>(buf_traj));
			}
		}

		/*-----------*/
		/* Play Back */
		/*-----------*/
		void ForPlayBack(int iFrame) {
			m_GameManager.m_Tanks[0].m_Instance.transform.position = m_Episode.m_LogHuTank[iFrame].m_State.m_Position;
			m_GameManager.m_Tanks[0].m_Instance.transform.eulerAngles = m_Episode.m_LogHuTank[iFrame].m_State.m_Euler * 180f / Mathf.PI;
			m_GameManager.m_Tanks[0].m_Shooting.m_FireForAI = m_Episode.m_LogHuTank[iFrame].m_Action.m_Fired;
			m_GameManager.m_Tanks[1].m_Instance.transform.position = m_Episode.m_LogAITank[iFrame].m_State.m_Position;
			m_GameManager.m_Tanks[1].m_Instance.transform.eulerAngles = m_Episode.m_LogAITank[iFrame].m_State.m_Euler * 180f / Mathf.PI;
			m_GameManager.m_Tanks[1].m_Shooting.m_FireForAI = m_Episode.m_LogAITank[iFrame].m_Action.m_Fired;
			//Debug.LogFormat("play back");
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

		private int iFrame = 0;
		/*-------------*/
		/* Switch Mode */
		/*-------------*/
		void SwitchMode() {
			//input Action
			//default mode
			if (m_IsRecording == true || m_IsPlayBack == false || m_Episode.m_LogAITank.Count < 1)
			{
				//set state
				float[] state = new float[2];
				Vector3 relPos = m_HuTankInfo.m_State.m_Position - m_AITankInfo.m_State.m_Position;
				Vector3 gloEul = m_AITankInfo.m_State.m_Euler;
				relPos = Quaternion.Euler(-gloEul * 180f / Mathf.PI) * relPos;
				state[0] = relPos.x;
				state[1] = relPos.z;
				//state[2] = relEul.y;

				float aMove = m_REINFORCE_Move.GetAction((float[])state.Clone());
				aMove = Mathf.Clamp(aMove, -1.0f, 1.0f);
				float aTurn = m_REINFORCE_Turn.GetAction((float[])state.Clone());
				aTurn = Mathf.Clamp(aTurn, -1.0f, 1.0f);
				float aFired = m_REINFORCE_Fired.GetAction((float[])state.Clone());
				Debug.LogFormat("move: {0}, turn: {1}, fire: {2}", aMove, aTurn, aFired);
				SetAction(aMove, aTurn, Convert.ToBoolean(aFired));
				//SetRandomAction();
				iFrame = 0;

			}
			//play back mode
			else if (m_IsPlayBack == true && iFrame < m_Episode.m_LogAITank.Count)
			{
				ForPlayBack(iFrame);
				iFrame++;
			}
			//play back mode to default mode
			else if (m_IsPlayBack == true && m_Episode.m_LogAITank.Count <= iFrame)
			{
				m_IsPlayBack = false;
				iFrame = 0;
			}
			else {
			}

		}

		/*----------------------------*/
		/* Main Loop for Control Tank */
		/*----------------------------*/
		void FixedUpdate()
		{

			//set states
			SetAITankInfo();
			SetHuTankInfo();

			//set action
			SwitchMode();

			if (m_IsRecording == true)
			{
				//get information for logging
				m_Episode.m_LogAITank.Add((TankInfo)m_AITankInfo.Clone());
				m_Episode.m_LogHuTank.Add((TankInfo)m_HuTankInfo.Clone());
			}
		}

		/*-------------------------*/
		/* write log file function */
		/*-------------------------*/
		void SaveEpisode(string file_name)
		{
            string serializeDataPath = Application.dataPath + file_name;
			XmlUtil.Seialize<Episode>(serializeDataPath, (Episode)m_Episode );
		}

		/*------------------------*/
		/* read log file function */
		/*------------------------*/
		Episode LoadEpisode(string file_name) {
			string serializeDataPath = Application.dataPath + file_name;
			Episode buf = XmlUtil.Deserialize<Episode>(serializeDataPath);
			return (Episode)buf.Clone();
		}

		void WriteLogTextFile() {
			//StreamWriter writer = null;
			//writer = new StreamWriter(@"Assets/LogFiles/log.csv", false, System.Text.Encoding.Default);

			//for (int i = 0; i < m_Trajectory.Count; i++)
			//{
			//	TankStateStruct bufState = m_Trajectory[i].m_State;
			//	TankActionStruct bufAction = m_Trajectory[i].m_Action;
			//	float reward = m_Trajectory[i].m_Reward;
			//	writer.Write(i);
			//	writer.Write("," + bufState.m_Position.x);
			//	writer.Write("," + bufState.m_Position.y);
			//	writer.Write("," + bufState.m_Position.z);
			//	writer.Write("," + bufState.m_Euler.x);
			//	writer.Write("," + bufState.m_Euler.y);
			//	writer.Write("," + bufState.m_Euler.z);
			//	writer.Write("," + bufAction.m_MovementInput);
			//	writer.Write("," + bufAction.m_TurnInput);
			//	writer.Write("," + bufAction.m_Fired);
			//	writer.Write("," + reward);
			//	writer.Write("\n");
			//}
			//writer.Flush();
			//writer.Close();
		}
	}
}