// using System;
// using System.Collections.Generic;
//
// namespace Shiny;
//
//
// // class will implement INotifyPropertyChanged - if class already does this, then no INPC generation?
// public partial class TestClass
// {
//     [BindSetting("nameofkeyvaluestore")]
//     public partial string Value { get; set; } // can be private/internal/protected set - must be partial property as must class
//     
//     
//     // TODO: generate INPC?
// }
//
//
// // class must be partial
// public partial class TestRepoClass : IRepositoryEntity
// {
//     [RepositoryValue] // source gens IRepositoryEntity implementation automatically
//     public partial int? RepoValue { get; set; } // TODO: support private/internal/protected set for source gen
//
//
//     public void Store(IDictionary<string, object> data)
//     {
//         if (this.RepoValue != null)
//             data.Add(nameof(this.RepoValue), this.RepoValue.Value);
//     }
//
//
//     public void Inflate(IReadOnlyDictionary<string, object> data)
//     {
//         if (data.ContainsKey(nameof(this.RepoValue)))
//             this.RepoValue = (int?)data[nameof(this.RepoValue)];
//     }
// }
//
//
// [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
// public class BindSettingAttribute(string AttributeKey) : Attribute;
//
//
// public interface IRepositoryEntity
// {
//     void Store(IDictionary<string, object> data);
//     void Inflate(IReadOnlyDictionary<string, object> data);
// }
//
// [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
// public class RepositoryValueAttribute : Attribute;