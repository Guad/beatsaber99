package main

import (
	"fmt"
	"sync"
	"time"

	"github.com/gorilla/websocket"
)

type ClientState int

const (
	WaitingClientState ClientState = iota
	MatchmakingClientState
	PlayingClientState
	LeftClientState
)

type Client struct {
	sync.Mutex

	conn      *websocket.Conn
	session   *Session
	name      string
	id        string
	lastState PlayerStateUpdatePacket
	items     *ItemManager
	state     ClientState

	oldScore int

	cumulativeScore int
	currentScore    int

	joinTime time.Time
}

func (c *Client) Send(packet interface{}) {
	c.Lock()
	defer c.Unlock()

	c.conn.WriteJSON(packet)
}

func (c *Client) Kick() {
	c.Lock()
	defer c.Unlock()

	c.conn.Close()
}

func (c *Client) Left() {
	c.state = LeftClientState

	if c.session != nil {
		c.session.RemovePlayer(c)
	}
}

func (c *Client) updateScore() {
	if c.currentScore > c.lastState.Score {
		c.cumulativeScore += c.currentScore
	}

	c.currentScore = c.lastState.Score
}

func (c *Client) Score() int {
	return c.currentScore + c.cumulativeScore
}

func (c *Client) String() string {
	return fmt.Sprintf("%v (%v)", c.name, c.id)
}
