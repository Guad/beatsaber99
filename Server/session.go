package main

type Session struct {
	players []*Client
}

func (s Session) Send(data interface{}) {
	for _, player := range s.players {
		player.Send(data)
	}
}

func (s Session) RemovePlayer(player *Client) {
	for i := range s.players {
		if s.players[i] == player {
			s.players = append(s.players[:i], s.players[i+1:]...)
			break
		}
	}

	if len(s.players) == 0 {
		s.Destroy()
	} else {
		s.Send(PlayersLeftPacket{
			Type:         "PlayersLeftPacket",
			TotalPlayers: len(s.players),
		})
	}
}

func (s Session) Destroy() {
	// TODO
}
