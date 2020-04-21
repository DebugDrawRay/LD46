using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PBJ.Configuration;
using FMOD.Studio;
using FMODUnity;

namespace PBJ
{
	public class GodController : MonoBehaviour
	{
		public static GodController Instance;

		[SerializeField]
		private Animator m_anim;
		[SerializeField]
		private SpriteRenderer m_requestDisplay;
		[SerializeField]
		private GameObject m_requestContainer;
		[SerializeField]
		private GameObject m_happyIcon;
		[SerializeField]
		private float m_openDist;
		[SerializeField]
		private float m_requestDist;
		[SerializeField]
		private LayerMask m_itemLayer;
		[SerializeField]
		private float m_closeDelay;
		[SerializeField]
		private float m_statusIconHold;
		[SerializeField]
		private float m_requestDelay;
		[SerializeField]
		[EventRef]
		private string m_requestSound;
		[SerializeField]
		[EventRef]
		private string m_requestGoodSound;
		[SerializeField]
		[EventRef]
		private string m_eatSound;
		[SerializeField]
		[EventRef]
		private string m_deathSound;
		[SerializeField]
		[EventRef]
		private string m_evolveSound;

		[SerializeField]
		private Vector2 m_containerEvolvePos;
		private Vector2 m_containerHome;
		[SerializeField]
		private Vector2 m_happyEvolvePos;
		private Vector2 m_happyHome;

		private bool m_hasOpened;
		private float m_lastOpenTime;

		private string m_request;
		private Coroutine m_checkRequest;

		private bool m_hasRequest;

		private int m_remainingItems;

		private bool m_canEvolve;
		public bool CanEvolve
		{
			get
			{
				return m_canEvolve;
			}
		}
		private PlayerStatus m_player
		{
			get
			{
				return PlayerStatus.Instance;
			}
		}
		private void Awake()
		{
			if (Instance != null)
			{
				Destroy(Instance.gameObject);
			}
			Instance = this;

			m_happyHome = m_happyIcon.transform.position;
			m_containerHome = m_requestContainer.transform.position;
		}
		private void Start()
		{
		}
		public void Spawn()
		{
			m_anim.Rebind();
			MakeNewRequest();
			m_hasRequest = true;
			m_requestContainer.SetActive(false);
			m_happyIcon.SetActive(false);
			m_canEvolve = true;
		}
		private void Update()
		{
			CheckNearbyItems();
		}
		private void CheckNearbyItems()
		{
			float playerDist = Vector2.Distance(transform.position, m_player.transform.position);

			if (m_hasRequest)
			{
				m_requestContainer.SetActive(playerDist <= m_requestDist);
			}
			ObjectController obj = Physics2D.OverlapCircle(transform.position, m_openDist, m_itemLayer)?.GetComponent<ObjectController>();
			if ((playerDist <= m_openDist && m_player.GetComponent<HeldObjectManager>().HasItem) ||
			(obj != null && obj.CurrentState.Thrown))
			{
				if (!m_hasOpened)
				{
					m_anim.SetBool(AnimationConst.Open, true);
					m_hasOpened = true;
				}
				m_lastOpenTime = Time.time;
			}
			else
			{
				if (m_hasOpened)
				{
					if (Time.time > m_lastOpenTime + m_closeDelay)
					{
						m_anim.SetBool(AnimationConst.Open, false);
						m_hasOpened = false;
					}
				}
			}


		}
		public void Feed(ObjectController obj)
		{
			GameController.Instance.IncreaseSustinence(obj.SustinenceProvided);
			if (obj.Id == m_request)
			{
				GameController.Instance.IncreaseHappiness(obj.HappinessProvided);

			}
			if (m_checkRequest != null)
			{
				StopCoroutine(m_checkRequest);
			}
			m_anim.SetTrigger(AnimationConst.Chomp);
			bool success = obj.Id == m_request;
			if (success)
			{
				m_remainingItems--;
			}
			m_checkRequest = StartCoroutine(CheckRequest(success));
			GameController.Instance.CurrentState.ItemsEaten++;
			Destroy(obj.gameObject);
		}
		public void MakeNewRequest()
		{
			RuntimeManager.PlayOneShot(m_requestSound);
			ItemDB.Item item = GameController.Instance.ItemDb[Random.Range(0, GameController.Instance.ItemDb.Length)];
			m_request = item.Category;
			m_requestDisplay.sprite = item.Sprite;
			m_remainingItems = GameController.Instance.ItemsBeforeNewRequest;
			HUDController.Instance.UpdateCategory(item.Sprite);
		}
		public void Kill()
		{
			if (m_checkRequest != null)
			{
				StopCoroutine(m_checkRequest);
			}
			m_hasRequest = false;
			m_requestContainer.SetActive(false);
			m_anim.SetTrigger(AnimationConst.Death);
			RuntimeManager.PlayOneShot(m_deathSound);
		}
		private IEnumerator CheckRequest(bool successful)
		{
			m_hasRequest = false;
			m_canEvolve = false;
			RuntimeManager.PlayOneShot(m_eatSound);
			m_requestContainer.SetActive(false);
			yield return new WaitForSeconds(m_closeDelay);
			if (successful)
			{
				RuntimeManager.PlayOneShot(m_requestGoodSound);
				m_happyIcon.SetActive(true);
				m_canEvolve = true;
				yield return new WaitForSeconds(m_requestDelay);
				m_happyIcon.SetActive(false);
			}
			if (m_remainingItems <= 0)
			{
				MakeNewRequest();
			}
			m_requestContainer.SetActive(true);
			yield return new WaitForSeconds(m_statusIconHold);
			m_requestContainer.SetActive(false);
			m_hasRequest = true;
		}

		public void Evolve()
		{
			m_anim.SetBool(AnimationConst.IsEvolved, true);
			RuntimeManager.PlayOneShot(m_evolveSound);
			m_requestContainer.transform.position = m_containerEvolvePos;
			m_happyIcon.transform.position = m_happyEvolvePos;
		}
	}
}