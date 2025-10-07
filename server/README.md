
# Mod examples for v4.0.0

A collection of example mods that perform various actions in SPT

# Setup
The solution has numbered folders, starting at 1 and work downwards to find examples with increasing complexity.

Each mod imports multiple NUGET packages. These are used as libraries of the server code.

### Prerequisites
 [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
 Visual Studio Community / Ryder
 
### **Essential Concepts**
Prioritize understanding Dependency Injection and Inversion of Control, the architectural principles SPT adopts.

 - [A quick intro to Dependency Injection](https://www.freecodecamp.org/news/a-quick-intro-to-dependency-injection-what-it-is-and-when-to-use-it-7578c84fa88f/)
 - [Understanding Inversion of Control (IoC) Principle](https://medium.com/@amitkma/understanding-inversion-of-control-ioc-principle-163b1dc97454)

### Build
`Visual Studio > Build > Rebuild Solution`
`Rider > TODO`
 
## Distribution
- Build the project in 'Release' mode
- Copy the folder inside: `mod\bin\Release` into your servers `/mods` folder
- Start server
