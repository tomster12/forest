using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) throw new System.Exception("GameManager already exists in the scene!");
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
