using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PBJ.Configuration;

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

		private Vector2 m_facingDir;
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

		[SerializeField]
		private Animator m_anim;
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
	}
}