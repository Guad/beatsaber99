FROM golang:alpine AS build-env

# Install certs
RUN apk add --no-cache git
RUN apk add --no-cache ca-certificates
ENV CGO_ENABLED=0

WORKDIR /app

ADD go.mod go.mod
ADD go.sum go.sum

RUN go mod download

ADD . .


RUN go build -o app

FROM alpine

# Import certificates
COPY --from=build-env /etc/ssl/certs/ca-certificates.crt /etc/ssl/certs

WORKDIR /app
COPY --from=build-env /app/app /app/
ENTRYPOINT ./app
