using DG.Tweening;
using TJ.Animation;
using UnityEngine;
using UnityEngine.UI;

public class InfoState : State
{
    public Canvas               canvas;
    public UISequenceController uiSeq;
    public Button               backBtn;
    public Button               homeBtn;

    private void Awake()
    {
        canvas.enabled = false;
        backBtn.onClick.AddListener(OnBackBtnClicked);
        homeBtn.onClick.AddListener(OnHomeBtnClicked);
    }

    private void OnHomeBtnClicked()
    {
        GameManager.Instance.ChangeState(AppStates.Entry);
    }

    private void OnBackBtnClicked()
    {
        GameManager.Instance.ChangeState(AppStates.Entry);
    }

    public override void Enter()
    {
        Debug.Log("Entering Entry State");
        canvas.enabled = true;
        uiSeq.PlaySequence();
    }

    public override float Exit()
    {
        Debug.Log("Exiting Entry State");
        var seq = uiSeq.PlaySequence(true);
        seq.OnComplete(() => canvas.enabled = false);
        return seq.Duration();
    }
}