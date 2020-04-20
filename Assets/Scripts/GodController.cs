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
		private Vector2 m_requestContainerLargePositionOffset;
		[SerializeField]
		private float m_openDist;
		[SerializeField]
		private float m_requestDist;
		[SerializeField]
		private LayerMask m_itemLayer;
		[SerializeField]
		private float m_evolveDelay;
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
		private string m_requestBadSound;
		[SerializeField]
		[EventRef]
		private string m_mouthOpenSound;
		[SerializeField]
		[EventRef]
		private string m_eatSound;
		[SerializeField]
		[EventRef]
		private string m_deathSound;
		[SerializeField]
		[EventRef]
		private string m_evolveSound;


		private bool m_hasOpened;
		private float m_lastOpenTime;

		private string m_request;
		private Coroutine m_checkRequest;
		private Coroutine m_evolveRoutine;

		private bool m_hasRequest;

		[HideInInspector]
		public bool IsEvolving;

		public delegate void EvolveCompleteEventCallback();

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
		}

		private void Update()
		{
			if (IsEvolving)
				return;

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
			m_checkRequest = StartCoroutine(CheckRequest(obj.Id == m_request));
			GameController.Instance.CurrentState.ItemsEaten++;
			Destroy(obj.gameObject);
		}

		public void MakeNewRequest()
		{
			RuntimeManager.PlayOneShot(m_requestSound);
			ItemDB.Item item = GameController.Instance.ItemDb[Random.Range(0, GameController.Instance.ItemDb.Length)];
			m_request = item.Prefab.GetComponent<ObjectController>().Id;
			m_requestDisplay.sprite = item.Sprite;
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
			RuntimeManager.PlayOneShot(m_eatSound);
			m_requestContainer.SetActive(false);
			yield return new WaitForSeconds(m_requestDelay);
			if(successful)
			{
				RuntimeManager.PlayOneShot(m_requestGoodSound);
				yield return new WaitForSeconds(m_requestDelay);
				MakeNewRequest();
			}
			m_requestContainer.SetActive(true);
			yield return new WaitForSeconds(m_statusIconHold);
			m_requestContainer.SetActive(false);
			m_hasRequest = true;
		}

		public void Evolve(EvolveCompleteEventCallback eventCallback)
		{
			if (m_checkRequest != null)
				StopCoroutine(m_checkRequest);

			IsEvolving = true;
			m_evolveRoutine = StartCoroutine(EvolveRoutine(eventCallback));
		}

		private IEnumerator EvolveRoutine(EvolveCompleteEventCallback eventCallback)
		{
			
			m_anim.SetBool(AnimationConst.IsEvolved, true);
			m_anim.SetTrigger(AnimationConst.Evolve);
			m_hasRequest = false;
			RuntimeManager.PlayOneShot(m_evolveSound);
			m_requestContainer.SetActive(false);

			Vector2 newPosition = m_requestContainer.transform.position;
			newPosition += m_requestContainerLargePositionOffset;
			m_requestContainer.transform.position = newPosition;

			yield return new WaitForSeconds(m_evolveDelay);

			IsEvolving = false;
			m_requestContainer.SetActive(true);
			eventCallback.Invoke();
		}
	}
}