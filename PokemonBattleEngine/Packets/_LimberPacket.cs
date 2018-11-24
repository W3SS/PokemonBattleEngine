﻿using Ether.Network.Packets;
using Kermalis.PokemonBattleEngine.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kermalis.PokemonBattleEngine.Packets
{
    // TODO: Transform this into an ability action packet
    public sealed class PLimberPacket : INetPacket
    {
        public const short Code = 0x19;
        public IEnumerable<byte> Buffer { get; }

        public readonly byte PokemonId;
        public readonly bool Prevented; // TODO: Remember why I put this

        public PLimberPacket(PPokemon pkmn, bool prevented)
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(Code));
            bytes.Add(PokemonId = pkmn.Id);
            bytes.Add((byte)((Prevented = prevented) ? 1 : 0));
            Buffer = BitConverter.GetBytes((short)bytes.Count).Concat(bytes);
        }
        public PLimberPacket(byte[] buffer)
        {
            Buffer = buffer;
            using (var r = new BinaryReader(new MemoryStream(buffer)))
            {
                r.ReadInt16(); // Skip Code
                PokemonId = r.ReadByte();
                Prevented = r.ReadBoolean();
            }
        }

        public void Dispose() { }
    }
}
