using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PBJ
{
	public class PlayerStatus : MonoBehaviour
	{
		public float MaxSpeed;
		public int MaxCarry;
		public int MaxItemWeight;

		public float PickupRange;
		public float PickupSpeed;
		public AnimationCurve PickupSpeedCurve;

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

		public void SetCanAct(bool act)
		{
			m_canAct = act;
		}

		public void SetFacing(Vector2 dir)
		{
			if (dir != Vector2.zero)
			{
				m_facingDir = dir;
			}
		}
	}
}