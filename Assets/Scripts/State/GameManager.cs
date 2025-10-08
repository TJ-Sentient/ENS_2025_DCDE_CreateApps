public class GameManager : StateMachine<GameManager>
{
    public AppStates     startingState;

    private void Start()
    {
        ChangeState(startingState);
    }
}