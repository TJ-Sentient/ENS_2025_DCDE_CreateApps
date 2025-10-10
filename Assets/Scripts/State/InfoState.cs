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
    
    public SelectableBtn        categoryBtn;
    public UISequenceController categorySeq;
    public SelectableBtn        awardsBtn;
    public UISequenceController awardsSeq;
    
    private SelectableBtn _currentBtn;

    private void Awake()
    {
        canvas.enabled = false;
        backBtn.onClick.AddListener(OnBackBtnClicked);
        homeBtn.onClick.AddListener(OnHomeBtnClicked);
        categoryBtn.onSelect.AddListener(OnCategoryBtnClicked);
        awardsBtn.onSelect.AddListener(OnAwardsBtnClicked);
    }

    private void OnAwardsBtnClicked(SelectableBtn selectableBtn)
    {
        if (_currentBtn == selectableBtn) return;
        categoryBtn.UnSelect();
        awardsBtn.Select();
        var seq= categorySeq.PlaySequence(true);
        awardsSeq.SetToStart();
        seq.OnComplete(() =>
        {
            categorySeq.gameObject.SetActive(false);
            awardsSeq.gameObject.SetActive(true);
            awardsSeq.PlaySequence();
        });
        _currentBtn = selectableBtn;
    }

    private void OnCategoryBtnClicked(SelectableBtn selectableBtn)
    {
        if (_currentBtn == selectableBtn) return;
        awardsBtn.UnSelect();
        categoryBtn.Select();
        var seq= awardsSeq.PlaySequence(true);
        categorySeq.SetToStart();
        seq.OnComplete(() =>
        {
            awardsSeq.gameObject.SetActive(false);
            categorySeq.gameObject.SetActive(true);
            categorySeq.PlaySequence();
        });
        _currentBtn = selectableBtn;
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
        InitializeIcons();
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

    private void InitializeIcons()
    {
        categorySeq.gameObject.SetActive(true);
        awardsSeq.gameObject.SetActive(false);
        categoryBtn.Select();
        awardsBtn.UnSelect();
        _currentBtn = categoryBtn;
        categorySeq.SetToStart();
        awardsSeq.SetToStart();
    }
}