using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PBJ
{
	public class AnimationEventHelper : MonoBehaviour
	{
        [SerializeField]
        private UnityEvent m_event;

        public void TriggerEvent()
        {
            m_event.Invoke();
        } 
	}
}