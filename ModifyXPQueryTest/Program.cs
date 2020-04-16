using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModifyXPQueryTest {
  class Program {
    static void Main(string[] args) {
      XpoDefault.DataLayer = XpoDefault.GetDataLayer(
        MSSqlConnectionProvider.GetConnectionString("(localdb)\\mssqllocaldb", "ModifyXPQueryTest"),
        AutoCreateOption.DatabaseAndSchema);

      CreateTestData();

      using (var uow = new UnitOfWork()) {
        // This works - obviously
        //var data = CollectionData(uow);

        // This also works - returning data from an XPQuery, but without projection
        var data = QueryDataFullObjects(uow);

        // This wouldn't work - if the type is not Person, I couldn't modify it
        // var data = QueryDataPartialObjects(uow);

        ModifyObjects(data);

        uow.CommitChanges();
      }

      OutputData();
    }

    // This returns fully functional XPO objects that can be modified later.
    static IEnumerable<Person> QueryDataFullObjects(UnitOfWork uow) {
      return from p in new XPQuery<Person>(uow)
             where p.Age > 30
             select p;
    }

    // Purely hypothetical scenario here, using projection with the expression.
    // This can be done of course, but obviously it wouldn't even be possible to 
    // return the data because the type is unknown. I could create a type, but 
    // that type wouldn't be tracked by XPO and couldn't be modified.
    // 
    //static IEnumerable<???> QueryDataPartialObjects(UnitOfWork uow) {
    //  return from p in new XPQuery<Person>(uow)
    //         where p.Age > 30
    //         select new { p.Age };
    //}

    static IEnumerable<Person> CollectionData(UnitOfWork uow) {
      return new XPCollection<Person>(uow, new BinaryOperator("Age", 30, BinaryOperatorType.Greater));
    }

    static void ModifyObjects(IEnumerable<Person> people) {
      foreach(var p in people) {
        p.Age = p.Age + 13;
      }
    }

    static void OutputData() {
      using (var uow = new UnitOfWork()) {
        var data = new XPCollection<Person>(uow);
        foreach (var p in data) {
          Console.WriteLine($"Name: {p.Name}, Age: {p.Age}");
        }
      }
    }

    static void CreateTestData() {
      using (var uow = new UnitOfWork()) {
        if (!new XPQuery<Person>(uow).Any()) {
          new Person(uow) { Name = "Willy", Age = 33 };
          new Person(uow) { Name = "Bob", Age = 13 };
          new Person(uow) { Name = "Harry", Age = 47 };
          new Person(uow) { Name = "Anna", Age = 25 };
          new Person(uow) { Name = "Jasmine", Age = 64 };
          new Person(uow) { Name = "Judy", Age = 19 };
          uow.CommitChanges();
        }
      }
    }
  }

  public class Person: XPObject {
    public Person(Session session) : base(session) {}

    private string name;
    public string Name {
      get {
        return name;
      }
      set {
        SetPropertyValue<string>("Name", ref name, value);
      }
    }

    private int age;

    public int Age {
      get {
        return age;
      }
      set {
        SetPropertyValue<int>("Age", ref age, value);
      }
    }
  }
}
