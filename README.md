# UCLA-Monopoly

## Client - Server Communication
Client-Server communication is done by sending JSON objects over a WebSocket connection. The specifications for the JSON objects are as follows.

### Numbered Requests
All requests come with a corresponding number to uniquely identify the request. This way, if multiple requests are made, the server can choose to respond in any arbitrary order.

Client requests and Server responses therefore will always have a `request_id` field as shown below.

```javascript
{
  // Content related to specific request
  // ...
  request_id: <unique_id_number>
}
```

### Client Requests to Server
The following specifies the requests a client can make to a server, and their syntactic form.

#### Joining a Game
To join a live game, the client will send a message specifying that he/she wishes to join the game.
```javascript
{
  request: "JOIN_GAME",
  request_id: <unique_id_number>
}
```
The server will respond with success or failure.

#### Location Info Requests
Whenever dealing with data about location (requesting data about other players, mapping GPS coordinates to current location), the client needs to specify the type of request and the latitude, longitude coordinates of the location the client is attempting to identify.
```javascript
{
  request: "LOC_INFO",
  location: "<latitude>,<longitude>",
  request_id: <unique_id_number>
}
```
The server will respond in a manner specified in the next major section on success; responds with failure otherwise.

#### Active Players Request
The client may want to know who is currently in the game and metadata regarding them (names, bank account, locations owned, etc). To do this, the following request is made.
```javascript
{
  request: "PLAYER_INFO"
  request_id: <unique_id_number>
}
```
The server will respond in a manner specified in the next major section; responds with failure otherwise.

#### Making Purchases
The client may want to make purchases in game (land, other perks, etc.). The client will then send the following request to the server.
```javascript
{
  request: "PURCHASE",
  purchase_code: <integer_code>,
  tier: <ith_tier>, // Tiers described below
  request_id: <unique_id_number>
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
  status: 200,
  request_id: <unique_id_number>,
  // Extra data...
}

// Specifies Failure
{
  status: 400,
  request_id: <unique_id_number>,
  // Extra data...
}
```

#### Location Data
Whenever responding about locations, each location is described with the following attributes.

```javascript
{
  status: 200,
  request_id: <unique_id_number>,
  name: <unique name identifier>,
  northeast: <northeast_gps_coordinates>,
  southwest: <southwest_gps_coordinates>,
  purchase_code: <integer_code>,
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
  request_id: <unique_id_number>,
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
