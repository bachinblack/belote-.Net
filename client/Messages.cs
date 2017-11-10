using System;
using ProtoBuf;

namespace CardGameServ
{
    [ProtoContract]
    public class InitMsg
    {
        [ProtoMember(1)]
        public char[] cards;
        [ProtoMember(2)]
        public string[] players;
        [ProtoMember(3)]
        public char threadId;
    }

    [ProtoContract]
    public class Msg
    {
        [ProtoMember(1)]
        public string text;
    }

    [ProtoContract]
    public class Hello
    {
        [ProtoMember(1)]
        public string text;
    }

    [ProtoContract]
    public class Back
    {
        [ProtoMember(1)]
        public string text;
    }

    [ProtoContract]
    public class GameCommand
    {
        [ProtoMember(1)]
        public char meta;
        [ProtoMember(2)]
        public char[] data;
    }
}