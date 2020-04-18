﻿using System.Collections;
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
        public LayerMask ItemMask;

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
	}
}