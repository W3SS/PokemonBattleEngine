﻿using Ether.Network.Packets;
using Kermalis.PokemonBattleEngine.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kermalis.PokemonBattleEngine.Packets
{
    public sealed class PPkmnHPChangedPacket : INetPacket
    {
        public const short Code = 0x0A;
        public IEnumerable<byte> Buffer { get; }

        public readonly byte VictimId;
        public readonly int Change;

        public PPkmnHPChangedPacket(PPokemon victim, int change)
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(Code));
            bytes.Add(VictimId = victim.Id);
            bytes.AddRange(BitConverter.GetBytes(Change = change));
            Buffer = BitConverter.GetBytes((short)bytes.Count).Concat(bytes);
        }
        public PPkmnHPChangedPacket(byte[] buffer)
        {
            Buffer = buffer;
            using (var r = new BinaryReader(new MemoryStream(buffer)))
            {
                r.ReadInt16(); // Skip Code
                VictimId = r.ReadByte();
                Change = r.ReadInt32();
            }
        }

        public void Dispose() { }
    }
}
