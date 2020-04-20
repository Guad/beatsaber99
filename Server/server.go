package main

import (
	"encoding/json"
	"net/http"
	"sync/atomic"
	"time"

	"github.com/guad/bsaber99/util"

	"github.com/gorilla/websocket"
	log "github.com/sirupsen/logrus"
)

var upgrader = websocket.Upgrader{}
var totalConnections int32

func clientLoop(ws *websocket.Conn, realip string) {
	client := &Client{
		joinTime: time.Now(),
		conn:     ws,
		items:    &ItemManager{},
		oldScore: -1,
		ip:       realip,
	}

	client.items.client = client

	atomic.AddInt32(&totalConnections, 1)

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

	client.items.client = nil
	client.items = nil

	atomic.AddInt32(&totalConnections, -1)
}

func serveWs(w http.ResponseWriter, r *http.Request) {
	realip := util.GetClientIP(r)
	ws, err := upgrader.Upgrade(w, r, nil)

	if err != nil {
		log.Println("upgrade error:", err)
		return
	}

	defer ws.Close()
	clientLoop(ws, realip)
}

func connectionLogger() {
	lastConnections := atomic.LoadInt32(&totalConnections)
	for {
		newConnections := atomic.LoadInt32(&totalConnections)
		if newConnections != lastConnections {
			log.WithFields(log.Fields{
				"connections": newConnections,
			}).Info("Total connections changed")
		}

		lastConnections = newConnections
		time.Sleep(2 * time.Second)
	}
}
