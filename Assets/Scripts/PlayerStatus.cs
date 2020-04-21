using UnityEngine;
using PBJ.Configuration;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;

namespace PBJ
{
	public class PlayerStatus : MonoBehaviour
	{
		public static PlayerStatus Instance;

		public int MaxHealth;
		public float MaxSpeed;
		public int MaxCarry
		{
			get
			{
				return m_maxCarry;
			}
		}
		[SerializeField]
		private int m_maxCarry;

		public float PickupRange;
		public float PickupSpeed;
		public float PickupCooldown;

		public float ThrowCooldown;
		public float ThrowForce
		{
			get
			{
				return m_throwForce;
			}
		}
		[SerializeField]
		private int m_throwForce;
		public LayerMask ItemMask;

		[SerializeField]
		private float m_knockbackStrength;
		[SerializeField]
		private float m_knockbackLength;
		[SerializeField]
		private Animator m_anim;
		[SerializeField]
		[FMODUnity.EventRef]
		private string m_damageSound;
		[SerializeField]
		[FMODUnity.EventRef]
		private string m_deathSound;
		[SerializeField]
		private GameObject m_shadow;
		private Tween m_knockTween;
		private Vector2 m_facingDir;
		private int m_currentHealth;

		private bool m_canBeDamaged = true;
		public Vector2 FacingDir
		{
			get
			{
				return m_facingDir;
			}
		}
		public bool CanAct
		{
			get
			{
				return m_canAct && !m_pause && GameController.Instance.CurrentGameState == GameController.GameState.Playing;
			}
		}
		public bool Damaged
		{
			get
			{
				return m_currentHealth < MaxHealth;
			}
		}
		public bool Dead
		{
			get
			{
				return m_currentHealth <= 0;
			}
		}
		private bool m_canAct;
		private bool m_pause;

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
			Spawn();
		}
		public void Spawn()
		{
			m_shadow.SetActive(true);
			SetCanAct(true);
			m_currentHealth = MaxHealth;
			HUDController.Instance.AdjustHealth(m_currentHealth);
			SetFacing(new Vector2(0, -1));
			m_anim.Rebind();
		}
		public void SetCanAct(bool act)
		{
			m_canAct = act;
		}
		public void SetPaused(bool pause)
		{
			m_pause = pause;
		}

		public void SetFacing(Vector2 dir)
		{
			if (GameController.Instance.CurrentGameState != GameController.GameState.Playing)
				return;

			m_facingDir = dir;
			m_anim.SetFloat(AnimationConst.FaceX, m_facingDir.x);
			m_anim.SetFloat(AnimationConst.FaceY, m_facingDir.y);
		}

		public void Damage(int amount, Vector2 origin)
		{
			if (m_canBeDamaged)
			{
				SetCanAct(false);
				m_canBeDamaged = false;
				m_currentHealth--;
				HUDController.Instance.AdjustHealth(m_currentHealth);
				if (Dead)
				{
					Death();
					Vector2 dir = ((Vector2)transform.position - origin).normalized * m_knockbackStrength;

					if (m_knockTween != null && m_knockTween.IsPlaying())
					{
						m_knockTween.Kill();
					}
					Vector2 pos = (Vector2)transform.position + dir;
					m_knockTween = transform.DOMove(pos, m_knockbackLength).SetEase(Ease.OutExpo).SetUpdate(UpdateType.Fixed).Play();
				}
				else
				{
					m_anim.SetTrigger(AnimationConst.Damage);
					RuntimeManager.PlayOneShot(m_damageSound);
					Vector2 dir = ((Vector2)transform.position - origin).normalized * m_knockbackStrength;

					if (m_knockTween != null && m_knockTween.IsPlaying())
					{
						m_knockTween.Kill();
					}
					Vector2 pos = (Vector2)transform.position + dir;
					m_knockTween = transform.DOMove(pos, m_knockbackLength).SetEase(Ease.OutExpo).SetUpdate(UpdateType.Fixed).OnComplete(OnKnockbackComplete).Play();
				}
			}

		}

		private void Death()
		{
			m_shadow.SetActive(false);
			GetComponent<HeldObjectManager>().ScatterStack();
			SetCanAct(false);
			m_anim.SetTrigger(AnimationConst.Death);
			RuntimeManager.PlayOneShot(m_deathSound);
		}

		public void DrainHealth(int drain)
		{
			m_currentHealth -= drain;
			HUDController.Instance.AdjustHealth(m_currentHealth);
			if (Dead)
			{
				Death();
			}
			else
			{
				m_anim.SetTrigger(AnimationConst.Damage);
				RuntimeManager.PlayOneShot(m_damageSound);
			}
		}

		public void RestoreHealth(int restore)
		{
			m_currentHealth = Mathf.Clamp(m_currentHealth + restore, 0, MaxHealth);
			HUDController.Instance.AdjustHealth(m_currentHealth);
		}

		private void OnKnockbackComplete()
		{
			SetCanAct(true);
			m_canBeDamaged = true;
		}

		private void OnColliderEnter2D(Collider2D hit)
		{
			if (m_knockTween != null && m_knockTween.IsPlaying())
			{
				m_knockTween.Kill();
			}
		}
	}
}