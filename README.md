# DataPacker

A .NET library to combine multiple objects into one small byte array and vice versa. It also comes with a small class serializer.

Types of Writers/Readers

* Sequential
* Indexed

# Usage

All entries must be primitives. 
If you want to add an object use the:

```C#
CompactFormatter
```

## Default SequenceWriter/SequenceReader

Structure: Sequential

```C#
using var ms = new MemoryStream(); // any type of data stream
using var writer = new SequenceWriter(ms, DataStructure.Sequential);
writer.Add("Hello World!");
writer.Add(123);
writer.Add(3.14159265358979323824);
writer.Write();
```

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[MIT](https://choosealicense.com/licenses/mit/)
