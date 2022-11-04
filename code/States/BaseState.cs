using Sandbox;

namespace Murder;

public abstract partial class BaseState : BaseNetworkable
{
	[Net]
	public TimeUntil TimeLeft { get; protected set; }

	public virtual int FreezeDuration => 0;
	public string TimeLeftFormatted => TimeLeft.Relative.TimerString();

	private TimeUntil _nextSecondTime = 0f;

	public void Start()
	{
		if ( Host.IsServer && FreezeDuration > 0 )
			TimeLeft = FreezeDuration;

		OnStart();
	}

	public void Finish()
	{
		if ( Host.IsServer )
			TimeLeft = 0f;

		OnFinish();
	}

	public virtual void OnPlayerSpawned( Player player )
	{
		Game.Current.MoveToSpawnpoint( player );
	}

	public virtual void OnPlayerKilled( Player player )
	{
		player.MakeSpectator( true );
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
		if ( Host.IsServer && TimeLeft )
			OnTimeUp();
	}

	protected virtual void OnStart() { }

	protected virtual void OnFinish() { }

	protected virtual void OnTimeUp() { }

	protected async void StartRespawnTimer( Player player )
	{
		await GameTask.DelaySeconds( 1 );

		if ( player.IsValid() && Game.Current.State == this )
			player.Respawn();
	}
}
