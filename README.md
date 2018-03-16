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

#### Location Info Requests
Whenever dealing with data about location (requesting data about other players, mapping GPS coordinates to current location), the client needs to specify the type of request and the latitude, longitude coordinates of the location the client is attempting to identify.
```javascript
{
  operation: "LOC_INFO",
  parameters: {
                name: <name>,
                lat: <lat>,
                long: <long>
              }
  request_id: <unique_id_number>
}
```
The server will respond in a manner specified in the next major section on success; responds with failure otherwise.

#### Active Players Request (NOT FOR SUBMISSION)
The client may want to know who is currently in the game and metadata regarding them (names, bank account, locations owned, etc). To do this, the following request is made.
```javascript
{
  operation: "PLAYER_INFO"
  request_id: <unique_id_number>
}
```
The server will respond in a manner specified in the next major section; responds with failure otherwise.

#### Making Purchases
The client may want to make purchases in game (land, other perks, etc.). The client will then send the following request to the server.
```javascript
{
  operation: "PURCHASE",
  purchase_code: <integer_code>,
  tier: <ith_tier>, // Tiers described below
  how_long: <hours_elapsed_time>,
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
  success: true,
  request_id: <unique_id_number>,
  // Extra data...
}

// Specifies Failure
{
  success: false,
  request_id: <unique_id_number>,
  reason: <reason_for_failure>,
  
  /* If reason == "balance" */
  player_balance: <client_balance>,
  amount_required: <amount_required>,
  
  /* If reason == "owned" */
  location_owner: <location_owner>,
  location_owned_until: <unix_timestamp>,
  
  /* If reason == "bad_param" */
  param_name: <first_bad_param>,
  explanation: <missing_or_malformed>,
  
  /* Could also have reason == "loc_dne" */
  
  // Extra data...
}
```

#### Location Data
Whenever responding about locations, each location is described with the following attributes.

```javascript
{
  success: true,
  request_id: <unique_id_number>,
  name: <unique name identifier>,
  ne_lat: <northeast_gps_latitude>,
  ne_long: <northeast_gps_longitude>,
  sw_lat: <southwest_gps_latitude>,
  sw_long: <southwest_gps_longitude>,
  owner: <owner>,
  owned_until: <unix_timestamp>,
  tier: <tier_owned>,
  price: <location_purchase_price>,
  tax: <location_tax>,
}
```
Note that the `northeast` and `southwest` GPS coordinates exactly define a rectangular region.

#### Player Data (NOT FOR SUBMISSION)
When responding about players, the following is expected.

```javascript
// Overall response
{
  success: true
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

#### Another Player Makes a Purchase
```
  {
      operation: "purchase_made",
      location: <location_name>,
      owner: <owner_name>,
      ownedUntil: <unix_timestamp>,
  }
```
### HTTP Communication

#### Joining a Game
To join a live game, the client will send a message specifying that he/she wishes to join the game.

```POST /players/addplayer/?player_name=<player_name>```

The server will respond with an HTTP Status Code.

#### Taxation
Note: `player_name` is the name of the client, not the owner of the location.

```PUT /locations/visited/?player_name=<player_name>&location_name=<location_name>```

This will return a HTTP Status Code.

###$ All Locations

```GET /locations/alllocations/```

This will return a JSON list of all locations in the same format as the location data

