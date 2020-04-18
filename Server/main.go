package main

import (
	"encoding/json"
	"log"
	"math/rand"
	"net/http"
	"time"

	"github.com/gorilla/websocket"
)

var upgrader = websocket.Upgrader{}

func clientLoop(ws *websocket.Conn) {
	defer ws.Close()

	client := &Client{
		conn:  ws,
		items: &ItemManager{},
	}

	client.items.client = client

	// TODO: Kick clients that dont send a ConnectionPacket in the first 10 seconds

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

func main() {
	rand.Seed(time.Now().UTC().UnixNano())

	go matchmake()

	http.HandleFunc("/ws", serveWs)

	log.Println("Started BS99 server")
	log.Fatal(http.ListenAndServe("0.0.0.0:6969", nil))
}
