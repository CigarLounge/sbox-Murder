using Sandbox;

namespace Murder;

public abstract partial class GameState : BaseNetworkable
{
	public static GameState Current => GameManager.Instance.State;

	[Net] public TimeUntil TimeLeft { get; protected set; }
	public virtual int Duration => 0;
	public string TimeLeftFormatted => TimeLeft.Relative.TimerString();
	private TimeUntil _nextSecondTime = 0f;

	public void Start()
	{
		if ( Game.IsServer && Duration > 0 )
			TimeLeft = Duration;

		OnStart();
	}

	public void Finish()
	{
		if ( Game.IsServer )
			TimeLeft = 0f;

		OnFinish();
	}

	public virtual void OnPlayerSpawned( Player player )
	{
		GameManager.Instance.MoveToSpawnpoint( player );
	}

	public virtual void OnPlayerKilled( Player player )
	{
		player.MakeSpectator();
	}

	public virtual void OnPlayerJoin( Player player ) { }

	public virtual void OnPlayerLeave( Player player ) { }

	public virtual void OnTick()
	{
		if ( _nextSecondTime )
		{
			OnSecond();
			_nextSecondTime = 1f;
		}
	}

	public virtual void OnSecond()
	{
		if ( Game.IsServer && TimeLeft )
			OnTimeUp();
	}

	protected virtual void OnStart() { }

	protected virtual void OnFinish() { }

	protected virtual void OnTimeUp() { }
}
