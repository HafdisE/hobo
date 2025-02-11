﻿using UnityEngine;
using FML;

namespace Behaviour {
    /// <summary>
    /// The base structure that all BML chunks have in common.
    /// All chunks inherit from this class.
    /// </summary>
	public abstract class BMLChunk {
        public string ID { get; set; }
        /// <summary>
        /// Return the character that is to execute the behaviour chunk.
        /// </summary>
        /// <value>The character.</value>
        public Participant Character { get; protected set; }
        /// <summary>
        /// Get the type of the chunk.
        /// </summary>
        /// <value>The type of the chunk.</value>
        public virtual BMLChunkType Type { get; protected set; }

        /// <summary>
        /// Returns the start sync point of the behaviour.
        /// </summary>
        /// <value>A float in seconds.</value>
        public float Start { get; protected set; }
        /// <summary>
        /// Returns the end sync point of the behaviour.
        /// </summary>
        /// <value>A float in seconds (relative to start).</value>
        public float End { get; protected set; }

        /// <summary>
        /// Prevent overriding currently executing behaviours etc.
        /// </summary>
        public int Priority { get; protected set; }

        /// <summary>
        /// Gets the time of a sync point. To be used to synchronise
        /// behaviour with generated speech.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
		public virtual float GetTime(SyncPoints point) {
            return 0f;
        }

        /// <summary>
        /// Sync up a sync point.
        /// </summary>
        /// <param name="thisPoint">E.g. the end of the action</param>
        /// <param name="withThis">The time to sync up with</param>
		public void Sync(SyncPoints thisPoint, float withThis) {
            float point = GetTime(thisPoint);
            float diff = point - withThis;
            Debug.Log(ID + "point " + point + " diff " + diff);
            Start -= diff;
        }

    }
}
