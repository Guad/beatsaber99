package main

import (
	"fmt"
	"math/rand"
	"time"

	"github.com/guad/bsaber99/items"
)

type ItemManager struct {
	client *Client

	currentAttacker *Client
	attackTime      time.Time

	currentItem *items.ItemType
}

func (im *ItemManager) Tick() {
	if im.currentItem == nil && im.client.lastState.CurrentCombo > items.ItemMinCombo {
		// Roll for item
		if rand.Float64() < items.ItemDropChance {
			// Choose item
			chosen := rand.Intn(len(items.AllItems))

			im.currentItem = &items.AllItems[chosen]

			im.client.Send(EventLogPacket{
				Type: "EventLogPacket",
				Text: fmt.Sprintf("You rolled a new item: %v!", string(*im.currentItem)),
			})

			im.client.Send(GiveItemPacket{
				Type:     "GiveItemPacket",
				ItemType: string(*im.currentItem),
			})
		}
	}
}

func (im *ItemManager) ActivateItem() {
	if im.currentItem == nil {
		return
	}

	if items.IsItemOffensive(*im.currentItem) {
		target := im.chooseRandomOpponent()

		if target != nil {
			target.Send(ActivateItemPacket{
				Type:     "ActivateItemPacket",
				ItemType: string(*im.currentItem),
			})

			target.items.currentAttacker = im.client
			target.items.attackTime = time.Now()

			target.Send(EventLogPacket{
				Type: "EventLogPacket",
				Text: fmt.Sprintf("%v attacked you with %v!", im.client.name, string(*im.currentItem)),
			})

			im.client.Send(EventLogPacket{
				Type: "EventLogPacket",
				Text: fmt.Sprintf("You attacked %v with %v!", target.name, string(*im.currentItem)),
			})
		}
	}

	im.currentItem = nil
}

func (im *ItemManager) chooseRandomOpponent() *Client {
	var target *Client

	if len(im.client.session.players) <= 1 {
		return nil
	}

	for target == nil {
		indx := rand.Intn(len(im.client.session.players))

		if im.client.session.players[indx] != im.client {
			target = im.client.session.players[indx]
		}
	}

	return target
}
