package main

import (
	"fmt"
	"math/rand"
	"time"

	"github.com/guad/bsaber99/items"
)

var funnyDeathMessages = []string{
	"%v bit the dust!",
	"%v left our plane of existance!",
	"%v didn't swing hard enough!",
}

var funnyKillMessages = []string{
	"%v killed %v!",
	"%v destroyed %v!",
	"%v demolished %v!",
	"%v removed %v",
	"%v decomissioned %v!",
}

var funnyComboBreakMessages = []string{
	"%v broke their combo!",
	"%v screwed up their streak!",
	"%v lost their streak!",
	"%v screwed the pooch!",
}

var funnyOnFireMessage = []string{
	"%v is on fire!",
	"%v is unstoppable!",
	"%v is on a roll!",
}

func createDeathMessagePacket(player *Client) EventLogPacket {
	if player.items.currentAttacker == nil ||
		time.Now().Sub(player.items.attackTime) > 10*time.Second {
		chosen := rand.Intn(len(funnyDeathMessages))

		return EventLogPacket{
			Type: "EventLogPacket",
			Text: fmt.Sprintf(funnyDeathMessages[chosen], player.name),
		}
	} else {
		chosen := rand.Intn(len(funnyKillMessages))

		return EventLogPacket{
			Type: "EventLogPacket",
			Text: fmt.Sprintf(funnyKillMessages[chosen], player.name),
		}
	}
}

func createFunnyMessageForStateUpdate(sender *Client, packet PlayerStateUpdatePacket) {
	// Player broke their combo, but it's not a song change
	if sender.lastState.CurrentCombo > packet.CurrentCombo &&
		sender.lastState.Score < packet.Score &&
		sender.lastState.CurrentCombo > items.ItemMinCombo {
		chosen := rand.Intn(len(funnyComboBreakMessages))

		sender.session.Send(EventLogPacket{
			Type: "EventLogPacket",
			Text: fmt.Sprintf(funnyComboBreakMessages[chosen], sender.name),
		})
	}

	if sender.lastState.CurrentCombo < 200 &&
		packet.CurrentCombo > 200 {
		chosen := rand.Intn(len(funnyOnFireMessage))

		sender.session.Send(EventLogPacket{
			Type: "EventLogPacket",
			Text: fmt.Sprintf(funnyOnFireMessage[chosen], sender.name),
		})
	}

	sender.items.Tick()
}
