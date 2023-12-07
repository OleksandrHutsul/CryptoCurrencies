# CryptoCurrencies
The task that I have implemented:
I had to create a program that displays various information related to cryptocurrencies. This involves using one (or several) open APIs:
• CoinCap: https://docs.coincap.io/ (information about several APIs is available here)
The use of ready-made libraries for working with APIs, as well as libraries for working with HTTP (except for the standard HttpClient), is prohibited. 
The use of template mechanisms to create a project is also prohibited. Libraries for drawing diagrams, working with JSON, Inversion of Control, and MVVM are allowed.
The program should support the following functions:
• It should be a multi-page program with navigation.
• The main page displays the first N currencies by popularity on a certain market (or the top 10 currencies returned by the API).
• A page with the ability to view detailed information about a currency: price, volume, price change, on which markets it can be purchased and at what price 
(the ability to navigate to the currency page on the market is a plus).
• The ability to search for a currency by name or code.

Any design of the application interface is allowed, but it should be aesthetic. Rational use of templates is encouraged. The architecture and code of the program should be as clean as possible. 
It would be a plus if the program implemented additional features (the more, the better):
• Displaying currency quote charts (Japanese candlestick chart or another).
• A page where you can convert one currency to another (ignoring the method and possible commission).
• Light/dark theme support.
• Support for multiple localizations.

In this project, I used the following APIs:
https://api.coincap.io/v2/rates
https://api.coincap.io/v2/assets

I also had to use https://api.coincap.io/v2/candles, but for some reason, when I followed the link, there was simply no data. So, I tried to use the Github API, 
but I received a 403 error and couldn't resolve it. Therefore, for the candlestick chart, I used a JSON file in the project.

The project supports three languages (Ukrainian, French, and English) and two themes (light and dark). I tried to use the MVVM pattern, but I encountered problems with the 
ViewModels and couldn't implement it. However, in the future, I will definitely fix this project and upload an updated version so that everyone interested can learn from it 
and create something of their own.
