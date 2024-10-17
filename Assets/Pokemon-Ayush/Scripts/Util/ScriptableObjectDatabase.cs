using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableObjectDatabase<T> : MonoBehaviour where T : ScriptableObject
{
    static Dictionary<string, T> objects;

    public static void Init()
    {
        objects = new Dictionary<string, T>();
        var objectArray = Resources.LoadAll<T>("");
        foreach (var obj in objectArray)
        {
            if (objects.ContainsKey(obj.name))
            {
                Debug.LogError($"There are 2 pokemons with the name {obj.name}");
                continue;
            }
            else
            {
                objects[obj.name] = obj;
            }
        }
    }

        public static T GetObjectByName(string name)
        {
            if (!objects.ContainsKey(name))
            {
                Debug.LogError($"Pokemon with name {name} not found wihtin database");
                return null;
            }

            return objects[name];
        }
    
}
