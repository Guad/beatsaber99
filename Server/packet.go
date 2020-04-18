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
	case "PlayerStateUpdatePacket":
		p := PlayerStateUpdatePacket{}
		json.Unmarshal(bp.data, &p)
		p.Dispatch(sender)
		break
	case "ActivateItemPacket":
		p := ActivateItemPacket{}
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

type EnqueueSongPacket struct {
	Type           string  `json:"type,omitempty"`
	Characteristic string  `json:"Characteristic,omitempty"`
	LevelID        string  `json:"LevelID,omitempty"`
	Difficulty     string  `json:"Difficulty,omitempty"`
	Speed          float64 `json:"Speed,omitempty"`
}

type WinnerPacket struct {
	Type string `json:"type,omitempty"`
}

type EventLogPacket struct {
	Type string `json:"type,omitempty"`
	Text string `json:"Text,omitempty"`
}

type PlayerStateUpdatePacket struct {
	Type         string  `json:"type,omitempty"`
	Score        int     `json:"Score,omitempty"`
	Energy       float64 `json:"Energy,omitempty"`
	CurrentCombo int     `json:"CurrentCombo,omitempty"`
}

func (p PlayerStateUpdatePacket) Dispatch(player *Client) {
	if player.session == nil || player.session.state != Playing {
		return
	}

	createFunnyMessageForStateUpdate(player, p)
	player.lastState = p
}

type GiveItemPacket struct {
	Type     string `json:"type,omitempty"`
	ItemType string `json:"ItemType,omitempty"`
}

type ActivateItemPacket struct {
	Type     string `json:"type,omitempty"`
	ItemType string `json:"ItemType,omitempty"`
}

func (p ActivateItemPacket) Dispatch(sender *Client) {
	if sender.session == nil || sender.session.state != Playing {
		return
	}

	sender.items.ActivateItem()
}
