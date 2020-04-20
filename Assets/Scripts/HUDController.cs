using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PBJ
{
	public class HUDController : MonoBehaviour
	{
		public static HUDController Instance;
		[SerializeField]
		private Image m_hungerMeter;
		[SerializeField]
		private Image m_happyMeter;
		[SerializeField]
		private Image m_healthMeter;

		private void Awake()
		{
			if (Instance != null)
			{
				Destroy(Instance.gameObject);
			}
			Instance = this;
		}

        public void AdjustHealth(float health)
        {
            m_healthMeter.fillAmount = health;
        }
        public void AdjustHappy(float happy)
        {
            m_happyMeter.fillAmount = happy;

        }
        public void AdjustHunger(float hunger)
        {
            m_hungerMeter.fillAmount = hunger;
        }
	}
}