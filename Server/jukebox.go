package main

import (
	"log"
	"math/rand"
)

func pickRandomSong() string {
	choice := rand.Intn(len(AllSongsIDs))
	song := AllSongsIDs[choice]

	log.Println("Picked song,", song)

	return song
}

func pickRandomCustomSong() string {
	choice := rand.Intn(len(CustomSongsIDs))
	song := CustomSongsIDs[choice]

	log.Println("Picked song,", song)

	return song
}
