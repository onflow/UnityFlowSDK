using System;
using UnityEngine;
using System.Collections.Generic;


internal class UnityThreadExecutor : MonoBehaviour
{
    //our (singleton) instance
    private static UnityThreadExecutor instance = null;

    //Holds actions received from another Thread. Will be coped to actionCopiedQueueUpdateFunc then executed from there
    private static List<Action> actionQueue = new List<Action>();

    //holds Actions copied from actionQueuesUpdateFunc to be executed
    List<Action> actionCopiedQueue = new List<Action>();

    // Used to know if whe have new Action function to execute. This prevents the use of the lock keyword every frame
    private volatile static bool noActionQueueToExecute = true;

    //Used to initialize UnityThreadExecutor. Call once before any function here
    internal static void Init(bool visible = false)
    {
        if (instance != null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            // add an invisible game object to the scene
            GameObject obj = new GameObject("MainThreadExecuter");
            if (!visible)
            {
                obj.hideFlags = HideFlags.HideAndDontSave;
            }

            DontDestroyOnLoad(obj);
            instance = obj.AddComponent<UnityThreadExecutor>();
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    internal static void ExecuteInUpdate(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException("action");
        }

        lock (actionQueue)
        {
            actionQueue.Add(action);
            noActionQueueToExecute = false;
        }
    }

    void Update()
    {
        if (noActionQueueToExecute)
        {
            return;
        }

        //Clear the old actions from the actionCopiedQueueUpdateFunc queue
        actionCopiedQueue.Clear();
        lock (actionQueue)
        {
            //Copy actionQueuesUpdateFunc to the actionCopiedQueueUpdateFunc variable
            actionCopiedQueue.AddRange(actionQueue);
            //Now clear the actionQueuesUpdateFunc since we've done copying it
            actionQueue.Clear();
            noActionQueueToExecute = true;
        }

        // Loop and execute the functions from the actionCopiedQueueUpdateFunc
        for (int i = 0; i < actionCopiedQueue.Count; i++)
        {
            actionCopiedQueue[i].Invoke();
        }
    }

    void OnDisable()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}