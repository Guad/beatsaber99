package main

import (
	"math/rand"
)

func pickRandomSong() string {
	choice := rand.Intn(len(AllSongsIDs))
	song := AllSongsIDs[choice]

	return song
}

func pickRandomCustomSong() string {
	choice := rand.Intn(len(CustomSongsIDs))
	song := CustomSongsIDs[choice]

	return song
}
