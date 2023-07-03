using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ClientSingleton : MonoBehaviour
{
    private ClientGameManager gameManager;

    private static ClientSingleton instance;

    public static ClientSingleton Instance 
    {
        get 
        { 
            if (instance != null) 
                return instance;  
                
            instance = FindObjectOfType<ClientSingleton>();
        
            if (instance == null) 
            {

                Debug.LogError("No client singleton in the scene.");

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

    public async Task CreateClient()
    {
        gameManager = new ClientGameManager();

        await gameManager.InitAsync();
    }
}
