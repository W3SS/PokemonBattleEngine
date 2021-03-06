﻿using Ether.Network.Packets;
using Kermalis.PokemonBattleEngine.Battle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kermalis.PokemonBattleEngine.Packets
{
    public sealed class PBEPlayerJoinedPacket : INetPacket
    {
        public const short Code = 0x01;
        public IEnumerable<byte> Buffer { get; }

        public bool IsMe { get; }
        public byte Index { get; }
        public string TrainerName { get; }

        public PBEPlayerJoinedPacket(bool isMe, byte index, string trainerName)
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(Code));
            bytes.Add((byte)((IsMe = isMe) ? 1 : 0));
            bytes.Add(Index = index);
            bytes.AddRange(PBEUtils.StringToBytes(TrainerName = trainerName));
            Buffer = BitConverter.GetBytes((short)bytes.Count).Concat(bytes);
        }
        public PBEPlayerJoinedPacket(byte[] buffer, PBEBattle battle)
        {
            Buffer = buffer;
            using (var r = new BinaryReader(new MemoryStream(buffer)))
            {
                r.ReadInt16(); // Skip Code
                IsMe = r.ReadBoolean();
                Index = r.ReadByte();
                TrainerName = PBEUtils.StringFromBytes(r);
            }
        }

        public void Dispose() { }
    }
}
