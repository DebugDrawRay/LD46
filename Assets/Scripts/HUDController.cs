using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMOD.Studio;
using FMODUnity;
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
		[SerializeField]
		private Image m_warning;
		[SerializeField]
		[EventRef]
		private string m_warnSound;

		private bool m_warnSoundPlay;

		private EventInstance m_warnInstance;
		private void Awake()
		{
			if (Instance != null)
			{
				Destroy(Instance.gameObject);
			}
			Instance = this;

			m_warnInstance = RuntimeManager.CreateInstance(m_warnSound);
			m_warnInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
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
		public void Warn(bool active)
		{
			m_warning.gameObject.SetActive(active);
			if (active)
			{
				if (!m_warnSoundPlay)
				{
					m_warnInstance.start();
					m_warnSoundPlay = true;
				}

			}
			else
			{
				if (m_warnSoundPlay)
				{
					m_warnInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
					m_warnSoundPlay = false;
				}

			}
		}
	}
}