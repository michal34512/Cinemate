using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionDot : MonoBehaviour
{
    public Color[] states = new Color[2];
    float TheTimer = 0;
    private void FixedUpdate()
    {
        TheTimer += Time.fixedDeltaTime;
        if(TheTimer>1)
        {
            TheTimer = 0;
            if (Connection.isConnected)
            {
                //Connected
                GetComponent<Image>().color = states[1];
            }
            else
            {
                //Disconnected
                GetComponent<Image>().color = states[0];
            }
        }
    }
}
