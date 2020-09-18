# API Calls

In the previous implementation, I had built multiple independent batch
processes. One of the reasons why I had multiple batch process was because the
application evolved, and requirements also evolved. You might call it
requirements creep, but I would say I did not know how the product would
function and was not sure from where I'll be getting the data.

This resulted in redundant code and work. I was not using some of the results
from one process in the next process - you know the mess that happens in
evolving applications.

This implementation is going to store all its results in the database and each
table/Collection is going to be added/deleted/updated by one process only. Other
processes will read data that is being populated by individual processes.

## Index Components

This will be accessible from endpoint /api/IndexComponents. The following
methods will be available:

| Get                | Get all index elements from all tracked indexes. Currently only S&P 500 & Nasdaq 100                                 |
|--------------------|----------------------------------------------------------------------------------------------------------------------|
| Get?Index=[S,N,O]  | Will return only firms from: S: S&P 500 Industries. N: Nasdaq 100 O: Overlapping/Both indexes                        |
| Create?Index=[S,N] | Will remove all index components that are not loaded today and add firms from the list of firms that is on the list. |
| Delete             | Will not be implemented to start with.                                                                               |
| Update             | Will not be implemented.                                                                                             |

An update will not be a direct implementation. However, when Create(insert) is
called it will do an update if needed.
