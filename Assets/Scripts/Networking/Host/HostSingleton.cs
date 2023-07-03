using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostSingleton : MonoBehaviour
{
    private HostGameManager gameManager;

    private static HostSingleton instance;

    public static HostSingleton Instance 
    {
        get 
        { 
            if (instance != null) 
                return instance;  
                
            instance = FindObjectOfType<HostSingleton>();
        
            if (instance == null) 
            {

                Debug.LogError("No host singleton in the scene.");

                return null;
            }

            return instance;
        }

        private set {}
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void CreateHost()
    {
        gameManager = new HostGameManager();
    }
}
