package main

import (
	"math/rand"
	"net/http"
	"sync"
	"time"

	"github.com/guad/bsaber99/db"
	log "github.com/sirupsen/logrus"
)

var database *db.DB
var serverStartup *sync.WaitGroup

func main() {
	rand.Seed(time.Now().UTC().UnixNano())
	log.SetFormatter(&log.JSONFormatter{})

	serverStartup = &sync.WaitGroup{}
	serverStartup.Add(2)

	database = db.New()

	go matchmake()
	go StartIdlekicker()
	go connectionLogger()

	http.HandleFunc("/ws", serveWs)

	serverStartup.Wait()
	log.Println("Started BS99 server")
	log.Fatal(http.ListenAndServe("0.0.0.0:6969", nil))
}
