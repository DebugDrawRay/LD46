using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PBJ.Configuration;

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
		private float m_openDist;
				[SerializeField]
		private float m_requestDist;
		[SerializeField]
		private LayerMask m_itemLayer;
		[SerializeField]
		private float m_closeDelay;

		private bool m_hasOpened;
		private float m_lastOpenTime;

		private string m_request;

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
		}
		private void Update()
		{
			CheckNearbyItems();
		}
		private void CheckNearbyItems()
		{
			float playerDist = Vector2.Distance(transform.position, m_player.transform.position);
			m_requestContainer.SetActive(playerDist <= m_requestDist);

			if (Physics2D.OverlapCircle(transform.position, m_openDist, m_itemLayer) ||
			(playerDist <= m_openDist && m_player.GetComponent<HeldObjectManager>().HasItem))
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
			GameController.Instance.CurrentState.Sustinence += obj.SustinenceProvided;
			if (obj.name == m_request)
			{
				GameController.Instance.CurrentState.Happiness += obj.HappinessProvided;
			}
			Destroy(obj.gameObject);
		}
		public void MakeNewRequest()
		{
			ItemDB.Item item = GameController.Instance.ItemDb[Random.Range(0, GameController.Instance.ItemDb.Length)];
			m_request = item.Prefab.name;
			m_requestDisplay.sprite = item.Sprite;
		}
		public void Kill()
		{

		}
	}
}