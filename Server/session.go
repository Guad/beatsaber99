package main

import (
	"sync"
)

type SessionState int

const (
	Matchmaking SessionState = iota
	Playing
)

type Session struct {
	sync.RWMutex

	players []*Client
	state   SessionState
}

func (s *Session) Send(data interface{}) {
	s.RLock()
	defer s.RUnlock()

	for _, player := range s.players {
		player.Send(data)
	}
}

func (s *Session) RemovePlayer(player *Client) {
	s.Lock()
	defer s.Unlock()

	for i := range s.players {
		if s.players[i] == player {
			s.players = append(s.players[:i], s.players[i+1:]...)
			break
		}
	}

	if s.state == Playing {
		if len(s.players) == 0 {
			s.Destroy()
		} else {
			s.Send(PlayersLeftPacket{
				Type:         "PlayersLeftPacket",
				TotalPlayers: len(s.players),
			})
		}
	}
}

func (s Session) Destroy() {
	// TODO
}
