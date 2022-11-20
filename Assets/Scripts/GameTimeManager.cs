using UnityEngine;
using Mirror;

public class GameTimeManager : NetworkBehaviour
{
    private float gameTime;
    
    private void Update()
    {
        gameTime += Time.deltaTime;
    }

    public float GetGameTime()
    {
        return gameTime;
    }
}