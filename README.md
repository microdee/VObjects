Update note: for compatible version see branch V1. This version will introduce breaking changes with naming and stuff for a couple of weeks.

VObject 2 (VVEETOP)
===================

VObject or now Weak typed Object Pipeline (VVETOP) is a toolkit for vvvv to handle the base .NET object class. (Instead of being an immediate wrapper base type. That is gone now entirely)

There are couple of helper classes which help you organize your objects. First is VObjectCollection which is basically a dictionary with some extra data.

Second is for wrapping primitive datatypes (values, strings, raw (streams), vectors, colors and matrices) is the PrimitiveObject class and its set of nodes very similar to Velcrome's messages. See girlpower for their usage. However note you can directly cast primitive types to an object with AsWeakObject node from mcropack and use them directly inside a VObjectCollection if you fancy.

Third: above mentioned classes are all a derived member of the VPathQueryable abstract class which allows for simple retrieval of nested objects by an XPath like syntax if nested containing objects are also derived from this class. Note it can reference an object at the end of the path hierarchy not inheriting VPathQueryable. Multiple objects can be queried by a Regex pattern or a single absolute reference can be used in quotes. Nested levels are separated by a user defined separator character, which is ¦ by default, the reason was that this character is the least likely to turn up in regex patterns. But if you don't use regex at all you can set a more common character like . or / etc.

syntax in practice:

**Usage**
```
"Absolute reference in quotes"¦RegexMultipleObjects.*?$
```

**Get everything from the third level**
```
.*?$¦.*?$¦.*?$
```

**Get everything from "Parent"**
```
"Parent"¦.*?$
```

**Get the "Child" object from every parent object**
```
.*?$¦"Child"
```

**Simple case with / separator**
```
"Parent"/"Child"/"Field"
```

Other Included libraries / wrappers using VVETOP
------------------------------------------------

**Smoothing**<br />
Execute a smoothing algorithm on a given value (simple types only yet, no vectors and matrices, however it would be simple to implement). Values are selected from PrimitiveObjects double or float types via VPath.

**JSON**<br />
There's a JToken wrapper migrated from the JSON nodes of sanch.

**Stopwatch**<br />
Simple system stopwatch wrapper so you can define custom time based actions on your set of objects

**WebSockets**<br />
Websocket-Sharp wrapper which is dubbed VebSocket in VVVV. Both servers and clients are implemented. Server nodes are still kind of experimental.

**HTTP**<br />
HTTP swiss army knife which allows you to have total control over sessions. Also supports everything you'd expect + cookies.
