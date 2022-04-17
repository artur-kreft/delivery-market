# delivery-market

The system has two types of users, Shipper and Carrier (the Shipper wants to ship things, 
and the Carrier wants to move those shipments from pickup location to destination location).

- Shipper can create new shipment requests with the following data: pickup address, 
  destination address, budget amount, additional information (stored as text).
- A carrier can book a shipment, agreeing on a price set by the shipper.
- A carrier can create an offer for a shipment with a different price.
- A shipper can reject or approve offers made by carriers.
- A shipment can be booked only once.

# project requirements

This project does not need any external system to be used.
If you run it as your own service you need to implement interfaces:
- IRepository for database engine
- INotification for notification engine