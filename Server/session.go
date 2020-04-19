package main

import (
	"sync"
	"time"

	log "github.com/sirupsen/logrus"
)

type SessionState int

const (
	Matchmaking SessionState = iota
	Playing
	Finished
)

type Session struct {
	sync.RWMutex

	id      string
	players []*Client
	state   SessionState
}

func (s *Session) StartSession() {
	s.RLock()
	defer s.RUnlock()

	for _, player := range s.players {
		player.state = PlayingClientState
	}

	s.state = Playing
	go StartIdlekickerForSession(s)
}

func (s *Session) Send(data interface{}) {
	s.RLock()
	defer s.RUnlock()

	for _, player := range s.players {
		player.Send(data)
	}
}

func (s *Session) RemovePlayer(player *Client) {
	// Be VERY careful with mutex unlocks.

	s.Lock()

	idx := -1
	for i := range s.players {
		if s.players[i] == player {
			idx = i
			break
		}
	}

	if idx == -1 {
		log.WithFields(log.Fields{
			"name":     player.name,
			"id":       player.id,
			"ip":       player.ip,
			"platform": player.platform,
		}).Warn("Could not find player index!")
		s.Unlock()
		return
	}

	s.players[idx] = s.players[len(s.players)-1]
	s.players[len(s.players)-1] = nil
	s.players = s.players[:len(s.players)-1]

	var lastPlayer *Client

	if len(s.players) == 1 {
		lastPlayer = s.players[0]
	}

	count := len(s.players)

	s.Unlock()

	if s.state == Playing {
		if count == 0 {
			s.Destroy()
		} else if count == 1 {
			winner := lastPlayer

			winner.Send(WinnerPacket{
				Type: "WinnerPacket",
			})

			log.WithFields(log.Fields{
				"name":        winner.name,
				"id":          winner.id,
				"ip":          winner.ip,
				"session_id":  s.id,
				"platform":    winner.platform,
				"sessionTime": time.Now().Sub(winner.joinTime).Seconds(),
				"score":       winner.Score(),
				"position":    1,
			}).Info("User has won the match")

		} else {
			s.Send(PlayersLeftPacket{
				Type:         "PlayersLeftPacket",
				TotalPlayers: len(s.players),
			})

			s.Send(createDeathMessagePacket(player))
		}
	}
}

func (s *Session) Destroy() {
	s.state = Finished
}
