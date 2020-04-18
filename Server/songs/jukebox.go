package songs

import (
	"log"
	"math/rand"
)

func PickRandomOfficialSong() string {
	choice := rand.Intn(len(AllSongsIDs))
	song := AllSongsIDs[choice]

	log.Println("Picked song,", song)

	return song
}

func PickRandomCustomSong() string {
	choice := rand.Intn(len(CustomSongsIDs))
	song := CustomSongsIDs[choice]

	log.Println("Picked song,", song)

	return song
}

func PickNCustomSongs(n int) []string {
	choices := []string{}

	for len(choices) < n {
		choice := rand.Intn(len(CustomSongsIDs))
		song := CustomSongsIDs[choice]
		dup := false

		for _, picked := range choices {
			if picked == song {
				dup = true
				break
			}
		}

		if !dup {
			choices = append(choices, song)
		}
	}

	return choices
}
