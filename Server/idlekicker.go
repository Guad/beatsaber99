package main

import (
	"time"

	log "github.com/sirupsen/logrus"
)

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
			log.WithFields(log.Fields{
				"name":        player.name,
				"id":          player.id,
				"ip":          player.conn.RemoteAddr().String(),
				"sessionTime": time.Now().Sub(player.joinTime).Seconds(),
				"state":       string(player.state),
			}).Info("Kicked user for not sending initial packet")

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
				log.WithFields(log.Fields{
					"name":        p.name,
					"id":          p.id,
					"ip":          p.conn.RemoteAddr().String(),
					"sessionTime": time.Now().Sub(p.joinTime).Seconds(),
					"state":       string(p.state),
					"score":       p.Score(),
				}).Info("Kicked user for idling in gameplay")
				p.Kick()
			} else {
				p.oldScore = p.lastState.Score
			}
		}
		session.RUnlock()
		time.Sleep(30 * time.Second)
	}
}
