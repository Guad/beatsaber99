package main

import (
	"math/rand"
)

func pickRandomSong() string {
	choice := rand.Intn(len(AllSongsIDs))
	song := AllSongsIDs[choice]

	return song
}
