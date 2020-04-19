using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PBJ
{
	public class GameController : MonoBehaviour
	{
		public static GameController Instance;

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

        public ItemDB.Item[] ItemDb
        {
            get
            {
                return m_itemDb.Items;
            }
        }

		private float m_lastHappinessDrain;
		private float m_lastSustinenceDrain;
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
			Instance = this;
		}

		private void Start()
		{
			m_state = new WorldState() { Happiness = m_initialHappiness, Sustinence = m_initialSustinence };
		}

		private void Update()
		{
			UpdateGodState();
		}

		private void UpdateGodState()
		{
            if(CurrentState.Sustinence <= 0)
            {
                Debug.Log("<color=red>GAME OVER</color>");
            }
            else
            {
			if (Time.time > m_lastSustinenceDrain + m_sustinenceDrainRate)
			{
				CurrentState.Sustinence -= m_sustinenceDrain;
				m_lastSustinenceDrain = Time.time;
			}
			if (Time.time > m_lastHappinessDrain + m_happinessDrainRate)
			{
				CurrentState.Happiness -= m_happinessDrain;
				m_lastHappinessDrain = Time.time;
			}
            }
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