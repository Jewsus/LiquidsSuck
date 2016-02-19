using System;
using System.IO;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace HoneySucks
{
	[ApiVersion(1, 22)]
	public class LiquidsSuck : TerrariaPlugin
	{
		public static bool doesHoneySuck = true;
        public static bool doesLavaSuck = true;

        #region Info
        public override string Name
		{
			get { return "LiquidsSuck"; }
		}
		
		public override string Author
		{
			get { return "Jewsus"; }
		}
		
		public override string Description
		{
			get { return "Thats how you get ants!"; }
		}
		
		public override Version Version
		{
			get { return Assembly.GetExecutingAssembly().GetName().Version; }
		}
		#endregion
		
		#region Initialize
		public override void Initialize()
		{
			ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
			ServerApi.Hooks.NetGetData.Register(this, OnGetData);
		}
        #endregion
		
		#region Dispose
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
				ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);
			}
			base.Dispose(disposing);
		}
		#endregion
		
		public LiquidsSuck(Main game)
			: base(game)
		{
			Order = 10;
		}

		void OnInitialize(EventArgs args)
		{
			Commands.ChatCommands.Add(new Command("liquidsucks.admin.toggle", ToggleHoney, "thoney")
				{
					HelpText = "Turns on/off honey drop from hive block mining. Default setting is off."
				});
            Commands.ChatCommands.Add(new Command("liquidsucks.admin.toggle", ToggleLava, "tlava")
            {
                HelpText = "Turns on/off lava drop from hellstone block mining. Default setting is off."
            });
        }
		
		#region OnGetData
		void OnGetData(GetDataEventArgs args)
		{
			if (!doesHoneySuck)
            if (!doesLavaSuck)
                    return;

			if (!args.Handled && args.MsgID == PacketTypes.Tile)
			{
				using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
				{
					var action = reader.ReadByte();
					var x = reader.ReadInt16();
					var y = reader.ReadInt16();
					var type = reader.ReadUInt16();
					
					//when this tile is destroyed...
					if (Main.tile[x, y].type != 225)
                    if (Main.tile[x, y].type != 58)
                            return;
					
					if (action == 0)
					{
						//remove the tile from play instead of sending to the graveyard...
						Main.tile[x, y].active(false);
						Main.tile[x, y].frameX = -1;
						Main.tile[x, y].frameY = -1;
						Main.tile[x, y].liquidType(0);
						Main.tile[x, y].liquid = 0;
						Main.tile[x, y].type = 0;
						TSPlayer.All.SendTileSquare(x, y);
						TShock.Players[args.Msg.whoAmI].SendTileSquare(x, y);
						
						//and special summon hellstone to the field
						Item itm = new Item();
						itm.SetDefaults(1125);
						int itemid = Item.NewItem(x * 16, y * 16, itm.width, itm.height, itm.type, 1, true, 0, true);
						NetMessage.SendData((int)PacketTypes.ItemDrop, -1, -1, "", itemid, 0f, 0f, 0f);
					}

				}
			}
		}
		#endregion

		void ToggleHoney(CommandArgs args)
		{
			doesHoneySuck = !doesHoneySuck;

			args.Player.SendSuccessMessage("Honey from hive block mining is now {0}.",
				(doesHoneySuck) ? "OFF" : "ON");
		}
        void ToggleLava(CommandArgs args)
        {
            doesLavaSuck = !doesLavaSuck;

            args.Player.SendSuccessMessage("Lava from hellstone block mining is now {0}.",
                (doesLavaSuck) ? "OFF" : "ON");
        }
    }
}
