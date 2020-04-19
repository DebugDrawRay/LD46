using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace PBJ
{
	public class ObjectController : MonoBehaviour
	{
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


        //Physics properties
        private float m_mass;
        private float m_linearDrag;
        private float m_angularDrag;


		private void Awake()
		{
			if (TryGetComponent<Rigidbody2D>(out m_rigid))
			{
                m_mass = m_rigid.mass;
                m_linearDrag = m_rigid.drag;
                m_angularDrag = m_rigid.angularDrag;
			}
			else
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
            m_collider.enabled = false;
		}

		public void Throw(Vector2 force)
		{
            m_rigid.bodyType = RigidbodyType2D.Dynamic;
            m_collider.enabled = true;
            m_rigid.AddForce(force, ForceMode2D.Impulse);
            transform.SetParent(null);
		}
	}
}