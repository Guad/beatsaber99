package main

import "time"

var idlewatch chan *Client

func StartIdlekicker() {
	idlewatch := make(chan *Client, 1000)

	for {
		player := <-idlewatch

		diff := time.Now().Sub(player.joinTime)
		if diff < 10*time.Second {
			time.Sleep(diff)
		}

		if player.state == LeftClientState {
			continue
		}

		if player.state == WaitingClientState || player.session == nil {
			player.Kick()
		}
	}
}

func StartIdlekickerForSession(session *Session) {
	for session.state == Playing {
		session.RLock()
		for _, p := range session.players {
			if p.oldScore == -1 {
				p.oldScore = p.lastState.Score
				continue
			}

			if p.lastState.Score == p.oldScore {
				// Player is idle, kick
				p.Kick()
			} else {
				p.oldScore = p.lastState.Score
			}
		}
		session.RUnlock()
		time.Sleep(30 * time.Second)
	}
}
