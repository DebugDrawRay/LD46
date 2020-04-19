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
		private int m_attackDamage;

		[SerializeField]
		private LayerMask m_invalidMask;
		[SerializeField]
		private LayerMask m_attackMask;

		private Vector2 m_currentPatrolPoint;
		private float m_waypointDistance;

		private Rigidbody2D m_rigid;

		private float m_lastPatrolTime;

        private Vector2 m_home;
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

		private enum State
		{
			Patrol,
			Chase,
			Attack
		}
		private State m_currentState;

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
		}
		private void EnterChase()
		{

		}
		private void UpdateChase()
		{

		}
		private void EnterAttack()
		{

		}
		private void UpdateAttack()
		{

		}

		// private void UpdateAxis(InputActionEventData data)
		// {
		// 	switch (data.actionId)
		// 	{
		// 		case Actions.HorizontalMove:
		// 			m_currentInput.x = data.GetAxisRaw();
		// 			break;
		// 		case Actions.VerticalMove:
		// 			m_currentInput.y = data.GetAxisRaw();
		// 			break;
		// 	}
		// 	if (m_status.CanAct)
		// 	{
		// 		m_status.SetFacing(m_currentInput.normalized);
		// 	}
		// }

		// private void UpdateMovement()
		// {
		// 	if (m_status.CanAct)
		// 	{
		// 		if (m_currentInput == Vector2.zero)
		// 		{
		// 			m_currentSpeed = 0;
		// 		}
		// 		else
		// 		{
		// 			m_currentSpeed = m_status.MaxSpeed;
		// 		}
		// 		Vector2 newPosition = (Vector2)transform.position + (m_currentInput.normalized * m_currentSpeed * Time.deltaTime);
		// 		m_rigid.MovePosition(newPosition);
		// 	}
		// }

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
	}
}