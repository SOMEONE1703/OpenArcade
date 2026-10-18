# Sample games

Two tiny self-hosted games for local development. `server/docker-compose.yml` serves this folder on
http://localhost:8090, a different origin from the web app, so they embed the same way a publisher's game would.
The API's development seed lists them as `snake` and `reaction`.

`.well-known/openarcade.txt` holds the seed games' domain verification token (`dev`), so the monthly
re-verification job keeps them listed. To test the publisher flow with these samples, add your game's token to
that file on its own line.
