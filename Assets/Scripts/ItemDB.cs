using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace PBJ
{
    [CreateAssetMenu(menuName = "PBJ/Create ItemDB", fileName = "NewItemDB")]
	public class ItemDB : ScriptableObject
	{
        public Item[] Items;

        [System.Serializable]
        public class Item
        {
            public GameObject Prefab;
            public Sprite Sprite;
        }
	}
}