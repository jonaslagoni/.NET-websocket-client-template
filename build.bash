#!/bin/bash
ag --templates ./ --output ./dist --param server="localhost" ../blackhawk-websocket-core/ws.yml template