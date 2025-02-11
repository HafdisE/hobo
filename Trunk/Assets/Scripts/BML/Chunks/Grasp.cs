﻿using UnityEngine;
using FML;
using UnityEngine.Events;

namespace Behaviour {
    /// <summary>
    /// Have the actor grasp an object
    /// </summary>
    public class Grasp : BMLChunk {
        public override BMLChunkType Type { get { return BMLChunkType.Grasping; } }
        /// <summary>
        /// Gets which hand the behaviour is to be executed on.
        /// </summary>
        /// <value>Which hand.</value>
        public Lexemes.Mode Mode { get; private set; }
        // <summary>
        /// Returns the target to be pointed at.
        /// </summary>
        /// <value>The target.</value>
        public GameObject Target { get; private set; }


        //sync points
        public float Ready { get; private set; }
        public float StrokeStart { get; private set; }
        public float Stroke { get; private set; }
        public float StrokeEnd { get; private set; }
        public float Relax { get; private set; }

        //TODO: try not to make things so codependent
        public UnityAction<ActorMotion.Arm> Callback;


        /// <summary>
        /// Initializes a new instance of the <see cref="Grasp"/> class.
        /// </summary>
        /// <param name="id">the name of the chunk</param>
        /// <param name="character">the actor</param>
        /// <param name="target">the target object</param>
        /// <param name="mode">wihich hand</param>
        /// <param name="callback">a delegate to call on grasping completion</param>
        /// <param name="start">The start of the movement</param>
        /// <param name="end">The duration of the movement</param>
        public Grasp(string id, Participant character, GameObject target, Lexemes.Mode mode,
                     UnityAction<ActorMotion.Arm> callback,
                       float start = 0f, float ready = -1f, float strokeStart = -1f,
                       float stroke = -1f, float strokeEnd = -1f, float relax = -1f,
                       float end = 1f) {
            ID = id;
            Character = character;
            Target = target;
            Mode = mode;
            Start = start;
            StrokeStart = strokeStart;
            Stroke = stroke;
            StrokeEnd = strokeEnd;
            Ready = ready;
            Relax = relax;
            End = end;
            Callback = callback;
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
            return string.Format("[Grasping: Start={0}, End={1}, Mode={2}, Target={3}, Ready={4}, StrokeStart={5}, Stroke={6}, StrokeEnd={7}, Relax={8}]", Start, End, Mode, Target, Ready, StrokeStart, Stroke, StrokeEnd, Relax);
        }
    }
}
