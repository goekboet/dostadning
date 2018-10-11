# Dostadning

Give dostadning permissions to upload a batch of auctions including images via [tradera SOAP-api](https://api.tradera.com) to your seller-account there. 

While it does solve a concrete problem the main purpose was to explore mixing f# with c# code. The conclusion was that it is nice to write but managing dependencies when developing on OSX is awkward. The main reason for that is because f# needs mono. Got many cannot load this and that because reasons. Never got fsharpi to work with the entity framework code which was one of the things I was looking forward to. Maybe better experience on windows.

## Getting Started

1. Create an account with [tradera developer center](https://api.tradera.com)
2. Export your credentials to environment variables:
```
export dostadning_tradera_appid=<Your application ID>
export dostadning_tradera_appkey=<Your application key>
export dostadning_tradera_pkey=<The public key>
```
3. Set up a user "dostadning" with permissions to create a database on a postgresql instance. Store the connection string in an environment variable
````
export dostadning_records_pgres_cs="Host=localhost; Username=dostadning; Password=<the password>;"
````
4. The frontend for dostadning is a simple cli. You run it most conveniently from `app/dostadning.cli`. `dotnet run -- help` will list the commands.

### Prerequisites

- *Postgresql 10.5* (I installed via `brew install postgres`). Dostadning needs a user named dostadning with permissions to create a database. Consult [npgsql](http://www.npgsql.org/efcore/) docs upon confusion.


### Installing

Install dependencies:
```
dotnet restore
```

The interface is not super well-documented but you can look at the [Commands.fs](app/dostadning.cli/Commands.fs) to work out what to do. Basically what you want is:
1. Make a note of your tradera username
2. Prepare a csv file with each auction you want to post to your seller. The file needs to have headers like in the [example testdata](app/dostadning.cli/testdata). Images needs go into a filestructure wher they're in directories with the same id as the auction. Example in testdata.
3. Create an account 
4. Add a tradera seller to your account 
5. Get consent to let dostadning post auctions to your account
6. Upload a lot of auctions at once. 

## Running the tests

`cd test/unit` then `dotnet test`. 

### Break down into end to end tests

The tests are focused on proving that the composition of observables does the right things in the right order.
