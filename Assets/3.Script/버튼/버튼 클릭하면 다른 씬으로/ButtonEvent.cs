using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonEvent : MonoBehaviour
{
    public void SceneLoader(string scenename)
    {
        PlayerPrefs.SetInt("Score", 0);
        SceneManager.LoadScene(scenename);
    }
}
