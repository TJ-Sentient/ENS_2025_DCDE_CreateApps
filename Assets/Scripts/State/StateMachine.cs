using System;
using System.Collections;
using System.Collections.Generic;
using TJ.Utils;
using UnityEngine;
using VInspector;

public abstract class StateMachine<T> : Singleton<T> where T : Component
{
    public State TransitionState { get; protected set; }
    
    protected State                        currentState;
    protected Dictionary<AppStates, State> states;
    protected bool                         isTransitioning;

    public override void Awake()
    {
        base.Awake();
        states = new Dictionary<AppStates, State>();
        
        State[] stateComponents = GetComponentsInChildren<State>();
        foreach (State state in stateComponents)
        {
            states[state.state] = state;
        }
    }

    [Button]
    public bool ChangeState(State newState)
    {
        if (newState == null || newState == currentState || isTransitioning)
            return false;
        
        StopAllCoroutines();
        TransitionState = newState;
        StartCoroutine(ChangeStateRoutine(newState));
        return true;
    }

    [Button]
    public bool ChangeState(AppStates newState)
    {
        if (states.TryGetValue(newState, out State state))
        {
            return ChangeState(state);
        }

        return false;
    }

    private IEnumerator ChangeStateRoutine(State newState)
    {
        isTransitioning = true;

        if (currentState != null)
        {
            float exitDuration = currentState.Exit();
            if (exitDuration > 0)
                yield return new WaitForSeconds(exitDuration);
        }

        currentState = newState;
        currentState.Enter();
        
        isTransitioning = false;
        TransitionState = null;
    }

    public State GetCurrentState()
    {
        return currentState;
    }
    
    public TState GetState<TState>() where TState : State
    {
        foreach (var state in states.Values)
        {
            if (state is TState typedState)
                return typedState;
        }
        return null;
    }
}