using Engine.Graphics;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemBlocksTexture : Subsystem
	{
		public Texture2D BlocksTexture
		{
			get;
			private set;
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			Display.DeviceReset += Display_DeviceReset;
			LoadBlocksTexture();
		}

		public override void Dispose()
		{
			Display.DeviceReset -= Display_DeviceReset;
			DisposeBlocksTexture();
		}

		private void LoadBlocksTexture()
		{
			SubsystemGameInfo subsystemGameInfo = base.Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
			BlocksTexture = BlocksTexturesManager.LoadTexture(subsystemGameInfo.WorldSettings.BlocksTextureName);
		}

		private void DisposeBlocksTexture()
		{
			if (BlocksTexture != null && !ContentManager.IsContent(BlocksTexture))
			{
				BlocksTexture.Dispose();
				BlocksTexture = null;
			}
		}

		private void Display_DeviceReset()
		{
			LoadBlocksTexture();
		}
	}
}
