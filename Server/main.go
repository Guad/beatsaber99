package main

import (
	"math/rand"
	"net/http"
	"time"

	log "github.com/sirupsen/logrus"
)

func main() {
	rand.Seed(time.Now().UTC().UnixNano())

	log.SetFormatter(&log.JSONFormatter{})

	go matchmake()
	go StartIdlekicker()

	http.HandleFunc("/ws", serveWs)

	log.Println("Started BS99 server")
	log.Fatal(http.ListenAndServe("0.0.0.0:6969", nil))
}
