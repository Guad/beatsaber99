package main

import "net"

func removePort(ip net.Addr) string {
	switch addr := ip.(type) {
	case *net.TCPAddr:
		return addr.IP.String()
	}

	return ip.String()
}
