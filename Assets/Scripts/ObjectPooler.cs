using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public interface IPoolable
    {
        void OnPooled();
        void OnUnpooled();
    }

    // This is just used in the inspector for preloading pooled items
    [System.Serializable]
    public class PoolItem
    {
        public string NameForPooling;
        public GameObject Prefab;
        public int NumberToPool;
        public bool shouldExpand;
    }

    [SerializeField]
    List<PoolItem> itemsToPool;

    // A dictionary of stacks. This is efficient because we can separate types of things by name. The dictionary lookup is 
    // O(1) and a Stack (I think) is O(1) for push and pop, not accounting for resizing when needed.
    Dictionary<string, Stack<GameObject>> objectMap = new Dictionary<string, Stack<GameObject>>();

    public static ObjectPooler Instance;

    private void Start()
    {
        Instance = this;
        PopulatePool();
    }

    void PopulatePool()
    {
        foreach (PoolItem item in itemsToPool)
        {
            for (int i = 0; i < item.NumberToPool; i++)
            {
                GameObject obj = Instantiate(item.Prefab);
                PoolObject(item.NameForPooling, obj);
            }
        }
    }

    /// <summary>
    /// Gets an object from the pool.
    /// </summary>
    /// <param name="name">The name/type of the object</param>
    /// <returns>A pooled GameObject if one exists, otherwise null</returns>
    public GameObject GetPooledObject(string name)
    {
        // Try to get a stack from the dictionary by name
        objectMap.TryGetValue(name, out var stack);

        // If we get one
        if (stack != null)
        {
            // And it's not empty
            if (stack.Count > 0)
            {
                // Pop from the stack
                var obj = stack.Pop();
                obj.SetActive(true); // Set it active

                // Call the interface (if applicable)
                if (obj.GetComponent<IPoolable>() is IPoolable poolable)
                    poolable.OnUnpooled();

                // Return it
                return obj;
            }
            else

                // Return null because we didn't have anything in the stack
                return null;
        }

        // Return null because there was no stack containing the name/type of object
        return null;
    }

    /// <summary>
    /// Puts an object into the pool, which handles deactivating the object
    /// </summary>
    /// <param name="name">The name/type of the object to store.</param>
    /// <param name="objectToPool">The actual GameObject to pool</param>
    public void PoolObject(string name, GameObject objectToPool)
    {
        // Try to get a stack from our dictionary
        objectMap.TryGetValue(name, out var stack);

        // If no stack was found, make a new one and add it to the map
        if (stack == null)
        {
            stack = new Stack<GameObject>(); // new stack
            objectMap.Add(name, stack); // add here
        }

        stack.Push(objectToPool); // push the GameObject to the stack

        // Call the interface (if applicable)
        if (objectToPool.GetComponent<IPoolable>() is IPoolable poolable)
            poolable.OnPooled();

        // Deactivate the object
        objectToPool.SetActive(false);
    }
}