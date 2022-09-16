using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public partial class ChatBox : Panel
{
	// private static readonly Color _allChatColor = PlayerStatus.Alive.GetColor();
	// private static readonly Color _spectatorChatColor = PlayerStatus.Spectator.GetColor();

	public static ChatBox Instance { get; private set; }

	private Panel EntryCanvas { get; init; }
	private TabTextEntry Input { get; init; }

	public bool IsOpen
	{
		get => HasClass( "open" );
		set
		{
			SetClass( "open", value );
			if ( value )
			{
				Input.Focus();
				Input.Text = string.Empty;
				Input.Label.SetCaretPosition( 0 );
			}
		}
	}

	public ChatBox()
	{
		Instance = this;

		EntryCanvas.PreferScrollToBottom = true;
		EntryCanvas.TryScrollToBottom();

		Input.AddEventListener( "onsubmit", Submit );
		Input.AddEventListener( "onblur", () => IsOpen = false );
	}

	public override void Tick()
	{
		if ( Local.Pawn is not Player player )
			return;

		if ( Sandbox.Input.Pressed( InputButton.Chat ) )
			IsOpen = true;

		if ( !IsOpen )
			return;

		switch ( player.CurrentChannel )
		{
			case Channel.All:
				Input.Style.BorderColor = Color.Green;
				return;
			case Channel.Spectator:
				Input.Style.BorderColor = Color.Green;
				return;
			case Channel.Team:
				Input.Style.BorderColor = player.Role.Color;
				return;
		}

		Input.Placeholder = string.Empty;
	}

	public void AddEntry( string name, string message, string classes = "" )
	{
		var entry = new ChatEntry( name, message );
		if ( !classes.IsNullOrEmpty() )
			entry.AddClass( classes );
		EntryCanvas.AddChild( entry );
	}

	public void AddEntry( string name, string message, Color? color )
	{
		var entry = new ChatEntry( name, message, color );
		EntryCanvas.AddChild( entry );
	}

	private void Submit()
	{
		if ( Input.Text.IsNullOrEmpty() )
			return;

		if ( Input.Text.Contains( '\n' ) || Input.Text.Contains( '\r' ) )
			return;

		if ( Input.Text == Strings.RTVCommand )
		{
			if ( Local.Client.GetValue<bool>( Strings.HasRockedTheVote ) )
			{
				AddInfo( "You have already rocked the vote!" );
				return;
			}
		}

		SendChat( Input.Text );
	}

	[ConCmd.Server]
	public static void SendChat( string message )
	{
		if ( ConsoleSystem.Caller.Pawn is not Player player )
			return;

		if ( message == Strings.RTVCommand )
		{
			Game.RockTheVote();
			return;
		}

		if ( !player.IsAlive() )
		{
			var clients = Game.Current.State is InProgress ? Utils.GetDeadClients() : Client.All;
			AddChat( To.Multiple( clients ), player.Client.Name, message, Channel.Spectator );
			return;
		}

		if ( player.CurrentChannel == Channel.All )
			AddChat( To.Everyone, player.Client.Name, message, player.CurrentChannel, player.Role.Info.ResourceId );
	}

	[ConCmd.Client( "murder_chat_add", CanBeCalledFromServer = true )]
	public static void AddChat( string name, string message, Channel channel, int roleId = -1 )
	{
		switch ( channel )
		{
			case Channel.All:
				Instance?.AddEntry( name, message, ResourceLibrary.Get<RoleInfo>( roleId ).Color );
				return;
			case Channel.Team:
				Instance?.AddEntry( $"(TEAM) {name}", message, ResourceLibrary.Get<RoleInfo>( roleId ).Color );
				return;
			case Channel.Spectator:
				Instance?.AddEntry( name, message, Color.Yellow );
				return;
		}
	}

	[ConCmd.Client( "murder_chat_add_info", CanBeCalledFromServer = true )]
	public static void AddInfo( string message )
	{
		Instance?.AddEntry( message, "", "info" );
	}
}
