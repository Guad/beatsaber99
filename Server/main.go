package main

import (
	"math/rand"
	"net/http"
	"time"

	"github.com/guad/bsaber99/db"
	log "github.com/sirupsen/logrus"
)

var database *db.DB

func main() {
	rand.Seed(time.Now().UTC().UnixNano())

	log.SetFormatter(&log.JSONFormatter{})

	database = db.New()

	go matchmake()
	go StartIdlekicker()

	http.HandleFunc("/ws", serveWs)

	log.Println("Started BS99 server")
	log.Fatal(http.ListenAndServe("0.0.0.0:6969", nil))
}
