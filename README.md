# DataPacker

A lightweight .NET library to combine multiple objects into one small byte array and vice versa. It also comes with a small class serializer.

# Usage

Types of Writers/Readers

* Sequential
* Indexed

Supported entries: All primitives, string, byte[]

## Default SequenceWriter/SequenceReader

```C#
using var ms = new MemoryStream(); // any type of data stream
using var writer = new SequenceWriter(ms);
writer.Add("Hello World!");
writer.Add(123);
writer.Add(3.141);
writer.Flush();
var arr = ms.ToArray(); // if you want the bytes
```

Named:
```C#
using var writer = new SequenceWriter(ms, DataStructure.SequentialNamed);
writer.Add("Description", "Alice is looking for Bob!");
```

Sequential data is saved as this:
```C#
[len, data, len, data, len, data...]
```

If the structure is a named:
```C#
[len name, name, len data, data, len name, name, len data, data]
```

Reading:
```C#
using var reader = new SequenceReader(ms);
reader.Read();
var str = reader.Entries[0].ToString();
var num = reader.Entries[1].ToInt32();
var pi = reader.Entries[2].ToDouble();
```

## Indexed SequenceWriter/SequenceReader

The indexed writer stores all data indexes in a book at the end of the array.

```C#
using var ms = new MemoryStream(); // any type of data stream
using var writer = new SequenceWriter(ms, DataStructure.Indexed);
writer.Add("Junk");
writer.Add("Hello World!");
writer.Add(111);
writer.Flush();
```

You can choose what to read:

```C#
using var reader = new SequenceReader(ms, DataStructure.Indexed);
reader.ReadOne(1); // read only index 1
var helloWorld = reader.Entries[0].ToString();
```
Ranges:
```C#
reader.Read(51, 69);
```

Append data to an existing sequence:

```C#
using var reader = new SequenceReader(ms, DataStructure.Indexed); // read the book
Console.WriteLine($"{reader.Available()} entries are available to read!");
// Append new data
using var writer = new SequenceWriter(ms, DataStructure.Indexed, appendReader: reader);
writer.Add("New Data 1!");
writer.Add("New Data 2!");
writer.Add("New Data 3!");
writer.Flush();
```

The indexed structure looks like this:
```C#
[data, data, data, .. book]
```
named:
```C#
[name, data, name, data, name, data, .. book]
```
## Using the CompactFormatter

If you want to serialize an object:

```C#
var list = new List<string>(); // some object
list.Add("Hello");
list.Add("World!");
var bytes = CompactFormatter.Serialize(list);
// do stuff..
var list = CompactFormatter.Deserialize<List<string>>(bytes);
```

SequenceWriter and CompactFormatter combined:
```C#
using var ms = new MemoryStream();
using var writer = new SequenceWriter(ms, DataStructure.SequentialNamed);
writer.Add("Name", "Alice");
writer.Add("Age", 22);
writer.Add("Data", CompactFormatter.Serialize(some object));
writer.Flush();
```

Get the object:
```C#
using var reader = new SequenceReader(ms, DataStructure.SequentialNamed);
reader.Read();
var bytes = reader["Data"];
var data = CompactFormatter.Deserialize<SomeClass>(bytes);
```

## TODO

* ~~Custom string encoding~~
* ~~Indexed sequence~~
* ~~Read ranges~~
* ~~Simple serializer~~
* Compressed formatter


## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[MIT](https://choosealicense.com/licenses/mit/)
