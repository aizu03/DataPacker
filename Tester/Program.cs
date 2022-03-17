#pragma warning disable CA1416

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DataPacker.Serialization;
using System.Numerics;
using Tester.Path;
using DataPacker;
using System.Text;
using static System.String;

namespace Tester;

public class Tester
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Begin tests...\n");

        TestSimpleClasses();
        Console.WriteLine("Test Simple Classes passed!\n");

        TestSimpleList(false);
        TestSimpleList(true);
        Console.WriteLine("Test Simple List passed!\n");

        TestArrays();
        Console.WriteLine("Test Arrays passed!\n");

        TestSelfReference();
        Console.WriteLine("Test References passed!\n");

        TestIndexed();
        Console.WriteLine("Test Indexed1 passed!\n");
        TestIndexedAdvanced();
        Console.WriteLine("Test Indexed2 passed!\n");

        Console.WriteLine("\nAll tests passed!");
    }

    private static void TestSimpleClasses()
    {
        var pointers = new Pointers();
        var simple = new Simple("Alice", 22, 1337, "wants Bobs attention!");
        var pi = 3.141592653589793238;
        var info = "Testing 123. Hello !?";

        using var ms = new MemoryStream();
        using var writer = new SequenceWriter(ms, DataStructure.Sequential, Encoding.ASCII);

        writer.Add(CompactFormatter.Serialize(simple));
        writer.Add(CompactFormatter.Serialize(pointers));
        writer.Add(pi);
        writer.Add(info);
        writer.Write(false);

        ms.Position = 0;

        using var reader = new SequenceReader(ms, DataStructure.Sequential, Encoding.ASCII, true);

        if (simple.description != CompactFormatter.Deserialize<Simple>(reader[0].data).description ||
            pointers.ip2 != CompactFormatter.Deserialize<Pointers>(reader[1].data).ip2 ||
            pi != reader[2].ToDouble() ||
            info != reader[3].ToString())
            throw new Exception();
        ;
    }

    private static void TestSimpleList(bool named)
    {
        var list = new List<TestEnum>
        {
            TestEnum.NotQuite,
            TestEnum.TryAgain,
            TestEnum.Yes,
            TestEnum.Right,
            TestEnum.Yes
        };

        if (named)
            list.Sort((a, b) => CompareOrdinal(a.ToString(), b.ToString()));

        var before = CompactFormatter.Serialize(list);
        using var ms = new MemoryStream();

        SequenceWriter writer;

        if (named)
        {
            writer = new SequenceWriter(ms, DataStructure.SequentialNamed, Encoding.ASCII);
            writer.Add("The list", before);
        }
        else
        {
            writer = new SequenceWriter(ms, DataStructure.Sequential, Encoding.ASCII);
            writer.Add(before);
        }

        writer.Write(false);
        writer.Dispose();

        ms.Position = 0;

        List<TestEnum> listRecovered;

        if (named)
        {
            using var reader = new SequenceReader(ms, DataStructure.SequentialNamed, Encoding.ASCII, true);
            listRecovered = CompactFormatter.Deserialize<List<TestEnum>>(reader["The list"].data);
        }
        else
        {
            using var reader = new SequenceReader(ms, DataStructure.Sequential, Encoding.ASCII, true);
            listRecovered = CompactFormatter.Deserialize<List<TestEnum>>(reader[0].data);
        }

        if (listRecovered.Count != 5) throw new Exception();
    }

    private static void TestArrays()
    {
        var arr = new ArraysTest
        {
            simple1 = new Vector2[] { new(1.14f, 2.3f), new(-4f, 90f) },
            simple2 = new[] { "Yes", "No" },
            multi = new[] { new[] { 55m, 34m }, new[] { 12m, 13m }, new[] { 88m, 99m }, new[] { 777m, 0m } },
            name = "Alice",
            age = 21,
            vector = new Vector2(19.8024f, 1.40124445f),
            testing = TestEnum.Yes
        };

        var bytes = CompactFormatter.Serialize(arr);
        var arrRec = CompactFormatter.Deserialize<ArraysTest>(bytes);

        if (arrRec.vector.X != arr.vector.X ||
            arrRec.simple2[1] != arr.simple2[1] ||
            arrRec.multi[3][0] != arr.multi[3][0]) throw new Exception();
    }

    private static void TestSelfReference()
    {
        var challenge = new ReferenceProblem();
        var bytes = CompactFormatter.Serialize(challenge);
        var challengeSolved = CompactFormatter.Deserialize<ReferenceProblem>(bytes);

        if (challengeSolved.someChild.stackOverflow != challengeSolved ||
            challengeSolved.someChild.me != challengeSolved.someChild ||
            challengeSolved.someChild.solved != "yes") throw new Exception();
        ;
    }

    private static void TestIndexed()
    {
        using var ms = new MemoryStream();
        using var writer = new SequenceWriter(ms, DataStructure.Indexed, Encoding.Unicode);

        writer.Add("Hello World!");
        writer.Add(33);
        writer.Add(42);
        writer.Add("Alice?");
        writer.Add(69);
        writer.Add('\n');
        writer.Add("End");

        writer.Write(false);
        ms.Position = 0;

        using var reader = new SequenceReader(ms, DataStructure.Indexed, Encoding.Unicode);

        Console.WriteLine($"{reader.Available()} entries are available to read!");

        // Read range
        reader.Read(2, 5, false);

        var num1 = reader[0].ToInt32();
        var name = reader[1].ToString();
        var num2 = reader[2].ToInt32();
        var chr = reader[3].ToChar();

        if (num1 != 42 || name != "Alice?" || num2 != 69 || chr != '\n') throw new Exception();

        reader.Entries.Clear();
        reader.ReadOne(0, false);
        reader.ReadOne(6);

        var first = reader[0].ToString();
        var last = reader[1].ToString();

        if (first != "Hello World!" || last != "End") throw new Exception();
    }

    private static void TestIndexedAdvanced()
    {
        using var ms = new MemoryStream();
        using var writer = new SequenceWriter(ms, DataStructure.IndexedNamed, Encoding.ASCII);

        writer.Add("e1", "Hello World!");
        writer.Add("e2", 33);
        writer.Add("e3", 42);
        writer.Add("e4", "Alice?");

        writer.Write(false);
        ms.Position = 0;

        using var reader = new SequenceReader(ms, DataStructure.IndexedNamed, Encoding.ASCII);
        Console.WriteLine($"{reader.Available()} entries are available to read!");
        reader.Read(1, 2, false);

        if (reader["e2"].ToInt32() != 33 || reader["e3"].ToInt32() != 42) throw new Exception();

        ms.Position = 0;

        // Append new data
        using var writerAppend = new SequenceWriter(ms, DataStructure.IndexedNamed, Encoding.ASCII, reader);

        writer.Add("e5", 69);
        writer.Add("e6", '\n');
        writer.Add("e7", "End");

        writer.Write(false);

        ms.Position = 0;
        using var reader2 = new SequenceReader(ms, DataStructure.IndexedNamed, Encoding.ASCII, true);
        if (reader2["e6"].ToChar() != '\n' || reader2["e5"].ToInt32() != 69) throw new Exception();

    }
}