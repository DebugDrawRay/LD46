using System.Collections;
using UnityEngine;
using Com.LuisPedroFonseca.ProCamera2D;
using FMOD.Studio;
using FMODUnity;

namespace PBJ
{
	public class GameController : MonoBehaviour
	{
		public static GameController Instance;

		public GameState CurrentGameState;
		public enum GameState
		{
			Playing,
			Paused,
			Gameover
		}

		[SerializeField]
		private int m_initialHappiness;
		[SerializeField]
		private int m_initialSustinence;
		[SerializeField]
		private int m_happinessDrain;
		[SerializeField]
		private float m_happinessDrainRate;
		[SerializeField]
		private int m_sustinenceDrain;
		[SerializeField]
		private float m_sustinenceDrainRate;
		[SerializeField]
		private ItemDB m_itemDb;

		[Header("Player")]
		[SerializeField]
		public int m_healthDrain;
		[SerializeField]
		public float m_healthDrainRate;
		[Header("Game Over Sequence")]
		[SerializeField]
		public float m_deathDelay;
		[SerializeField]
		public float m_timeToDeath;
		[SerializeField]
		public float m_timeAfterDeath;
		[SerializeField]
		public float m_cameraSpeed;
		[SerializeField]
		[FMODUnity.EventRef]
		private string m_theme;

		private EventInstance m_themeInstance;

		private bool m_deathStarted;


		public ItemDB.Item[] ItemDb
		{
			get
			{
				return m_itemDb.Items;
			}
		}

		private float m_lastHappinessDrain;
		private float m_lastSustinenceDrain;
		private float m_lastHealthDrain;
		private WorldState m_state;
		public WorldState CurrentState
		{
			get
			{
				return m_state;
			}
		}

		private void Awake()
		{
			if (Instance != null)
			{
				Destroy(Instance.gameObject);
			}
			Instance = this;
		}

		private void Start()
		{
			if (m_theme != null)
			{
				m_themeInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			}
			m_themeInstance = RuntimeManager.CreateInstance(m_theme);
			InitializeGame();
		}

		private void InitializeGame()
		{
			m_themeInstance.start();
			m_state = new WorldState() { Happiness = m_initialHappiness, Sustinence = m_initialSustinence };
			HUDController.Instance.AdjustHappy((float)m_state.Happiness / (float)m_initialHappiness);
			HUDController.Instance.AdjustHunger((float)m_state.Sustinence / (float)m_initialSustinence);

			m_deathStarted = false;
			GodController.Instance.Spawn();
			PlayerStatus.Instance.Spawn();
			ProCamera2D.Instance.RemoveAllCameraTargets();
			ProCamera2D.Instance.AddCameraTarget(PlayerStatus.Instance.transform);
			ProCamera2D.Instance.VerticalFollowSmoothness = 0;
			ProCamera2D.Instance.HorizontalFollowSmoothness = 0;
			ProCamera2D.Instance.CenterOnTargets();

		}

		private void Update()
		{
			if (CurrentGameState == GameState.Playing)
			{
				UpdateGodState();
			}
		}

		private void UpdateGodState()
		{
			if (!m_deathStarted)
			{
				if (CurrentState.Sustinence <= 0)
				{
					if (Time.time > m_lastHealthDrain + m_healthDrainRate)
					{
						PlayerStatus.Instance.DrainHealth(m_healthDrain);
						m_lastHealthDrain = Time.time;
						if (PlayerStatus.Instance.Dead)
						{
							StartCoroutine(DeathSequence());
							m_deathStarted = true;
							CurrentGameState = GameState.Gameover;
							Debug.Log("<color=red>GAME OVER</color>");
						}
					}
				}
				else
				{
					if (Time.time > m_lastSustinenceDrain + m_sustinenceDrainRate)
					{
						CurrentState.Sustinence -= m_sustinenceDrain;
						m_lastSustinenceDrain = Time.time;
						HUDController.Instance.AdjustHunger((float)m_state.Sustinence / (float)m_initialSustinence);
					}
					if (Time.time > m_lastHappinessDrain + m_happinessDrainRate)
					{
						CurrentState.Happiness -= m_happinessDrain;
						m_lastHappinessDrain = Time.time;
						HUDController.Instance.AdjustHappy((float)m_state.Happiness / (float)m_initialHappiness);
					}
				}

                HUDController.Instance.Warn(CurrentState.Sustinence <= m_initialSustinence / 3);
			}
		}

		public IEnumerator DeathSequence()
		{
			m_themeInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			yield return new WaitForSeconds(m_deathDelay);
			ProCamera2D.Instance.RemoveAllCameraTargets();
			ProCamera2D.Instance.AddCameraTarget(GodController.Instance.transform);
			ProCamera2D.Instance.VerticalFollowSmoothness = m_cameraSpeed;
			ProCamera2D.Instance.HorizontalFollowSmoothness = m_cameraSpeed;

			PlayerStatus.Instance.SetPaused(true);
			yield return new WaitForSeconds(m_timeToDeath);
			GodController.Instance.Kill();
			yield return new WaitForSeconds(m_timeToDeath);
            PauseMenuController.Instance.GameOver();
		}

		public void OnQuit()
		{
			m_themeInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		}

		public class WorldState
		{
			public float Mood
			{
				get
				{
					return (Happiness * HappinessAdjustment) +
					(ItemsEaten * ItemsEatenAdjustment) +
					(ItemsTouched * ItemsTouchedAdjustment) +
					(ItemsDestroyed * ItemsDestroyedAdjustment) +
					(PeopleStunned * PeopleStunnedAdjustment);
				}
			}
			public int Happiness;
			public int Sustinence;

			public int ItemsEaten;
			public int ItemsTouched;
			public int ItemsDestroyed;

			public int PeopleStunned;

			//Adjustments
			private const float HappinessAdjustment = 1;
			private const float ItemsEatenAdjustment = 1;
			private const float ItemsTouchedAdjustment = 1;
			private const float ItemsDestroyedAdjustment = 1;
			private const float PeopleStunnedAdjustment = 1;
		}
	}
}