package items

import (
	"strconv"

	"github.com/guad/bsaber99/db"
)

type ItemType string

const (
	Health1Item         ItemType = "+Health"              // Heal 0.1 energy
	Health2Item         ItemType = "++Health"             // Heal 0.2 energy
	Health3Item         ItemType = "+++Health"            // Heal 0.5 energy
	InvulnerabilityItem ItemType = "Invulnerability (5s)" // Invuln during 5s
	BrinkItem           ItemType = "One Hit Fail (5s)"    // Put player on brink for 5 seconds.
	PoisonItem          ItemType = "Poison (5s)"          // Dont increase energy for 5s
	ShieldItem          ItemType = "Shield (5s)"          // Ignore other players attacks
	NoArrowsItem        ItemType = "No Arrows (5s)"       // Remove arrows for 5s
	SwapNotesItem       ItemType = "Swap Notes (5s)"
	SendBombsItem       ItemType = "Send Bombs (5s)"
	GhostNotesItem      ItemType = "Ghost Notes (5s)"
	GhostArrowsItem     ItemType = "Ghost Arrows (5s)"
	DrainItem           ItemType = "Drain (5s)" // Drain health over time
)

var (
	ItemDropChance = 0.05
	ItemMinCombo   = 70
	OnFireMinCombo = 200
)

var AllItems = []ItemType{
	Health1Item,
	Health2Item,
	Health3Item,
	InvulnerabilityItem,
	BrinkItem,
	PoisonItem,
	ShieldItem,
	NoArrowsItem,
	SwapNotesItem,
	SendBombsItem,
	GhostNotesItem,
	GhostArrowsItem,
	DrainItem,
}

func IsItemOffensive(item ItemType) bool {
	switch item {
	case BrinkItem:
		fallthrough
	case PoisonItem:
		fallthrough
	case SwapNotesItem:
		fallthrough
	case SendBombsItem:
		fallthrough
	case GhostNotesItem:
		fallthrough
	case DrainItem:
		fallthrough
	case GhostArrowsItem:
		return true
	}
	return false
}

func UpdateChancesFromRedis(db *db.DB) {
	ItemMinCombo = int(fetchFloat(db, "ITEM_MIN_COMBO", float64(ItemMinCombo)))
	ItemDropChance = fetchFloat(db, "ITEM_DROP_CHANCE", ItemDropChance)
	OnFireMinCombo = int(fetchFloat(db, "ONFIRE_MIN_COMBO", float64(OnFireMinCombo)))
}

func fetchFloat(db *db.DB, key string, orelse float64) float64 {
	valueraw, err := db.Get(key)

	if err != nil {
		return orelse
	}

	value, err := strconv.ParseFloat(valueraw, 64)

	if err != nil {
		return orelse
	}

	return value
}
