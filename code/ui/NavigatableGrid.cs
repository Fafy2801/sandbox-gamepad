using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Sandbox.UI.Tests;

public partial class NavigatableGrid : Panel
{
	public VirtualScrollPanel Canvas;

	public int Selected { get; set; } = 0;

	public List<Panel> Grid { get; set; }
	public List<string> PanelData { get; set; }
	public SpawnMenu.MenuSection Section { get; set; }

	public NavigatableGrid()
	{
		Grid = new();
		PanelData = new();

		AddClass( "spawnpage" );
		AddChild( out Canvas, "canvas" );

		Canvas.Layout.AutoColumns = true;
		Canvas.Layout.ItemSize = new Vector2( 100, 100 );
	}

	public void SetSection( SpawnMenu.MenuSection section )
	{
		Canvas.OnCreateCell = ( cell, data ) =>
		{
			if ( section == SpawnMenu.MenuSection.Entities )
			{
				var entry = (LibraryAttribute)data;
				var btn = cell.Add.Button( entry.Title );
				btn.AddClass( "icon" );
				btn.AddEventListener( "onclick", () => ConsoleSystem.Run( "spawn_entity", entry.Name ) );
				btn.Style.Background = new PanelBackground
				{
					Texture = Texture.Load( $"/entity/{entry.Name}.png", false )
				};
				btn.AddEventListener( "onmouseover", () =>
				{
					for ( int i = 1; i < PanelData.Count; i++ )
						if ( PanelData[i] == entry.Name )
							Select( i );
				} );
				Grid.Add( btn );
				PanelData.Add( entry.Name );
			}
			else if ( section == SpawnMenu.MenuSection.Props )
			{
				var file = (string)data;
				var panel = cell.Add.Panel( "icon" );
				panel.AddEventListener( "onclick", () => ConsoleSystem.Run( "spawn", "models/" + file ) );
				panel.Style.Background = new PanelBackground
				{
					Texture = Texture.Load( $"/models/{file}_c.png", false )
				};
				panel.AddEventListener( "onmouseover", () =>
				{
					for ( int i = 1; i < PanelData.Count; i++ )
						if ( PanelData[i] == file )
							Select( i );
				} );
				Grid.Add( panel );
				PanelData.Add( file );
			}
		};
	}

	public void Select( int select )
	{
		for ( int i = 0; i < Grid.Count; i++ )
		{
			Panel panel = Grid[i];

			panel.SetClass( "active", i == select );
			Selected = select;
		}
	}

	public void SwitchHorizontal( bool right )
	{
		if ( Grid.Count <= 0 )
			return;

		int rows = MathX.FloorToInt( Canvas.Box.Rect.Size.x / (Grid[0].Box.Rect.Size.x + 10) );
		int select = Selected + (right ? 1 : -1);

		if ( select < 0 || select >= Grid.Count || (right && select % rows == 0) || (!right && select % rows == rows - 1) )
		{
			if ( right )
			{
				if ( select >= Grid.Count )
					if ( Grid.Count <= rows )
						select = 0;
					else
						select = Math.Max( Selected - rows + 1 + (rows - select % rows), 0 );
				else
					select = Math.Max( Selected - rows + 1, 0 );
			}
			else if ( Selected % rows == 0 )
				select = Math.Min( select + rows, Grid.Count - 1 );
		}

		Select( select );
	}

	public void SwitchVertical( bool down )
	{
		if ( Grid.Count <= 0 )
			return;

		int rows = MathX.FloorToInt( Canvas.Box.Rect.Size.x / (Grid[0].Box.Rect.Size.x + 10) );
		int columns = MathX.FloorToInt( Canvas.Box.Rect.Size.y / (Grid[0].Box.Rect.Size.y + 10) );
		int select = Selected + rows * (down ? 1 : -1);

		if ( select >= Grid.Count || select < 0 )
			if ( down )
				select = Selected % rows;
			else
			{
				select = rows * columns - (rows - Selected);

				while ( select >= Grid.Count && Grid.Count > 0 )
					select = Math.Max( select - rows, 0 );
			}

		Select( select );
	}
}
