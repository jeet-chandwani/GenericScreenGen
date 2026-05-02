# This file has reqs for adding datastore backend management to the solution

**Req 8.1** We need to create a backend data store to persist the data created/edited/deleted via the screens.

**Req 8.2** We could have a data store - represented by an interface say IDataStore. CDbDataStore and CJsonDataStore will be 2 implementation as defined below (for now to start - we will add more data store implementations later) 

**Req 8.3** Each data store info will have an unique id/name and have the config info in datastore.{id-name}.config.json file

**Req 8.4** Not every screen will need the data to be persisted or be associated with a data store details 

**Req 8.5** The association of screen id to datastore item will be saved in separate mapping registry - lets call it screen-datastore-mapping.{datastore id-name}.json. This 1 file per data store will have mappings for all screens desired. Use IDs instead of names wherever possible.

NOTE -- More reqs to come, but for now, I want to start with these and get the basic structure in place. We can then add more reqs for additional features like CRUD operations, querying, etc.