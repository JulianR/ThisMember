using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThisMember.Data.Interfaces;
using System.Data.Common;
using ThisMember.Core;
using System.Data;
using System.Reflection;
using ThisMember.Core.Interfaces;

namespace ThisMember.Data
{
  public class DataReaderMapper : IDataReaderMapper
  {
    public DataReaderMapper()
    {
    }

    private readonly Dictionary<Type, Func<IDataRecord, object>> mappingCache = new Dictionary<Type, Func<IDataRecord, object>>();

    public TDestination Map<TDestination>(IDataRecord reader)
    {
      var destinationType = typeof(TDestination);

      var destinationProperties = (from p in destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                   where p.CanWrite && !p.GetIndexParameters().Any()
                                   select (PropertyOrFieldInfo)p)
                                   .Union(from f in destinationType.GetFields()
                                          where !f.IsStatic
                                          select (PropertyOrFieldInfo)f);
      
      
      
      return default(TDestination);
    }
  }
}
