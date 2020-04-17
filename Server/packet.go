package main

import (
	"encoding/json"
	"log"
)

type Packet interface {
	Dispatch(sender *Client)
}

type BasePacket struct {
	Type string `json:"type,omitempty"`
	data []byte `json:"-"`
}

func (bp BasePacket) Dispatch(sender *Client) {
	switch bp.Type {
	case "ConnectionPacket":
		p := ConnectionPacket{}
		json.Unmarshal(bp.data, &p)
		p.Dispatch(sender)
		break
	}
}

type ConnectionPacket struct {
	Name string `json:"name,omitempty"`
	ID   string `json:"id,omitempty"`
}

func (p ConnectionPacket) Dispatch(sender *Client) {
	sender.name = p.Name
	sender.id = p.ID

	log.Printf("%v has connected.\n", sender.String())

	mainMatchmaker.queue <- sender
}

type StartPacket struct {
	Type         string `json:"type,omitempty"`
	TotalPlayers int    `json:"TotalPlayers,omitempty"`
	Difficulty   string `json:"Difficulty,omitempty"`
	LevelID      string `json:"LevelID,omitempty"`
}

type PlayersLeftPacket struct {
	Type         string `json:"type,omitempty"`
	TotalPlayers int    `json:"TotalPlayers,omitempty"`
}
