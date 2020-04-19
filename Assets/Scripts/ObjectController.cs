using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace PBJ
{
	public class ObjectController : MonoBehaviour
	{
        [SerializeField]
        private float m_objectHeight;
        public float ObjectHeight
        {
            get
            {
                return m_objectHeight;
            }
        }
		[SerializeField]
		private bool m_isExplosive;
		private bool m_isBreakable;

		public enum ObjectState
		{
			Breaking = 1,
			Exploding = 2
		}

		private ObjectState m_objectState;

		[SerializeField]
		private GameObject m_explosionPrefab;

		private Rigidbody2D m_rigid;
        private Collider2D m_collider;

		private void Awake()
		{
			if (!TryGetComponent<Rigidbody2D>(out m_rigid))
			{
				Debug.LogError("No rigidbody found");
			}
            if(!TryGetComponent<Collider2D>(out m_collider))
            {
                Debug.LogError("No collider found");
            }
		}
		public void Explode()
		{
			Assert.IsTrue(m_isExplosive);
			Assert.IsNotNull(m_explosionPrefab);

			m_objectState = ObjectState.Exploding;

			Instantiate(m_explosionPrefab, transform.position, transform.rotation);
			gameObject.SetActive(false);
		}

		public bool IsExplosive()
		{
			return m_isExplosive;
		}

		private bool CanExplode()
		{
			return m_isExplosive && m_objectState != ObjectState.Exploding && m_objectState != ObjectState.Breaking;
		}

		public void Break()
		{
			Assert.IsTrue(m_isBreakable);

			m_objectState = ObjectState.Breaking;

			if (m_isExplosive)
			{
				Explode();
			}
			else
			{
				// Break routine???
			}
		}

		public bool IsBreakable()
		{
			return m_isBreakable;
		}

		private bool CanBreak()
		{
			return m_isBreakable && m_objectState != ObjectState.Exploding && m_objectState != ObjectState.Breaking;
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			if (collision.gameObject.tag == "Explosion")
			{
				if (CanExplode())
					Explode();
				else if (CanBreak())
					Break();
			}
		}

		public void PickedUp()
		{
            m_rigid.bodyType = RigidbodyType2D.Kinematic;
            m_rigid.velocity = Vector3.zero;
            m_collider.enabled = false;
		}

		public void Throw(Vector2 origin, Vector2 force)
		{
            transform.SetParent(null);
            transform.position = origin;
            m_rigid.bodyType = RigidbodyType2D.Dynamic;
            m_collider.enabled = true;
            m_rigid.AddForce(force, ForceMode2D.Impulse);
		}
	}
}