This document describes the coding guidelines for any .NET for C# code for this project.

# Class and File Separation
If a class is more than 50 lines long in definition, it should be stored in its own file. 

# Domain Models
Domain models should largely contain the properties and the data fields associated to that data model. There should not be any attributes or functions but to remain largely 'anemic' style.

# Dependency Injection for Functional Classes or Behaviors
Dependency injection should be used as a method for providing inversion of control. For every functional class should have an accompanying interface that it implements. Any other class that wants to use this class should then use the interface instead of the implementation directly. For example, a Parser class would have its interface definition IParser in the same file at the top of the file with the Parser implementation. Any user of the Parser would then use constructor injection to inject in the IParser interface. If the interface is used by more than one implementation, move the interface out of the same file into its own file so that the interface can more easily be found. 

# Ports and Adapter
The .NET projects should follow the Ports and Adapters architecture where external system dependencies identified to be behind a port should not leak adapter specific details into the domain and application layer unless absolutely necessary. For example, an adapter that uses MongoDB should not have a reference to the MongoDB driver within the domain layer. 

## Port Interface
The application's usage of an adapter is limited to using the port interface for that adapter and not any direct implementation of the adapter. For example: the database adapter IDatabase will be the only interface the application layer can use. The mongo db adapter might implement the IDatabase with MongoDatabase but the details of how that is done is hidden in the adapter layer.

## Data Transfer Objects
If the adapter requires a different data model representation than the domain models, it should use the <Prefix>Converter pattern to convert the domain models into a Dto (DataTransferObject) for that adapter. The <DomainName>Dto would then be used within the adapter to perform the logic required by the adapter. For example: A Book domain model might be mapped to a BookDto if required when being used within an adapter. The adapter that retrieves a BookDto, might convert it back to a Book domain model before it is passed back to the calling Application layer.