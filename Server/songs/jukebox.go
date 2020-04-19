package songs

import (
	"math/rand"
	"strconv"

	"github.com/guad/bsaber99/db"
	log "github.com/sirupsen/logrus"
)

type jukeboxCache struct {
	songList     []string
	difficulties []string
	speeds       []float64
}

func createCache(db *db.DB) *jukeboxCache {
	return &jukeboxCache{
		songList:     fetchSongList(db),
		difficulties: fetchDifficultyList(db),
		speeds:       fetchSpeedList(db),
	}
}

func PickRandomOfficialSong() string {
	return pickRandomElementFromList(OfficialSongsIDs)
}

func PickRandomCustomSong(db *db.DB) string {
	songs := fetchSongList(db)

	return pickRandomElementFromList(songs)
}

func pickRandomElementFromList(songs []string) string {
	choice := rand.Intn(len(songs))
	song := songs[choice]
	return song
}

func PickNCustomSongs(n int, db *db.DB) []Song {
	cache := createCache(db)

	choices := []Song{}

	if n > len(cache.songList) {
		n = len(cache.songList)
	}

	for len(choices) < n {
		song := pickRandomElementFromList(cache.songList)
		dup := false

		for _, picked := range choices {
			if picked.SongID == song {
				dup = true
				break
			}
		}

		if !dup {
			choices = append(choices, Song{
				SongID:         song,
				Characteristic: "Standard",
				Difficulty:     cache.getDifficultyForLevel(len(choices)),
				Speed:          cache.getSpeedForLevel(len(choices)),
			})
		}
	}

	return choices
}

func (cache *jukeboxCache) getSpeedForLevel(level int) float64 {
	if level >= len(cache.speeds) {
		level = len(cache.speeds) - 1
	}

	return cache.speeds[level]
}

func (cache *jukeboxCache) getDifficultyForLevel(level int) string {
	if level >= len(cache.difficulties) {
		level = len(cache.difficulties) - 1
	}

	return cache.difficulties[level]
}

func fetchSongList(db *db.DB) []string {
	key := "SONGS_IN_ROTATION"
	songs, err := db.GetSet(key)

	if err != nil {
		// Fallback to our list
		log.
			WithError(err).
			Error("Failed to fetch song list from redis")

		return CustomSongsIDs
	}

	return songs
}

func fetchSpeedList(db *db.DB) []float64 {
	key := "SPEEDS_BY_LEVEL"
	speeds, err := db.GetList(key)

	if err != nil {
		// Fallback to our list
		log.
			WithError(err).
			Error("Failed to fetch speed list from redis")

		list := make([]float64, 6)
		for i := range list {
			list[i] = 1.0 + 0.1*float64(i)
		}

		return list
	}

	list := make([]float64, len(speeds))

	for i := range speeds {
		list[i], _ = strconv.ParseFloat(speeds[i], 64)
	}

	return list
}

func fetchDifficultyList(db *db.DB) []string {
	key := "DIFFICULTIES_BY_LEVEL"
	difficulties, err := db.GetList(key)

	if err != nil {
		// Fallback to our list
		log.
			WithError(err).
			Error("Failed to fetch difficulty list from redis")

		return []string{"Expert"}
	}

	return difficulties
}
