﻿using Sandbox;
using Sandbox.Tools;

[Library( "weapon_tool", Title = "Toolgun" )]
partial class Tool : Carriable
{
	[ConVar.ClientData( "tool_current" )]
	public static string UserToolCurrent { get; set; } = "tool_boxgun";

	public override string ViewModelPath => "models/weapons/v_toolgun.vmdl";

	[Net, Predicted]
	public BaseTool CurrentTool { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/weapons/toolgun.vmdl" );
	}

	public override void Simulate( Client owner )
	{
		UpdateCurrentTool( owner );

		CurrentTool?.Simulate();
	}

	private void UpdateCurrentTool( Client owner )
	{
		var toolName = owner.GetClientData<string>( "tool_current", "tool_boxgun" );
		if ( toolName == null )
			return;

		// Already the right tool
		if ( CurrentTool != null && CurrentTool.Parent == this && CurrentTool.Owner == owner.Pawn && CurrentTool.ClassInfo.IsNamed( toolName ) )
			return;

		if ( CurrentTool != null )
		{
			CurrentTool?.Deactivate();
			CurrentTool = null;
		}

		CurrentTool = Library.Create<BaseTool>( toolName, false );

		if ( CurrentTool != null )
		{
			CurrentTool.Parent = this;
			CurrentTool.Owner = owner.Pawn as Player;
			CurrentTool.Activate();
		}
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		CurrentTool?.Activate();
	}

	public override void ActiveEnd( Entity ent, bool dropped )
	{
		base.ActiveEnd( ent, dropped );

		CurrentTool?.Deactivate();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		CurrentTool?.Deactivate();
		CurrentTool = null;
	}

	public override void OnCarryDrop( Entity dropper )
	{
	}

	[Event.Frame]
	public void OnFrame()
	{
		if ( !IsActiveChild() ) return;

		CurrentTool?.OnFrame();
	}
}

namespace Sandbox.Tools
{
	public partial class BaseTool : NetworkComponent
	{
		public Tool Parent { get; set; }
		public Player Owner { get; set; }

		protected virtual float MaxTraceDistance => 10000.0f;

		// Set this to override the [Library]'s class default
		public string Description { get; set; } = null;

		public virtual void Activate()
		{
			CreatePreviews();
		}

		public virtual void Deactivate()
		{
			DeletePreviews();
			SpawnMenu.Instance?.ToolPanel?.DeleteChildren( true );
		}

		public virtual void Simulate()
		{

		}

		public virtual void OnFrame()
		{
			UpdatePreviews();
		}

		public virtual void CreateHitEffects( Vector3 pos, Vector3 normal = new Vector3(), bool continuous = false )
		{
			Parent?.CreateHitEffects( pos, normal, continuous );
		}

		protected string GetConvarValue(string name, string defaultValue = null)
		{
			return Host.IsServer
				? Owner.GetClientOwner().GetClientData<string>( name, defaultValue )
				: ConsoleSystem.GetValue( name, default );
		}
	}
}
