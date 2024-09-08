using UnityEngine;
using System.Collections;

public class FPS : MonoBehaviour {

    private int FrameCount;
    private float TimeDelta;
    public float FramesPerSecond { get; private set; }
    public float UpdateRate = 5;

    void Update()
    {
        FrameCount++;
        TimeDelta += Time.deltaTime;
        if (TimeDelta >= 1.0f / UpdateRate) {
            FramesPerSecond = FrameCount / TimeDelta;
            FrameCount = 0;
            TimeDelta -= 1.0f / UpdateRate;
        }
    }


    void OnGUI() {
        GUI.Label(new Rect(Screen.width-100, Screen.height-20, 100, 20), "FPS " + FramesPerSecond.ToString("0")); 
    }
}
