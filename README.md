VVVV.Packs.VObject
==================

A VObject is a generic object wrapper for vvvv and an Object-Oriented-Patching pipeline.

It is a base class which contains the actual wrapped object as "Content" and some methods for (de)serialization, cloning (deepcopy) and disposing. A wrapper class for your object needs to inherit from VObject which will contain your actual class. This is necessary so you can handle together radically different types of objects in VVVV in a single spread or a collection.

For Management there are 2 main classes, VObjectDictionary and VObjectCollection (which are also inherited from VObject). In VObjectDictionary you can only store VObjectCollections referenced by their string name. VObjectCollections on the other hand can store any kind of VObjects (including both VObjectCollections and dictionaries) which means they can be nested. In theory a collection can even hold itself but it's not tested and may result in undefined behavior.

Wrapping primitive datatypes (values, strings, raw (streams), vectors, colors and matrices) is done by PrimitiveObject class and its set of nodes very similar to Velcrome's messages. See girlpower for their usage.

Above mentioned classes are all a derived members of the VPathQueryable abstract class which allows for simple retrieval of nested objects by an XPath like syntax. Multiple objects can be queried by a Regex pattern or a single absolute reference can be used in quotes. Nested levels are separated by a user defined separator character, which is ¦ by default, the reason was that this character is the least likely in regex patterns. But if you don't use regex at all you can set a more common character like . or / etc.

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

Other Implementations
---------------------

**Smoothing**<br />
Execute a smoothing algorithm on a given value (simple types only yet, no vectors and matrices, however it would be simple to implement). Values are selected from PrimitiveObjects double or float types via VPath.

**JSON**<br />
There's a JObject wrapper migrated from the JSON nodes of sanch.

**Stopwatch**<br />
Simple system stopwatch wrapper so you can define custom time based actions on your set of VObjects

**Messages**<br />
Wrap and unwrap Velcrome's messages

**VOOG**<br />
VObject Oriented Gui framework. Implements also a TypeWriter clone which is able to handle text selection too, and Hittest methods from the addonpack. Modules require to have mcropack present

**WebSockets**<br />
Websocket-Sharp wrapper which is dubbed VebSocket in VVVV. Both servers and clients are implemented. Server nodes are still kind of experimental.

**HTTP**<br />
HTTP swiss army knife represented as VObjects so you have total control over sessions. Also supports everything you'd expect + cookies.
