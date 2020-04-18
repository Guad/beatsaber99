package db

import (
	"os"

	"github.com/go-redis/redis/v7"
)

type DB struct {
	redis *redis.Client
}

func New() *DB {
	client := redis.NewClient(&redis.Options{
		Addr:     os.Getenv("REDIS_HOST"),
		Password: os.Getenv("REDIS_PASS"),
		DB:       0,
	})

	_, err := client.Ping().Result()

	if err != nil {
		panic(err)
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
