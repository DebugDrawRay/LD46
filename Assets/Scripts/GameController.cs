using System.Collections;
using UnityEngine;
using Com.LuisPedroFonseca.ProCamera2D;
using FMOD.Studio;
using FMODUnity;
using UnityEngine.Assertions;

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
		private int m_maxHappiness;
		[SerializeField]
		private float m_happinessPercentToEvolve;
		[SerializeField]
		private int m_initialSustinence;
		[SerializeField]
		private int m_maxSustinence;
		[SerializeField]
		private int m_happinessDrain;
		[SerializeField]
		private float m_happinessDrainRate;
		[SerializeField]
		private int m_sustinenceDrain;
		[SerializeField]
		private float m_sustinenceDrainRate;
		[SerializeField]
		private int m_itemsBeforeNewRequest;
		[SerializeField]
		private float m_alertPercent = .3f;

		public int ItemsBeforeNewRequest
		{
			get
			{
				return m_itemsBeforeNewRequest;
			}
		}
		[SerializeField]
		private ItemDB m_itemDb;

		[Header("Player")]
		[SerializeField]
		public int m_healthDrain;
		[SerializeField]
		public float m_healthDrainRate;
		[SerializeField]
		public int m_healthRegen;
		[SerializeField]
		public float m_healthRegenRate;
		[SerializeField]
		private float m_healthPercent = .75f;
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

		private bool m_paused;

		public ItemDB.Item[] ItemDb
		{
			get
			{
				return m_itemDb.Items;
			}
		}
		public float Mood
		{
			get
			{
				return m_state.Mood;
			}
		}

		private float m_lastHappinessDrain;
		private float m_lastSustinenceDrain;
		private float m_lastHealthDrain;
		private float m_lastHealthRegen;
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
			m_state = new WorldState() { Happiness = 0, Sustinence = m_initialSustinence, IsEvolved = false, GameStart = Time.time };
			HUDController.Instance.AdjustHappy((float)m_state.Happiness / (float)m_maxHappiness);
			HUDController.Instance.AdjustHunger((float)m_state.Sustinence / (float)m_maxSustinence);
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
				if (PlayerStatus.Instance.Dead)
				{
					StartCoroutine(DeathSequence());
					m_deathStarted = true;
					CurrentGameState = GameState.Gameover;
					Debug.Log("<color=red>GAME OVER</color>");
				}
				else
				{
					if (PlayerStatus.Instance.Damaged)
					{
						if (CurrentState.Sustinence > (float)m_maxSustinence * m_healthPercent)
						{
							if (Time.time > m_lastHealthRegen + m_healthRegenRate)
							{
								PlayerStatus.Instance.RestoreHealth(m_healthRegen);
								m_lastHealthRegen = Time.time;
							}
						}
					}
					else
					{
						m_lastHealthRegen = Time.time;
					}
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
						if (CanEvolve())
						{
							Evolve();
						}
						if (Time.time > m_lastSustinenceDrain + m_sustinenceDrainRate)
						{
							CurrentState.Sustinence -= m_sustinenceDrain;
							m_lastSustinenceDrain = Time.time;
							HUDController.Instance.AdjustHunger((float)m_state.Sustinence / (float)m_maxSustinence);
						}
						if (Time.time > m_lastHappinessDrain + m_happinessDrainRate)
						{
							CurrentState.Happiness -= m_happinessDrain;
							m_lastHappinessDrain = Time.time;
							HUDController.Instance.AdjustHappy((float)m_state.Happiness / (float)m_maxHappiness);
						}
					}
					if (CurrentState.Happiness >= m_maxHappiness)
					{
						StartCoroutine(WinSequence());
						m_deathStarted = true;
						CurrentGameState = GameState.Gameover;
					}

				}
			}
			HUDController.Instance.Warn(!m_deathStarted && !PlayerStatus.Instance.Dead&& CurrentState.Sustinence <= (float)m_maxSustinence * m_alertPercent);

		}

		private bool CanEvolve()
		{
			return !CurrentState.IsEvolved && CurrentState.Happiness >= m_maxHappiness * m_happinessPercentToEvolve && GodController.Instance.CanEvolve;
		}

		private void Evolve()
		{
			// The GodController will start its Evolve routine and then set CurrentState.IsEvolved when finished
			GodController.Instance.Evolve();
		}

		public IEnumerator WinSequence()
		{
			m_themeInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			float time = Mathf.RoundToInt(100 * (Time.time - m_state.GameStart)) / 100f;
			yield return new WaitForSeconds(m_deathDelay);
			ProCamera2D.Instance.RemoveAllCameraTargets();
			ProCamera2D.Instance.AddCameraTarget(GodController.Instance.transform);
			ProCamera2D.Instance.VerticalFollowSmoothness = m_cameraSpeed;
			ProCamera2D.Instance.HorizontalFollowSmoothness = m_cameraSpeed;

			PlayerStatus.Instance.SetPaused(true);
			yield return new WaitForSeconds(m_timeToDeath);

			PauseMenuController.Instance.EndScreen(false, m_state.ItemsEaten, m_state.ItemsTouched, m_state.ItemsDestroyed, m_state.PeopleStunned, time);
		}

		public IEnumerator DeathSequence()
		{
			m_themeInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			float time = Mathf.RoundToInt(100 * (Time.time - m_state.GameStart)) / 100f;
			yield return new WaitForSeconds(m_deathDelay);
			ProCamera2D.Instance.RemoveAllCameraTargets();
			ProCamera2D.Instance.AddCameraTarget(GodController.Instance.transform);
			ProCamera2D.Instance.VerticalFollowSmoothness = m_cameraSpeed;
			ProCamera2D.Instance.HorizontalFollowSmoothness = m_cameraSpeed;

			PlayerStatus.Instance.SetPaused(true);
			yield return new WaitForSeconds(m_timeToDeath);
			GodController.Instance.Kill();
			yield return new WaitForSeconds(m_timeToDeath);

			PauseMenuController.Instance.EndScreen(true, m_state.ItemsEaten, m_state.ItemsTouched, m_state.ItemsDestroyed, m_state.PeopleStunned, time);
		}

		public void OnQuit()
		{
			m_themeInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		}

		public void IncreaseHappiness(int happiness)
		{
			m_state.Happiness = Mathf.Clamp(m_state.Happiness + happiness, 0, m_maxHappiness);
			HUDController.Instance.AdjustHappy((float)m_state.Happiness / (float)m_maxHappiness);
		}
		public void IncreaseSustinence(int sustience)
		{
			m_state.Sustinence = Mathf.Clamp(m_state.Sustinence + sustience, 0, m_maxSustinence);
			HUDController.Instance.AdjustHunger((float)m_state.Sustinence / (float)m_maxSustinence);
		}
		[System.Serializable]
		public class WorldState
		{
			public float Mood
			{
				get
				{
					return ((float)ItemsTouched * ItemsTouchedAdjustment) +
					((float)ItemsDestroyed * ItemsDestroyedAdjustment) +
					((float)PeopleStunned * PeopleStunnedAdjustment);
				}
			}

			public int Power
			{
				get
				{
					return Mathf.RoundToInt((float)Happiness * HappinessAdjustment);
				}
			}
			public int Happiness;
			public int Sustinence;
			public bool IsEvolved;

			public int ItemsEaten;
			public int ItemsTouched;
			public int ItemsDestroyed;

			public int PeopleStunned;

			public float GameStart;

			//Adjustments
			private const float HappinessAdjustment = .1f;
			private const float ItemsTouchedAdjustment = .05f;
			private const float ItemsDestroyedAdjustment = .25f;
			private const float PeopleStunnedAdjustment = .25f;
		}
	}
}