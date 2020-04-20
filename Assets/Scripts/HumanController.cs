using UnityEngine;
using DG.Tweening;
using PBJ.Configuration;
using FMOD.Studio;
using FMODUnity;
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
		[SerializeField]
		private float m_chaseDelay;

		[Header("Attack")]
		[SerializeField]
		private float m_attackRange;
		[SerializeField]
		private float m_attackSpeed;
		[SerializeField]
		private int m_attackDamage;
		[SerializeField]
		private int m_attackDelay;

		[Header("Status")]
		[SerializeField]
		private float m_maxHealth;
		[SerializeField]
		private float m_stunTime;

		[SerializeField]
		private LayerMask m_invalidMask;

		[SerializeField]
		private float m_knockbackStrength;
		[SerializeField]
		private float m_knockbackLength;
		[SerializeField]
		private Animator m_anim;

		private Tween m_knockTween;

		private Vector2 m_currentPatrolPoint;
		[SerializeField]
		private float m_waypointDistance;

		[SerializeField]
		[FMODUnity.EventRef]
		private string m_damageSound;
		[SerializeField]
		[FMODUnity.EventRef]
		private string m_stunSound;

		private Rigidbody2D m_rigid;

		private float m_lastPatrolTime;
		private float m_lastChaseTime;
		private float m_lastAttackTime;
		private float m_lastStunTime;

		private bool m_checkAttackRange;

		private Vector2 m_home;
		private Vector2 m_attackPosition;

		private float m_currentHealth;

		private bool m_canAct;
		private bool m_canBeDamaged;

		private float m_stoppedChaseTime;

		private bool m_cancelMovement;

		private float AttackRange
		{
			get
			{
				return m_attackRange;
			}
		}
		private float ChaseRange
		{
			get
			{
				Debug.Log(m_chaseRange * GameController.Instance.Mood);
				return m_chaseRange * GameController.Instance.Mood;
			}
		}
		private float ChaseTime
		{
			get
			{
				return m_chaseTime;
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
				return m_attackDamage;
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

		private bool Stunned
		{
			get
			{
				return m_currentHealth <= 0;
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
			m_currentHealth = m_maxHealth;
			m_canAct = true;
			m_canBeDamaged = true;
			EnterPatrol();

		}

		private void Update()
		{
			UpdateStun();
		}

		private void UpdateStun()
		{
			if (Stunned)
			{
				if (Time.time > m_lastStunTime + m_stunTime)
				{
					m_anim.SetBool(AnimationConst.Stun, false);
					m_currentHealth = m_maxHealth;
					EnterChase();
					OnKnockbackComplete();
				}
			}
		}

		private void FixedUpdate()
		{
			if (m_canAct && !Stunned)
			{
				UpdateStates();
			}
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
			m_anim.SetBool(AnimationConst.Running, false);
		}
		private void UpdateReturn()
		{
			Vector2 newPos = Vector2.MoveTowards(transform.position, m_home, m_patrolSpeed * Time.deltaTime);
			m_rigid.MovePosition(newPos);
			SetFacing((m_home - (Vector2)transform.position).normalized);
			m_anim.SetBool(AnimationConst.Moving, true);
			if (Vector2.Distance(transform.position, m_home) <= m_waypointDistance || m_cancelMovement)
			{
				if (m_cancelMovement)
				{
					m_cancelMovement = false;
					m_home = transform.position;
				}
				EnterPatrol();
				return;
			}
			float playerDist = Vector2.Distance(transform.position, PlayerStatus.Instance.transform.position);
			if (playerDist <= ChaseRange && !m_player.Dead)
			{
				if (Time.time > m_stoppedChaseTime + m_chaseDelay)
				{
					EnterChase();
					return;
				}
			}
		}

		private void EnterPatrol()
		{
			m_currentPatrolPoint = FindPatrolPoint();
			m_currentState = State.Patrol;
			m_anim.SetBool(AnimationConst.Running, false);
		}
		private void UpdatePatrol()
		{
			float playerDist = Vector2.Distance(transform.position, m_player.transform.position);
			if (playerDist > ChaseRange)
			{
				if (Time.time > m_lastPatrolTime + m_patrolDelay)
				{
					Vector2 newPos = Vector2.MoveTowards(transform.position, m_currentPatrolPoint, m_patrolSpeed * Time.deltaTime);
					m_rigid.MovePosition(newPos);
					SetFacing((m_currentPatrolPoint - (Vector2)transform.position).normalized);
					m_anim.SetBool(AnimationConst.Moving, true);
					if (Vector2.Distance(transform.position, m_currentPatrolPoint) <= m_waypointDistance || m_cancelMovement)
					{
						m_currentPatrolPoint = FindPatrolPoint();
						m_lastPatrolTime = Time.time;
						m_cancelMovement = false;
					}
				}
				else
				{
					m_anim.SetBool(AnimationConst.Moving, false);
				}
			}
			else
			{
				if (!m_player.Dead)
				{
					if (Time.time > m_stoppedChaseTime + m_chaseDelay)
					{
						Debug.Log("Chase");
						EnterChase();
						return;
					}
				}
			}
		}

		private void EnterChase()
		{
			m_currentState = State.Chase;
			m_lastChaseTime = Time.time;
			m_anim.SetBool(AnimationConst.Running, true);
		}
		private void UpdateChase()
		{
			if (m_player.Dead)
			{
				ReturnToHome();
				return;
			}
			if (Vector2.Distance(transform.position, m_player.transform.position) <= AttackRange)
			{
				EnterAttack();
				return;
			}
			Vector2 newPos = Vector2.MoveTowards(transform.position, m_player.transform.position, m_chaseSpeed * Time.deltaTime);
			m_rigid.MovePosition(newPos);
			SetFacing(((Vector2)m_player.transform.position - (Vector2)transform.position).normalized);
			m_anim.SetBool(AnimationConst.Moving, true);
			if (Time.time > m_lastChaseTime + ChaseTime)
			{
				m_stoppedChaseTime = Time.time;
				ReturnToHome();
				return;
			}
		}

		private void EnterAttack()
		{
			m_lastAttackTime = 0;
			m_checkAttackRange = false;
			m_currentState = State.Attack;
			m_attackPosition = m_player.transform.position;
			m_anim.SetBool(AnimationConst.Running, true);
		}
		private void UpdateAttack()
		{
			if (m_player.Dead)
			{
				ReturnToHome();
				return;
			}
			if (Time.time > m_lastAttackTime + m_attackDelay)
			{
				if (m_checkAttackRange)
				{
					if (Vector2.Distance(transform.position, m_player.transform.position) > AttackRange)
					{
						ReturnToHome();
						return;
					}
					else
					{
						m_checkAttackRange = false;
					}
				}
				Vector2 newPos = Vector2.MoveTowards(transform.position, m_attackPosition, AttackSpeed * Time.deltaTime);
				m_rigid.MovePosition(newPos);
				SetFacing((m_attackPosition - (Vector2)transform.position).normalized);
				m_anim.SetBool(AnimationConst.Moving, true);
				m_anim.SetBool(AnimationConst.Running, true);
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
				return pos;
			}
		}

		private void OnTriggerEnter2D(Collider2D hit)
		{
			if (!Stunned)
			{
				PlayerStatus player = null;
				if (hit.TryGetComponent<PlayerStatus>(out player))
				{
					player.Damage(AttackDamage, transform.position);
				}
				HeldObjectManager held = null;
				if (hit.TryGetComponent<HeldObjectManager>(out held))
				{
					held.ScatterStack();
				}
			}
		}
		private void OnCollisionEnter2D(Collision2D hit)
		{
			m_cancelMovement = true;
		}

		public void Damage(int amount, Vector2 origin)
		{
			if (m_canBeDamaged)
			{
				m_canAct = false;
				m_canBeDamaged = false;
				m_currentHealth--;

				Vector2 dir = ((Vector2)transform.position - origin).normalized * m_knockbackStrength;
				if (m_knockTween != null && m_knockTween.IsPlaying())
				{
					m_knockTween.Kill();
				}
				Vector2 pos = (Vector2)transform.position + dir;
				m_knockTween = transform.DOMove(pos, m_knockbackLength).SetEase(Ease.OutExpo).SetUpdate(UpdateType.Fixed);
				if (Stunned)
				{
					m_anim.SetBool(AnimationConst.Stun, true);
					Debug.Log("Why");
					m_lastStunTime = Time.time;
					RuntimeManager.PlayOneShot(m_stunSound);
					GameController.Instance.CurrentState.PeopleStunned++;
				}
				else
				{
					RuntimeManager.PlayOneShot(m_damageSound);
					m_anim.SetTrigger(AnimationConst.Damage);
					m_knockTween.OnComplete(OnKnockbackComplete);
				}
				m_knockTween.Play();

			}

		}
		private void OnKnockbackComplete()
		{
			m_canAct = true;
			m_canBeDamaged = true;
		}

		private void SetFacing(Vector2 dir)
		{
			if (dir != Vector2.zero)
			{
				m_anim.SetFloat(AnimationConst.FaceX, dir.x);
				m_anim.SetFloat(AnimationConst.FaceY, dir.y);
			}
		}
	}
}