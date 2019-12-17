using System;
using System.IO;
using System.IO.Streams;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace DisableDrops
{
	[ApiVersion(2, 1)]
    public class DisableDrops : TerrariaPlugin
    {
		public override Version Version
		{
			get { return new Version("1.0"); }
		}

		public override string Name
		{
			get { return "Return Drops"; }
		}

		public override string Author
		{
			get { return "Bippity"; }
		}

		public override string Description
		{
			get { return "Disable players from dropping items."; }
		}

		public DisableDrops(Main game) : base(game)
		{
			Order = 1;
		}

		public override void Initialize()
		{
			ServerApi.Hooks.NetGetData.Register(this, OnGetData);
			Commands.ChatCommands.Add(new Command("disabledrops.edit", DisableDropsCommand, "disabledrops"));
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);
			}
			base.Dispose(disposing);
		}


		bool enabled = true;
		public void DisableDropsCommand(CommandArgs args)
		{
			enabled = !enabled;

			if (enabled)
			{
				args.Player.SendWarningMessage("[Disable Drops] Drop disabling is enabled");
			}
			else
				args.Player.SendWarningMessage("[Disable Drops] Drop disabling is disabled");
		}

		public void OnGetData(GetDataEventArgs args)
		{
			if (args.MsgID == PacketTypes.ItemDrop)
			{
				if (args.Handled || !enabled)
					return;

				TSPlayer player = TShock.Players[args.Msg.whoAmI];

				if (!player.HasPermission("disabledrops.bypass")) //disabledrops.bypass l6d.user.itemdrop
				{
					using (var data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length))
					{
						Int16 id = data.ReadInt16();
						float posx = data.ReadSingle();
						float posy = data.ReadSingle();
						float velx = data.ReadSingle();
						float vely = data.ReadSingle();
						Int16 stacks = data.ReadInt16();
						int prefix = data.ReadByte();
						bool nodelay = data.ReadBoolean();
						Int16 netid = data.ReadInt16();

						//Item item = new Item();
						//item.SetDefaults(netid);
                        player.GiveItem(netid, player.Name, 1, 1, stacks, prefix);

                        if (id != 400)
							return;

						args.Handled = true;
                        //Console.WriteLine($"int16 id:{id}, float posx:{posx}, float posy:{posy}, float velx:{velx}, float vely:{vely}, int16 stacks:{stacks}, int preix:{prefix}, bool nodelay:{nodelay}, int16 netid:{netid}");
					}
					player.SendErrorMessage("[Disable Drops] You are not allowed to drop items.");
				}
			}
		}
	}
}
