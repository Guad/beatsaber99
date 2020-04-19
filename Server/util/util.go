package util

import (
	"crypto/rand"
	"math/big"
	"net"
	"net/http"
	"strings"
	"time"
)

func RemovePort(ip net.Addr) string {
	switch addr := ip.(type) {
	case *net.TCPAddr:
		return addr.IP.String()
	}

	return ip.String()
}

func GetClientIP(r *http.Request) string {
	if r.Header.Get("X-Forwarded-For") != "" {
		s := strings.Split(r.Header.Get("X-Forwarded-For"), " ")
		return s[len(s)-1]
	}

	if r.Header.Get("X-Real-Ip") != "" {
		return r.Header.Get("X-Real-Ip")
	}

	return ""
}

func GetUnixTimestampMilliseconds() int64 {
	return time.Now().UnixNano() / 1000000
}

func CryptoIntn(n int) int {
	max := big.NewInt(int64(n))
	result, _ := rand.Int(rand.Reader, max)

	return int(result.Int64())
}

func CryptoFloat64() float64 {
	result := CryptoIntn(0x7FFFFFFF)

	return float64(result) / float64(0x7FFFFFFF)
}
