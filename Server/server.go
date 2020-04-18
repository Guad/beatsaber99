package main

import (
	"encoding/json"
	"log"
	"net/http"
	"time"

	"github.com/gorilla/websocket"
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

	log.Printf("%v has disconnected.\n", client.String())
	client.Left()
}

func serveWs(w http.ResponseWriter, r *http.Request) {
	ws, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Println("upgrade:", err)
		return
	}

	defer ws.Close()

	clientLoop(ws)
}
