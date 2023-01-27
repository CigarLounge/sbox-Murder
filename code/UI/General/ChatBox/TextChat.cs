using Sandbox;
using Sandbox.UI;
using Sandbox.Util;

namespace Murder.UI;

[UseTemplate]
public partial class TextChat : Panel
{
	private static readonly Color _allChatColor = Role.Bystander.GetColor();
	private static readonly Color _spectatorChatColor = new Color32( 252, 219, 56 );

	public static TextChat Instance { get; private set; }

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

	public TextChat()
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
				Input.Style.BorderColor = _allChatColor;
				return;
			case Channel.Spectator:
				Input.Style.BorderColor = _spectatorChatColor;
				return;
		}

		Input.Placeholder = string.Empty;
	}

	public void AddEntry( string name, string message, string classes = "" )
	{
		var entry = new TextChatEntry( name, message );
		if ( !classes.IsNullOrEmpty() )
			entry.AddClass( classes );
		EntryCanvas.AddChild( entry );
	}

	public void AddEntry( string name, string message, Color? color )
	{
		var entry = new TextChatEntry( name, message, color );
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
			GameManager.RockTheVote();
			return;
		}

		if ( !player.IsAlive() )
		{
			var clients = GameManager.Instance.State is GameplayState ? Utils.GetDeadClients() : Client.All;
			AddChat( To.Multiple( clients ), player.Client.Name, message, Channel.Spectator );
			return;
		}

		if ( player.CurrentChannel == Channel.All )
			AddChat( To.Everyone, player.Client.Name, message, player.CurrentChannel );
	}

	[ConCmd.Client( "murder_chat_add", CanBeCalledFromServer = true )]
	public static void AddChat( string name, string message, Channel channel )
	{
		switch ( channel )
		{
			case Channel.All:
				Instance?.AddEntry( name, message, _allChatColor );
				return;
			case Channel.Spectator:
				Instance?.AddEntry( name, message, _spectatorChatColor );
				return;
		}
	}

	[ConCmd.Client( "murder_chat_add_info", CanBeCalledFromServer = true )]
	public static void AddInfo( string message )
	{
		Instance?.AddEntry( message, "", "info" );
	}
}
