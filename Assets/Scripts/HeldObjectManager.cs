using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using PBJ.Configuration.Input;

namespace PBJ
{
	public class HeldObjectManager : MonoBehaviour
	{
		[SerializeField]
		private Transform m_stackContainer;
		private Vector2 StackOrigin
		{
			get
			{
				return m_stackContainer.transform.position;
			}
		}

		private Player m_input;
		private PlayerStatus m_status;

		private float m_stackHeight = 0;
		private bool m_canAct = true;

		private Queue<ObjectController> m_objectStack = new Queue<ObjectController>();

		private const float m_pickupOffset = 2;
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

		private void Pickup(InputActionEventData data)
		{
			if (m_status.CanAct)
			{
				Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, m_status.PickupRange, m_status.ItemMask);
				if (hits.Length > 0)
				{
					float lastDist = m_status.PickupRange;
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
					if (obj != null)
					{

						StartCoroutine(PickupObject(obj));
					}
				}
			}
		}

		private IEnumerator PickupObject(ObjectController obj)
		{
            obj.PickedUp();
            m_objectStack.Enqueue(obj);
			m_status.SetCanAct(false);
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
			m_status.SetCanAct(true);
            m_stackHeight += obj.ObjectHeight;
		}

		private void Throw(InputActionEventData data)
		{
			if (m_objectStack.Count > 0)
			{
				ObjectController obj = m_objectStack.Dequeue();
				obj.Throw(transform.position, m_status.FacingDir * m_status.ThrowForce);
                ReorganizeStack();
			}
		}

        private void ReorganizeStack()
        {
            m_stackHeight = 0;
            foreach(Transform obj in m_stackContainer)
            {
                obj.transform.position = StackOrigin + new Vector2(0, m_stackHeight);
                m_stackHeight += obj.GetComponent<ObjectController>().ObjectHeight;
            }
        }
	}
}