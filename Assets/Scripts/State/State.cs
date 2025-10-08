using System;
using UnityEngine;

public abstract class State : MonoBehaviour
{
    public AppStates state;
    
    public abstract void Enter();

    public abstract float Exit();
    
}