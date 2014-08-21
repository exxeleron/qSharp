### Types mapping

The `QTypes` class defines mapping between the q and corresponding C# types.

```
| qSharp type identifier | q num type    | q type                | C# type       |
|------------------------|---------------|-----------------------|---------------|
| Bool                   | -1            | boolean               | bool          |
| BoolList               |  1            | boolean list          | bool[]        |
| Byte                   | -4            | byte                  | byte          |
| ByteList               |  4            | byte list             | byte[]        |
| Guid                   | -2            | guid                  | System.Guid   |
| GuidList               |  2            | guid list             | System.Guid[] |
| Short                  | -5            | short                 | short         |
| ShortList              |  5            | short list            | short[]       |
| Int                    | -6            | integer               | int           |
| IntList                |  6            | integer list          | int[]         |
| Long                   | -7            | long                  | long          |
| LongList               |  7            | long list             | long[]        |
| Float                  | -8            | real                  | float         |
| FloatList              |  8            | real list             | float[]       |
| Double                 | -9            | float                 | double        |
| DoubleList             |  9            | float list            | double[]      |
| Char                   | -10           | character             | char          |
| String                 |  10           | string                | char[]        |
| Symbol                 | -11           | symbol                | string        |
| SymbolList             |  11           | symbol list           | string[]      |
| Timestamp              | -12           | timestamp             | QTimestamp    |
| TimestampList          |  12           | timestamp list        | QTimestamp[]  |
| Month                  | -13           | month                 | QMonth        |
| MonthList              |  13           | month list            | QMonth[]      |
| Date                   | -14           | date                  | QDate         |
| DateList               |  14           | date list             | QDate[]       |
| Datetime               | -15           | datetime              | QDateTime     |
| DatetimeList           |  15           | datetime list         | QDateTime[]   |
| Timespan               | -16           | timespan              | QTimespan     |
| TimespanList           |  16           | timespan list         | QTimespan[]   |
| Minute                 | -17           | minute                | QMinute       |
| MinuteList             |  17           | minute list           | QMinute[]     |
| Second                 | -18           | second                | QSecond       |
| SecondList             |  18           | second list           | QSecond[]     |
| Time                   | -19           | time                  | QTime         |
| TimeList               |  19           | time list             | QTime[]       |
| GeneralList            |  0            | general list          | object[]      |
| Lambda                 |  100, 104     | function body         | QLambda       |
| Table                  |  98           | table                 | QTable        |
| KeyedTable             |  99           | keyed table           | QKeyedTable   |
| Dictionary             |  99           | dictionary            | QDictionary   |
```

### Temporal types
q language provides multiple types for operating on temporal data. The `qSharp` library provides a corresponding temporal class for each q temporal type. 

Instance of each class can be created:
- from from the underlying base type (`long` in case of `QTimespan` and `QTimestamp`, `double` in case of `QDateTime`, etc.),
- via conversion from System.DateTime instance,
- from q String representation via factory method `fromString(...)`

Every temporal class in the `qSharp` library implements the `IDateTime` interface:

```csharp
public object GetValue()      // Returns internal q representation of the temporal data
public DateTime ToDateTime()  // Represents a q date/time with the instance of System.DateTime
```

### Null values

The `QTypes` utility class exposes a static method `GetQNull(QType type)` that returns corresponding q null value of given type. Keep in mind that null values are only defined and available for primitive q types.

As null values in q are represented as arbitrary values, it is also possible to produce null value without explicitly calling the `GetQNull` method. q null values are mapped to C# according to the following table:

```
| kdb+ type | C# null                         |
|---------- | --------------------------------|
| bool      | false                           |
| byte      | (byte) 0                        |
| guid      | new UUID(0, 0)                  |
| short     | short.MinValue                  |
| int       | int.MinValue                    |
| long      | long.MinValue                   |
| real      | float.NaN                       |
| double    | double.NaN                      |
| character | ' '                             |
| symbol    | ""                              |
| timestamp | new QTimestamp(long.MinValue)   |
| month     | new QMonth(int.MinValue)        |
| date      | new QDate(int.MinValue)         |
| datetime  | new QDateTime(double.NaN)       |
| timespan  | new QTimespan(long.MinValue)    |
| minute    | new QMinute(int.MinValue)       |
| second    | new QSecond(int.MinValue)       |
| time      | new QTime(int.MinValue)         |
```