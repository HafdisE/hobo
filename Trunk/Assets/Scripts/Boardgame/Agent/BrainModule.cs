﻿using UnityEngine;
using System.Collections.Generic;
using Boardgame.GDL;
using FML;
using FML.Boardgame;
using Behaviour;
using Boardgame.Configuration;
using System;
using Boardgame.Networking;

namespace Boardgame.Agent {
    /// <summary>
    /// A script that attempts to make sense of the game state and uses the agent's
    /// personality to determine his reaction to it, and interprets FML chunks appropriately.
    /// Essentially the agent's brain...
    /// </summary>
    [RequireComponent(typeof(PersonalityModule), typeof(InputModule), typeof(BehaviourRealiser))]
    public class BrainModule : MonoBehaviour {
        private PersonalityModule pm;
        private Mood mood;
        private BehaviourRealiser behave;
        private ActorMotion motion;
        private Participant me;

        public GGPSettings Params;

        Param exploration;
        Param aggression;
        Param treeDiscount;
        Param chargeDiscount;
        Param agreeableness;
        Param randomError;

        /// <summary>
        /// Which player the agent is in the current game.
        /// </summary>
        public Player player;

        float expressionIntensity = 1f;
        float moodMod = 1f;
        public int noiseChance = 1;
        void Start() {
            pm = GetComponent<PersonalityModule>();
            behave = transform.parent.GetComponentInChildren<BehaviourRealiser>();
            me = new Participant();
            me.identikit = GetComponent<Identikit>();
            motion = transform.parent.GetComponentInChildren<ActorMotion>();
            mood = GetComponent<Mood>();
            Params = new GGPSettings(Config.GGP);
            InitPersonalityParams();
            Debug.Log(pm.GetAgreeableness());
        }

        public GGPSettings UpdateParams() {
            float arousal = mood.GetArousal();
            float valence = mood.GetValence();
            Params.Agreeableness = (int)agreeableness.Interpolate(valence, arousal);
            if (player == Player.First) {
                Params.FirstAggression = aggression.Interpolate(valence, arousal);
            } else {
                Params.SecondAggression = aggression.Interpolate(valence, arousal);
            }
            Params.RandomError = randomError.Interpolate(valence, arousal);
            Params.TreeDiscount = treeDiscount.Interpolate(valence, arousal);
            Params.ChargeDiscount = chargeDiscount.Interpolate(valence, arousal);
            Params.Exploration = (int)exploration.Interpolate(valence, arousal);

            return Params;
        }

        void InitPersonalityParams() {
            switch (pm.GetOpenness()) {
                case PersonalityModule.PersonalityValue.high:
                    exploration = new Param(40, 100);
                    break;
                case PersonalityModule.PersonalityValue.neutral:
                    exploration = new Param(20, 60);
                    break;
                case PersonalityModule.PersonalityValue.low:
                    exploration = new Param(5, 30);
                    break;
            }

            switch (pm.GetAgreeableness()) {
                case PersonalityModule.PersonalityValue.high:
                    agreeableness = new Param(10, 30, 0.8f, 0.2f);
                    aggression = new Param(0f, 0.3f);
                    treeDiscount = new Param(0.999f, 1f);
                    chargeDiscount = new Param(0.995f, 1f);
                    break;
                case PersonalityModule.PersonalityValue.neutral:
                    agreeableness = new Param(30, 40, 0.8f, 0.2f);
                    aggression = new Param(0.3f, 1f);
                    treeDiscount = new Param(0.995f, 0.999f);
                    chargeDiscount = new Param(0.99f, 0.995f);
                    break;
                case PersonalityModule.PersonalityValue.low:
                    agreeableness = new Param(-1, -1);
                    aggression = new Param(1f, 3f);
                    treeDiscount = new Param(0.99f, 0.995f);
                    chargeDiscount = new Param(0.97f, 0.99f);
                    break;
            }

            switch (pm.GetExtraversion()) {
                case PersonalityModule.PersonalityValue.high:
                    expressionIntensity = 1f;
                    noiseChance = 2;
                    break;
                case PersonalityModule.PersonalityValue.neutral:
                    expressionIntensity = 0.8f;
                    noiseChance = 3;
                    break;
                case PersonalityModule.PersonalityValue.low:
                    expressionIntensity = 0.65f;
                    noiseChance = 5;
                    break;
            }

            switch (pm.GetNeuroticism()) {
                case PersonalityModule.PersonalityValue.high:
                    randomError = new Param(0.999f, 1f, 0.2f, 0.8f);
                    idleDurationBaseValue = 10f;
                    moodMod = 1f;
                    break;
                case PersonalityModule.PersonalityValue.neutral:
                    randomError = new Param(0.99f, 0.999f, 0.2f, 0.8f);
                    idleDurationBaseValue = 17f;
                    moodMod = 0.85f;
                    break;
                case PersonalityModule.PersonalityValue.low:
                    randomError = new Param(0.9f, 0.99f, 0.2f, 0.8f);
                    idleDurationBaseValue = 25f;
                    moodMod = 0.6f;
                    break;
            }

            switch (pm.GetConscientiousness()) {
                case PersonalityModule.PersonalityValue.high:
                    exploration.Add(min: 15);
                    randomError.Add(min: 0.02f);
                    break;
                case PersonalityModule.PersonalityValue.neutral:
                    exploration.Add(min: 5);
                    randomError.Add(min: 0.005f);
                    break;
                case PersonalityModule.PersonalityValue.low:
                    exploration.Add(min: -4);
                    randomError.Add(min: -0.02f);
                    break;
            }

        }

        float idleDurationBaseValue;
        [SerializeField]
        float idleDuration;
        [SerializeField]
        float idleLeft = 0f;
        void Update() {
            idleLeft -= Time.deltaTime;
            idleDuration = idleDurationBaseValue / mood.GetArousal();
            if (idleLeft <= 0f) {
                idleLeft = idleDuration;
                int die = UnityEngine.Random.Range(0, 4);
                int hand = UnityEngine.Random.Range(0, 2);
                switch (die) {
                    case 0:
                        motion.SetPose(0, 0);
                        break;
                    case 1:
                        motion.SetPose(1, 1);
                        break;
                    case 2:
                        if (hand == 0) {
                            motion.SetPose(0, 2);
                        } else {
                            motion.SetPose(2, 0);
                        }
                        break;
                    case 3:
                        if (hand == 0) {
                            motion.SetPose(0, 3);
                        } else {
                            motion.SetPose(3, 0);
                        }
                        break;
                }
            }
        }
        #region React to move
        private Move myBestMove;
        private Dictionary<Move, FMove> firstMoves;
        private Dictionary<Move, FMove> secondMoves;
        private float firstAverageSims, secondAverageSims;
        private Switch surprised = new Switch();
        private Switch impatient = new Switch();
        public Switch ReduceTurnTime = new Switch();
        /// <summary>
        /// Using the data currently stored, attempt to work out whether the
        /// move was a "surprise" and such for the AI
        /// </summary>
        /// <param name="move">The move in question</param>
        /// <param name="who">The player who made it</param>
        /// <returns>The agent's reaction</returns>
        private void react(Move move, Player who) {
            float averageSims = (who == Player.First ? firstAverageSims : secondAverageSims);
            var moves = (who == Player.First ? firstMoves : secondMoves);
            if (moves == null) return;
            FMove m;
            try {
                m = moves[move];
            } catch (Exception e) {
                Debug.LogWarning("Sync issue");
                Debug.Log(move);
                var ms = new Move[moves.Count];
                moves.Keys.CopyTo(ms, 0);
                Debug.Log(Tools.Stringify<Move>.Array(ms));
                return;
            }
            Debug.Log(m.Who);
            if (m.Who != who) return;
            if ((m.Simulations < averageSims * 0.8f && who != player) || (who == player && !move.Equals(myBestMove))) {
                Debug.Log("SURPRISED");
                surprised.Enable();
            }

        }
        #endregion

        #region React to general game state

        public float confidence = 0;
        private int impatience = 0;
        private int confidentCount = 0;
        private int notConfidentCount = 0;
        private int disproportionateFavour = 0;
        private int opponentDisproportionateFavour = 0;
        private int weightedUCTOverFoe = 0;
        private int weightedUCTUnderFoe = 0;
        private int highSimCount = 0;
        /// <summary>
        /// Evaluates the state of the game via data fed by the GGP AI.
        /// Adjusts mood accordingly.
        /// </summary>
        /// <param name="d">The data</param>
        /// <param name="isMyTurn">whether it is the agent's turn or not</param>
        public void EvaluateConfidence(Networking.FeedData d, bool isMyTurn) {
            if (d.Best == null) return;
            if (isMyTurn) {
                myBestMove = d.Best;
                if (player == Player.First) {
                    firstMoves = d.Moves;
                    firstAverageSims = (float)d.AverageSimulations;
                } else {
                    secondMoves = d.Moves;
                    secondAverageSims = (float)d.AverageSimulations;
                }
            } else {
                if (player == Player.First) {
                    secondMoves = d.Moves;
                    secondAverageSims = (float)d.AverageSimulations;
                } else {
                    firstMoves = d.Moves;
                    firstAverageSims = (float)d.AverageSimulations;
                }
            }
            float myUCT, foeUCT, myWUCT, foeWUCT, valence = 0, arousal = 0;
            float previousConfidence = confidence;
            valence = 0;
            arousal = 0;
            if (player == Player.First) {
                myUCT = d.Moves[d.Best].FirstUCT;
                foeUCT = d.Moves[d.Best].SecondUCT;
                myWUCT = d.FirstWeightedUCT;
                foeWUCT = d.SecondWeightedUCT;
            } else {
                myUCT = d.Moves[d.Best].SecondUCT;
                foeUCT = d.Moves[d.Best].FirstUCT;
                myWUCT = d.SecondWeightedUCT;
                foeWUCT = d.FirstWeightedUCT;
            }

            //increment counters for various states
            //best move is by far most simulated and is advantageous for me
            count(d.SimulationStdDev > 2f && myUCT + 5f > foeUCT, ref disproportionateFavour);
            count(myUCT < foeUCT + 5f, ref opponentDisproportionateFavour);
            //my total weighted uct is greater than my foe's
            count(myWUCT + 5f > foeWUCT, ref weightedUCTOverFoe);
            count(myWUCT < foeWUCT + 5f, ref weightedUCTUnderFoe);
            //sim count high
            count(Config.GGP.Limit * 0.8 < d.TotalSimulations, ref highSimCount);

            //only start affecting confidence if these have been true for a few iterations
            //and stop affecting confidence when it has been true for a generous while
            if (stabilised(disproportionateFavour)) {
                confidence += 3f * weight(disproportionateFavour);
            }
            if (stabilised(weightedUCTOverFoe)) confidence += 0.5f * weight(weightedUCTOverFoe);
            if (stabilised(weightedUCTUnderFoe)) confidence -= 0.5f * weight(weightedUCTUnderFoe);
            if (stabilised(opponentDisproportionateFavour)) {
                valence -= moodMod * 3f * weight(opponentDisproportionateFavour);
                confidence -= 8f * weight(opponentDisproportionateFavour);
            }
            if (stabilised(highSimCount)) confidence += 0.5f * weight(highSimCount);

            count(confidence > previousConfidence, ref confidentCount);
            count(confidence < previousConfidence, ref notConfidentCount);


            float diff = Mathf.Abs(confidence - previousConfidence);
            if (diff > 0.7f) {
                arousal += 7f;
            }

            if (stabilised(highSimCount)) {
                arousal -= moodMod * 8f * weight(highSimCount);
            }

            if (stabilised(notConfidentCount)) {
                //Debug.Log("DROP");
                valence -= moodMod * 8f * weight(notConfidentCount);
                arousal += moodMod * 5f * weight(notConfidentCount);
            } else if (stabilised(confidentCount)) {
                //Debug.Log("INCR");
                valence += moodMod * 8f * weight(confidentCount);
                arousal -= moodMod * 5f * weight(confidentCount);
            }

            if (confidence > 10) {
                if (!isMyTurn) {
                    impatience++;
                    if (impatience == 80) {
                        impatient.Enable();
                        ReduceTurnTime.Enable();
                        impatience = 0;
                    }
                }
            } else {
                impatience = 0;
            }

            mood.Evaluate(valence, arousal);

        }

        private float weight(int val) {
            return (float)(80 - val) / 80;
        }

        private bool stabilised(int val) {
            return val > 5 && val < 50;
        }
        private void count(bool truthy, ref int val) {
            if (val > 300) {
                val = 0;
                return;
            }
            if (truthy) val = val + 1;
            else val = 0;
        }

        #endregion

        #region Transform FML into BML and helpers
        /// <summary>
        /// Transforms FML into BML and executes
        /// </summary>
        /// <param name="body">the FML body containing the FML functions to be interpreted</param>
        private void interpret(FMLBody body) {
            foreach (var chunk in body.chunks) {
                BMLBody curr = new BMLBody();
                chunk.BMLRef = curr;
                foreach (var function in chunk.functions) {
                    Vocalisation voc = new Vocalisation("Vocal", chunk.owner, 1f, mood.GetArousal(), mood.GetValence());
                    switch (function.Function) {
                        case FMLFunction.FunctionType.BOARDGAME_REACT_MOVE:
                            ReactMoveFunction react = function as ReactMoveFunction;
                            this.react(react.MoveToReact[0], react.MyMove ? player : (player == Player.First ? Player.Second : Player.First));
                            FaceEmotion faceReact;
                            Posture poser = new Posture("postureReact", chunk.owner, Behaviour.Lexemes.Stance.SITTING, 0f, 8f);
                            if (surprised.Check()) {
                                int coin = UnityEngine.Random.Range(0, 3);
                                faceReact = new FaceEmotion("ConfusedFace", chunk.owner, 0f, 1.4f * expressionIntensity, 0.8f * expressionIntensity);
                                if (coin < 2) {
                                    poser.AddPose(Behaviour.Lexemes.BodyPart.RIGHT_ARM, Behaviour.Lexemes.BodyPose.FIST_COVER_MOUTH);
                                    curr.AddChunk(poser);
                                }
                                coin = UnityEngine.Random.Range(0, noiseChance);
                                curr.AddChunk(faceReact);
                                if (coin == 0) curr.AddChunk(voc);
                            }
                            break;
                        case FMLFunction.FunctionType.BOARDGAME_CONSIDER_MOVE:
                            //tbh this shouldn't be here
                            if (impatient.Check()) {
                                Debug.Log("IMPATIENT");
                                Gaze glanceAtPlayer = new Gaze("glanceAtPlayer", chunk.owner, motion.Player, Behaviour.Lexemes.Influence.HEAD, start: 0f, end: 0.25f, priority: 2);
                                curr.AddChunk(glanceAtPlayer);
                                curr.AddChunk(voc);
                            } else {
                                ConsiderMoveFunction func = function as ConsiderMoveFunction;
                                if (func.MoveToConsider.Type == MoveType.MOVE) {
                                    Gaze glanceFrom = new Gaze("glanceAtFrom", chunk.owner, BoardgameManager.Instance.GetCellObject(func.MoveToConsider.From),
                                        Behaviour.Lexemes.Influence.HEAD, start: 0f, end: 2f);
                                    curr.AddChunk(glanceFrom);
                                    Gaze glanceTo = new Gaze("glanceAtTo", chunk.owner, BoardgameManager.Instance.GetCellObject(func.MoveToConsider.To),
                                        Behaviour.Lexemes.Influence.HEAD, start: 2.1f, end: 2f, priority: 2);
                                    curr.AddChunk(glanceTo);
                                } else {
                                    Gaze glanceTo = new Gaze("glanceAtCell", chunk.owner, BoardgameManager.Instance.GetCellObject(func.MoveToConsider.To),
                                       Behaviour.Lexemes.Influence.HEAD, start: 0f, end: 2f);
                                    curr.AddChunk(glanceTo);
                                }
                            }

                            break;
                        case FMLFunction.FunctionType.BOARDGAME_MAKE_MOVE:
                            MakeMoveFunction move = function as MakeMoveFunction;
                            PhysicalCell from, to;
                            BoardgameManager.Instance.GetMoveFromTo(move.MoveToMake[0], player, out from, out to);
                            Grasp reach = new Grasp("reachTowardsPiece", chunk.owner, from.gameObject,
                                Behaviour.Lexemes.Mode.LEFT_HAND, (arm) => { graspPiece(from, arm); }, 0, end: 1.5f);
                            Posture leanReach = new Posture("leantowardsPiece", chunk.owner, Behaviour.Lexemes.Stance.SITTING, 0, end: 1.5f, priority: 2);
                            Gaze lookReach = new Gaze("glanceAtReach", chunk.owner, from.gameObject, Behaviour.Lexemes.Influence.HEAD, start: 0f, end: 1.25f);
                            Debug.Log((int)(Vector3.Distance(from.transform.position, transform.position) * 40));
                            leanReach.AddPose(Behaviour.Lexemes.BodyPart.WHOLEBODY, Behaviour.Lexemes.BodyPose.LEANING_FORWARD,
                                (int)(Vector3.Distance(from.transform.position, transform.position) * 40));
                            Place place = new Place("placePiece", chunk.owner, to.gameObject,
                                Behaviour.Lexemes.Mode.LEFT_HAND, (piece) => {
                                    placePiece(piece, to);
                                    BoardgameManager.Instance.MoveMade(move.MoveToMake, player); BoardgameManager.Instance.SyncState(); BoardgameManager.Instance.MakeNoise(to.id);
                                },
                                1.25f, end: 2f);
                            Posture leanPlace = new Posture("leantowardsCell", chunk.owner, Behaviour.Lexemes.Stance.SITTING, 1.25f, end: 1.5f, priority: 3);
                            leanPlace.AddPose(Behaviour.Lexemes.BodyPart.WHOLEBODY, Behaviour.Lexemes.BodyPose.LEANING_FORWARD,
                                (int)(Vector3.Distance(to.transform.position, transform.position) * 40));
                            Gaze lookPlace = new Gaze("glanceAtPlace", chunk.owner, to.gameObject, Behaviour.Lexemes.Influence.HEAD, start: 1.25f, end: 2f, priority: 2);
                            Gaze glance = new Gaze("glanceAtPlayer", chunk.owner, motion.Player, Behaviour.Lexemes.Influence.HEAD, start: 2f, end: 1.25f, priority: 3);
                            curr.AddChunk(glance);
                            curr.AddChunk(lookReach);
                            curr.AddChunk(lookPlace);
                            curr.AddChunk(leanReach);
                            curr.AddChunk(leanPlace);
                            curr.AddChunk(reach);
                            curr.AddChunk(place);
                            break;
                        case FMLFunction.FunctionType.EMOTION:
                            //transform expression
                            EmotionFunction f = function as EmotionFunction;
                            float v = Mathf.Clamp(f.Valence, 0, 2);

                            if (v < Config.Neutral) {
                                v = (v - Config.Neutral) * 0.8f + Config.Neutral;
                            }
                            float a = Mathf.Clamp(f.Arousal, 0, 2);
                            if (a > Config.Neutral) {
                                a = (a - Config.Neutral) * 0.6f + Config.Neutral;
                            }
                            FaceEmotion fe = new FaceEmotion("emote " + f.Arousal + " " + f.Valence, chunk.owner, 0f, a, v);
                            float lean = Mathf.Clamp((f.Arousal - Config.Neutral) * 30, -20, 30);
                            Posture emoLean = new Posture("emoteLean", chunk.owner, Behaviour.Lexemes.Stance.SITTING, 0, end: 3f);
                            emoLean.AddPose(Behaviour.Lexemes.BodyPart.WHOLEBODY, Behaviour.Lexemes.BodyPose.LEANING_FORWARD, (int)lean);
                            curr.AddChunk(emoLean);
                            curr.AddChunk(fe);
                            break;
                    }
                }
            }
            //sort out timing
            foreach (var chunk in body.chunks) {
                if (chunk.timing != null) {
                    switch (chunk.timing.Primitive) {
                        case Primitive.MustEndBefore:
                            chunk.timing.ChunkReference.BMLRef.ExecuteAfter(chunk.BMLRef);
                            break;
                        case Primitive.StartImmediatelyAfter:
                            chunk.BMLRef.ExecuteAfter(chunk.timing.ChunkReference.BMLRef);
                            break;
                    }
                }
            }

            //let's refactor this later so that we don't have to do 3N
            foreach (var chunk in body.chunks) {
                behave.ScheduleBehaviour(chunk.BMLRef);
            }
        }

        /// <summary>
        /// Callback for grasping pieces, called upon reaching target
        /// </summary>
        /// <param name="cell">Cell from where to get piece</param>
        /// <param name="arm">The arm that should take the piece</param>
        private void graspPiece(PhysicalCell cell, ActorMotion.Arm arm) {
            GameObject p = cell.RemovePiece();
            p.transform.SetParent(arm.transform);
            p.transform.localPosition = Vector3.zero;
            arm.holding = p.transform;
        }

        /// <summary>
        /// Callback for placing pieces, called upon reaching cell
        /// </summary>
        /// <param name="piece">The piece gameObject</param>
        /// <param name="newCell">Cell in which to place piece</param>
        private void placePiece(GameObject piece, PhysicalCell newCell) {
            newCell.Place(piece);
        }
        #endregion

        #region FML generation
        float lastTime = -1f;
        /// <summary>
        /// Generates the appropriate FML for considering the given move.
        /// Every 2 seconds, generates the emotion only.
        /// Every 4 seconds, generates both emotion and move consideration.
        /// Interprets it instantly.
        /// </summary>
        /// <param name="move">Move to consider.</param>
        public void ConsiderMove(Move move) {
            if (lastTime == -1) lastTime = Time.time;
            FMLBody body = new FMLBody();
            if (Time.time >= lastTime + 4f) {
                MentalChunk mc = new MentalChunk();
                //eye movement every 4
                mc.AddFunction(new ConsiderMoveFunction(move));
                mc.owner = me;
                body.AddChunk(mc);
                body.AddChunk(getEmotion());
                interpret(body);
                lastTime = Time.time;
            } else if (Time.time >= lastTime + 2f) {
                //emotions every second
                body.AddChunk(getEmotion());
                interpret(body);
            }

        }

        /// <summary>
        /// Generates the FML for executing a move and interprets it.
        /// </summary>
        /// <param name="moves"></param>
        public void ExecuteMove(List<Move> moves) {
            FMLBody body = new FMLBody();
            PerformativeChunk pc = new PerformativeChunk();

            pc.AddFunction(new MakeMoveFunction(moves));
            pc.owner = me;
            body.AddChunk(pc);
            body.AddChunk(getEmotion());

            interpret(body);
        }

        /// <summary>
        /// Generate a reaction to a move
        /// </summary>
        /// <param name="moves">Moves</param>
        /// <param name="who">who made the move(s)</param>
        public void ReactMove(List<Move> moves, Player who) {
            FMLBody body = new FMLBody();
            PerformativeChunk pc = new PerformativeChunk();

            pc.AddFunction(new ReactMoveFunction(moves, who == player));
            pc.owner = me;
            body.AddChunk(pc);

            interpret(body);
        }

        /// <summary>
        /// Helper that generates an emotion function.
        /// </summary>
        /// <returns>emotion function with data from current mood</returns>
        private MentalChunk getEmotion() {
            MentalChunk chunk = new MentalChunk();
            chunk.AddFunction(new EmotionFunction(mood.GetArousal(), mood.GetValence()));
            chunk.owner = me;
            return chunk;
        }
        #endregion
    }
}
