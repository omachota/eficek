# Developer documentation

#### Project structure

The project is divided into 4 parts. `Eficek`, `Eficek.Database`, `Eficek.Gtfs` and `Eficek.Tests`.

##### Eficek

This part contains all of the API controllers and the structures they return, most of the services and real-time delay data handling.

##### Eficek.Database

Contains structures for the AdminController, database entities and a class for calculating password hashes. Currently SHA256 is used to calculate the hashes.

##### Eficek.Gtfs

This library contains structures needed by gtfs. There is also `NetworkBuilder.cs` that loads data and builds search graph, that is stored into `Network` class. Note that certain parts are not yet implemented - e. g. trip exceptions.

##### Eficek.Tests

There are a few tests. Contributions welcomed.

#### UTM coordinates

The Coordinates of all stops are projected into UTM coordinates. UTM coordinates allow to split an area into smaller boxes of fixed size. These boxes are then used to efficiently search for nearby stops, especially when creating pedestrian connections.

#### Computation of the search graph

At first, data is loaded into memory and moved for easier access to defined structures. Stops are assigned appropriate UTM coordinates and then stopgroups are generated from stops.

`BuildStopTimesGraph` generates nodes of the search graph and connects trips with edges. After that nodes are divided by stopId, sorted by time and connected by edges to allow waiting at stops.

Finally, pedestrian connections are calculated and corresponding edges are added to the search graph.

If you would like to read more about the problem,please visit the [KSP](https://ksp.mff.cuni.cz/h/ulohy/32/zadani3.html#task-32-3-6) website.

#### Search

The connection search uses the Dijkstra shortest path algorithm. In my opinion, the algorithm is not fast enough to serve 5 or more connections per search. Concurrency may be necessary to speed it up. The search returns nodes and edges present on the shortest path. From these nodes and edges, connection structure is computed are returned.

#### Updates to the Network

Updating the `Network` is done using immutability. There is a `NetworkSingletonService` service in the core that is responsible for updating the network. Concurrent updates are strictly forbidden. When the update is almost finished, `NetworkSingletonService` atomically changes the reference of the `Network` to the newly built network.

`NetworkService` is set to be scoped. Since then, each request will get a new instance `NetworkService`.

#### Administrator panel

User credentials are stored in the database. Passwords are not stored, only hashes with salt. The project uses local `sqlite` database, which is created if missing. Default administration credentials are `admin:admin`.