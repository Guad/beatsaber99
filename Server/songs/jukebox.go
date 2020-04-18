package songs

import (
	"log"
	"math/rand"
)

func PickRandomSong() string {
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
