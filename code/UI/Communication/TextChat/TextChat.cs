using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;

namespace Murder.UI;

public partial class TextChat : Panel
{
	private static TextChat Instance { get; set; }

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

	private const int MaxItems = 100;
	private const float MessageLifetime = 10f;

	private Panel Canvas { get; set; }
	private TextEntry Input { get; set; }

	private readonly Queue<TextChatEntry> _entries = new();

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		Canvas.PreferScrollToBottom = true;
		Input.AcceptsFocus = true;
		Input.AllowEmojiReplace = true;

		Instance = this;
	}

	public override void Tick()
	{
		if ( Sandbox.Input.Pressed( InputButton.Chat ) )
			Open();
	}

	private void AddEntry( TextChatEntry entry )
	{
		Canvas.AddChild( entry );
		Canvas.TryScrollToBottom();

		entry.BindClass( "stale", () => entry.Lifetime > MessageLifetime );

		_entries.Enqueue( entry );
		if ( _entries.Count > MaxItems )
			_entries.Dequeue().Delete();
	}

	private void Open()
	{
		AddClass( "open" );
		Input.Focus();
		Canvas.TryScrollToBottom();
	}

	private void Close()
	{
		RemoveClass( "open" );
		Input.Blur();
		Input.Text = string.Empty;
		Input.Label.SetCaretPosition( 0 );
	}

	private void Submit()
	{
		var message = Input.Text.Trim();
		Input.Text = "";

		Close();

		if ( string.IsNullOrWhiteSpace( message ) )
			return;

		if ( message == "!rtv" && Game.LocalClient.HasRockedTheVote() )
		{
			AddInfo( "You have already rocked the vote!" );
			return;
		}

		SendChat( message );
	}

	[ConCmd.Server]
	public static void SendChat( string message )
	{
		if ( ConsoleSystem.Caller.Pawn is not Player player )
			return;

		if ( message == "!rtv" )
		{
			GameManager.RockTheVote();
			return;
		}

		if ( player.IsAlive() || GameState.Current is not GameplayState )
		{
			AddEntry( To.Everyone, player, message );

		}
		else if ( !player.IsAlive() )
		{
			var spectators = Utils.GetClientsWhere( p => !p.IsAlive() );

			AddEntry( To.Multiple( spectators ), player, message );
		}
	}

	[ClientRpc]
	public static void AddEntry( Player player, string message )
	{
		Instance.AddEntry( new TextChatEntry( player.Client, message ) );
	}

	[ClientRpc]
	public static void AddInfo( string message )
	{
		Instance?.AddEntry( new TextChatEntry( message, Color.FromBytes( 253, 196, 24 ) ) );
	}
}

public partial class TextEntry : Sandbox.UI.TextEntry
{
	public event Action OnTabPressed;

	public override void OnButtonTyped( string button, KeyModifiers km )
	{
		if ( button == "tab" )
		{
			OnTabPressed?.Invoke();
			return;
		}

		base.OnButtonTyped( button, km );
	}
}
