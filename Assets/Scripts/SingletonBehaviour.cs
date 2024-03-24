using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class SingletonBehavior<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance != null) return instance;

            instance = FindObjectOfType(typeof(T)) as T;
            if (instance != null) return instance;

            var temp = new GameObject(typeof(T).Name);
            instance = temp.AddComponent<T>();
            return instance;
        }
    }

    protected virtual void Awake()
    {
        instance = this as T;
        OnCreated();
    }

    protected virtual void OnCreated()
    {
    }
}