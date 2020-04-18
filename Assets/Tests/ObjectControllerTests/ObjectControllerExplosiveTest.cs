using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectControllerExplosiveTest : MonoBehaviour
{
    [SerializeField]
    private ObjectController m_objectController;
    private bool m_isFired = false;


    private void OnMouseDown()
    {
        if (m_isFired)
            return;

        if (m_objectController.IsExplosive())
        {
            m_objectController.Explode();
            m_isFired = true;
        }
    }
}
