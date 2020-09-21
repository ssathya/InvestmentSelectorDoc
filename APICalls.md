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

| Operation          | Remarks                                                                                                              |
|--------------------|----------------------------------------------------------------------------------------------------------------------|
| Get                | Get all index elements from all tracked indexes. Currently only S&P 500 & NASDAQ 100                                 |
| Get?Index=[S,N,O]  | Will return only firms from S: S&P 500 Industries. N: NASDAQ 100 O: Overlapping/Both indexes                         |
| Create?Index=[S,N] | Will remove all index components that are not loaded today and add firms from the list of firms that is on the list. |
| Delete             | Will not be implemented to start with.                                                                               |
| Update             | Will not be implemented.                                                                                             |

An update will not be a direct implementation. However, when Create(insert) is
called it will do an update if needed.

## Daily Price

This will be accessible from the endpoint /api/DailyPrice. The following methods
will be available:

| Operation                         | Remarks                                                                                                                    |
|-----------------------------------|----------------------------------------------------------------------------------------------------------------------------|
| Get                               | Get the latest prices for all securities that are included in the indexes.                                                 |
| Get?Symbol=[SYMBOL(s)]            | Return the latest price for the symbol(s) that is passed as a parameter.                                                   |
| Create?Security=\{Security details\} | Will create a new record for the security that is being called with. Will also remove any other record(s) for this symbol. |
| Delete/Symbol                | Delete the record for the Security.                                                                                        |
| Update                            | Will not be implemented.                                                                                                   |

## Historic Price

This will be accessible from the endpoint /api/HistoricPrice. 
The following methods will be available:

| Operation                         | Remarks                                                                                                                    |
|-----------------------------------|----------------------------------------------------------------------------------------------------------------------------|
|Get|Not available; no point dumping the entire historic price in one go|
|Get?Symbol= \{Symbol\} | Returns the last obtained historic price for a given symbol. Maximum delay will be 24 hours data|
|Create?Symbol=\{Symbol\} | Will delete existing records for the provided symbol and create a new record with body text|
|Delete/Symbol| Will delete any records for the given symbol and delete all recodes before the work day.|
|Update|Will not be implemented.|

## Security Analysis

This will be accessible from the endpoint /api/SecurityAnalysis.
The following methods will be available:

| Operation                         | Remarks                                                                                                                    |
|-|-|
|Get|Not available; no point dumping the entire collection in one go.|
|Get?Symbol= \{Symbol\} | Returns the last analysis for a given symbol.|
|Create?Symbol=\{Symbol\} | Will delete existing record if any for the provided symbol and create a new record with the body value(s)|
|Delete/Symbol| Will delete any records for the given symbol and delete all recodes before a yet to be defined time frame.|
|Update| Will not be implemented.|

## Index Values

These values can be accessed from the endpoint /api/indexValues.
The following methods will be available:

| Operation                         | Remarks                                                                                                                    |
|-|-|
|Get|Return ETF values and calculated Index values of all tracking index ETFS|
|Get?Symbol=\{Symbol\}|Return ETF values and calculated Index values of specific tracking index ETF|
|Create?Symbol=\{Symbol\}|Delete any existing record for the specific ETF and creates a new one.|
|Delete/Symbol|Will delete any records for the given symbol and delete all recodes before the work day.|
|Update| Will not be implemented.|

There is a reason why I'm planning to handle all creates as Updates and not going to implement updates.
Anytime we are going to update the value we are actually creating a new record that is effective for that
particular time. So I think there should be no updates and only creates.










