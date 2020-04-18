package items

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

	ItemDropChance = 0.01
	// ItemDropChance = 1
	ItemMinCombo = 80
	// ItemMinCombo = 5
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
	case GhostArrowsItem:
		return true
	}
	return false
}
