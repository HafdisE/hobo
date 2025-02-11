package util.requests;

import java.util.List;
import java.util.ArrayList;

import org.ggp.base.player.event.PlayerTimeEvent;
import org.ggp.base.player.gamer.Gamer;
import org.ggp.base.player.gamer.event.GamerNewMatchEvent;
import org.ggp.base.player.gamer.event.GamerUnrecognizedMatchEvent;
import org.ggp.base.player.gamer.exception.MetaGamingException;
import org.ggp.base.player.request.grammar.Request;
import org.ggp.base.util.game.Game;
import org.ggp.base.util.game.LocalGameRepository;
import org.ggp.base.util.gdl.factory.GdlFactory;
import org.ggp.base.util.gdl.grammar.GdlConstant;
import org.ggp.base.util.logging.GamerLogger;
import org.ggp.base.util.match.Match;
import org.ggp.base.util.statemachine.exceptions.MoveDefinitionException;

import gamer.MCTS.MCTSControlValues;
import gamer.UnityGamer;

public final class UnityRequest extends Request {
    private Game game;
    private UnityGamer gamer;
    private String matchId;
    private String gameName;
    private int playClock;
    private GdlConstant roleName;
    private int startClock;
    private MCTSControlValues controlValues;

    public UnityRequest(Gamer gamer, GdlConstant roleName, String matchId,
                        String gameName, int startClock, int playClock,
                        List<Double> controlValues) {
        this.gamer = (UnityGamer) gamer;
        this.matchId = matchId;
        this.startClock = startClock;
        this.playClock = playClock;
        this.gameName = gameName;
        this.roleName = roleName;
        this.game = null;
        this.controlValues = new MCTSControlValues(new ArrayList<>(controlValues));
        System.out.println("Managed to make a UnityRequest");
    }

    @Override
    public String getMatchId() {
        return matchId;
    }

    @Override
    public String process(long receptionTime) {
        // Ensure that we aren't already playing a match. If we are,
        // ignore the message, saying that we're busy.
        if (gamer.getMatch() != null) {
            GamerLogger.logError("GamePlayer", "Got unity message while already busy playing a game: ignoring.");
            return "busy";
        }

        // Create the new match, and handle all of the associated logistics
        // in the gamer to indicate that we're starting a new match.
        LocalGameRepository repo = new LocalGameRepository();
        game = repo.getGame(gameName);
        Match match = new Match(matchId, -1, startClock, playClock, game, null);
        gamer.setMatch(match);
        gamer.setRoleName(roleName);
        System.out.println("managed to set gamer variables");

        // Finally, have the gamer begin metagaming.
        try {
            gamer.setValues(controlValues);
            gamer.metaGame(gamer.getMatch().getStartClock() * 1000 + receptionTime);
            System.out.println("Managed to start metagaming");
        } catch (MetaGamingException e) {
            GamerLogger.logStackTrace("GamePlayer", e);
            e.printStackTrace();
            // Upon encountering an uncaught exception during metagaming,
            // assume that indicates that we aren't actually able to play
            // right now, and tell the server that we're busy.
            gamer.setMatch(null);
            gamer.setRoleName(null);
            return "busy";
        }

        try{
            return "ready:false:" + gamer.getLegalMoves(gamer.getOtherRole()).toString()
                   + ":" +  gamer.getCurrentState().toString();
        } catch (MoveDefinitionException e){
           return "ERROR" + e ;
        }
    }

    @Override
    public String toString() {
        return "start";
    }
}
