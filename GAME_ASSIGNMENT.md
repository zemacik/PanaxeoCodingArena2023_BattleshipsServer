# [Coding Arena — Boost your coding skill and win cool prizes — Panaxeo — Crazy good software teams](https://www.panaxeo.com/coding-arena)

## Competition •

#### Code a bot. Sink ships. Win prizes.
Timer runs out on December 15.

#### **tl;dr:**

-   Your task is to code the most efficient Battleships bot in whatever language you love. [Explore the rules](https://www.panaxeo.com/coding-arena/#rules).

-   You solve various maps over [an API](https://www.panaxeo.com/coding-arena/#api).

-   The player whose bot achieved the best score before 23:59:59, Dec 15, ‘23 wins cool prizes.

-   There are three categories with one winner in each. [Login to play.](https://www.panaxeo.com/coding-arena/#login)

-   [Join us on Discord](https://discord.gg/zN5X3Ejrym) and chat with fellow contestants.

-   Still not sure? [Read our FAQs.](https://www.panaxeo.com/coding-arena/#faq)


### **Welcome to the Coding Arena**

The Coding Arena is a [Panaxeo](https://panaxeo.com/) tradition. Every year, we measure our egos coding skills. For the second time in forever, it’s open to the public! **Y’all are very welcome to enter!**

This year is all about Battleships. Your goal is to win 200 matches and destroy all enemy ships by firing at coordinates in a square grid. Unfamiliar with the basic rules of this game? Read on.

**Login below, get your API token and start playing Battleships with us.**

### **How to play?**

Sink all ships to finish a map. There are 200 maps in a single game. Your bot can play up to 20 games overall.

The bot starts a map by firing at coordinates. The map is a 12x12 grid. There are 6 ships to sink. Once you sink them all, the map ends.

Your goal is to use the least amount of turns to finish all maps in a game. This is your score. The lower your score, the higher you rise in the leaderboard.

[Explore the API ↓](https://www.panaxeo.com/coding-arena#api)

**The fleet to sink:**Avengers Helicarrier (9 spaces), Carrier (5 spaces), Battleship (4), Destroyer (3), Submarine (3), and Patrol Boat (2).

#### There’s a special twist, though!

-   An Avengers Helicarrier™ has been stolen! (Along with those other ships.) By destroying it, you’ll release the captured Avengers. One of them can help you with their unique abilities.

-   Thor can reveal 11 spaces at once with his lightning. The one you’re firing at + 10 random spaces. All in a single turn.

-   Hulk can sink a whole ship at once. If you fire at a space with an actual ship and ask Hulk for help, he’ll destroy the entire ship.

-   Iron Man will use his scanners to reveal the smallest undiscovered ship.

-   You can use an Avenger ability in the next turn immediately after destroying the Helicarrier. Only one Avenger is available per match, only once.

    [Explore the API calls to learn more.](https://www.panaxeo.com/coding-arena#api)


#### Map rules

-   Ships can only be placed vertically or horizontally.

-   Diagonal placement is not allowed.

-   No part of a ship may hang off the edge of the map.

-   Ships may not overlap each other.

-   No ship can touch another ship, not even diagonally.

-   Once the guessing begins, the players may not move the ships.

## The Battleship API

### Endpoints

---

#### Getting the status of an ongoing game 📋

`GET` `/fire`

##### Path parameters

> None

##### Query parameters

> | name | allowed value | required | description |
> | --- | --- | --- | --- |
> | test | `yes` | false | Query will be performed on test data for simulation, no change to real game data, user score / number of tries etc. |

> | name | type | data type | description |
> | --- | --- | --- | --- |
> | Authorization | Bearer token | string | Authorization token copied from the UI |

##### Responses

> | http code | content-type | response |
> | --- | --- | --- |
> | `200` | `application/json; charset=utf-8` | [FireResponse](https://www.panaxeo.com/#fireresponse) |
> | `400` | `application/json; charset=utf-8` | `{"error": "Max tries already reached"}` |
> | `400` | `application/json; charset=utf-8` | `{"error": "Invalid query parameter or query parameter value supplied"}` |
> | `403` | `application/json; charset=utf-8` | `{"error": "Unauthorized"}` |

##### Example cURL

> ```
>  curl --request GET --url https://europe-west1-ca-2023-dev.cloudfunctions.net/battleshipsApi/fire --header 'Authorization: Bearer $token'
> ```

---

#### Firing at specified position 🎯

`GET` `/fire/**:row**/**:column**`

##### Path parameters

> | name | data type | required | description |
> | --- | --- | --- | --- |
> | row | integer | true | vertical position on the board from range \[0,11\] |
> | column | integer | true | horizontal position on the board from range \[0,11\] |

##### Query parameters

> | name | allowed value | required | description |
> | --- | --- | --- | --- |
> | test | `yes` | false | Query will be performed on test data for simulation, no change to real game data, user score / number of tries etc. |

> | name | type | data type | description |
> | --- | --- | --- | --- |
> | Authorization | Bearer token | string | Authorization token copied from the UI |

##### Responses

> | http code | content-type | response |
> | --- | --- | --- |
> | `200` | `application/json; charset=utf-8` | [FireResponse](https://www.panaxeo.com/#fireresponse) |
> | `400` | `application/json; charset=utf-8` | `{"error": "Max tries already reached"}` |
> | `400` | `application/json; charset=utf-8` | `{"error": "Invalid values for row or column"}` |
> | `400` | `application/json; charset=utf-8` | `{"error": "Invalid query parameter or query parameter value supplied"}` |
> | `403` | `application/json; charset=utf-8` | `{"error": "Unauthorized"}` |

##### Example cURL

> ```
>  curl --request GET --url https://europe-west1-ca-2023-dev.cloudfunctions.net/battleshipsApi/fire/:row/:column --header 'Authorization: Bearer $token'
> ```

---

#### Firing at specified position with help of avenger 🦸‍♂

`GET` `/fire/**:row**/**:column**/avenger/**:avenger**`

##### Path parameters

> | name | data type | required | description |
> | --- | --- | --- | --- |
> | row | integer | true | vertical position on the board from range \[0,11\] |
> | column | integer | true | horizontal position on the board from range \[0,11\] |
> | avenger | string | true | use the power of specified avenger (hulk, ironman or thor) |

##### Avenger abilities explained

-   *thor* ability will hit 10 random map points at maximum (at maximum = if there are fewer untouched map points available than 10, all of them will be targeted by this ability)
-   *ironman* ability will return 1 map point of the smallest non-destroyed ship, this map point will be unaffected (the purpose of this ability is to give a hint to the user)
-   *hulk* ability will destroy the whole ship if the map point specified by the row/column combination at the api endpoint hits the ship (all the map points belonging to this ship will be marked as destroyed)

##### Query parameters

> | name | allowed value | required | description |
> | --- | --- | --- | --- |
> | test | `yes` | false | Query will be performed on test data for simulation, no change to real game data, user score / number of tries etc. |

> | name | type | data type | description |
> | --- | --- | --- | --- |
> | Authorization | Bearer token | string | Authorization token copied from the UI |

##### Responses

> | http code | content-type | response |
> | --- | --- | --- |
> | `200` | `application/json; charset=utf-8` | [AvengerFireResponse](https://www.panaxeo.com/#avengerfireresponse) |
> | `400` | `application/json; charset=utf-8` | `{"error": "Max tries already reached"}` |
> | `400` | `application/json; charset=utf-8` | `{"error": "Invalid values for row or column"}` |
> | `400` | `application/json; charset=utf-8` | `{"error": "Invalid value for avenger"}` |
> | `400` | `application/json; charset=utf-8` | `{"error": "Avenger unavailable"}` |
> | `400` | `application/json; charset=utf-8` | `{"error": "Invalid query parameter or query parameter value supplied"}` |
> | `403` | `application/json; charset=utf-8` | `{"error": "Unauthorized"}` |

##### Example cURL

> ```
>  curl --request GET --url https://europe-west1-ca-2023-dev.cloudfunctions.net/battleshipsApi/fire/:row/:column/avenger/:avenger --header 'Authorization: Bearer $token'
> ```

---

#### Reset ongoing game ↻

*Calling this endpoint results in resetting the ongoing game, but your attempt will be counted as one full game without saving the score.*


`GET` `/reset`

##### Path parameters

> None

##### Query parameters

> | name | allowed value | required | description |
> | --- | --- | --- | --- |
> | test | `yes` | false | Query will be performed on test data for simulation, no change to real game data, user score / number of tries etc. |

> | name | type | data type | description |
> | --- | --- | --- | --- |
> | Authorization | Bearer token | string | Authorization token copied from the UI |

##### Responses

> | http code | content-type | response |
> | --- | --- | --- |
> | `200` | `application/json; charset=utf-8` | `{"availableTries": number }` |
> | `400` | `application/json; charset=utf-8` | `{"error": "Max tries already reached"}` |
> | `400` | `application/json; charset=utf-8` | `{"error": "No ongoing game found"}` |
> | `400` | `application/json; charset=utf-8` | `{"error": "Invalid query parameter or query parameter value supplied"}` |
> | `403` | `application/json; charset=utf-8` | `{"error": "Unauthorized"}` |

##### Example cURL

> ```
>  curl --request GET --url https://europe-west1-ca-2023-dev.cloudfunctions.net/battleshipsApi/reset --header 'Authorization: Bearer $token'
> ```

### Response models

---

#### FireResponse

Model details

> | name | data type | description |
> | --- | --- | --- |
> | grid | string | 144 chars (12x12 grid) representing updated state of map, '\*' is unknown, 'X' is ship, '.' is water. |
> | cell | string | Result after firing at given position ('.' or 'X'). This field may be empty ('') if player calls /fire endpoint or tries to fire at already revealed position. |
> | result | boolean | Denotes if fire action was valid. E.g. if player calls /fire endpoint or fire at already revealed position, this field will be false. |
> | avengerAvailable | boolean | Avenger availability after the player's move. |
> | mapId | integer | ID of the map, on which was called last player's move. This value will change when player beats current map. |
> | mapCount | integer | Fixed number of maps which are required to complete before completing one full game. |
> | moveCount | integer | Number of valid moves which were made on the current map. Invalid moves such as firing at the same position multiple times are not included. |
> | finished | boolean | Denotes if player successfully finished currently ongoing game => if player completed *mapCount* maps. Valid move after getting *true* in this field results in new game (or error if player has already achieved max number of tries). |

##### Example

```
{
    "grid": "*...**********************XX.**********************************.***********.***********.***********************************************.********",
    "cell": "X",
    "result": true,
    "avengerAvailable": false,
    "mapId": 0,
    "mapCount": 2,
    "moveCount": 10,
    "finished": false
}
```

---

#### AvengerFireResponse

Model details

> AvengerFireResponse includes all the fields from [FireResponse](https://www.panaxeo.com/#fireresponse) and adds the following
>
> | name | data type | description |
> | --- | --- | --- |
> | avengerResult | {mapPoint: {x: integer, y: integer }, hit: boolean}\[\] | *mapPoint*'s values *x (row)* and *y (column)* denote coordinates which were affected by avenger ability. Value of *hit* denotes, if coordinates specified by *mapPoint* have hit a ship. |

##### Example

```
{
    "grid": "*...**********************XX.****..****************************.***********.***********.***********************************************.********",
    "cell": ".",
    "result": true,
    "avengerAvailable": false,
    "mapId": 0,
    "mapCount": 2,
    "moveCount": 12,
    "finished": false,
    "avengerResult": [
        {
            "mapPoint": {
                "x": 9,
                "y": 2
            },
            "hit": false
        }
    ]
}
```

### **Frequently asked
questions**

-   We want a level playing field for students and juniors. that’s why there are categories – juniors and seniors. There will be a winner in each of these categories.

    Not sure which one to choose from? Here’s a rule of thumb:

    – With less than 2 years of professional coding experience, pick the junior category.

    – With more than 2 years of coding experience, pick the senior category.

-   The test API is for functionality and communication tests. It simulates a real game but the number of requests is limited to 200 per day.

    It's there for you to try the API without committing to a real game.

-   Coding arena started out as a Panaxeans-only competition, so it’s a matter of legacy. Plus, only Panaxeans can win eternal glory in the Hall of Fame. [Wanna become a Panaxean?](https://www.panaxeo.com/jobs)

-   You can enter the Coding Arena at any point up to 15 December 2023. Time of entry doesn’t matter; your score does.
    