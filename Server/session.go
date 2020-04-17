package main

import (
	"log"
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
	// s.RLock()
	// defer s.RUnlock()

	for _, player := range s.players {
		player.Send(data)
	}
}

func (s *Session) RemovePlayer(player *Client) {
	// s.Lock()
	//defer s.Unlock()

	idx := -1
	for i := range s.players {
		if s.players[i] == player {
			idx = i
			break
		}
	}

	if idx == -1 {
		log.Println("Could not find player index!")
		return
	}

	s.players[idx] = s.players[len(s.players)-1]
	s.players[len(s.players)-1] = nil
	s.players = s.players[:len(s.players)-1]

	if s.state == Playing {
		if len(s.players) == 0 {
			s.Destroy()
		} else if len(s.players) == 1 {
			log.Println("Session winner:", s.players[0].String())
			s.players[0].Send(WinnerPacket{
				Type: "WinnerPacket",
			})
		} else {
			s.Send(PlayersLeftPacket{
				Type:         "PlayersLeftPacket",
				TotalPlayers: len(s.players),
			})

			s.Send(createDeathMessagePacket(player))
		}
	}
}

func (s Session) Destroy() {
	// TODO
}
