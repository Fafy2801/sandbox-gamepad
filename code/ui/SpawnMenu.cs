using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Sandbox.UI.Tests;
using System.Collections.Generic;
using System.Linq;

[Library]
public partial class SpawnMenu : Panel
{
	public static SpawnMenu Instance;
	readonly Panel toollist;

	public NavigatableGrid Spawns { get; private set; }
	public NavigatableGrid Entities { get; private set; }
	public List<Button> Tabs { get; private set; }

	public List<string> Tools { get; private set; }
	public List<Button> ToolButtons { get; private set; }
	public int ToolSelected { get; set; }

	public enum MenuSection  { Props, Entities, Tools };
	public static MenuSection ActiveSection { get; set; } = MenuSection.Props;

	public SpawnMenu()
	{
		Instance = this;
		Tabs = new();

		StyleSheet.Load( "/ui/SpawnMenu.scss" );

		var left = Add.Panel( "left" );
		{
			var tabs = left.AddChild<ButtonGroup>();
			tabs.AddClass( "tabs" );

			var body = left.Add.Panel( "body" );

			{
				Spawns = body.AddChild<NavigatableGrid>();
				Spawns.SetSection( MenuSection.Props );
				Tabs.Add(tabs.AddButtonActive( "Props", ( b ) => {
					Spawns.SetClass( "active", b );
					Entities.SetClass( "active", !b );
					ActiveSection = MenuSection.Props;
				} ));

				foreach ( var file in FileSystem.Mounted.FindFile( "models", "*.vmdl_c.png", true ) )
				{
					if ( string.IsNullOrWhiteSpace( file ) ) continue;
					if ( file.Contains( "_lod0" ) ) continue;
					if ( file.Contains( "clothes" ) ) continue;

					var item = file.Remove( file.Length - 6 );
					if ( Spawns.PanelData.Contains( item ) )
						continue;

					Spawns.Canvas.AddItem( item );
				}

				if ( Spawns.Grid.Count > 0 )
					Spawns.Select( 0 );

				Spawns.SetClass( "active", true );

				Entities = body.AddChild<NavigatableGrid>();
				Entities.SetSection( MenuSection.Entities );
				Tabs.Add(tabs.AddButtonActive( "Entities", ( b ) => {
					Spawns.SetClass( "active", !b );
					Entities.SetClass( "active", b );
					ActiveSection = MenuSection.Entities;
				} ));

				var ents = Library.GetAllAttributes<Entity>().Where( x => x.Spawnable ).OrderBy( x => x.Title ).ToArray();

				foreach ( var entry in ents )
				{
					Entities.Canvas.AddItem( entry );
				}
			}
		}

		var right = Add.Panel( "right" );
		{
			var tabs = right.Add.Panel( "tabs" );
			{
			Tabs.Add( tabs.Add.Button( "Tools" ));
				
				//tabs.Add.Button( "Tools" ).AddClass( "active" );
				//tabs.Add.Button( "Utility" );
			}

			var body = right.Add.Panel( "body" );
			{
				toollist = body.Add.Panel( "toollist" );
				{
					RebuildToolList();
				}
				body.Add.Panel( "inspector" );
			}
		}

	}

	void RebuildToolList()
	{
		toollist.DeleteChildren( true );
		ToolButtons = new();
		Tools = new();
		ToolSelected = 0;

		int i = 0;
		foreach ( var entry in Library.GetAllAttributes<Sandbox.Tools.BaseTool>() )
		{
			if ( entry.Title == "BaseTool" )
				continue;

			bool active = entry.Name == ConsoleSystem.GetValue( "tool_current" );

			var button = toollist.Add.Button( entry.Title );
			button.SetClass( "active", active );

			button.AddEventListener( "onclick", () =>
			{
				ConsoleSystem.Run( "tool_current", entry.Name );
				ConsoleSystem.Run( "inventory_current", "weapon_tool" );

				foreach ( var child in toollist.Children )
					child.SetClass( "active", child == button );
			} );

			if ( active )
				ToolSelected = i;

			Tools.Add( entry.Name );
			ToolButtons.Add( button );

			i++;
		}
	}

	public void ToolSelect(int select )
	{
		for ( int i = 0; i < ToolButtons.Count; i++ )
		{
			ToolButtons[i].SetClass( "active", i == select );
		}

		ToolSelected = select;
		ConsoleSystem.Run( "tool_current", Tools[select] );
	}

	public override void Tick()
	{
		base.Tick();

		//Parent.SetClass( "spawnmenuopen", Input.Down( InputButton.Menu ) );
	}

	public override void OnHotloaded()
	{
		base.OnHotloaded();

		RebuildToolList();
	}
}
