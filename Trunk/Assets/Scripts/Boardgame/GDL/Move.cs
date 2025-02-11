﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boardgame.GDL {
    public enum MoveType {
        MOVE,
        PLACE,
        REMOVE,
        CAPTURE
    }

    public class Move {
        public Move(string from, string to) {
            Type = MoveType.MOVE;
            From = from;
            To = to;
        }

        public Move(MoveType type, string from, string to) {
            Type = type;
            From = from;
            To = to;
        }

        public Move(MoveType type, string at) {
            Type = type;
            From = at;
            To = at;
        }

        public MoveType Type {
            get;
            protected set;
        }

        public string From {
            get; protected set;
        }
        public string To {
            get; protected set;
        }

        public override string ToString() {
            if(Type != MoveType.MOVE && Type != MoveType.CAPTURE) {
                return string.Format("( {0}: {1} )", Type, From);
            }
            return string.Format("( {0}: {1} -> {2} )", Type, From, To);
        }

        public override bool Equals(object obj) {
            if (obj == null) return false;
            if (obj.GetType().Equals(this.GetType())) {
                Move other = obj as Move;
                return other.Type == Type && other.From == From && other.To == To;
            } else return false;
        }

        public override int GetHashCode() {
            int hash = (Type.GetHashCode() >> 4) + (From.GetHashCode() >> 2) + To.GetHashCode();
            return hash;
        }
    }
}
