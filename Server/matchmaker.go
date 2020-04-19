package main

import (
	"os"
	"strconv"
	"time"

	"github.com/google/uuid"
	"github.com/guad/bsaber99/items"
	"github.com/guad/bsaber99/songs"
	log "github.com/sirupsen/logrus"
)

type Matchmaker struct {
	queue chan *Client
}

var mainMatchmaker *Matchmaker

var MinPlayersForSession = 1

func startSession(session *Session) {
	starterSong := songs.PickRandomOfficialSong()
	starterDifficulty := "Expert"
	numSongsToSend := 25

	starttime := getUnixTimestampMilliseconds() + 5000

	session.Send(StartPacket{
		TotalPlayers:    len(session.players),
		Type:            "StartPacket",
		Difficulty:      starterDifficulty,
		LevelID:         starterSong,
		ServerStartTime: starttime,
	})

	choices := songs.PickNCustomSongs(numSongsToSend, database)

	for i := 0; i < len(choices); i++ {
		session.Send(EnqueueSongPacket{
			Type:           "EnqueueSongPacket",
			Characteristic: choices[i].Characteristic,
			Difficulty:     choices[i].Difficulty,
			Speed:          choices[i].Speed,
			LevelID:        choices[i].SongID,
		})
	}

	session.StartSession()

	log.WithFields(log.Fields{
		"session_id":          session.id,
		"players":             len(session.players),
		"starting_song":       starterSong,
		"starting_difficulty": starterDifficulty,
	}).Info("Session started")

	go items.UpdateChancesFromRedis(database)
}

func updateMinPlayersForSession() {
	if result, err := database.Get("MATCHMAKER_MIN_PLAYERS"); err == nil {
		MinPlayersForSession, _ = strconv.Atoi(result)
	} else if val, ok := os.LookupEnv("MATCHMAKER_MIN_PLAYERS"); ok {
		MinPlayersForSession, _ = strconv.Atoi(val)
	}
}

func matchmake() {
	updateMinPlayersForSession()

	mainMatchmaker = &Matchmaker{
		queue: make(chan *Client, MinPlayersForSession),
	}

	serverStartup.Done()

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

			updateMinPlayersForSession()
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
