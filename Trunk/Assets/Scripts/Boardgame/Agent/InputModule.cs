﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using Boardgame.Networking;
using Boardgame.Configuration;

namespace Boardgame.Agent {
    /// <summary>
    /// Listens to the boardgame state and GGP AI and relays events to the appropriate
    /// locations
    /// </summary>
    [RequireComponent(typeof(PersonalityModule), typeof(BrainModule))]
    public class InputModule : MonoBehaviour {
        private BrainModule bm;

        private bool isMyTurn = false;

        void Start() {
            bm = GetComponent<BrainModule>();
            BoardgameManager.Instance.OnMakeMove.AddListener(OnMoveMade);
            //if we want to be able to support multiple agents, they should refer
            //to their instance of a connection monitor, and connection monitor should not
            //be a singleton
            ConnectionMonitor.Instance.OnFeedUpdate.AddListener(CheckStatus);
            ConnectionMonitor.Instance.OnGameUpdate.AddListener(CheckGame);

        }

        /// <summary>
        /// Receive game state and execute moves if necessary
        /// </summary>
        /// <param name="data">The game state data</param>
        public void CheckGame(GameData data) {
            if (data.IsDone) return;
            if (data.IsStart && data.LegalMoves.Count == 0) {
                bm.player = Player.First;
            } else if (data.IsStart) {
                bm.player = Player.Second;
            }
            var move = data.MovesMade;
            if (!data.IsStart && bm.player == data.Control && move.Count > 0) {
                bm.ExecuteMove(move);
            }

            isMyTurn = !data.IsHumanPlayerTurn;
        }

        /// <summary>
        /// Receive evaluation information from the AI
        /// and relay to the brain. 
        /// </summary>
        /// <param name="data">evaluation info</param>
        public void CheckStatus(FeedData data) {
            bm.EvaluateConfidence(data, isMyTurn);
            bm.ConsiderMove(data.Best);
            if (isMyTurn) {
                if (bm.ReduceTurnTime.Check()) {
                    ConnectionMonitor.Instance.ModifyRequestTime(0.5f);
                }
                if (data.Moves.Count == 1) ConnectionMonitor.Instance.SetMaxRequestTime(1f);
            }

            ConnectionMonitor.Instance.WriteFeed(bm.UpdateParams());
        }

        /// <summary>
        /// If a move has been made, react to it
        /// </summary>
        /// <param name="moves">Moves</param>
        /// <param name="player">by whom</param>
        public void OnMoveMade(List<GDL.Move> moves, Player player) {
            Debug.Log("InputModule acknowledging a move has been made by " + player.ToString());
            bm.ReactMove(moves, player);
        }
    }
}