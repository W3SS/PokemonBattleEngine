﻿using Ether.Network.Packets;
using Kermalis.PokemonBattleEngine.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kermalis.PokemonBattleEngine.Packets
{
    public sealed class PMovePPChangedPacket : INetPacket
    {
        public const short Code = 0x17;
        public IEnumerable<byte> Buffer { get; }

        public readonly byte VictimId;
        public readonly PMove Move;
        public readonly int Change;

        public PMovePPChangedPacket(PPokemon victim, PMove move, int change)
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(Code));
            bytes.Add(VictimId = victim.Id);
            bytes.AddRange(BitConverter.GetBytes((ushort)(Move = move)));
            bytes.AddRange(BitConverter.GetBytes(Change = change));
            Buffer = BitConverter.GetBytes((short)bytes.Count).Concat(bytes);
        }
        public PMovePPChangedPacket(byte[] buffer)
        {
            Buffer = buffer;
            using (var r = new BinaryReader(new MemoryStream(buffer)))
            {
                r.ReadInt16(); // Skip Code
                VictimId = r.ReadByte();
                Move = (PMove)r.ReadUInt16();
                Change = r.ReadInt32();
            }
        }

        public void Dispose() { }
    }
}
