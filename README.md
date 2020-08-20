# A3: OOP, Database design, optimization and code

## introduction

For this assignment, for the final product of assignments 3+4, you will receive a set of requirements and ultimately create

1. C# object model (defining objects, attributes, and methods)
2. a database accurately corresponding to your object model
3. A powershell script to load data into the database
4. A powershell script to test the current state of the database
5. C# code to load respective objects into memory
6.  An additional C# that match a specific C# Interface (that will be provided) that will execute a particular operation on the database - I will provide a test C# program, you must run your code with my test code, and your code must pass my provided test
7. you will optimize the database using techniques taught in a later class

We will discuss this assignment more in class.

## Object Model

The program you are building would (if extended) be a tool for users to interact with H19 data, zoom in on graphs, etc. As much of the relevant of the operations should be in the application memory.

For simplicity, your project is in WA state only, so no need to model the 'State' object.

You are building only the database interaction level code.

The main operations are:

## 1. Queries

### A. Display "hot" counties at a date or date range

- This will similar to the query in the previous assignment. The program will display the top N county, dates, and case deltas info. However, your code will return a list of relevant objects.

For example, with some class heirarchy, you need to implement:

```csharp
public List<County> getHotCounties() {}

class CaseDatum {

   DateTime when;

   int cases;

}

class County {

  String countyName;

   int population;

  List<caseDatum> samples;

}
```

### B. return first county (object) where delta cases per capita is greater than a given number

-- simple query on county based on derived table

if you choose EF w/LinQ is similar to the example in my LinQ-ER demo

## 2. Display county info including timeserie data for daterange/all time

- your code needs to return a county object with the entire timeseries of cases for use by other code (e.g. analysis engine) . County info includes population so that per-capita calculations can be made

## 3. County case prediction

Your code needs to store (and retrieve) a case prediction data calculated by existing code. This data is in the same format as #2

// this is county data "for the future"

same name/structure but goes to a different table or no table. for you, those objects are not bound to the db.

You will receive C# Interfaces
```csharp
   public interface IH19Datum{};

   public interface ICountyInfo{};
```

(very similar to above)

And also the signature of the  calls for questions above



For data loading, you can use your existing csv loading code (to load data in the existing format),

and perform any post-processing in C#, or use another method.   
