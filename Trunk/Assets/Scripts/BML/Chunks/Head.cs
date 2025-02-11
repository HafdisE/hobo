﻿using UnityEngine;
using System.Collections;
using FML;

namespace Behaviour {
    /// <summary>
    /// Head gestures and actions
    /// </summary>
	public class Head : BMLChunk {
        public override BMLChunkType Type { get { return BMLChunkType.Head; } }
        /// <summary>
        /// Get how many times the head behaviour should be repeated, if repeatable.
        /// </summary>
        /// <value>The number of repetitions.</value>
        public int Repetition { get; private set; }
        /// <summary>
        /// Get the exaggeration of the head movement.
        /// </summary>
        /// <value>The amount.</value>
        public float Amount { get; private set; }
        /// <summary>
        /// Gets which head behaviour to execute.
        /// </summary>
        /// <value>The head behaviour lexeme.</value>
        public Lexemes.Head Lexeme { get; private set; }

        //sync points
        public float Ready { get; private set; }
        public float StrokeStart { get; private set; }
        public float Stroke { get; private set; }
        public float StrokeEnd { get; private set; }
        public float Relax { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="Head"/> class.
        /// </summary>
        /// <param name="id">name of the chunk</param>
        /// <param name="character">the actor</param>
        /// <param name="repetition">whether the action should repeat</param>
        /// <param name="amount">the degree/intensity of the action</param>
        /// <param name="lexeme">which action</param>
        /// <param name="start">the start of the action</param>
        /// <param name="ready">when the action is ready</param>
        /// <param name="strokeStart">the start of the stroke</param>
        /// <param name="stroke">the exact moment of the stroke</param>
        /// <param name="strokeEnd">the end of the stroke</param>
        /// <param name="relax">relaxation of the action</param>
        /// <param name="end">duration of the action</param>
        public Head(string id, Participant character, int repetition, float amount, Lexemes.Head lexeme,
                       float start = 0f, float ready = -1f, float strokeStart = -1f,
                       float stroke = -1f, float strokeEnd = -1f, float relax = -1f,
                       float end = 1f) {
            ID = id;
            Character = character;
            Repetition = repetition;
            Amount = amount;
            Lexeme = lexeme;
            Start = start;
            StrokeStart = strokeStart;
            Stroke = stroke;
            StrokeEnd = strokeEnd;
            Ready = ready;
            Relax = relax;
            End = end;
        }

        public override float GetTime(SyncPoints point) {
            switch (point) {
                case SyncPoints.Start:
                    return Start;
                case SyncPoints.Ready:
                    return Start + Ready;
                case SyncPoints.Relax:
                    return Start + Relax;
                case SyncPoints.End:
                    return Start + End;
                case SyncPoints.StrokeStart:
                    return Start + StrokeStart;
                case SyncPoints.Stroke:
                    return Start + Stroke;
                case SyncPoints.StrokeEnd:
                    return Start + StrokeEnd;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        public override string ToString() {
            return string.Format("[Head: Start={0}, End={1}, Repetition={2}, Amount={3}, Lexeme={4}, Ready={5}, StrokeStart={6}, Stroke={7}, StrokeEnd={8}, Relax={9}]", Start, End, Repetition, Amount, Lexeme, Ready, StrokeStart, Stroke, StrokeEnd, Relax);
        }
    }
}
