using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PBJ
{
    public class PipBar : MonoBehaviour
    {
        [SerializeField]
        private Pip[] _pips;

        public void AdjustCount(int count)
        {
            for (int i = 0; i < _pips.Length; i++)
            {
                _pips[i].AdjustVisiblity(i + 1 <= count);
            }
        }
        [System.Serializable]
        public struct Pip
        {
            public Image[] Images;

            public void AdjustVisiblity(bool active)
            {
                foreach (Image i in Images)
                {
                    Color c = i.color;
                    c.a = active ? 1 : 0;
                    i.color = c;
                }

            }

        }
    }
}