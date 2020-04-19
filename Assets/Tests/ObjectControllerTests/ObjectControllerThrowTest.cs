using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PBJ
{
    public class ObjectControllerThrowTest : MonoBehaviour
    {
        [SerializeField]
        private ObjectController m_objectController;

        [SerializeField]
        private float m_throwForce = 25;

        [SerializeField]
        private Vector2 m_throwDirection = new Vector2(1, 0);

        private bool m_isFired = false;

        private void OnMouseOver()
        {
            if (m_isFired)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                // Left click to throw
                m_objectController.Throw(transform.position, m_throwDirection * m_throwForce);
                m_isFired = true;
            }
            if (Input.GetMouseButtonDown(1))
            {
                Debug.Log("Pressed secondary button.");
                // Right click to break
                m_objectController.Break();
                m_isFired = true;
            }
        }
    }
}