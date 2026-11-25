using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InGameTest : MonoBehaviour
{
    void Awake()
    {
        StructSizeTest.RunAllTests();
    }

    void Update()
    {
        if(Keyboard.current[Key.Escape].isPressed) Application.Quit();
    }
}