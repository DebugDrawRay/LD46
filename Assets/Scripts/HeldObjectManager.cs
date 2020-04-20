using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using PBJ.Configuration;
using PBJ.Configuration.Input;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
namespace PBJ
{
	public class HeldObjectManager : MonoBehaviour
	{
		[SerializeField]
		private Transform m_stackContainer;
		[SerializeField]
		private float m_reorgSpeed;
		private Vector2 StackOrigin
		{
			get
			{
				return m_stackContainer.transform.position;
			}
		}

		private Player m_input;
		private PlayerStatus m_status;
		[SerializeField]
		private Animator m_anim;
		[SerializeField]
		private float m_scatterStrength;
		[SerializeField]
		private LayerMask m_throwTargetLayer;
		[SerializeField]
		[FMODUnity.EventRef]
		private string m_pickupSound;
		[SerializeField]
		[FMODUnity.EventRef]
		private string m_throwSound;

		private float m_stackHeight = 0;

		private bool m_canThrow = true;
		private float m_lastThrowTime;
		private bool m_canPickup = false;
		private float m_lastPickupTime;

		private Queue<ObjectController> m_objectStack = new Queue<ObjectController>();

		private Coroutine m_pickup;

		public bool HasItem
		{
			get
			{
				return m_objectStack.Count > 0;
			}
		}
		private const float m_pickupOffset = 2.5f;
		private const int StackCurveSamples = 10;
		private void Awake()
		{
			m_input = ReInput.players.GetPlayer(0);
			m_input.AddInputEventDelegate(Pickup, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Actions.Throw);
			m_input.AddInputEventDelegate(Throw, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Actions.Pickup);

			if (!TryGetComponent<PlayerStatus>(out m_status))
			{
				Debug.LogError("No status found");
			}
		}

		private void Update()
		{
			if (!m_canThrow)
			{
				if (Time.time > m_lastThrowTime + m_status.ThrowCooldown)
				{
					m_canThrow = true;
				}
			}
			if (!m_canPickup)
			{
				if (Time.time > m_lastPickupTime + m_status.PickupCooldown)
				{
					m_canPickup = true;
				}
			}
		}

		private void Pickup(InputActionEventData data)
		{
			if (m_canPickup && m_status.CanAct && m_objectStack.Count < m_status.MaxCarry)
			{
				Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, m_status.PickupRange, m_status.ItemMask);
				if (hits.Length > 0)
				{
					float lastDist = 1000;
					ObjectController obj = null;
					foreach (Collider2D hit in hits)
					{
						float dist = Vector2.Distance(transform.position, hit.transform.position);
						if (dist < lastDist)
						{
							obj = hit.gameObject.GetComponent<ObjectController>();
							lastDist = dist;
						}
					}
					if (obj != null && !obj.CurrentState.Thrown && !obj.CurrentState.Held)
					{
						if (m_pickup != null)
						{
							StopCoroutine(m_pickup);
						}
						m_lastPickupTime = Time.time;
						m_canPickup = false;
						m_anim.SetTrigger(AnimationConst.Pickup);
						m_pickup = StartCoroutine(PickupObject(obj));
						GameController.Instance.CurrentState.ItemsTouched++;
					}
				}
			}
		}

		private IEnumerator PickupObject(ObjectController obj)
		{
			m_status.SetCanAct(false);
			obj.PickedUp();
			RuntimeManager.PlayOneShot(m_pickupSound);
			m_objectStack.Enqueue(obj);
			Vector2 p0 = (Vector2)obj.transform.position;
			Vector2 p1 = (Vector2)obj.transform.position + new Vector2(0, m_stackHeight + m_pickupOffset);
			Vector2 p2 = StackOrigin + new Vector2(0, m_stackHeight);
			float t;
			Vector2 position;
			int currentPoint = 0;
			while (currentPoint < StackCurveSamples)
			{
				t = currentPoint / (StackCurveSamples - 1.0f);
				position = (1.0f - t) * (1.0f - t) * p0 + 2.0f * (1.0f - t) * t * p1 + t * t * p2;
				float speed = m_status.PickupSpeed * Time.deltaTime;
				obj.transform.position = Vector2.MoveTowards(obj.transform.position, position, speed);
				if (Vector2.Distance(obj.transform.position, position) <= .05f)
				{
					currentPoint++;
				}
				yield return null;
			}
			obj.transform.SetParent(m_stackContainer);
			obj.transform.position = p2;
			m_stackHeight += obj.ObjectHeight;
			m_anim.SetBool(AnimationConst.Carry, true);
			m_status.SetCanAct(true);
		}

		private void Throw(InputActionEventData data)
		{
			if (m_status.CanAct && m_canThrow && m_objectStack.Count > 0)
			{
				RuntimeManager.PlayOneShot(m_throwSound);
				m_anim.SetTrigger(AnimationConst.Throw);
				ObjectController obj = m_objectStack.Dequeue();
				obj.transform.DOComplete();

				Vector2 throwDir = m_status.FacingDir * m_status.ThrowForce;
				RaycastHit2D hit = Physics2D.Raycast(transform.position, m_status.FacingDir, Mathf.Infinity, m_throwTargetLayer);
				if (hit.collider != null)
				{
					throwDir = ((Vector2)hit.transform.position - (Vector2)obj.transform.position).normalized * m_status.ThrowForce;
				}

				obj.Throw(obj.transform.position, throwDir);
				ReorganizeStack();
				m_lastThrowTime = Time.time;
				m_canThrow = false;
			}
			m_anim.SetBool(AnimationConst.Carry, m_objectStack.Count > 0);
		}

		private void ReorganizeStack()
		{
			m_stackHeight = 0;
			foreach (Transform obj in m_stackContainer)
			{
				obj.transform.DOComplete();
				obj.transform.DOLocalMove(new Vector2(0, m_stackHeight), m_reorgSpeed).SetEase(Ease.OutBounce).Play();
				m_stackHeight += obj.GetComponent<ObjectController>().ObjectHeight;
			}
		}

		public void ScatterStack()
		{
			if (m_pickup != null)
			{
				StopCoroutine(m_pickup);
			}
			while (m_objectStack.Count > 0)
			{
				ObjectController obj = m_objectStack.Dequeue();
				obj.transform.DOComplete();
				Vector2 dir = Random.insideUnitCircle.normalized;
				obj.Drop(obj.transform.position, dir * m_scatterStrength);
			}
			m_stackHeight = 0;
			m_anim.SetBool(AnimationConst.Carry, false);
		}
	}
}