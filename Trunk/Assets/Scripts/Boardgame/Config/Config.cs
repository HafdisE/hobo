﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Boardgame.Configuration {
    public static class Config {
        public static string MatchID = "1234";
        public static string GameName = "mylla";
        public static int StartTime = 20;
        public static int TurnTime = 10;
        public static int Turns = -1;

        public static int Low = 0;
        public static int Neutral = 1;
        public static int High = 2;

        public static int Extraversion = Neutral;
        public static int Agreeableness = Neutral;
        public static int Neuroticism = Neutral;
        public static int Conscientiousness = Neutral;
        public static int Openness = Neutral;


        //ggp defaults
        public static Networking.GGPSettings GGP = new Networking.GGPSettings(
            rave: 100,
            grave: 40,
            randomErr: 0,
            epsilon: 0.0f,
            treeDiscount: 0.997f,
            chargeDiscount: 0.99f,
            limit: 4000,
            firstAggro: 1.0f,
            secondAggro: 0.5f,
            firstDefense: 0.5f,
            secondDefense: 0.5f,
            horizon: 10000,
            chargeDepth: 10000,
            exploration: 45,
            agreeableness: 10
        );

        public static void SetValue(string which, string value) {
            //Debug.Log(which + " " + value);
            value = value.Trim();
            switch (which) {
                case "GameName":
                    GameName = value;
                    break;
                case "MatchID":
                    MatchID = value;
                    break;
                case "StartTime":
                case "TurnTime":
                case "Turns":
                    SetInt(which, value);
                    break;
                default:
                    SetInt(which, value, Low, High);
                    break;
            }
        }

        private static void SetInt(string which, string value, int min = -1, int max = 120) {
            int intValue;
            if (!Int32.TryParse(value, out intValue)) {
                Debug.LogError("Integer expected, did not receive integer.");
            }
            intValue = Mathf.Clamp(intValue, min, max);

            switch (which) {
                case "StartTime":
                    StartTime = intValue;
                    break;
                case "TurnTime":
                    TurnTime = intValue;
                    break;
                case "Turns":
                    Turns = intValue;
                    break;
                case "Extraversion":
                    Extraversion = intValue;
                    break;
                case "Agreeableness":
                    Agreeableness = intValue;
                    break;
                case "Neuroticism":
                    Neuroticism = intValue;
                    break;
                case "Conscientiousness":
                    Conscientiousness = intValue;
                    break;
                case "Openness":
                    Openness = intValue;
                    break;
            }
        }


    }
}
