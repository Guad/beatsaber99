package main

import (
	"log"
	"os"
	"strconv"
)

type Matchmaker struct {
	queue chan *Client
}

var mainMatchmaker *Matchmaker

var MinPlayersForSession = 1

func startSession(session *Session) {
	session.RLock()
	defer session.RUnlock()

	log.Println("Starting session...")

	session.Send(StartPacket{
		TotalPlayers: len(session.players),
		Type:         "StartPacket",
		Difficulty:   "Expert",
		LevelID:      pickRandomSong(),
	})

	// 5 songs queue
	for i := 0; i < 5; i++ {
		session.Send(EnqueueSongPacket{
			Type:           "EnqueueSongPacket",
			Characteristic: "Standard",
			Difficulty:     "Expert",
			LevelID:        pickRandomSong(),
		})
	}

	session.state = Playing
}

func matchmake() {
	if val, ok := os.LookupEnv("MATCHMAKER_MIN_PLAYERS"); ok {
		MinPlayersForSession, _ = strconv.Atoi(val)
	}

	mainMatchmaker = &Matchmaker{
		queue: make(chan *Client),
	}

	for {
		currentSession := &Session{
			players: make([]*Client, 0, MinPlayersForSession),
			state:   Matchmaking,
		}

		for len(currentSession.players) < MinPlayersForSession {
			player := <-mainMatchmaker.queue

			currentSession.Lock()
			currentSession.players = append(currentSession.players, player)
			currentSession.Unlock()
		}

		startSession(currentSession)
	}
}
