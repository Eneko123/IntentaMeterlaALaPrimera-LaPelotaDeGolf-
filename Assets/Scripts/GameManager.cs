using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int attempts = 0;
    private int maxAttempts = 10;

    void Awake()
    {
        Instance = this;
    }

    public void OnBallHit()
    {
        attempts++;
        if (attempts >= maxAttempts)
        {
            // GameOver();
        }
    }

}
