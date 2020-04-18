package main

import (
	"encoding/json"
	"net/http"
	"time"

	"github.com/gorilla/websocket"
	log "github.com/sirupsen/logrus"
)

var upgrader = websocket.Upgrader{}

func clientLoop(ws *websocket.Conn, realip string) {
	client := &Client{
		joinTime: time.Now(),
		conn:     ws,
		items:    &ItemManager{},
		oldScore: -1,
		ip:       realip,
	}

	client.items.client = client

	idlewatch <- client

	for {
		_, message, err := ws.ReadMessage()

		if err != nil {
			break
		}

		var packet BasePacket
		err = json.Unmarshal(message, &packet)

		if err != nil {
			// Ignore?
			continue
		}

		packet.data = message
		packet.Dispatch(client)
	}

	position := -1

	if client.session != nil {
		position = len(client.session.players) + 1
	}

	client.Left()

	log.WithFields(log.Fields{
		"name":        client.name,
		"id":          client.id,
		"ip":          client.ip,
		"platform":    client.platform,
		"sessionTime": time.Now().Sub(client.joinTime).Seconds(),
		"state":       string(client.state),
		"score":       client.Score(),
		"position":    position,
	}).Info("User has disconnected")
}

func serveWs(w http.ResponseWriter, r *http.Request) {
	ws, err := upgrader.Upgrade(w, r, nil)

	realip := r.Header.Get("X-Forwarded-For")

	if err != nil {
		log.Println("upgrade error:", err)
		return
	}

	defer ws.Close()
	clientLoop(ws, realip)
}
