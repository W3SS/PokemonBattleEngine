﻿using Ether.Network.Packets;
using System;

namespace Kermalis.PokemonBattleEngine.Network
{
    public sealed class PRequestActionPacket : INetPacketStream
    {
        public const int Code = 7;
        public byte[] Buffer => new byte[] { 4, 0, 0, 0, 7, 0, 0, 0 };

        public PRequestActionPacket() { }
        public PRequestActionPacket(byte[] buffer) { }

        public int Size => throw new NotImplementedException();
        public long Position => throw new NotImplementedException();
        public T Read<T>() => throw new NotImplementedException();
        public T[] ReadArray<T>(int amount) => throw new NotImplementedException();
        public void Write<T>(T value) => throw new NotImplementedException();
        public void Dispose() { }
    }
}
