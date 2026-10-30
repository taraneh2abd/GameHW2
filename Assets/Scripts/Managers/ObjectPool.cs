using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> : MonoBehaviour where T : Component
{
    [SerializeField] private T prefab;
    [SerializeField] private int initialSize = 8;

    private readonly Queue<T> available = new Queue<T>();
    private readonly List<T> all = new List<T>();

    public T Prefab => prefab;
    public int TotalCount => all.Count;
    public int AvailableCount => available.Count;

    public void Initialize(T prefabToUse, int size)
    {
        prefab = prefabToUse;
        initialSize = size;
        WarmUp(initialSize);
    }

    private void Awake()
    {
        if (prefab != null && all.Count == 0)
        {
            WarmUp(initialSize);
        }
    }

    private void WarmUp(int count)
    {
        for (int i = 0; i < count; i++)
        {
            CreateNewInstance();
        }
    }

    private T CreateNewInstance()
    {
        T instance = Instantiate(prefab, transform);
        instance.gameObject.SetActive(false);
        all.Add(instance);
        available.Enqueue(instance);
        return instance;
    }

    public T Get()
    {
        T instance = available.Count > 0 ? available.Dequeue() : CreateNewInstance();
        instance.gameObject.SetActive(true);
        return instance;
    }

    public void Release(T instance)
    {
        if (instance == null)
        {
            return;
        }

        instance.gameObject.SetActive(false);
        instance.transform.SetParent(transform);
        available.Enqueue(instance);
    }
}
