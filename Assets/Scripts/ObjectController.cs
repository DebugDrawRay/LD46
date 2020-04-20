using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using FMOD.Studio;
using FMODUnity;
namespace PBJ
{
	public class ObjectController : MonoBehaviour
	{
        public string Id;
		[SerializeField]
		private float m_objectHeight;
		public float ObjectHeight
		{
			get
			{
				return m_objectHeight;
			}
		}

		[Header("Explosion Attributes")]
		[SerializeField]
		private bool m_isExplosive;
		[SerializeField]
		private float m_isExplosionDelaySec;
		[SerializeField]
		private GameObject m_explosionPrefab;

		[Header("Break Attributes")]
		[SerializeField]
		private bool m_isBreakable;
		[SerializeField]
		private GameObject m_breakPrefab;
		[SerializeField]
		private uint m_numberOfBreakObjects;
		[SerializeField]
		private float m_breakObjectsSpawnRadius;
		[SerializeField]
		[Range(0f, 100f)]
		private float m_breakObjectsSpawnRadiusVariationPercent;
		[SerializeField]
		private float m_breakObjectsSpawnForce;
		[SerializeField]
		[Range(0f, 100f)]
		private float m_breakObjectsSpawnForceVariationPercent;

		[Space]

		[SerializeField]
		// Amount of damage this object will take upon collision
		private int m_selfDamage;
		[SerializeField]
		// Amount of damage that this object can inflict upon other objects
		private int m_inflictDamage;

		[SerializeField]
		private int m_sustinenceProvided;
		public int SustinenceProvided
		{
			get
			{
				return m_sustinenceProvided;
			}
		}
		[SerializeField]
		private int m_happinessProvided;
		public int HappinessProvided
		{
			get
			{
				return m_happinessProvided;
			}
		}

		[SerializeField]
		[FMODUnity.EventRef]
		private string m_collideSound;
		[SerializeField]
		[FMODUnity.EventRef]
		private string m_breakSound;


		//how much time to stay in the thrown state if not acted upon by a collision
		private const float TimeInThrown = 2f;

		public int InflictDamage
		{
			get
			{
				return m_inflictDamage;
			}
		}


		[SerializeField]
		private int m_health;

		[System.Serializable]
		public struct ObjectState
		{
			public bool Breaking;
			public bool Exploding;
			public bool Thrown;
			public bool Held;
		}

		[SerializeField]
		private ObjectState m_objectState;

		public ObjectState CurrentState
		{
			get
			{
				return m_objectState;
			}
		}
		private Rigidbody2D m_rigid;
		private Collider2D m_collider;

		private float m_lastThrowTime;

		private void OnDrawGizmosSelected()
		{
			Gizmos.DrawWireSphere(transform.position, m_breakObjectsSpawnRadius);
		}

		private void Awake()
		{
			// TODO: Add correct preprocessor to compile out these if statements.
			if (m_isExplosive)
			{
				Assert.IsTrue(m_health > 0, "Explosive objects need health greater than 0");
			}
			if (m_isBreakable)
			{
				Assert.IsTrue(m_health > 0, "Breakable objects need health greater than 0");
			}


			if (!TryGetComponent<Rigidbody2D>(out m_rigid))
			{
				Debug.LogError("No rigidbody found");
			}
			if (!TryGetComponent<Collider2D>(out m_collider))
			{
				Debug.LogError("No collider found");
			}
		}

		private void Update()
		{
			if (m_objectState.Thrown)
			{
				if (Time.time > m_lastThrowTime + TimeInThrown)
				{
					m_objectState.Thrown = false;
				}
			}
		}
		private IEnumerator ExplodeRoutine()
		{
			yield return new WaitForSeconds(m_isExplosionDelaySec);
			Instantiate(m_explosionPrefab, transform.position, transform.rotation);
			gameObject.SetActive(false);
		}

		public void Explode()
		{
			Assert.IsTrue(m_isExplosive);
			Assert.IsNotNull(m_explosionPrefab);

			if (!CanExplode())
				return;


			m_objectState.Exploding = true;
			StartCoroutine(ExplodeRoutine());
		}

		public bool IsExplosive()
		{
			return m_isExplosive;
		}

		private bool CanExplode()
		{
			return m_isExplosive && !m_objectState.Exploding && !m_objectState.Breaking;
		}

		public void Break()
		{
			Assert.IsTrue(m_isBreakable);
            RuntimeManager.PlayOneShot(m_breakSound);
			GameController.Instance.CurrentState.ItemsDestroyed++;

			m_objectState.Breaking = true;
			Instantiate(m_explosionPrefab, transform.position, transform.rotation);

			if (m_breakPrefab)
			{
				Assert.IsTrue(m_numberOfBreakObjects > 0, "Breakable objects that spawn smaller objects need number of break objects greater than zero");

				// Spawn the break objects and push them away from the epicenter
				for (int i = 0; i < m_numberOfBreakObjects; i++)
				{
					float angleDeg = Mathf.Lerp(0f, 360f, (float)i / m_numberOfBreakObjects);

					// Calculate the spawn position along the spawn radius
					float spawnPositionAngleDeg = angleDeg + (360f * Random.Range(0f, m_breakObjectsSpawnRadiusVariationPercent / 100f));
					float radiusPositionX = m_breakObjectsSpawnRadius * Mathf.Cos(spawnPositionAngleDeg * Mathf.Deg2Rad);
					float radiusPositionY = m_breakObjectsSpawnRadius * Mathf.Sin(spawnPositionAngleDeg * Mathf.Deg2Rad);
					Vector2 spawnPosition = new Vector2(transform.position.x + radiusPositionX, transform.position.y + radiusPositionY);

					GameObject spawnedObject = Instantiate(m_breakPrefab, spawnPosition, transform.rotation);
					if (spawnedObject.TryGetComponent(out Rigidbody2D spawnedRb))
					{
						// Calculate the angle for the force to be applied
						float spawnForceAngleDeg = angleDeg + (360f * Random.Range(0f, m_breakObjectsSpawnForceVariationPercent / 100f));
						float forceX = m_breakObjectsSpawnForce * Mathf.Cos(spawnForceAngleDeg * Mathf.Deg2Rad);
						float forceY = m_breakObjectsSpawnForce * Mathf.Sin(spawnForceAngleDeg * Mathf.Deg2Rad);
						spawnedRb.AddForce(new Vector2(forceX, forceY));
					}
				}
			}

			gameObject.SetActive(false);
		}

		public bool IsBreakable()
		{
			return m_isBreakable;
		}

		private bool CanBreak()
		{
			return m_isBreakable && !m_objectState.Exploding && !m_objectState.Breaking;
		}

		public void Damage(int damageAmount)
		{
			m_health -= damageAmount;
			if (m_health <= 0)
			{
				if (CanBreak())
					Break();
			}
		}

		public void PickedUp()
		{
			m_rigid.bodyType = RigidbodyType2D.Kinematic;
			m_rigid.velocity = Vector3.zero;
			m_objectState.Held = true;
			m_collider.enabled = false;
		}

		public void Throw(Vector2 origin, Vector2 force)
		{
			m_objectState.Held = false;
			m_objectState.Thrown = true;
			transform.SetParent(null);
			transform.position = origin;
			m_rigid.bodyType = RigidbodyType2D.Dynamic;
			m_collider.enabled = true;
			m_rigid.AddForce(force, ForceMode2D.Impulse);
			m_lastThrowTime = Time.time;
		}

		public void Drop(Vector2 origin, Vector2 force)
		{
			transform.SetParent(null);
			transform.position = origin;
			m_rigid.bodyType = RigidbodyType2D.Dynamic;
			m_collider.enabled = true;
			m_rigid.AddForce(force, ForceMode2D.Impulse);
			m_objectState.Held = false;
		}

		public void OnCollisionEnter2D(Collision2D collision)
		{
			if (collision.gameObject.TryGetComponent(out GodController god))
			{
				god.Feed(this);
				Destroy(gameObject);
			}

			if (m_objectState.Thrown)
			{
				Debug.Log("Collide");
				m_objectState.Thrown = false;

				if (collision.gameObject.TryGetComponent(out ObjectController other))
				{
					other.Damage(m_inflictDamage);
                    RuntimeManager.PlayOneShot(m_collideSound);
				}
				if (collision.gameObject.TryGetComponent(out HumanController human))
				{
					human.Damage(m_inflictDamage, transform.position);
				}
				Damage(m_selfDamage);
			}
		}
	}
}
