using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace ThisMember.Data.Interfaces
{
  public interface IDataReaderMapper
  {
    TDestination Map<TDestination>(IDataRecord reader);
  }
}
