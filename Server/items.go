package main

import (
	"fmt"
	"math/rand"
	"time"
)

type ItemType string

const (
	Health1Item         ItemType = "+Health"         // Heal 0.1 energy
	Health2Item         ItemType = "++Health"        // Heal 0.2 energy
	Health3Item         ItemType = "+++Health"       // Heal 0.5 energy
	InvulnerabilityItem ItemType = "Invulnerability" // Invuln during 10s
	BrinkItem           ItemType = "One Hit Fail"    // Put player on brink for 5 seconds.

	// ItemDropChance = 0.01
	ItemDropChance = 1
	// ItemMinCombo = 80
	ItemMinCombo = 5
)

var AllItems = []ItemType{
	Health1Item,
	Health2Item,
	Health3Item,
	InvulnerabilityItem,
	BrinkItem,
}

type ItemManager struct {
	client *Client

	currentAttacker *Client
	attackTime      time.Time

	currentItem *ItemType
}

func (im *ItemManager) Tick() {

	if im.currentItem == nil && im.client.lastState.CurrentCombo > ItemMinCombo {
		// Roll for item
		if rand.Float64() < ItemDropChance {
			// Choose item
			chosen := rand.Intn(len(AllItems))

			im.currentItem = &AllItems[chosen]

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

	switch *im.currentItem {
	case BrinkItem:
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

		break

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
