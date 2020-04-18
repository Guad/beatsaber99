package main

import (
	"encoding/json"
	"net/http"
	"time"

	"github.com/gorilla/websocket"
	log "github.com/sirupsen/logrus"
)

var upgrader = websocket.Upgrader{}

func clientLoop(ws *websocket.Conn) {
	defer ws.Close()

	client := &Client{
		joinTime: time.Now(),
		conn:     ws,
		items:    &ItemManager{},
		oldScore: -1,
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

	log.WithFields(log.Fields{
		"name":        client.name,
		"id":          client.id,
		"ip":          client.conn.RemoteAddr().String(),
		"sessionTime": time.Now().Sub(client.joinTime).Seconds(),
		"state":       string(client.state),
		"score":       client.Score(),
		"position":    position,
	}).Info("User has disconnected")
	client.Left()
}

func serveWs(w http.ResponseWriter, r *http.Request) {
	ws, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Println("upgrade error:", err)
		return
	}

	defer ws.Close()

	clientLoop(ws)
}
