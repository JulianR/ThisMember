using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThisMember.Core;

namespace ThisMember.Test
{
  [TestClass]
  public class CamelCaseTests
  {
    [TestMethod]
    public void CamelSplitTest()
    {
      const string Test1 = "CompanyName";
      const string Test2 = "Company";
      const string Test3 = "company";
      const string Test4 = "companyname";
      const string Test5 = "companyName";
      const string Test6 = "CompanyNameTest";
      const string Test7 = "CompanyNAmeTest";
      const string Test8 = "ABCCompanyNameTest";
      const string Test9 = "CompanyID";

      var result = CamelCaseHelper.SplitOnCamelCase(Test1);

      Assert.AreEqual("Company", result[0]);
      Assert.AreEqual("Name", result[1]);

      result = CamelCaseHelper.SplitOnCamelCase(Test2);

      Assert.AreEqual("Company", result[0]);

      result = CamelCaseHelper.SplitOnCamelCase(Test3);

      Assert.AreEqual("company", result[0]);

      result = CamelCaseHelper.SplitOnCamelCase(Test4);

      Assert.AreEqual("companyname", result[0]);

      result = CamelCaseHelper.SplitOnCamelCase(Test5);

      Assert.AreEqual("company", result[0]);
      Assert.AreEqual("Name", result[1]);

      result = CamelCaseHelper.SplitOnCamelCase(Test6);

      Assert.AreEqual("Company", result[0]);
      Assert.AreEqual("Name", result[1]);
      Assert.AreEqual("Test", result[2]);

      result = CamelCaseHelper.SplitOnCamelCase(Test7);

      Assert.AreEqual("Company", result[0]);
      Assert.AreEqual("NAme", result[1]);
      Assert.AreEqual("Test", result[2]);

      result = CamelCaseHelper.SplitOnCamelCase(Test8);

      Assert.AreEqual("ABCCompany", result[0]);
      Assert.AreEqual("Name", result[1]);
      Assert.AreEqual("Test", result[2]);

      result = CamelCaseHelper.SplitOnCamelCase(Test9);

      Assert.AreEqual("Company", result[0]);
      Assert.AreEqual("ID", result[1]);
    }
  }
}
