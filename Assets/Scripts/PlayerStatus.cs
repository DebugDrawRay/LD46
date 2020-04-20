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
		public int MaxCarry;
		public int MaxItemWeight;

		public float PickupRange;
		public float PickupSpeed;

		public float ThrowCooldown;
		public float ThrowForce;

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
				return m_canAct && !m_pause;
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
			SetCanAct(true);
			m_currentHealth = MaxHealth;
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

			if (dir != Vector2.zero)
			{
				m_facingDir = dir;
				if (dir.x != 0)
				{
					m_anim.SetFloat(AnimationConst.FaceX, m_facingDir.x);
				}
				if (dir.y != 0)
				{
					m_anim.SetFloat(AnimationConst.FaceY, m_facingDir.y);
				}
			}
		}

		public void Damage(int amount, Vector2 origin)
		{
			if (m_canBeDamaged)
			{
				SetCanAct(false);
				m_canBeDamaged = false;
				m_currentHealth--;
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
			GetComponent<HeldObjectManager>().ScatterStack();
			SetCanAct(false);
			m_anim.SetTrigger(AnimationConst.Death);
			RuntimeManager.PlayOneShot(m_deathSound);
		}

		public void DrainHealth(int drain)
		{
			m_currentHealth -= drain;
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

		private void OnKnockbackComplete()
		{
			SetCanAct(true);
			m_canBeDamaged = true;
		}
	}
}