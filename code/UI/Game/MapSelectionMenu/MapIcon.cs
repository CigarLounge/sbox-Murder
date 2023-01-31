using Sandbox;
using Sandbox.UI;
using System;
using System.Threading.Tasks;

namespace Murder.UI;

public partial class MapIcon : Panel
{
	public string Ident { get; set; }
	public int Votes { get; set; }

	private Package _data;

	protected void VoteMap() => MapSelectionState.SetVote( Ident );

	protected override async Task OnParametersSetAsync()
	{
		_data = await Package.Fetch( Ident, true );

		if ( _data?.PackageType != Package.Type.Map )
		{
			Delete();
			return;
		}

		StateHasChanged();
	}

	protected override int BuildHash() => HashCode.Combine( Votes );
}
