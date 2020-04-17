package main

import (
	"fmt"

	"github.com/gorilla/websocket"
)

type Client struct {
	conn      *websocket.Conn
	session   *Session
	name      string
	id        string
	lastState PlayerStateUpdatePacket
	items     *ItemManager
}

func (c *Client) Send(packet interface{}) {
	c.conn.WriteJSON(packet)
}

func (c *Client) Left() {
	if c.session != nil {
		c.session.RemovePlayer(c)
	}
}

func (c *Client) String() string {
	return fmt.Sprintf("%v (%v)", c.name, c.id)
}
