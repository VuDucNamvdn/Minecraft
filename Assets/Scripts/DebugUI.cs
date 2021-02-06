using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour
{
    [SerializeField] Text text;

    // Update is called once per frame
    void Update()
    {
        if (text != null)
            text.text = "FPS: "+ (1 / Time.deltaTime).ToString();
    }
}
