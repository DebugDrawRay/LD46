using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PBJ
{
    public class ExplosionStat : MonoBehaviour
    {
        [SerializeField]
        private Animator m_animator;

        [SerializeField]
        private int DamageAmount;

        public void Start()
        {
            m_animator.SetTrigger("Explode"); // TODO: Change this to use id instead of string
        }

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.TryGetComponent(out ObjectController objectController))
            {
                objectController.Damage(DamageAmount);
            }
        }
    }
}