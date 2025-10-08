using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using TJ.Animation;
using UnityEngine;
using UnityEngine.UI;
using UTool.TabSystem;

[HasTabField]
public class EntryState : State
{
    public                              Canvas               canvas;
    public                              UISequenceController uiSeq;
    public                              Button               nextBtn;
    public UserTracker userTracker;
    [TabField][SerializeField] private float                idleTime = 60f;

    [SerializeField] [ReadOnly] private float idleTimer;
    private                             bool  isActive;


    private void Awake()
    {
        canvas.enabled = false;
        nextBtn.onClick.AddListener(OnNextBtnClicked);
    }

    private void Start()
    {
        userTracker.CreateUserSession();
    }

    private void Update()
    {
        if (!isActive) return;

        if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            ResetIdleTimer();
        }

        idleTimer += Time.deltaTime;
        if (idleTimer >= idleTime)
        {
            ResetIdleTimer();
            GameManager.Instance.ChangeState(AppStates.Entry);
            userTracker.CreateUserSession();
        }
    }

    private void OnNextBtnClicked()
    {
        GameManager.Instance.ChangeState(AppStates.Info);
    }

    public override void Enter()
    {
        isActive = false;
        ResetIdleTimer();
        Debug.Log("Entering Entry State");
        canvas.enabled = true;
        uiSeq.PlaySequence();
    }

    public override float Exit()
    {
        isActive = true;
        ResetIdleTimer();
        Debug.Log("Exiting Entry State");
        var seq = uiSeq.PlaySequence(true);
        seq.OnComplete(() => canvas.enabled = false);
        return seq.Duration();
    }
    
    private void ResetIdleTimer()
    {
        idleTimer = 0f;
    }
}