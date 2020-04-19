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

        [Header("Explosion Attributes")]
        [SerializeField]
        private bool m_isExplosive;
        [SerializeField]
        private float m_isExplosionDelaySec;
        [SerializeField]
        private GameObject m_explosionPrefab;

        [Space]
        [SerializeField]
        private bool m_isBreakable;
        [SerializeField]
        private GameObject m_breakPrefab;
        [SerializeField]
        private uint m_numberOfBreakObjects;
        [SerializeField]
        private float m_breakObjectsSpawnRadius;
        [SerializeField]
        // Amount of damage this object will take upon collision
        private int m_selfDamage;
        [SerializeField]
        // Amount of damage that this object can inflict upon other objects
        private int m_inflictDamage;

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

        public struct ObjectState
        {
            public bool Breaking;
            public bool Exploding;
            public bool Thrown;
            public bool Damaging;
            public bool SelfDamaging;
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
            if(m_objectState.Thrown)
            {
                if(Time.time > m_lastThrowTime + TimeInThrown)
                {
                    Debug.Log("Done");
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

            if (m_isExplosive)
            {
                Explode();
                return;
            }

            m_objectState.Breaking = true;


            for (int i = 0; i < m_numberOfBreakObjects; i++)
            {
                GameObject spawnedObject = Instantiate(m_breakPrefab, transform.position, transform.rotation);
                if (spawnedObject.TryGetComponent(out Rigidbody2D spawnedRb))
                {
                    float x = Mathf.Lerp(-1, 1, (float) i / m_numberOfBreakObjects) * m_breakObjectsSpawnRadius;
                    float y = x;
                    spawnedRb.AddForce(new Vector2(x, y));
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
            m_objectState.Thrown = true;
            m_lastThrowTime = Time.time;
        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
            if (m_objectState.Thrown)
            {
                m_objectState.Thrown = false;

                if (collision.gameObject.TryGetComponent(out ObjectController other))
                {
                    other.Damage(m_inflictDamage);
                }
                if(collision.gameObject.TryGetComponent(out HumanController human ))
                {
                    human.Damage(m_inflictDamage, transform.position);
                }

                Damage(m_selfDamage);
            }
        }
    }
}
