using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PBJ
{
	public class HumanController : MonoBehaviour
	{
		[Header("Patrol")]
		[SerializeField]
		private float m_patrolRange;
		[SerializeField]
		private float m_patrolSpeed;
		[SerializeField]
		private float m_patrolDelay;

		[Header("Chase")]
		[SerializeField]
		private float m_chaseRange;
		[SerializeField]
		private float m_chaseSpeed;
		[SerializeField]
		private float m_chaseTime;

		[Header("Attack")]
		[SerializeField]
		private float m_attackRange;
		[SerializeField]
		private float m_attackSpeed;
		[SerializeField]
		private int m_attackDamage;
		[SerializeField]
		private int m_attackDelay;

		[SerializeField]
		private LayerMask m_invalidMask;
		[SerializeField]
		private LayerMask m_attackMask;

		private Vector2 m_currentPatrolPoint;
		private float m_waypointDistance;

		private Rigidbody2D m_rigid;

		private float m_lastPatrolTime;
		private float m_lastChaseTime;
		private float m_lastAttackTime;

        private bool m_checkAttackRange;

		private Vector2 m_home;
		private Vector2 m_attackPosition;

		private float AttackRange
		{
			get
			{
				return m_attackRange;// ToDo: Add mood adjustment
			}
		}
		private float ChaseRange
		{
			get
			{
				return m_chaseRange;// ToDo: Add mood adjustment
			}
		}
		private float ChaseTime
		{
			get
			{
				return m_chaseTime;// ToDo: Add mood adjustment
			}
		}
		private float AttackSpeed
		{
			get
			{
				return m_attackSpeed;// ToDo: Add mood adjustment
			}
		}
		private int AttackDamage
		{
			get
			{
				return m_attackDamage;// ToDo: Add mood adjustment
			}
		}
		private enum State
		{
			ReturnToHome,
			Patrol,
			Chase,
			Attack
		}
		private State m_currentState;

		private PlayerStatus m_player
		{
			get
			{
				return PlayerStatus.Instance;
			}
		}
		private void Awake()
		{
			if (!TryGetComponent<Rigidbody2D>(out m_rigid))
			{
				Debug.LogError("No rigidbody assigned");
			}
		}

		private void Start()
		{
			m_home = transform.position;
		}

		private void FixedUpdate()
		{
			UpdateStates();
		}

		private void UpdateStates()
		{
			switch (m_currentState)
			{
				case State.ReturnToHome:
					UpdateReturn();
					break;
				case State.Patrol:
					UpdatePatrol();
					break;
				case State.Chase:
					UpdateChase();
					break;
				case State.Attack:
					UpdateAttack();
					break;
			}
		}
		private void ReturnToHome()
		{
			m_currentState = State.ReturnToHome;
		}
		private void UpdateReturn()
		{
			Vector2 newPos = Vector2.MoveTowards(transform.position, m_home, m_patrolSpeed * Time.deltaTime);
			m_rigid.MovePosition(newPos);
			if (Vector2.Distance(transform.position, m_home) <= m_waypointDistance)
			{
				EnterPatrol();
			}
		}

		private void EnterPatrol()
		{
			m_currentPatrolPoint = FindPatrolPoint();
			m_currentState = State.Patrol;
		}
		private void UpdatePatrol()
		{
			float playerDist = Vector2.Distance(transform.position, PlayerStatus.Instance.transform.position);
			if (playerDist > ChaseRange)
			{
				if (Time.time > m_lastPatrolTime + m_patrolDelay)
				{
					Vector2 newPos = Vector2.MoveTowards(transform.position, m_currentPatrolPoint, m_patrolSpeed * Time.deltaTime);
					m_rigid.MovePosition(newPos);
					if (Vector2.Distance(transform.position, m_currentPatrolPoint) <= m_waypointDistance)
					{
						m_currentPatrolPoint = FindPatrolPoint();
					}
				}
			}
			else
			{
				EnterChase();
			}
		}

		private void EnterChase()
		{
			m_currentState = State.Chase;
			m_lastChaseTime = Time.time;
		}
		private void UpdateChase()
		{
            if (Vector2.Distance(transform.position, m_player.transform.position) <= AttackRange)
			{
				EnterAttack();
			}
			Vector2 newPos = Vector2.MoveTowards(transform.position, m_player.transform.position, m_chaseSpeed * Time.deltaTime);
			m_rigid.MovePosition(newPos);
			if (Time.time > m_lastChaseTime + ChaseTime)
			{
				ReturnToHome();
			}
		}

		private void EnterAttack()
		{
            m_lastAttackTime = 0;
            m_checkAttackRange = false;
			m_currentState = State.Attack;
			m_attackPosition = m_player.transform.position;
		}
		private void UpdateAttack()
		{
			if (Time.time > m_lastAttackTime + m_attackDelay)
			{
                if(m_checkAttackRange)
                {
                    if(Vector2.Distance(transform.position, m_player.transform.position) > AttackRange)
                    {
                        EnterPatrol();
                    }
                    else
                    {
                        m_checkAttackRange = false;
                    }
                }
				Vector2 newPos = Vector2.MoveTowards(transform.position, m_attackPosition, AttackSpeed * Time.deltaTime);
				m_rigid.MovePosition(newPos);

				if (Vector2.Distance(transform.position, m_attackPosition) <= m_waypointDistance)
				{
					m_lastAttackTime = Time.time;
                    m_checkAttackRange = true;
				}
			}
		}

		private Vector2 FindPatrolPoint()
		{
			Vector2 pos = m_home + (Random.insideUnitCircle * m_patrolRange);
			if (Physics2D.OverlapPoint(pos, m_invalidMask))
			{
				return FindPatrolPoint();
			}
			else
			{
				m_lastPatrolTime = Time.time;
				return pos;
			}
		}

        private void OnTriggerEnter2D(Collider2D hit)
        {
            PlayerStatus player = null;
            if(hit.TryGetComponent<PlayerStatus>(out player))
            {
                player.Damage(AttackDamage, transform.position);
            }
        }
	}
}