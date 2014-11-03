using System;
using UnityEngine;

namespace Assets.Scripts
{
	public static class PrefabsHelper
	{
        public static GameObject InstantiateCell(Transform parent)
        {
            return Instantiate("Cell", parent);
        }

        public static GameObject InstantiateCircle(Transform parent)
        {
            return Instantiate("Circle", parent);
        }

        public static GameObject InstantiateLine(Transform parent)
        {
            return Instantiate("Line", parent);
        }

        public static GameObject InstantiateCard(Transform parent)
        {
            return Instantiate("Card", parent);
        }

        public static GameObject InstantiateSlot(Transform parent)
        {
            return Instantiate("Slot", parent);
        }

        private static GameObject Instantiate(string name, Transform parent)
        {
            try
            {
                var instance = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("Prefabs/" + name, typeof(GameObject)));

                instance.name = name;
                instance.transform.parent = parent;
                instance.transform.localScale = Vector3.one;

                return instance;
            }
            catch
            {
                throw new Exception("Prefab not found: " + name);
            }
        }
	}
}