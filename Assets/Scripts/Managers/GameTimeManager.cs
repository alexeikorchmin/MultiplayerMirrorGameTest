using UnityEngine;
using Mirror;

public class GameTimeManager : NetworkBehaviour
{
    private static float gameTime;

    public static float GetGameTime()
    {
        return gameTime;
    }

    [Server]
    private void FixedUpdate()
    {
        gameTime += Time.deltaTime;
    }
}