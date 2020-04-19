package main

import (
	"net"
	"net/http"
	"strings"
	"time"
)

func removePort(ip net.Addr) string {
	switch addr := ip.(type) {
	case *net.TCPAddr:
		return addr.IP.String()
	}

	return ip.String()
}

func getClientIP(r *http.Request) string {
	if r.Header.Get("X-Forwarded-For") != "" {
		s := strings.Split(r.Header.Get("X-Forwarded-For"), " ")
		return s[len(s)-1]
	}

	if r.Header.Get("X-Real-Ip") != "" {
		return r.Header.Get("X-Real-Ip")
	}

	return ""
}

func getUnixTimestampMilliseconds() int64 {
	return time.Now().UnixNano() / 1000000
}
