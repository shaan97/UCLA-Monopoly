# UCLA-Monopoly

## Client - Server Communication
Client-Server communication is done by sending JSON objects over a WebSocket connection. The specifications for the JSON objects are as follows.

### Client Requests to Server
The following specifies the requests a client can make to a server, and their syntactic form.

#### Creating a Game
To create a game, the client will send a message specifying that he/she wishes to create a game with a specified name/identifier.
```javascript
{
  request: "CREATE_GAME",
  game_id: <GAME_ID>      // This is a string
}
```
The server will respond with success or failure.

#### Joining a Game
To join a live game, the client will send a message specifying that he/she wishes to join a game with a specified name/identifier.
```javascript
{
  request: "JOIN_GAME",
  game_id: <GAME_ID>      // This is a string
}
```
The server will respond with success or failure.

#### Location Info Requests
Whenever dealing with data about location (requesting data about other players, mapping GPS coordinates to current location), the client needs to specify the type of request and the latitude, longitude coordinates of the location the client is attempting to identify.
```javascript
{
  request: "LOC_INFO"
  location: "<latitude>,<longitude>"
}
```
The server will respond in a manner specified in the next major section on success; responds with failure otherwise.

#### Active Players Request
The client may want to know who is currently in the game and metadata regarding them (names, bank account, locations owned, etc). To do this, the following request is made.
```javascript
{
  request: "PLAYER_INFO"
}
```
The server will respond in a manner specified in the next major section; responds with failure otherwise.

#### Making Purchases
The client may want to make purchases in game (land, other perks, etc.). The client will then send the following request to the server.
```javascript
{
  request: "PURCHASE",
  purchase_code: <integer_code>,
  tier: <ith_tier> // Tiers described below
}
```
The server will respond with success or failure and will adjust its internal state if necessary to reflect the purchase.

### Server Responses to Client
The following specifies how a server will respond to a client, and their a syntactic form.

#### Success and Failure
All responses must specify whether there is success or failure. All invalid requests will result in failure.

Success/Failure takes the form of the HTTP Status Codes (see https://en.wikipedia.org/wiki/List_of_HTTP_status_codes). The following gives example, and likely typical, status codes.

```javascript
// Specifies Success
{
  status: 200
  // Extra data...
}

// Specifies Failure
{
  status: 400
  // Extra data...
}
```

#### Location Data
Whenever responding about locations, each location is described with the following attributes.

```javascript
{
  status: 200,
  name: <unique name identifier>,
  northeast: <northeast_gps_coordinates>
  southwest: <southwest_gps_coordinates>
  purchase_code: <integer_code>
  prices: [
            // Array where i^th entry is the i^th tier
          ],
  taxes:  [
            // Array where i^th entry is the tax for the i^th tier
          ]
}
```
Note that the `northeast` and `southwest` GPS coordinates exactly define a rectangular region.

#### Player Data
When responding about players, the following is expected.

```javascript
// Overall response
{
  status: 200,
  players: [
              // Array of players
           ]
}

// Player objects
{
  name: <player_name>,
  credits: <Number of Credits in Bank>
  locations:  [
                // Array of locations owned by player
              ]
  tiers:      [
                // Array of corresponding tiers owned by player
              ]
}
```
