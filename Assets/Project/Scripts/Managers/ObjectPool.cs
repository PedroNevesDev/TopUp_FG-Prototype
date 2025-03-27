using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectPool : Singleton<ObjectPool>
{
    // Dictionary to hold pools of different GameObject types (or prefabs)
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();

    // Dictionary to store pooled objects with their original prefab
    private Dictionary<GameObject, GameObject> objectToPrefabMap = new Dictionary<GameObject, GameObject>();

    // Method to get an object from the pool
    public GameObject GetObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject objectToSpawn;

        if (poolDictionary.ContainsKey(prefab) && poolDictionary[prefab].Count > 0)
        {
            // Get an object from the pool
            objectToSpawn = poolDictionary[prefab].Dequeue();
            
            // Ensure the object is fully reset before use
            PrepareObjectForSpawn(objectToSpawn, position, rotation);
        }
        else
        {
            // If the pool is empty, instantiate a new object
            Debug.Log("Pool is empty, spawning new object.");
            objectToSpawn = Instantiate(prefab, position, rotation);
        }

        // Track the prefab for this object
        objectToPrefabMap[objectToSpawn] = prefab;

        return objectToSpawn;
    }

    // Method to properly prepare an object for spawning
    private void PrepareObjectForSpawn(GameObject obj, Vector3 position, Quaternion rotation)
    {


        // Reset local position and rotation in case of previous pooled state
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        // Set final position and rotation
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        // Optional: Reset any components that might need resetting
        ResetObjectComponents(obj);
    }

    // Method to reset components that might need a reset when pooling
    private void ResetObjectComponents(GameObject obj)
    {
        // Example: Reset Rigidbody
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Example: Reset Particle System
        ParticleSystem ps = obj.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Stop();
            ps.Clear();
        }

        // Add more component resets as needed for your specific use cases
    }

    // Method to return an object to the pool with enhanced safety
    public void ReturnObject(GameObject objectToReturn)
    {
        // Null check
        if (objectToReturn == null)
        {
            Debug.LogWarning("Attempting to return null object to pool");
            return;
        }

        // Try to get the prefab from our mapping
        if (!objectToPrefabMap.TryGetValue(objectToReturn, out GameObject prefab))
        {
            Debug.LogError("Could not determine original prefab for object: " + objectToReturn.name);
            return;
        }

        // Use a coroutine-like approach to ensure deactivation
        // This helps prevent potential frame-timing issues
        objectToReturn.SetActive(false);

        // Ensure the transform is reset and parented to the pool
        objectToReturn.transform.SetParent(transform);
        objectToReturn.transform.localPosition = Vector3.zero;
        objectToReturn.transform.localRotation = Quaternion.identity;

        // Ensure the pool for this prefab exists
        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab] = new Queue<GameObject>();
        }

        // Enqueue the object
        poolDictionary[prefab].Enqueue(objectToReturn);
    }

    // Prewarm method remains the same
    public void PrewarmPool(GameObject prefab, int count)
    {
        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab] = new Queue<GameObject>();
        }

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);

            // Track the prefab
            objectToPrefabMap[obj] = prefab;

            // Add to pool
            poolDictionary[prefab].Enqueue(obj);
        }
    }

    // Convenience method for self-return
    public void ReturnSelf(GameObject objectToReturn)
    {
        ReturnObject(objectToReturn);
    }
}