﻿using UnityEngine;
using PBJ.Configuration;
using DG.Tweening;
namespace PBJ
{
	public class PlayerStatus : MonoBehaviour
	{
		public static PlayerStatus Instance;

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

		private Tween m_knockTween;
		private Vector2 m_facingDir;

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
				return m_canAct;
			}
		}
		private bool m_canAct = true;

		private void Awake()
		{
			Instance = this;
		}
		public void SetCanAct(bool act)
		{
			m_canAct = act;
		}

		public void SetFacing(Vector2 dir)
		{
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
			if(m_canBeDamaged)
			{
				SetCanAct(false);
				m_canBeDamaged = false;
				m_anim.SetTrigger(AnimationConst.Damage);
				Vector2 dir = ((Vector2)transform.position - origin).normalized * m_knockbackStrength;

				if(m_knockTween != null && m_knockTween.IsPlaying())
				{
					m_knockTween.Kill();
				}
				Vector2 pos = (Vector2)transform.position + dir;
				m_knockTween = transform.DOMove(pos, m_knockbackLength).SetEase(Ease.OutExpo).SetUpdate(UpdateType.Fixed).OnComplete(OnKnockbackComplete).Play();
			}

		}

		private void OnKnockbackComplete()
		{
			SetCanAct(true);
			m_canBeDamaged = true;
		}
	}
}