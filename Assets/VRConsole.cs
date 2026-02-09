using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 
using UnityEngine.UI;

 
using TMPro;

public class VRConsole : MonoBehaviour
{
    public TextMeshProUGUI consoleText;
    private string log = "";

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        log += logString + "\n";
        consoleText.text = log;
    }
	
	public void ClearConsole()
    {
        log = "";
        consoleText.text = log;
    }
	
}
