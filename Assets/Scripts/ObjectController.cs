using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ObjectController : MonoBehaviour
{
    [SerializeField]
    private bool m_isExplosive;
    private bool m_isBreakable;

    public enum ObjectState
    {
        Breaking = 1,
        Exploding = 2
    }

    private ObjectState m_objectState;

    [SerializeField]
    private GameObject m_explosionPrefab;

    public void Explode()
    {
        Assert.IsTrue(m_isExplosive);
        Assert.IsNotNull(m_explosionPrefab);

        m_objectState = ObjectState.Exploding;

        Instantiate(m_explosionPrefab, transform.position, transform.rotation);
        gameObject.SetActive(false);
    }

    public bool IsExplosive()
    {
        return m_isExplosive;
    }

    private bool CanExplode()
    {
        return m_isExplosive && m_objectState != ObjectState.Exploding && m_objectState != ObjectState.Breaking;
    }

    public void Break()
    {
        Assert.IsTrue(m_isBreakable);

        m_objectState = ObjectState.Breaking;

        if (m_isExplosive)
        {
            Explode();
        }
        else
        {
            // Break routine???
        }
    }

    public bool IsBreakable()
    {
        return m_isBreakable;
    }

    private bool CanBreak()
    {
        return m_isBreakable && m_objectState != ObjectState.Exploding && m_objectState != ObjectState.Breaking;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Explosion")
        {
            if (CanExplode())
                Explode();
            else if (CanBreak())
                Break();
        }
    }

}
