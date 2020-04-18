package main

import (
	"log"
	"os"
	"strconv"

	"github.com/guad/bsaber99/songs"
)

type Matchmaker struct {
	queue chan *Client
}

var mainMatchmaker *Matchmaker

var MinPlayersForSession = 2

func startSession(session *Session) {
	log.Println("Starting session...")

	session.Send(StartPacket{
		TotalPlayers: len(session.players),
		Type:         "StartPacket",
		Difficulty:   "Expert",
		LevelID:      songs.PickRandomSong(),
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

	session.state = Playing
}

func matchmake() {
	if val, ok := os.LookupEnv("MATCHMAKER_MIN_PLAYERS"); ok {
		MinPlayersForSession, _ = strconv.Atoi(val)
	}

	mainMatchmaker = &Matchmaker{
		queue: make(chan *Client, MinPlayersForSession),
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
			player.session = currentSession
			currentSession.Unlock()
		}

		startSession(currentSession)
	}
}
