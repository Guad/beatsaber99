package main

import "log"

type Matchmaker struct {
	queue chan *Client
}

var mainMatchmaker *Matchmaker

const MinPlayersForSession = 1

func startSession(players []*Client) {
	log.Println("Starting session...")

	sesh := &Session{
		players: players,
	}

	sesh.Send(StartPacket{
		TotalPlayers: len(players),
		Type:         "StartPacket",
		Difficulty:   "Expert",
		LevelID:      "100Bills",
	})
}

func matchmake() {
	mainMatchmaker = &Matchmaker{
		queue: make(chan *Client),
	}

	for {
		currentSession := make([]*Client, 0, MinPlayersForSession)

		for len(currentSession) < MinPlayersForSession {
			player := <-mainMatchmaker.queue
			currentSession = append(currentSession, player)
		}

		startSession(currentSession)
	}
}
