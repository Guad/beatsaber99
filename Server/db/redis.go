package db

import (
	"os"
	"time"

	"github.com/go-redis/redis/v7"
	log "github.com/sirupsen/logrus"
)

type DB struct {
	redis *redis.Client
}

func New() *DB {
	client := redis.NewClient(&redis.Options{
		Addr:        os.Getenv("REDIS_HOST"),
		Password:    os.Getenv("REDIS_PASS"),
		DB:          0,
		DialTimeout: 500 * time.Millisecond,
	})

	_, err := client.Ping().Result()

	if err != nil {
		log.WithError(err).Error("Failed to dial redis")
	}

	return &DB{
		redis: client,
	}
}

func (db *DB) Get(key string) (string, error) {
	return db.redis.Get(key).Result()
}

func (db *DB) GetList(key string) ([]string, error) {
	return db.redis.LRange(key, 0, -1).Result()
}

func (db *DB) GetSet(key string) ([]string, error) {
	return db.redis.SMembers(key).Result()
}
