# Evan.Dynamic

## DynamicObject

## Basic Usage
```cs
var list = new List<int>();
dynamic listProxy = DynamicObject.CreateProxy(list); // return IObjectProxy<List<int>>

listProxy.Add(1);
```
