
# Application Flow:

1.  Pick only firms that are included in major indices: S&P 500 and Nasdaq 100

2.  Look for the firms that are traded with high dollar volume.

    -   Instead of setting a number let us select the top 100 firms by dollar
        volume. This way the selection criteria will be self-adapting.

3.  Obtain the fundamental analysis for the selected firms.

# Data Source

## Index List

The Index list(s) will be refreshed daily. Though it does not change often I am
not aware of the calendar when it is updated and hence a daily refresh is done.

### S&P 500

Standard & Poor’s 500 or S&P 500 is a market-capitalization-weighted index of
the 500 largest U.S. publicly traded companies. The S&P does not currently
provide the total list of all 500 companies on its website, outside of the top
10.

#### Limitations of the S&P 500 Index

One of the limitations to the S&P and other indexes that are market-cap weighted
arises when stocks in the index become overvalued meaning, they rise higher than
their fundamentals warrant. If a stock has a heavy weighting in the index while
being overvalued, the stock typically inflates the overall value or price of the
index.

#### Data source for S&P 500

[Wikipedia](https://en.wikipedia.org/wiki/List_of_S%26P_500_companies). Though
it is not the most reliable source, the page is regularly maintained and usually
reliable. I use Google Sheets to extract the content from this page and provide
the required data.

### Nasdaq 100

Nasdaq 100 is a basket of the 100 largest, most actively traded U.S. companies
listed on the Nasdaq 100 stock exchange. The index includes companies from
various industries except for the financial industry, like commercial and
investment banks. These non-financial sectors include retail, biotechnology,
industrial, technology, health care, and others.

#### Criteria for Eligibility

For inclusion in the Nasdaq 100, index securities must be listed exclusively on
a Nasdaq exchange. This can include common stocks, ordinary shares, American
depositary receipt (ADR), and tracking stocks. Twenty-seven countries are tied
to companies represented in the index. Other grounds for inclusion comprises
market capitalization and liquidity. While there is no minimum requirement for
market capitalization the index itself represents the top 100 largest companies
listed on the Nasdaq.

#### Data source for Nasdaq 100

[Wikipedia](https://en.wikipedia.org/wiki/NASDAQ-100#Components). Though it is
not the most reliable source, the page is regularly maintained and usually
reliable. I use Google Sheets to extract the content from this page and provide
the required data.

## Security Price

Before I prepared this document and the previous implementation of this strategy
prices were obtained from google finance, supplemented from other free
third-party providers. Though these data feeds worked okay for
*proof-of-concept* I want to implement the system using a stream that *almost
guarantees the data it provides.* i.e. a reliable data source. Hence, I’m going
to get pricing data from T.D. Ameritrade.

### Pricing data

The application needs two types of data. Daily price, i.e. current/last trading
price and historic price. The application will use the following hierarchy to
obtain prices. The current price need not be *current* but at least within the
past 10 minutes will be acceptable.

-   TD Ameritrade: offers both current and historic prices for all securities
    that one can trade at TD Ameritrade.

-   Fin Hub: Cannot be used as a firehose but can be used to supplement missing
    data from TD Ameritrade.

-   IEXCloud: Same as above. Has limits on the number of calls/minute/month but
    can be used to supplement data.

-   The default price of \$1.00/- per security. We give this last option to
    minimize unexpected divide by zero errors.

## Security Volume

Price and volume go hand in hand and usually, the providers for Security prices
will also provide volume.

## Fundamentals Analysis

All listed firms need to file their
[10-K](https://www.investopedia.com/terms/1/10-k.asp) and 10-Q and using data
from these filings one can compute the company’s profitability, Leverage,
Liquidity, Source of Funds, and Operating Efficiency. Piotroski F-score is
designed to identify fundamentally strong stocks. One can evaluate the F-score
using the following filing numbers in 10-K filings.

| Profitability                              |                                                                                 |
|--------------------------------------------|---------------------------------------------------------------------------------|
| Return on Assets                           | Income (Loss)/Total Assets                                                      |
| Operating Cash Flow                        | Cash from Operating Activities                                                  |
| Change in Return of Assets                 | Income (Loss)/Total Assets vs previous filing period ratio.                     |
| Accruals                                   | (Cash from Operating Activities/Total Assets) – (Income (Loss)/Total Assets)    |
| Leverage, Liquidity, Source of Funds       |                                                                                 |
| Change in the Leverage ratio               | Long Term Debt/Total Assets vs previous filing period ratio.                    |
| Change in Current ratio                    | Total Current Assets/Total current Liabilities vs previous filing period ratio. |
| Change in the number of shares outstanding | Total weighted average shares outstanding vs previous filing period.            |
| Operating Efficiency                       |                                                                                 |
| Change in Gross Margin                     | Gross Profit/Revenue vs previous filing period ratio.                           |
| Change in Asset Turnover ratio             | Revenue vs previous filing period.                                              |

[Simfin](https://simfin.com) computes the Piotroski F-score for securities and
offers the results as a feed. We’ll leverage this feed to do the fundamental
analysis of the firms.

## Calculations

Using the historic prices we obtain from external sources we’ll compute the
firm’s momentum and price volatility. These calculations will be simple
mathematical calculations using standardized formulas.

## Database Tables

It will be much easier if we would have the tables/collections listed as a
table.

| Collection Name   | Use                                                                                   | Refresh Rate                                                                                                                                          |
|-------------------|---------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------|
| Index Industries  | Have a list of industries that are included in S&P 500 and Nasdaq 100                 | Will refresh every evening. Though the components are not updated regularly the application relies on volunteer contribution to update our data feed. |
| Selected Firms    | Top 100 firms by dollar volume. Historic data will be stored for two calendar months. | Every trading day.                                                                                                                                    |
| Daily Price       | Trade prices for all index securities.                                                | Every trading day.                                                                                                                                    |
| Historic Price    | Closing prices for all index industries.                                              | Every trading day.                                                                                                                                    |
| Security Analysis | Computed momentum and volatility values of all index industries.                      | Every trading day.                                                                                                                                    |
| Index values      | Tracker for Index ETFs                                                                | Every trading day.                                                                                                                                    |
|                   |                                                                                       |                                                                                                                                                       |

Let us see what each collection will hold:


| Collection name   | Remarks                                                                                                                                                                                                                                                                                                                                           |
|-------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Index Industries  | We Will have a filed to indicate if the firm was listed in S&P 500, or Nasdaq 100, or both. Not sure how it will be used but I do not lose much by holding this information.                                                                                                                                                                      |
| Selected Firms    | If possible, obtain the Piotroski score for all firms and rank the firms by Piotroski score, dollar volume. Connection to Simfin is slow and might cost a high service charge from AWS. If updates take forever then limit to top 100 dollar volume securities.                                                                                   |
| Daily Price       | Needs to be obtained after the market closes at 4:00 P.M. each trading day. Though the prices are updated continuously we should not compete for resources. The ideal time to obtain closing prices will be around 4:30 P.M. to 6:30 P.M. Note: that data needs to be fetched in batches. TDA will not allow more than 100 securities per minute. |
| Historic Price    | Obtain the historic prices on each trading day or after every trading day.                                                                                                                                                                                                                                                                        |
| Security Analysis | Computed values like momentum, efficiency ratio, and volatility. The analysis is done based on Historic prices so this table needs to be updated only when Historic prices have been updated.                                                                                                                                                     |
| Index Values      | We are not going to fetch the index values directly but obtain the tracking ETF prices. Google sheets source already has a sheet for these values.                                                                                                                                                                                                |
