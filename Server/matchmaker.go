package main

import (
	"os"
	"strconv"
	"time"

	"github.com/google/uuid"
	"github.com/guad/bsaber99/songs"
	log "github.com/sirupsen/logrus"
)

type Matchmaker struct {
	queue chan *Client
}

var mainMatchmaker *Matchmaker

var MinPlayersForSession = 2

func startSession(session *Session) {
	starterSong := songs.PickRandomOfficialSong()
	starterDifficulty := "Expert"

	session.Send(StartPacket{
		TotalPlayers: len(session.players),
		Type:         "StartPacket",
		Difficulty:   starterDifficulty,
		LevelID:      starterSong,
	})

	// 5 songs queue
	choices := songs.PickNCustomSongs(5)

	for i := 0; i < 5; i++ {
		session.Send(EnqueueSongPacket{
			Type:           "EnqueueSongPacket",
			Characteristic: "Standard",
			Difficulty:     "Expert",
			Speed:          1.0 + 0.1*float64(i),
			LevelID:        choices[i],
		})
	}

	session.StartSession()

	log.WithFields(log.Fields{
		"session_id":          session.id,
		"players":             len(session.players),
		"starting_song":       starterSong,
		"starting_difficulty": starterDifficulty,
	}).Info("Session started")
}

func matchmake() {
	if val, ok := os.LookupEnv("MATCHMAKER_MIN_PLAYERS"); ok {
		MinPlayersForSession, _ = strconv.Atoi(val)
	}

	mainMatchmaker = &Matchmaker{
		queue: make(chan *Client, MinPlayersForSession),
	}

	for {
		matchmakingStart := time.Now()

		currentSession := &Session{
			players: make([]*Client, 0, MinPlayersForSession),
			state:   Matchmaking,
			id:      uuid.New().String(),
		}

		for len(currentSession.players) < MinPlayersForSession {
			player := <-mainMatchmaker.queue

			if player.state != MatchmakingClientState {
				continue
			}

			currentSession.Lock()
			currentSession.players = append(currentSession.players, player)
			player.session = currentSession
			currentSession.Unlock()

			if len(currentSession.players) == 1 {
				matchmakingStart = time.Now()
			}
		}

		matchmakingEnd := time.Now().Sub(matchmakingStart)

		log.WithFields(log.Fields{
			"session_id": currentSession.id,
			"duration":   matchmakingEnd.Seconds(),
			"players":    len(currentSession.players),
		}).Info("Matchmaking successful")

		startSession(currentSession)
	}
}
