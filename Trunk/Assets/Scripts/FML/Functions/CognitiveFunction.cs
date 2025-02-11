﻿using System.Collections.Generic;

namespace FML {
    public class CognitiveFunction : FMLFunction {
        public enum MentalState {
            PLANNING,
            THINKING,
            REMEMBERING
        }

        public override FunctionType Function { get { return FunctionType.COGNITIVE_PROCESS; } }
        public MentalState Type;

        public CognitiveFunction(MentalState type) {
            Type = type;
        }
    }
}
