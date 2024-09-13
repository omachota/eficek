# User documentation 

Please note that travelling according to the searched connection is at your own risk. The API allows you to search for:

###### Routing

- `Search`: connections between two stops
- `SearchVia`: connections between two stops with a stop between
- `Coverage`: stops, that you can reach within a given interval

###### Stops

- `GetNearby`: nearby stops that are less than 700 metres away from a given location
- `Search`: stopgroup[^1] id, which is used for searching a connection
- `Departures`: departures from a given stopgroup

To help you get started with the API, an online tool is available at address: `API_url/swagger/index.html`. All of the endpoints described above are there. All you need to do is click on the endpoint you want, fill in the required field and click `Execute`. You will also find there structures, that the API returns. 

#### Routing

##### Search

Currently, the API optimises for the earliest arrival at a destination. If more connection reach the destination at the same moment, the connection with the lowest travelled distance is returned. To help you with travelling, if a delay is information known, it is shown in the result.

##### SearchVia

Works nearly the same way as `Search`. The only difference is that the search from `Via` stopgroup is performed from the stop where you have arrived at. 

##### Coverage

The coverage endpoint helps you to find stopgroups, that are reachable within a given interval from a stopgroup. Returned stopgroups are order by duration. 

#### Stops

##### GetNearby

Finds all of the stopgroups that are less than 700 metres away from the given coordinates.

##### Search

Allows you to find stopgroup ids. Returns all the stopgroups, that contain the searched text.

##### Departures

Displays all of the departures from a stopgroup. The departures are sorted by time and the number of departures is limited to 100 at the moment.

#### Self hosting

This project requires `dotnet-8-sdk` and a rust compiler. From the libraries, you will need the latest `libproj`, `sqlite3` available on your machine. To build the project, simply run the `dotnet build --release` command inside `Eficek` directory. If you have a problem with building the rust library, go into the `utm_convert` directory and execute `cargo build --release `. `Cargo` should download and update the necessary packages and than build `utm_convert` library. Note that this may take some time, especially on slower machines. Than get back to the `Eficek` directory and run `dotnet build--release`.

To run the project, execute `dotnet run --release`. When the API starts, it will download necessary data and build a search graph. Once this is done, you can start communicating with the API.

[^1]: Stopgroup is a group of individual stops.

#### Administrator panel

The administrator part of the API allows you to update the search graph while the API is running. As an administrator, you can create users (called Network Managers) that will have a right to update the network. With the administrator account, you are also allowed to modify and delete the users.
