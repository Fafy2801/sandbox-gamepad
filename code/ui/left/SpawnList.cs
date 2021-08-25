using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Tests;
using System;
using System.Collections.Generic;

[Library]
public partial class SpawnList : Panel
{
	public VirtualScrollPanel Canvas;

	public int Selected { get; set; } = 0;

	public List<Panel> Grid { get; set; }

	public SpawnList()
	{
		Grid = new List<Panel>();
		AddClass( "spawnpage" );
		AddChild( out Canvas, "canvas" );

		Canvas.Layout.AutoColumns = true;
		Canvas.Layout.ItemSize = new Vector2( 100, 100 );
		Canvas.OnCreateCell = ( cell, data ) =>
		{
			var file = (string)data;
			var panel = cell.Add.Panel( "icon" );
			panel.AddEventListener( "onclick", () => ConsoleSystem.Run( "spawn", "models/" + file ) );
			panel.Style.Background = new PanelBackground
			{
				Texture = Texture.Load( $"/models/{file}_c.png", false )
			};

			Grid.Add( panel );
		};

		foreach ( var file in FileSystem.Mounted.FindFile( "models", "*.vmdl_c.png", true ) )
		{
			if ( string.IsNullOrWhiteSpace( file ) ) continue;
			if ( file.Contains( "_lod0" ) ) continue;
			if ( file.Contains( "clothes" ) ) continue;

			Canvas.AddItem( file.Remove( file.Length - 6 ) );
		}

		if (Grid.Count > 0)
			Select( 0 );
	}

	public void Select( int select )
	{
		for (int i=0; i < Grid.Count; i++ )
		{
			Panel panel = Grid[i];

			panel.SetClass( "active", i == select );
			Selected = select;
		}
	}

	public void SwitchHorizontal( bool right )
	{
		int rows = MathX.FloorToInt( Canvas.Box.Rect.Size.x / (Grid[0].Box.Rect.Size.x + 5) );
		int select = Selected + (right ? 1 : -1);

		if ( select < 0 || select >= Grid.Count || (right && select % rows == 0) || (!right && select % rows == rows - 1) )
		{
			if ( right )
			{
				if ( select == Grid.Count )
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
		int rows = MathX.FloorToInt( Canvas.Box.Rect.Size.x / (Grid[0].Box.Rect.Size.x + 5) );
		int columns = MathX.FloorToInt( Canvas.Box.Rect.Size.y / (Grid[0].Box.Rect.Size.y + 5) );
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
